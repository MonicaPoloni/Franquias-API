using Franquias.Api.DTOs;

namespace Franquias.Api.Services;

public interface IFranqueadoraService
{
    Task<ResultadoPaginado<FranqueadoraResponseDto>> ListarAsync(ParametrosPaginacao? paginacao);
    Task<FranqueadoraResponseDto> ObterPorIdAsync(int id);
    Task<FranqueadoraResponseDto> CriarAsync(FranqueadoraCreateDto dto);
    Task<FranqueadoraResponseDto> AtualizarAsync(int id, FranqueadoraCreateDto dto);
    Task RemoverAsync(int id);
}
