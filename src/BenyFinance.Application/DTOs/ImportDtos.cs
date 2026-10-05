using System;
using System.Collections.Generic;

namespace BenyFinance.Application.DTOs;

public record ImportPreviewItemDto(
    int RowIndex,
    DateTime Date,
    string Description,
    decimal Amount,
    string Type,
    string SuggestedCategoryName,
    Guid? SuggestedCategoryId,
    string PaymentMethod,
    Guid? CardId,
    string Status,
    bool IsValid,
    string? ErrorMessage,
    bool CreateCategoryIfMissing
);

public record ImportPreviewDto(
    int TotalRows,
    int ValidRows,
    List<ImportPreviewItemDto> Items
);

public record ImportConfirmItemDto(
    DateTime Date,
    string Description,
    decimal Amount,
    string Type,
    Guid CategoryId,
    string PaymentMethod,
    Guid? CardId,
    string Status,
    bool CreateCategory,
    string? NewCategoryName,
    string? NewCategoryColor
);

public record ImportConfirmDto(
    string Source,
    List<ImportConfirmItemDto> Items
);
