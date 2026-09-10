using Franquias.Api.DTOs;

namespace Franquias.Api.Services;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}
