using Franquias.Api.DTOs;

namespace Franquias.Api.Services;

public interface IUnidadeFranqueadaService
{
    Task<ResultadoPaginado<UnidadeFranqueadaResponseDto>> ListarAsync(UnidadeFranqueadaFiltroDto? filtro);
    Task<UnidadeFranqueadaResponseDto> ObterPorIdAsync(int id);
    Task<UnidadeFranqueadaResponseDto> CriarAsync(UnidadeFranqueadaCreateDto dto);
    Task<UnidadeFranqueadaResponseDto> AtualizarAsync(int id, UnidadeFranqueadaCreateDto dto);
    Task RemoverAsync(int id);
    Task<UnidadeFranqueadaResponseDto> AtualizarStatusAsync(int id, bool ativo);
}
