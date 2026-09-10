using Franquias.Api.DTOs;

namespace Franquias.Api.Services;

public interface IRelatorioService
{
    Task<List<FaturamentoPorUnidadeDto>> FaturamentoPorUnidadeAsync(DateTime? dataInicio, DateTime? dataFim);
    Task<List<RankingUnidadeDto>> RankingUnidadesAsync(DateTime? dataInicio, DateTime? dataFim);
    Task<TotalRoyaltiesDto> TotalRoyaltiesAsync(DateTime? dataInicio, DateTime? dataFim);
    Task<List<ProdutoMaisVendidoDto>> ProdutosMaisVendidosAsync(DateTime? dataInicio, DateTime? dataFim, int top);
    Task<List<EstoqueCriticoDto>> EstoqueCriticoAsync();
    Task<List<ChamadosPorStatusDto>> ChamadosPorStatusAsync();
}
