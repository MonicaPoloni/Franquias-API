using Franquias.Api.Data;
using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Franquias.Api.Services;

// Esse Service é diferente dos outros: ele não faz CRUD, só LEITURA e cálculo
// (soma, contagem, agrupamento) em cima de dados que já existem. Por isso usa
// o ApplicationDbContext direto, sem passar por repositório - é só consulta.
public class RelatorioService : IRelatorioService
{
    private readonly ApplicationDbContext _contexto;

    public RelatorioService(ApplicationDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<List<FaturamentoPorUnidadeDto>> FaturamentoPorUnidadeAsync(DateTime? dataInicio, DateTime? dataFim)
    {
        // Começamos pela lista de Unidades (não pelas Vendas) de propósito:
        // assim uma unidade que não vendeu nada no período aparece com total
        // zero, em vez de simplesmente sumir do relatório.
        var resultado = await _contexto.UnidadesFranqueadas
            .AsNoTracking()
            .Select(u => new FaturamentoPorUnidadeDto
            {
                UnidadeFranqueadaId = u.Id,
                UnidadeFranqueadaNome = u.Nome,
                TotalFaturado = _contexto.Vendas
                    .Where(v => v.UnidadeFranqueadaId == u.Id
                        && (!dataInicio.HasValue || v.DataVenda >= dataInicio.Value)
                        && (!dataFim.HasValue || v.DataVenda <= dataFim.Value))
                    .Sum(v => (decimal?)v.ValorTotal) ?? 0,
                QuantidadeVendas = _contexto.Vendas.Count(v => v.UnidadeFranqueadaId == u.Id
                    && (!dataInicio.HasValue || v.DataVenda >= dataInicio.Value)
                    && (!dataFim.HasValue || v.DataVenda <= dataFim.Value))
            })
            .OrderByDescending(f => f.TotalFaturado)
            .ToListAsync();

        return resultado;
    }

    public async Task<List<RankingUnidadeDto>> RankingUnidadesAsync(DateTime? dataInicio, DateTime? dataFim)
    {
        // O ranking é o mesmo cálculo do faturamento por unidade, só que
        // numerado (1º, 2º, 3º...) - por isso reaproveitamos o método acima
        // em vez de duplicar a consulta.
        var faturamento = await FaturamentoPorUnidadeAsync(dataInicio, dataFim);

        return faturamento
            .Select((f, indice) => new RankingUnidadeDto
            {
                Posicao = indice + 1,
                UnidadeFranqueadaId = f.UnidadeFranqueadaId,
                UnidadeFranqueadaNome = f.UnidadeFranqueadaNome,
                TotalFaturado = f.TotalFaturado
            })
            .ToList();
    }

    public async Task<TotalRoyaltiesDto> TotalRoyaltiesAsync(DateTime? dataInicio, DateTime? dataFim)
    {
        var consulta = _contexto.Cobrancas.AsNoTracking().AsQueryable();
        if (dataInicio.HasValue)
            consulta = consulta.Where(c => c.DataVencimento >= dataInicio.Value);
        if (dataFim.HasValue)
            consulta = consulta.Where(c => c.DataVencimento <= dataFim.Value);

        var totalCobrado = await consulta.SumAsync(c => c.ValorCobranca);
        var totalPago = await consulta
            .Where(c => c.Status == StatusCobranca.Paga)
            .SumAsync(c => c.ValorCobranca);
        var quantidade = await consulta.CountAsync();

        return new TotalRoyaltiesDto
        {
            TotalCobrado = totalCobrado,
            TotalPago = totalPago,
            TotalPendente = totalCobrado - totalPago,
            QuantidadeCobrancas = quantidade
        };
    }

    public async Task<List<ProdutoMaisVendidoDto>> ProdutosMaisVendidosAsync(DateTime? dataInicio, DateTime? dataFim, int top)
    {
        var itens = _contexto.ItensVenda.AsNoTracking().AsQueryable();
        if (dataInicio.HasValue)
            itens = itens.Where(i => i.Venda.DataVenda >= dataInicio.Value);
        if (dataFim.HasValue)
            itens = itens.Where(i => i.Venda.DataVenda <= dataFim.Value);

        return await itens
            .GroupBy(i => new { i.ProdutoServicoId, i.ProdutoServico.Nome })
            .Select(g => new ProdutoMaisVendidoDto
            {
                ProdutoServicoId = g.Key.ProdutoServicoId,
                ProdutoNome = g.Key.Nome,
                QuantidadeVendida = g.Sum(i => i.Quantidade),
                TotalFaturado = g.Sum(i => i.Quantidade * i.PrecoUnitario)
            })
            .OrderByDescending(p => p.QuantidadeVendida)
            .Take(top)
            .ToListAsync();
    }

    public async Task<List<EstoqueCriticoDto>> EstoqueCriticoAsync()
    {
        // "Crítico" = a quantidade atual já chegou no mínimo (ou passou dele) -
        // é o sinal de que a unidade precisa repor esse produto.
        return await _contexto.Estoques
            .Include(e => e.UnidadeFranqueada)
            .Include(e => e.ProdutoServico)
            .AsNoTracking()
            .Where(e => e.QuantidadeAtual <= e.QuantidadeMinima)
            .OrderBy(e => e.QuantidadeAtual)
            .Select(e => new EstoqueCriticoDto
            {
                EstoqueId = e.Id,
                UnidadeFranqueadaNome = e.UnidadeFranqueada.Nome,
                ProdutoNome = e.ProdutoServico.Nome,
                QuantidadeAtual = e.QuantidadeAtual,
                QuantidadeMinima = e.QuantidadeMinima
            })
            .ToListAsync();
    }

    public async Task<List<ChamadosPorStatusDto>> ChamadosPorStatusAsync()
    {
        // Agrupamos primeiro (isso o banco de dados faz bem), e só transformamos
        // o enum em texto (ToString) depois de já ter a lista em mãos - o banco
        // não sabe converter um enum do C# pra texto sozinho.
        var contagens = await _contexto.ChamadosSuporte
            .AsNoTracking()
            .GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key, Quantidade = g.Count() })
            .ToListAsync();

        return contagens
            .Select(c => new ChamadosPorStatusDto { Status = c.Status.ToString(), Quantidade = c.Quantidade })
            .ToList();
    }
}
