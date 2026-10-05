using System;
using System.Threading.Tasks;
using BenyFinance.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace BenyFinance.Application.Interfaces;

public interface IImportService
{
    Task<ImportPreviewDto> PreviewAsync(Guid userId, IFormFile file, string source);
    Task<int> ConfirmAsync(Guid userId, ImportConfirmDto confirmDto);
}
