using Franquias.Api.DTOs;

namespace Franquias.Api.Services;

public interface IPerfilService
{
    Task<ResultadoPaginado<PerfilResponseDto>> ListarAsync(ParametrosPaginacao? paginacao);
    Task<PerfilResponseDto> ObterPorIdAsync(int id);
    Task<PerfilResponseDto> CriarAsync(PerfilCreateDto dto);
    Task<PerfilResponseDto> AtualizarAsync(int id, PerfilCreateDto dto);
    Task RemoverAsync(int id);
}
