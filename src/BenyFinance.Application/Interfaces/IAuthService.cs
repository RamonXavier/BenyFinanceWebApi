using System.Threading.Tasks;
using BenyFinance.Application.DTOs;

namespace BenyFinance.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
}
