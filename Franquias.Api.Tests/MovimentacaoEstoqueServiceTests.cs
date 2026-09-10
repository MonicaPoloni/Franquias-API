using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Repositories;
using Franquias.Api.Services;

namespace Franquias.Api.Tests;

public class MovimentacaoEstoqueServiceTests
{
    private static IMovimentacaoEstoqueService CriarServico(Data.ApplicationDbContext contexto) => new MovimentacaoEstoqueService(
        new Repository<MovimentacaoEstoque>(contexto),
        new Repository<Estoque>(contexto),
        new Repository<Fornecedor>(contexto),
        contexto);

    private static async Task<Estoque> CriarEstoqueAsync(Data.ApplicationDbContext contexto, int quantidadeInicial)
    {
        var unidade = await SementeDeDados.CriarUnidadeFranqueadaAsync(contexto);
        var produto = await SementeDeDados.CriarProdutoAsync(contexto);
        var estoque = new Estoque
        {
            UnidadeFranqueada = unidade,
            ProdutoServico = produto,
            QuantidadeAtual = quantidadeInicial,
            QuantidadeMinima = 5
        };
        contexto.Estoques.Add(estoque);
        await contexto.SaveChangesAsync();
        return estoque;
    }

    [Fact]
    public async Task CriarAsync_Entrada_DeveAumentarQuantidadeAtual()
    {
        // Arrange: estoque começa com 100 unidades
        var contexto = ContextoDeTeste.Criar();
        var estoque = await CriarEstoqueAsync(contexto, quantidadeInicial: 100);
        var servico = CriarServico(contexto);

        // Act: registra entrada de 50
        var resultado = await servico.CriarAsync(new MovimentacaoEstoqueCreateDto
        {
            EstoqueId = estoque.Id,
            Tipo = TipoMovimentacaoEstoque.Entrada,
            Quantidade = 50
        });

        // Assert: 100 + 50 = 150
        Assert.Equal(150, resultado.QuantidadeAtualAposMovimentacao);
    }

    [Fact]
    public async Task CriarAsync_Saida_DeveDiminuirQuantidadeAtual()
    {
        // Arrange: estoque começa com 100 unidades
        var contexto = ContextoDeTeste.Criar();
        var estoque = await CriarEstoqueAsync(contexto, quantidadeInicial: 100);
        var servico = CriarServico(contexto);

        // Act: registra saída de 30
        var resultado = await servico.CriarAsync(new MovimentacaoEstoqueCreateDto
        {
            EstoqueId = estoque.Id,
            Tipo = TipoMovimentacaoEstoque.Saida,
            Quantidade = 30
        });

        // Assert: 100 - 30 = 70
        Assert.Equal(70, resultado.QuantidadeAtualAposMovimentacao);
    }

    [Fact]
    public async Task CriarAsync_SaidaMaiorQueEstoqueDisponivel_DeveLancarArgumentException()
    {
        // Arrange: estoque só tem 10 unidades
        var contexto = ContextoDeTeste.Criar();
        var estoque = await CriarEstoqueAsync(contexto, quantidadeInicial: 10);
        var servico = CriarServico(contexto);

        // Act + Assert: tenta tirar 999 - não pode deixar o estoque negativo
        await Assert.ThrowsAsync<ArgumentException>(() => servico.CriarAsync(new MovimentacaoEstoqueCreateDto
        {
            EstoqueId = estoque.Id,
            Tipo = TipoMovimentacaoEstoque.Saida,
            Quantidade = 999
        }));
    }

    [Fact]
    public async Task CriarAsync_Ajuste_DeveDefinirQuantidadeExata()
    {
        // Arrange: estoque começa com 100, mas uma contagem física achou só 87
        var contexto = ContextoDeTeste.Criar();
        var estoque = await CriarEstoqueAsync(contexto, quantidadeInicial: 100);
        var servico = CriarServico(contexto);

        // Act
        var resultado = await servico.CriarAsync(new MovimentacaoEstoqueCreateDto
        {
            EstoqueId = estoque.Id,
            Tipo = TipoMovimentacaoEstoque.Ajuste,
            Quantidade = 87
        });

        // Assert: Ajuste define o valor exato, não soma nem subtrai
        Assert.Equal(87, resultado.QuantidadeAtualAposMovimentacao);
    }
}
