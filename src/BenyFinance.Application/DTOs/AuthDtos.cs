using System;

namespace BenyFinance.Application.DTOs;

public record LoginDto(string Email, string Password);

public record RegisterDto(string Name, string Email, string Password);

public record UserDto(Guid Id, string Name, string Email);

public record AuthResponseDto(string Token, UserDto User);

public record ForgotPasswordDto(string Email);

public record ResetPasswordDto(string Email, string Token, string NewPassword);
