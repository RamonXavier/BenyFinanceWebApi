using System;

namespace BenyFinance.Application.DTOs;

public record CategoryDto(Guid Id, string Name, string Color);

public record CreateCategoryDto(string Name, string Color);
