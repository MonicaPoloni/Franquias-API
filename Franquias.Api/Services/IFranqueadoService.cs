using Franquias.Api.DTOs;

namespace Franquias.Api.Services;

public interface IFranqueadoService
{
    Task<ResultadoPaginado<FranqueadoResponseDto>> ListarAsync(ParametrosPaginacao? paginacao);
    Task<FranqueadoResponseDto> ObterPorIdAsync(int id);
    Task<FranqueadoResponseDto> CriarAsync(FranqueadoCreateDto dto);
    Task<FranqueadoResponseDto> AtualizarAsync(int id, FranqueadoCreateDto dto);
    Task RemoverAsync(int id);
}
