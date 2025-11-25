using System;

namespace BenyFinance.Application.DTOs;

public record CreditCardDto(
    Guid Id,
    string Name,
    decimal Limit,
    string Bank,
    int ClosingDay,
    int DueDay
);

public record CreateCreditCardDto(
    string Name,
    decimal Limit,
    string Bank,
    int ClosingDay,
    int DueDay
);
