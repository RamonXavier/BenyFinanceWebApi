using System;

namespace BenyFinance.Application.DTOs;

public record RecurringTemplateDto(
    Guid Id,
    string Description,
    decimal Amount,
    string Category,
    Guid CategoryId
);

public record CreateRecurringTemplateDto(
    string Description,
    decimal Amount,
    string Category, // Name
    Guid? CategoryId
);
