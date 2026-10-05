using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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

    private static readonly HashSet<string> IncomeKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "transferencia recebida", "pix recebido", "recebido", "deposito", "reembolso", "restituicao", "credito", "entrada"
    };

    private static readonly HashSet<string> TransferKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "transferencia enviada", "pix enviado", "transferencia", "envio", "ted", "doc"
    };

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
        var existingDescriptions = (await _transactionRepository.GetAllByUserIdAsync(userId, null, null, null))
            .Select(t => t.Description)
            .GroupBy(d => d.ToLower())
            .ToDictionary(g => g.Key, g => g.Count());

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.GetEncoding("UTF-8"));
        var lines = new List<string>();
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (!string.IsNullOrWhiteSpace(line)) lines.Add(line);
        }

        if (lines.Count == 0) throw new Exception("Arquivo vazio");
        // Remove header
        lines.RemoveAt(0);

        var items = new List<ImportPreviewItemDto>();
        int idx = 0;
        foreach (var l in lines)
        {
            idx++;
            try
            {
                if (string.IsNullOrWhiteSpace(l)) continue;
                var parsed = source.ToLower() switch
                {
                    "nubank" => ParseNubank(l),
                    "mercadopago" => ParseMercadoPago(l),
                    _ => ParseGeneric(l)
                };

                if (parsed == null) continue;
                var (date, amount, desc) = parsed.Value;

                // Determine type: if amount > 0 income, if amount < 0 expense
                var type = amount >= 0 ? "income" : "expense";
                var absDesc = desc.ToLower();

                // Skip transfers if they look like moving money? But keep as expense/income? We'll classify based on amount sign.
                // Suggest category based on description similarity
                var suggested = SuggestCategory(categories, absDesc, existingDescriptions);
                items.Add(new ImportPreviewItemDto(
                    idx,
                    date,
                    desc,
                    Math.Abs(amount),
                    type,
                    suggested.Name,
                    suggested.Id,
                    "cash",
                    null,
                    "pending",
                    true,
                    null,
                    false
                ));
            }
            catch (Exception ex)
            {
                items.Add(new ImportPreviewItemDto(
                    idx, DateTime.Today, l, 0, "expense", categories.FirstOrDefault()?.Name ?? "Outros", categories.FirstOrDefault()?.Id, "cash", null, "pending", false, ex.Message, false
                ));
            }
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

            var tx = new Transaction
            {
                Date = item.Date,
                Description = item.Description,
                Amount = item.Amount,
                Type = type,
                CategoryId = catId,
                PaymentMethod = pm,
                CardId = item.CardId,
                Status = st,
                UserId = userId
            };
            await _transactionRepository.AddAsync(tx);
            count++;
        }
        return count;
    }

    private (DateTime date, decimal amount, string desc)? ParseNubank(string line)
    {
        // CSV: Data,Valor,Identificador,Descrição
        // Example: 01/04/2026,-14.00,uuid,Descrição
        var parts = SplitCsv(line);
        if (parts.Length < 4) return null;
        if (!DateTime.TryParseExact(parts[0].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            if (!DateTime.TryParse(parts[0].Trim(), CultureInfo.GetCultureInfo("pt-BR"), out date)) return null;
        }
        var valStr = parts[1].Trim().Replace(".", "").Replace(",", ".");
        if (!decimal.TryParse(valStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount)) return null;
        var desc = parts[3].Trim();
        return (date, amount, desc);
    }

    private (DateTime date, decimal amount, string desc)? ParseMercadoPago(string line)
    {
        // Try common MP formats; fallback generic
        return ParseGeneric(line);
    }

    private (DateTime date, decimal amount, string desc)? ParseGeneric(string line)
    {
        var parts = SplitCsv(line);
        if (parts.Length < 3) return null;
        if (DateTime.TryParseExact(parts[0].Trim(), new[] { "dd/MM/yyyy", "yyyy-MM-dd", "MM/dd/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            var valStr = parts[1].Trim().Replace("R$", "").Replace(".", "").Replace(",", ".");
            if (decimal.TryParse(valStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
            {
                var desc = string.Join(" ", parts.Skip(2)).Trim('"');
                return (date, amount, desc);
            }
        }
        return null;
    }

    private string[] SplitCsv(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;
        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"' && (i == 0 || line[i - 1] != '\\'))
            {
                inQuotes = !inQuotes;
                continue;
            }
            if (c == ',' && !inQuotes)
            {
                result.Add(sb.ToString());
                sb.Clear();
                continue;
            }
            sb.Append(c);
        }
        result.Add(sb.ToString());
        return result.ToArray();
    }

    private (Guid? Id, string Name) SuggestCategory(List<Category> categories, string absDesc, Dictionary<string, int> existing)
    {
        // 1. Exact match in existing transactions descriptions -> suggest category from that pattern? Or simple heuristic
        // Simple heuristic: look for keywords in categories? Or common names
        // Try to find category name contained in desc
        foreach (var cat in categories)
        {
            if (absDesc.Contains(cat.Name.ToLower())) return (cat.Id, cat.Name);
        }
        // Common mappings
        if (absDesc.Contains("super") || absDesc.Contains("mercado") || absDesc.Contains("padaria") || absDesc.Contains("ifd") || absDesc.Contains("loja") || absDesc.Contains("atacado") || absDesc.Contains("comercio"))
            return (FindCategoryId(categories, "Alimentação"), "Alimentação");
        if (absDesc.Contains("uber") || absDesc.Contains("99") || absDesc.Contains("posto") || absDesc.Contains("combustivel") || absDesc.Contains("gasolina") || absDesc.Contains("shell") || absDesc.Contains("ipiranga"))
            return (FindCategoryId(categories, "Transporte"), "Transporte");
        if (absDesc.Contains("farmacia") || absDesc.Contains("hospital") || absDesc.Contains("consulta") || absDesc.Contains("medico") || absDesc.Contains("drogaria") || absDesc.Contains("clínica"))
            return (FindCategoryId(categories, "Saúde"), "Saúde");
        if (absDesc.Contains("energia") || absDesc.Contains("agua") || absDesc.Contains("internet") || absDesc.Contains("telefone") || absDesc.Contains("net") || absDesc.Contains("claro") || absDesc.Contains("vivo") || absDesc.Contains("tim") || absDesc.Contains("enel"))
            return (FindCategoryId(categories, "Contas"), "Contas");
        if (absDesc.Contains("aluguel") || absDesc.Contains("condominio") || absDesc.Contains("iptu"))
            return (FindCategoryId(categories, "Moradia"), "Moradia");
        if (absDesc.Contains("cinema") || absDesc.Contains("netflix") || absDesc.Contains("spotify") || absDesc.Contains("ifood") || absDesc.Contains("restaurante") || absDesc.Contains("delivery") || absDesc.Contains("lazer"))
            return (FindCategoryId(categories, "Lazer"), "Lazer");
        if (absDesc.Contains("vestuario") || absDesc.Contains("roupa") || absDesc.Contains("shoes") || absDesc.Contains("magazine") || absDesc.Contains("renner"))
            return (FindCategoryId(categories, "Vestuário"), "Vestuário");
        if (absDesc.Contains("salario") || absDesc.Contains("pagamento") || absDesc.Contains("remuneração"))
            return (FindCategoryId(categories, "Salário"), "Salário");
        // Default
        return (categories.FirstOrDefault(c => c.Name.Equals("Outros", StringComparison.OrdinalIgnoreCase))?.Id, "Outros");
    }

    private Guid? FindCategoryId(List<Category> categories, string name)
    {
        return categories.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Id;
    }
}
