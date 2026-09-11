using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Franquias.Api.Controllers;

// Relatórios são informação gerencial (números do negócio), então liberamos
// pra quem toca a operação (Administrador, Franqueado, Gerente) - igual à
// regra que já usamos em Estoque/Venda. Suporte não precisa desses números.
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{PerfilNomes.Administrador},{PerfilNomes.Franqueado},{PerfilNomes.Gerente}")]
public class RelatoriosController : ControllerBase
{
    private readonly IRelatorioService _service;

    public RelatoriosController(IRelatorioService service)
    {
        _service = service;
    }

    // Ex: /api/relatorios/faturamento-por-unidade?dataInicio=2026-09-01&dataFim=2026-09-30
    [HttpGet("faturamento-por-unidade")]
    public async Task<ActionResult<List<FaturamentoPorUnidadeDto>>> FaturamentoPorUnidade(
        [FromQuery] FiltroPeriodoDto filtro) =>
        Ok(await _service.FaturamentoPorUnidadeAsync(filtro.DataInicio, filtro.DataFim));

    [HttpGet("ranking-unidades")]
    public async Task<ActionResult<List<RankingUnidadeDto>>> RankingUnidades([FromQuery] FiltroPeriodoDto filtro) =>
        Ok(await _service.RankingUnidadesAsync(filtro.DataInicio, filtro.DataFim));

    [HttpGet("royalties-por-unidade")]
    public async Task<ActionResult<List<RoyaltiesPorUnidadeDto>>> RoyaltiesPorUnidade([FromQuery] FiltroPeriodoDto filtro) =>
        Ok(await _service.RoyaltiesPorUnidadeAsync(filtro.DataInicio, filtro.DataFim));

    [HttpGet("royalties-totais")]
    public async Task<ActionResult<TotalRoyaltiesDto>> RoyaltiesTotais([FromQuery] FiltroPeriodoDto filtro) =>
        Ok(await _service.TotalRoyaltiesAsync(filtro.DataInicio, filtro.DataFim));

    // "top" limita quantos produtos aparecem no ranking (padrão: os 10 mais vendidos).
    [HttpGet("produtos-mais-vendidos")]
    public async Task<ActionResult<List<ProdutoMaisVendidoDto>>> ProdutosMaisVendidos(
        [FromQuery] FiltroPeriodoDto filtro, [FromQuery] int top = 10) =>
        Ok(await _service.ProdutosMaisVendidosAsync(filtro.DataInicio, filtro.DataFim, top));

    [HttpGet("estoque-critico")]
    public async Task<ActionResult<List<EstoqueCriticoDto>>> EstoqueCritico() =>
        Ok(await _service.EstoqueCriticoAsync());

    [HttpGet("chamados-por-status")]
    public async Task<ActionResult<List<ChamadosPorStatusDto>>> ChamadosPorStatus() =>
        Ok(await _service.ChamadosPorStatusAsync());
}
