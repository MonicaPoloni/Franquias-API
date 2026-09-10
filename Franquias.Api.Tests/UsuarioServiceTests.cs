using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Repositories;
using Franquias.Api.Services;
using Microsoft.AspNetCore.Identity;

namespace Franquias.Api.Tests;

public class UsuarioServiceTests
{
    // Cria o UsuarioService já com um Perfil "Franqueado" cadastrado no banco
    // de teste (precisamos disso porque o cadastro público sempre usa esse perfil).
    private static async Task<(IUsuarioService servico, Perfil perfilFranqueado)> CriarServicoComPerfisAsync(
        Data.ApplicationDbContext contexto)
    {
        var perfilFranqueado = new Perfil { Nome = "Franqueado" };
        contexto.Perfis.Add(perfilFranqueado);
        await contexto.SaveChangesAsync();

        var usuarioRepositorio = new UsuarioRepository(contexto);
        var perfilRepositorio = new Repository<Perfil>(contexto);
        var hasher = new PasswordHasher<Usuario>();
        var servico = new UsuarioService(usuarioRepositorio, perfilRepositorio, hasher);

        return (servico, perfilFranqueado);
    }

    [Fact]
    public async Task RegistrarAsync_ComEmailNovo_DeveCriarUsuario()
    {
        // Arrange
        var contexto = ContextoDeTeste.Criar();
        var (servico, perfil) = await CriarServicoComPerfisAsync(contexto);

        // Act
        var usuario = await servico.RegistrarAsync(new RegistrarUsuarioDto
        {
            Nome = "Maria",
            Email = "maria@teste.com",
            Senha = "senha123",
            PerfilId = perfil.Id
        });

        // Assert
        Assert.Equal("maria@teste.com", usuario.Email);
        Assert.Equal("Franqueado", usuario.Perfil);
    }

    [Fact]
    public async Task RegistrarAsync_ComEmailJaCadastrado_DeveLancarArgumentException()
    {
        // Arrange: cadastra o primeiro usuário com um e-mail
        var contexto = ContextoDeTeste.Criar();
        var (servico, perfil) = await CriarServicoComPerfisAsync(contexto);
        await servico.RegistrarAsync(new RegistrarUsuarioDto
        {
            Nome = "Maria",
            Email = "maria@teste.com",
            Senha = "senha123",
            PerfilId = perfil.Id
        });

        // Act + Assert: tenta cadastrar de novo com o MESMO e-mail
        await Assert.ThrowsAsync<ArgumentException>(() => servico.RegistrarAsync(new RegistrarUsuarioDto
        {
            Nome = "Outra Maria",
            Email = "maria@teste.com",
            Senha = "outrasenha",
            PerfilId = perfil.Id
        }));
    }

    [Fact]
    public async Task CadastrarContaPublicaAsync_DeveSempreUsarPerfilFranqueado()
    {
        // Arrange
        var contexto = ContextoDeTeste.Criar();
        var (servico, _) = await CriarServicoComPerfisAsync(contexto);

        // Act: o cadastro público nem tem campo de perfil no DTO - é assim que
        // garantimos, no código, que ninguém vira Administrador se auto-cadastrando.
        var usuario = await servico.CadastrarContaPublicaAsync(new CadastroPublicoDto
        {
            Nome = "Joao",
            Email = "joao@teste.com",
            Senha = "senha123"
        });

        // Assert
        Assert.Equal("Franqueado", usuario.Perfil);
    }
}
