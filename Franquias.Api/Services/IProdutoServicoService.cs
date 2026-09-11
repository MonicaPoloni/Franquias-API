using Franquias.Api.DTOs;

namespace Franquias.Api.Services;

public interface IProdutoServicoService
{
    Task<ResultadoPaginado<ProdutoServicoResponseDto>> ListarAsync(ProdutoServicoFiltroDto? filtro);
    Task<ProdutoServicoResponseDto> ObterPorIdAsync(int id);
    Task<ProdutoServicoResponseDto> CriarAsync(ProdutoServicoCreateDto dto);
    Task<ProdutoServicoResponseDto> AtualizarAsync(int id, ProdutoServicoCreateDto dto);
    Task RemoverAsync(int id);
    Task<ProdutoServicoResponseDto> AtualizarStatusAsync(int id, bool ativo);
}
