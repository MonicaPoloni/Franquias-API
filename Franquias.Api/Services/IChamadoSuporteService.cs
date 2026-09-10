using Franquias.Api.DTOs;

namespace Franquias.Api.Services;

public interface IChamadoSuporteService
{
    Task<ResultadoPaginado<ChamadoSuporteResponseDto>> ListarAsync(ParametrosPaginacao? paginacao);
    Task<ChamadoSuporteResponseDto> ObterPorIdAsync(int id);
    Task<ChamadoSuporteResponseDto> CriarAsync(ChamadoSuporteCreateDto dto);
    Task<ChamadoSuporteResponseDto> AtualizarStatusAsync(int id, ChamadoSuporteAtualizarStatusDto dto);
    Task RemoverAsync(int id);
}
