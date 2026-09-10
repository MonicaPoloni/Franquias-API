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
    public async Task CriarAsync_DeveCalcularValorCobrancaAPartirDoPercentual()
    {
        // Arrange: faturamento de 10.000 com 5% de royalty
        var contexto = ContextoDeTeste.Criar();
        var unidade = await SementeDeDados.CriarUnidadeFranqueadaAsync(contexto);
        var servico = CriarServico(contexto);

        // Act
        var cobranca = await servico.CriarAsync(new CobrancaCreateDto
        {
            UnidadeFranqueadaId = unidade.Id,
            Competencia = new DateOnly(2026, 9, 1),
            FaturamentoBase = 10000,
            PercentualRoyalty = 5,
            DataVencimento = new DateTime(2026, 10, 5)
        });

        // Assert: 10.000 x 5% = 500
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
            FaturamentoBase = 10000,
            PercentualRoyalty = 5,
            DataVencimento = new DateTime(2026, 10, 5)
        });
        await servico.MarcarComoPagaAsync(cobranca.Id);

        // Act + Assert: tentar pagar de novo não pode ser permitido
        await Assert.ThrowsAsync<InvalidOperationException>(() => servico.MarcarComoPagaAsync(cobranca.Id));
    }
}
