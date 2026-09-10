using Franquias.Api.DTOs;

namespace Franquias.Api.Services;

public interface IMovimentacaoEstoqueService
{
    Task<ResultadoPaginado<MovimentacaoEstoqueResponseDto>> ListarAsync(MovimentacaoEstoqueFiltroDto? filtro);
    Task<MovimentacaoEstoqueResponseDto> ObterPorIdAsync(int id);
    Task<MovimentacaoEstoqueResponseDto> CriarAsync(MovimentacaoEstoqueCreateDto dto);
}
