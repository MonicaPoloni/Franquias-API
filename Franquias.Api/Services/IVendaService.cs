using Franquias.Api.DTOs;

namespace Franquias.Api.Services;

public interface IVendaService
{
    Task<ResultadoPaginado<VendaResponseDto>> ListarAsync(VendaFiltroDto? filtro);
    Task<VendaResponseDto> ObterPorIdAsync(int id);
    Task<VendaResponseDto> CriarAsync(VendaCreateDto dto);
}
