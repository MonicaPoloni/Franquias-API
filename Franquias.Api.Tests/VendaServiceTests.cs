using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Franquias.Api.Tests;

public class VendaServiceTests
{
    [Fact]
    public async Task CriarAsync_ComEstoqueSuficiente_DeveDebitarEstoqueECalcularTotal()
    {
        // Arrange: produto custa 10, e tem 100 unidades em estoque
        var contexto = ContextoDeTeste.Criar();
        var unidade = await SementeDeDados.CriarUnidadeFranqueadaAsync(contexto);
        var produto = await SementeDeDados.CriarProdutoAsync(contexto, preco: 10);
        var usuario = await SementeDeDados.CriarUsuarioAsync(contexto);
        contexto.Estoques.Add(new Estoque
        {
            UnidadeFranqueada = unidade,
            ProdutoServico = produto,
            QuantidadeAtual = 100,
            QuantidadeMinima = 5
        });
        await contexto.SaveChangesAsync();

        var servico = new VendaService(contexto);

        // Act: vende 5 unidades
        var venda = await servico.CriarAsync(new VendaCreateDto
        {
            UnidadeFranqueadaId = unidade.Id,
            UsuarioId = usuario.Id,
            FormaPagamento = FormaPagamento.Pix,
            Itens = [new ItemVendaCreateDto { ProdutoServicoId = produto.Id, Quantidade = 5 }]
        });

        // Assert: total = 5 x 10 = 50
        Assert.Equal(50, venda.ValorTotal);

        // E o estoque, que era 100, agora tem que estar em 95
        var estoqueAtualizado = await contexto.Estoques.FirstAsync(e => e.ProdutoServicoId == produto.Id);
        Assert.Equal(95, estoqueAtualizado.QuantidadeAtual);
    }

    [Fact]
    public async Task CriarAsync_ComEstoqueInsuficiente_NaoDeveSalvarVendaNemMexerNoEstoque()
    {
        // Arrange: só tem 3 unidades em estoque
        var contexto = ContextoDeTeste.Criar();
        var unidade = await SementeDeDados.CriarUnidadeFranqueadaAsync(contexto);
        var produto = await SementeDeDados.CriarProdutoAsync(contexto, preco: 10);
        var usuario = await SementeDeDados.CriarUsuarioAsync(contexto);
        contexto.Estoques.Add(new Estoque
        {
            UnidadeFranqueada = unidade,
            ProdutoServico = produto,
            QuantidadeAtual = 3,
            QuantidadeMinima = 1
        });
        await contexto.SaveChangesAsync();

        var servico = new VendaService(contexto);

        // Act: tenta vender 10, mas só tem 3
        await Assert.ThrowsAsync<ArgumentException>(() => servico.CriarAsync(new VendaCreateDto
        {
            UnidadeFranqueadaId = unidade.Id,
            UsuarioId = usuario.Id,
            FormaPagamento = FormaPagamento.Pix,
            Itens = [new ItemVendaCreateDto { ProdutoServicoId = produto.Id, Quantidade = 10 }]
        }));

        // Assert: como a venda falhou, NADA deve ter sido salvo - nem a venda,
        // nem uma baixa parcial no estoque. Isso é o que garante a atomicidade
        // que comentamos no VendaService (um único SaveChangesAsync no final).
        Assert.Empty(contexto.Vendas);
        var estoque = await contexto.Estoques.FirstAsync(e => e.ProdutoServicoId == produto.Id);
        Assert.Equal(3, estoque.QuantidadeAtual);
    }

    [Fact]
    public async Task CriarAsync_ComItemDoTipoServico_NaoDeveMexerNoEstoque()
    {
        // Arrange: um Serviço (ex: "Instalação") não tem controle de estoque físico
        var contexto = ContextoDeTeste.Criar();
        var unidade = await SementeDeDados.CriarUnidadeFranqueadaAsync(contexto);
        var servicoVendido = await SementeDeDados.CriarProdutoAsync(contexto, preco: 50, tipo: TipoProdutoServico.Servico);
        var usuario = await SementeDeDados.CriarUsuarioAsync(contexto);

        var servico = new VendaService(contexto);

        // Act: não existe nenhum registro de Estoque pra esse serviço, mas a
        // venda deve funcionar mesmo assim, porque serviço não baixa estoque.
        var venda = await servico.CriarAsync(new VendaCreateDto
        {
            UnidadeFranqueadaId = unidade.Id,
            UsuarioId = usuario.Id,
            FormaPagamento = FormaPagamento.CartaoCredito,
            Itens = [new ItemVendaCreateDto { ProdutoServicoId = servicoVendido.Id, Quantidade = 1 }]
        });

        // Assert
        Assert.Equal(50, venda.ValorTotal);
    }
}
