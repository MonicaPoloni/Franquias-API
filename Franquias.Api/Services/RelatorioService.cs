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
        var unidades = await _contexto.UnidadesFranqueadas
            .AsNoTracking()
            .Select(u => new { u.Id, u.Nome })
            .ToListAsync();

        var consultaVendas = _contexto.Vendas.AsNoTracking().AsQueryable();
        if (dataInicio.HasValue)
            consultaVendas = consultaVendas.Where(v => v.DataVenda >= dataInicio.Value);
        if (dataFim.HasValue)
            consultaVendas = consultaVendas.Where(v => v.DataVenda <= dataFim.Value);

        // O SQLite não sabe somar "decimal" direto no SQL (é uma limitação do
        // provider - ver o erro "cannot apply aggregate operator Sum on
        // expressions of type decimal"). Por isso trazemos só o Id da unidade
        // e o valor de cada venda pra memória, e somamos aqui em C#.
        var vendas = await consultaVendas
            .Select(v => new { v.UnidadeFranqueadaId, v.ValorTotal })
            .ToListAsync();

        var vendasPorUnidade = vendas
            .GroupBy(v => v.UnidadeFranqueadaId)
            .ToDictionary(g => g.Key, g => (Total: g.Sum(v => v.ValorTotal), Quantidade: g.Count()));

        return unidades
            .Select(u =>
            {
                vendasPorUnidade.TryGetValue(u.Id, out var dados);
                return new FaturamentoPorUnidadeDto
                {
                    UnidadeFranqueadaId = u.Id,
                    UnidadeFranqueadaNome = u.Nome,
                    TotalFaturado = dados.Total,
                    QuantidadeVendas = dados.Quantidade
                };
            })
            .OrderByDescending(f => f.TotalFaturado)
            .ToList();
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

    public async Task<List<RoyaltiesPorUnidadeDto>> RoyaltiesPorUnidadeAsync(DateTime? dataInicio, DateTime? dataFim)
    {
        var consultaCobrancas = _contexto.Cobrancas.AsNoTracking().AsQueryable();
        if (dataInicio.HasValue)
            consultaCobrancas = consultaCobrancas.Where(c => c.DataVencimento >= dataInicio.Value);
        if (dataFim.HasValue)
            consultaCobrancas = consultaCobrancas.Where(c => c.DataVencimento <= dataFim.Value);

        // De novo trazemos pra memória antes de somar (decimal + SQLite).
        var cobrancas = await consultaCobrancas
            .Select(c => new { c.UnidadeFranqueadaId, c.ValorCobranca, c.Status })
            .ToListAsync();

        var unidades = await _contexto.UnidadesFranqueadas
            .AsNoTracking()
            .Select(u => new { u.Id, u.Nome })
            .ToListAsync();

        var cobrancasPorUnidade = cobrancas
            .GroupBy(c => c.UnidadeFranqueadaId)
            .ToDictionary(g => g.Key, g => new
            {
                Total = g.Sum(c => c.ValorCobranca),
                Pago = g.Where(c => c.Status == StatusCobranca.Paga).Sum(c => c.ValorCobranca),
                Quantidade = g.Count()
            });

        return unidades
            .Select(u =>
            {
                cobrancasPorUnidade.TryGetValue(u.Id, out var dados);
                var total = dados?.Total ?? 0;
                var pago = dados?.Pago ?? 0;
                return new RoyaltiesPorUnidadeDto
                {
                    UnidadeFranqueadaId = u.Id,
                    UnidadeFranqueadaNome = u.Nome,
                    TotalCobrado = total,
                    TotalPago = pago,
                    TotalPendente = total - pago,
                    QuantidadeCobrancas = dados?.Quantidade ?? 0
                };
            })
            .OrderByDescending(r => r.TotalCobrado)
            .ToList();
    }

    public async Task<TotalRoyaltiesDto> TotalRoyaltiesAsync(DateTime? dataInicio, DateTime? dataFim)
    {
        var consulta = _contexto.Cobrancas.AsNoTracking().AsQueryable();
        if (dataInicio.HasValue)
            consulta = consulta.Where(c => c.DataVencimento >= dataInicio.Value);
        if (dataFim.HasValue)
            consulta = consulta.Where(c => c.DataVencimento <= dataFim.Value);

        // Mesmo motivo do relatório de faturamento: SQLite não soma "decimal"
        // no SQL, então trazemos os valores pra memória e somamos em C#.
        var cobrancas = await consulta
            .Select(c => new { c.ValorCobranca, c.Status })
            .ToListAsync();

        var totalCobrado = cobrancas.Sum(c => c.ValorCobranca);
        var totalPago = cobrancas.Where(c => c.Status == StatusCobranca.Paga).Sum(c => c.ValorCobranca);

        return new TotalRoyaltiesDto
        {
            TotalCobrado = totalCobrado,
            TotalPago = totalPago,
            TotalPendente = totalCobrado - totalPago,
            QuantidadeCobrancas = cobrancas.Count
        };
    }

    public async Task<List<ProdutoMaisVendidoDto>> ProdutosMaisVendidosAsync(DateTime? dataInicio, DateTime? dataFim, int top)
    {
        var consultaItens = _contexto.ItensVenda.AsNoTracking().AsQueryable();
        if (dataInicio.HasValue)
            consultaItens = consultaItens.Where(i => i.Venda.DataVenda >= dataInicio.Value);
        if (dataFim.HasValue)
            consultaItens = consultaItens.Where(i => i.Venda.DataVenda <= dataFim.Value);

        // De novo, trazemos os itens pra memória antes de agrupar/somar,
        // porque o TotalFaturado é "decimal" e o SQLite não soma esse tipo no SQL.
        var itens = await consultaItens
            .Select(i => new { i.ProdutoServicoId, i.ProdutoServico.Nome, i.Quantidade, i.PrecoUnitario })
            .ToListAsync();

        return itens
            .GroupBy(i => new { i.ProdutoServicoId, i.Nome })
            .Select(g => new ProdutoMaisVendidoDto
            {
                ProdutoServicoId = g.Key.ProdutoServicoId,
                ProdutoNome = g.Key.Nome,
                QuantidadeVendida = g.Sum(i => i.Quantidade),
                TotalFaturado = g.Sum(i => i.Quantidade * i.PrecoUnitario)
            })
            .OrderByDescending(p => p.QuantidadeVendida)
            .Take(top)
            .ToList();
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
