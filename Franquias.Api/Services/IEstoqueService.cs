using Franquias.Api.DTOs;

namespace Franquias.Api.Services;

public interface IEstoqueService
{
    Task<ResultadoPaginado<EstoqueResponseDto>> ListarAsync(EstoqueFiltroDto? filtro);
    Task<EstoqueResponseDto> ObterPorIdAsync(int id);
    Task<EstoqueResponseDto> CriarAsync(EstoqueCreateDto dto);
    Task<EstoqueResponseDto> AtualizarAsync(int id, EstoqueUpdateDto dto);
    Task RemoverAsync(int id);
}
