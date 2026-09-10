using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Repositories;
using Franquias.Api.Services;

namespace Franquias.Api.Tests;

public class ChamadoSuporteServiceTests
{
    private static IChamadoSuporteService CriarServico(Data.ApplicationDbContext contexto) => new ChamadoSuporteService(
        new Repository<ChamadoSuporte>(contexto),
        new Repository<UnidadeFranqueada>(contexto),
        new UsuarioRepository(contexto),
        contexto);

    [Fact]
    public async Task CriarAsync_DeveNascerComStatusAbertoESemDataFechamento()
    {
        // Arrange
        var contexto = ContextoDeTeste.Criar();
        var unidade = await SementeDeDados.CriarUnidadeFranqueadaAsync(contexto);
        var usuario = await SementeDeDados.CriarUsuarioAsync(contexto);
        var servico = CriarServico(contexto);

        // Act
        var chamado = await servico.CriarAsync(new ChamadoSuporteCreateDto
        {
            UnidadeFranqueadaId = unidade.Id,
            UsuarioAberturaId = usuario.Id,
            Titulo = "Impressora não funciona",
            Descricao = "A impressora de cupom parou de funcionar"
        });

        // Assert
        Assert.Equal(StatusChamadoSuporte.Aberto, chamado.Status);
        Assert.Null(chamado.DataFechamento);
    }

    [Fact]
    public async Task AtualizarStatusAsync_ParaResolvido_DevePreencherDataFechamento()
    {
        // Arrange
        var contexto = ContextoDeTeste.Criar();
        var unidade = await SementeDeDados.CriarUnidadeFranqueadaAsync(contexto);
        var usuario = await SementeDeDados.CriarUsuarioAsync(contexto);
        var servico = CriarServico(contexto);
        var chamado = await servico.CriarAsync(new ChamadoSuporteCreateDto
        {
            UnidadeFranqueadaId = unidade.Id,
            UsuarioAberturaId = usuario.Id,
            Titulo = "Titulo",
            Descricao = "Descricao"
        });

        // Act
        var chamadoAtualizado = await servico.AtualizarStatusAsync(chamado.Id, new ChamadoSuporteAtualizarStatusDto
        {
            Status = StatusChamadoSuporte.Resolvido
        });

        // Assert
        Assert.Equal(StatusChamadoSuporte.Resolvido, chamadoAtualizado.Status);
        Assert.NotNull(chamadoAtualizado.DataFechamento);
    }

    [Fact]
    public async Task AtualizarStatusAsync_DeAbertoParaResolvidoEDeVoltaParaAberto_DeveLimparDataFechamento()
    {
        // Arrange: um chamado que foi resolvido e depois reaberto (o cliente
        // reclamou que o problema voltou)
        var contexto = ContextoDeTeste.Criar();
        var unidade = await SementeDeDados.CriarUnidadeFranqueadaAsync(contexto);
        var usuario = await SementeDeDados.CriarUsuarioAsync(contexto);
        var servico = CriarServico(contexto);
        var chamado = await servico.CriarAsync(new ChamadoSuporteCreateDto
        {
            UnidadeFranqueadaId = unidade.Id,
            UsuarioAberturaId = usuario.Id,
            Titulo = "Titulo",
            Descricao = "Descricao"
        });
        await servico.AtualizarStatusAsync(chamado.Id, new ChamadoSuporteAtualizarStatusDto { Status = StatusChamadoSuporte.Resolvido });

        // Act: reabre o chamado
        var chamadoReaberto = await servico.AtualizarStatusAsync(chamado.Id, new ChamadoSuporteAtualizarStatusDto
        {
            Status = StatusChamadoSuporte.Aberto
        });

        // Assert: não faz sentido um chamado "Aberto" ter data de fechamento
        Assert.Null(chamadoReaberto.DataFechamento);
    }
}
