using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BenyFinance.Application.DTOs;
using BenyFinance.Application.Interfaces;
using BenyFinance.Domain.Entities;
using BenyFinance.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BenyFinance.Application.Services;

public class ImportService : IImportService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ImportService(ITransactionRepository transactionRepository, ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<ImportPreviewDto> PreviewAsync(Guid userId, IFormFile file, string source)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Arquivo inválido");

        var categories = (await _categoryRepository.GetAllByUserIdAsync(userId)).ToList();
        var rows = new List<(DateTime date, decimal amount, string desc)>();

        var isPdf = file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
        if (isPdf)
        {
            using var pdfStream = file.OpenReadStream();
            rows = ParseMercadoPagoPdf(pdfStream);
        }
        else
        {
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var lines = new List<string>();
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (!string.IsNullOrWhiteSpace(line)) lines.Add(line);
            }
            if (lines.Count == 0) throw new Exception("Arquivo vazio");
            lines.RemoveAt(0); // header

            foreach (var l in lines)
            {
                try
                {
                    var parsed = source.ToLower() switch
                    {
                        "nubank" => ParseNubank(l),
                        "mercadopago" => ParseMercadoPago(l),
                        _ => ParseGeneric(l)
                    };
                    if (parsed != null) rows.Add(parsed.Value);
                }
                catch
                {
                    // linha inválida: ignora
                }
            }
        }

        if (rows.Count == 0)
            throw new Exception("Nenhuma transação encontrada no arquivo");

        var items = new List<ImportPreviewItemDto>();
        int idx = 0;
        foreach (var (date, amount, desc) in rows)
        {
            idx++;
            var type = amount >= 0 ? "income" : "expense";
            var suggested = SuggestCategory(categories, desc.ToLower());
            items.Add(new ImportPreviewItemDto(idx, date, desc, Math.Abs(amount), type, suggested.Name, suggested.Id, "cash", null, "pending", true, null, false));
        }

        return new ImportPreviewDto(items.Count, items.Count(i => i.IsValid), items);
    }

    public async Task<int> ConfirmAsync(Guid userId, ImportConfirmDto confirmDto)
    {
        var categories = (await _categoryRepository.GetAllByUserIdAsync(userId)).ToList();
        int count = 0;
        foreach (var item in confirmDto.Items)
        {
            var catId = item.CategoryId;
            if (item.CreateCategory && !string.IsNullOrWhiteSpace(item.NewCategoryName))
            {
                var existing = categories.FirstOrDefault(c => c.Name.Equals(item.NewCategoryName, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    catId = existing.Id;
                }
                else
                {
                    var newCat = new Category { Name = item.NewCategoryName, Color = item.NewCategoryColor ?? "#6366f1", UserId = userId };
                    await _categoryRepository.AddAsync(newCat);
                    categories.Add(newCat);
                    catId = newCat.Id;
                }
            }
            if (!Enum.TryParse<TransactionType>(item.Type, true, out var type)) continue;
            if (!Enum.TryParse<PaymentMethod>(item.PaymentMethod, true, out var pm)) pm = PaymentMethod.Cash;
            if (!Enum.TryParse<TransactionStatus>(item.Status, true, out var st)) st = TransactionStatus.Pending;
            var tx = new Transaction { Date = item.Date, Description = item.Description, Amount = item.Amount, Type = type, CategoryId = catId, PaymentMethod = pm, CardId = item.CardId, Status = st, UserId = userId };
            await _transactionRepository.AddAsync(tx);
            count++;
        }
        return count;
    }

    private List<(DateTime date, decimal amount, string desc)> ParseMercadoPagoPdf(Stream stream)
    {
        var result = new List<(DateTime, decimal, string)>();
        var pendingCarry = new List<string>();
        using var doc = UglyToad.PdfPig.PdfDocument.Open(stream);
        foreach (var page in doc.GetPages())
        {
            var sorted = page.GetWords()
                .OrderByDescending(w => w.BoundingBox.Top)
                .ThenBy(w => w.BoundingBox.Left)
                .ToList();
            var lines = new List<(double y, List<UglyToad.PdfPig.Content.Word> words)>();
            foreach (var w in sorted)
            {
                if (lines.Count > 0 && Math.Abs(lines[lines.Count - 1].y - w.BoundingBox.Top) <= 4)
                    lines[lines.Count - 1].words.Add(w);
                else
                    lines.Add((w.BoundingBox.Top, new List<UglyToad.PdfPig.Content.Word> { w }));
            }

            var rows = new List<(double y, DateTime date, decimal amount, List<(double y, string text)> items)>();
            var descs = new List<(double y, string text)>();
            double headerY = -1;
            bool inDetail = false;

            foreach (var (y, ws) in lines)
            {
                var byX = ws.OrderBy(w => w.BoundingBox.Left).ToList();
                var full = string.Join(" ", byX.Select(w => w.Text));
                if (string.IsNullOrWhiteSpace(full)) continue;

                if (full.Contains("DETALHE DOS MOVIMENTOS", StringComparison.OrdinalIgnoreCase))
                {
                    inDetail = true;
                    headerY = -1;
                    continue;
                }
                bool isHeader = full.Contains("ID da", StringComparison.OrdinalIgnoreCase)
                    || (full.Contains("Data", StringComparison.OrdinalIgnoreCase) && full.Contains("Valor", StringComparison.OrdinalIgnoreCase));
                if (isHeader)
                {
                    inDetail = true;
                    headerY = y;
                    continue;
                }
                if (!inDetail) continue;
                if (headerY >= 0 && y > headerY) continue;
                if (Regex.IsMatch(full, @"^\d{1,2}/\d{1,2}$")) continue;

                var descText = CleanMpDesc(byX.Where(w => w.BoundingBox.Left >= 60 && w.BoundingBox.Left < 197).Select(w => w.Text));
                var dateWord = byX.FirstOrDefault(w => w.BoundingBox.Left < 60 && Regex.IsMatch(w.Text, @"^\d{2}-\d{2}-\d{4}$"));
                if (dateWord != null && DateTime.TryParseExact(dateWord.Text, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    var valWord = byX.FirstOrDefault(w => w.BoundingBox.Left >= 290 && w.BoundingBox.Left < 355 && TryParseDecimal(w.Text, out _));
                    if (valWord != null && TryParseDecimal(valWord.Text, out var amount))
                    {
                        var items = new List<(double, string)>();
                        if (descText.Length > 0) items.Add((y, descText));
                        rows.Add((y, date, amount, items));
                    }
                    continue;
                }

                if (descText.Length > 0) descs.Add((y, descText));
            }

            if (rows.Count > 0 && pendingCarry.Count > 0)
            {
                var first = rows.OrderByDescending(r => r.y).First();
                for (int i = 0; i < pendingCarry.Count; i++)
                    first.items.Add((first.y + 1000 - i * 0.01, pendingCarry[i]));
                pendingCarry.Clear();
            }

            foreach (var (dy, text) in descs)
            {
                if (rows.Count == 0)
                {
                    pendingCarry.Add(text);
                    continue;
                }
                var nearest = rows.OrderBy(r => Math.Abs(r.y - dy)).First();
                if (Math.Abs(nearest.y - dy) <= 25) nearest.items.Add((dy, text));
                else pendingCarry.Add(text);
            }

            foreach (var r in rows.OrderByDescending(r => r.y))
            {
                var parts = r.items.OrderByDescending(i => i.y).Select(i => i.text).ToList();
                var desc = parts.Count > 0 ? string.Join(" ", parts) : "Lançamento Mercado Pago";
                result.Add((r.date, r.amount, desc));
            }
        }
        return result;
    }

    private string CleanMpDesc(IEnumerable<string> words)
    {
        var s = string.Join(" ", words);
        s = Regex.Replace(s, @"\s*R\$\s*", " ");
        s = Regex.Replace(s, @"\s+", " ").Trim();
        return s;
    }

    private (DateTime date, decimal amount, string desc)? ParseNubank(string line)
    {
        var parts = SplitCsv(line);
        if (parts.Length < 4) return null;
        if (!DateTime.TryParseExact(parts[0].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            if (!DateTime.TryParse(parts[0].Trim(), CultureInfo.GetCultureInfo("pt-BR"), out date)) return null;
        if (!TryParseDecimal(parts[1], out var amount)) return null;
        return (date, amount, parts[3].Trim());
    }

    private (DateTime date, decimal amount, string desc)? ParseMercadoPago(string line)
    {
        var parts = SplitCsv(line);
        if (parts.Length < 3) return null;
        int dateIdx = 0; int valIdx = -1;
        for (int i = 0; i < Math.Min(parts.Length, 4); i++)
        {
            if (DateTime.TryParseExact(parts[i].Trim(), new[] { "dd/MM/yyyy", "dd/MM/yy", "yyyy-MM-dd" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            { dateIdx = i; break; }
        }
        for (int i = 0; i < parts.Length; i++)
        {
            var v = parts[i].Trim();
            if (v.Contains("R$") || Regex.IsMatch(v, @"[-+]?\d+[.,]\d+"))
            { if (i == dateIdx) continue; valIdx = i; break; }
        }
        if (!DateTime.TryParseExact(parts[dateIdx].Trim(), new[] { "dd/MM/yyyy", "dd/MM/yy", "yyyy-MM-dd" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) return null;
        if (valIdx == -1 || !TryParseDecimal(parts[valIdx], out var amt)) return null;
        var descParts = parts.Where((p, i) => i != dateIdx && i != valIdx).ToArray();
        var descStr = descParts.Length > 0 ? string.Join(" ", descParts).Trim('"') : parts[1].Trim('"');
        return (d, amt, descStr);
    }

    private (DateTime date, decimal amount, string desc)? ParseGeneric(string line)
    {
        var parts = SplitCsv(line);
        if (parts.Length < 3) return null;
        if (DateTime.TryParseExact(parts[0].Trim(), new[] { "dd/MM/yyyy", "yyyy-MM-dd", "MM/dd/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            if (TryParseDecimal(parts[1], out var amount))
            {
                var desc = string.Join(" ", parts.Skip(2)).Trim('"');
                return (date, amount, desc);
            }
        }
        return null;
    }

    private bool TryParseDecimal(string v, out decimal amt)
    {
        amt = 0;
        if (string.IsNullOrWhiteSpace(v)) return false;
        v = v.Trim().Replace("R$", "").Replace(" ", "");
        if (v.Contains("(") && v.Contains(")")) v = "-" + v.Replace("(", "").Replace(")", "");
        if (v.StartsWith("+")) v = v.Substring(1);
        v = v.Replace(".", "").Replace(",", ".");
        return decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out amt);
    }

    private string[] SplitCsv(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;
        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"' && (i == 0 || line[i - 1] != '\\')) { inQuotes = !inQuotes; continue; }
            if (c == ',' && !inQuotes) { result.Add(sb.ToString()); sb.Clear(); continue; }
            sb.Append(c);
        }
        result.Add(sb.ToString());
        return result.ToArray();
    }

    private (Guid? Id, string Name) SuggestCategory(List<Category> categories, string absDesc)
    {
        foreach (var cat in categories)
        {
            if (absDesc.Contains(cat.Name.ToLower())) return (cat.Id, cat.Name);
        }
        if (absDesc.Contains("super") || absDesc.Contains("mercado") || absDesc.Contains("padaria") || absDesc.Contains("ifd") || absDesc.Contains("loja") || absDesc.Contains("atacado"))
            return (FindCategoryId(categories, "Alimentação"), "Alimentação");
        if (absDesc.Contains("uber") || absDesc.Contains("99") || absDesc.Contains("posto") || absDesc.Contains("combustivel") || absDesc.Contains("gasolina"))
            return (FindCategoryId(categories, "Transporte"), "Transporte");
        if (absDesc.Contains("farmacia") || absDesc.Contains("hospital") || absDesc.Contains("consulta") || absDesc.Contains("medico") || absDesc.Contains("drogaria"))
            return (FindCategoryId(categories, "Saúde"), "Saúde");
        if (absDesc.Contains("energia") || absDesc.Contains("agua") || absDesc.Contains("internet") || absDesc.Contains("telefone") || absDesc.Contains("net") || absDesc.Contains("claro") || absDesc.Contains("vivo") || absDesc.Contains("tim"))
            return (FindCategoryId(categories, "Contas"), "Contas");
        if (absDesc.Contains("aluguel") || absDesc.Contains("condominio") || absDesc.Contains("iptu"))
            return (FindCategoryId(categories, "Moradia"), "Moradia");
        if (absDesc.Contains("cinema") || absDesc.Contains("netflix") || absDesc.Contains("spotify") || absDesc.Contains("ifood") || absDesc.Contains("restaurante") || absDesc.Contains("delivery") || absDesc.Contains("lazer"))
            return (FindCategoryId(categories, "Lazer"), "Lazer");
        if (absDesc.Contains("vestuario") || absDesc.Contains("roupa") || absDesc.Contains("renner") || absDesc.Contains("magazine"))
            return (FindCategoryId(categories, "Vestuário"), "Vestuário");
        if (absDesc.Contains("salario") || absDesc.Contains("pagamento") || absDesc.Contains("remuneração"))
            return (FindCategoryId(categories, "Salário"), "Salário");
        return (categories.FirstOrDefault(c => c.Name.Equals("Outros", StringComparison.OrdinalIgnoreCase))?.Id, "Outros");
    }

    private Guid? FindCategoryId(List<Category> categories, string name)
    {
        return categories.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Id;
    }
}
