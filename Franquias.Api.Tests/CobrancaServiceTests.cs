using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Repositories;
using Franquias.Api.Services;

namespace Franquias.Api.Tests;

public class CobrancaServiceTests
{
    private static ICobrancaService CriarServico(Data.ApplicationDbContext contexto) => new CobrancaService(
        new Repository<Cobranca>(contexto),
        new Repository<UnidadeFranqueada>(contexto),
        contexto);

    [Fact]
    public async Task CriarAsync_DeveSomarAsVendasDoMesEDepoisAplicarOPercentual()
    {
        // Arrange: duas vendas em setembro/2026 somando 10.000, e uma venda em
        // agosto (fora do período) que NÃO deve entrar na conta.
        var contexto = ContextoDeTeste.Criar();
        var unidade = await SementeDeDados.CriarUnidadeFranqueadaAsync(contexto);
        var usuario = await SementeDeDados.CriarUsuarioAsync(contexto);

        contexto.Vendas.AddRange(
            new Venda { UnidadeFranqueadaId = unidade.Id, UsuarioId = usuario.Id, DataVenda = new DateTime(2026, 9, 5), ValorTotal = 6000, FormaPagamento = FormaPagamento.Pix },
            new Venda { UnidadeFranqueadaId = unidade.Id, UsuarioId = usuario.Id, DataVenda = new DateTime(2026, 9, 20), ValorTotal = 4000, FormaPagamento = FormaPagamento.Pix },
            new Venda { UnidadeFranqueadaId = unidade.Id, UsuarioId = usuario.Id, DataVenda = new DateTime(2026, 8, 31), ValorTotal = 999, FormaPagamento = FormaPagamento.Pix }
        );
        await contexto.SaveChangesAsync();

        var servico = CriarServico(contexto);

        // Act
        var cobranca = await servico.CriarAsync(new CobrancaCreateDto
        {
            UnidadeFranqueadaId = unidade.Id,
            Competencia = new DateOnly(2026, 9, 1),
            PercentualRoyalty = 5,
            DataVencimento = new DateTime(2026, 10, 5)
        });

        // Assert: faturamento = 6000 + 4000 = 10.000 (a venda de agosto fica de fora);
        // valor da cobrança = 10.000 x 5% = 500
        Assert.Equal(10000, cobranca.FaturamentoBase);
        Assert.Equal(500, cobranca.ValorCobranca);
        Assert.Equal(StatusCobranca.Pendente, cobranca.Status);
    }

    [Fact]
    public async Task MarcarComoPagaAsync_QuandoJaEstaPaga_DeveLancarInvalidOperationException()
    {
        // Arrange: cria a cobrança e já paga ela uma vez
        var contexto = ContextoDeTeste.Criar();
        var unidade = await SementeDeDados.CriarUnidadeFranqueadaAsync(contexto);
        var servico = CriarServico(contexto);
        var cobranca = await servico.CriarAsync(new CobrancaCreateDto
        {
            UnidadeFranqueadaId = unidade.Id,
            Competencia = new DateOnly(2026, 9, 1),
            PercentualRoyalty = 5,
            DataVencimento = new DateTime(2026, 10, 5)
        });
        await servico.MarcarComoPagaAsync(cobranca.Id);

        // Act + Assert: tentar pagar de novo não pode ser permitido
        await Assert.ThrowsAsync<InvalidOperationException>(() => servico.MarcarComoPagaAsync(cobranca.Id));
    }
}
