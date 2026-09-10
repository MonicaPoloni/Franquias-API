using Franquias.Api.Configurations;
using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Repositories;
using Franquias.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Franquias.Api.Tests;

public class AuthServiceTests
{
    // Configuração de JWT só pra teste. A chave precisa ter pelo menos 32
    // caracteres (regra do algoritmo HMAC-SHA256), senão o TokenService quebra.
    private static JwtSettings ConfiguracaoJwtDeTeste() => new()
    {
        Key = "chave-secreta-so-para-os-testes-automatizados-000",
        Issuer = "Franquias.Api.Testes",
        Audience = "Franquias.Api.Testes",
        ExpiresInMinutes = 60
    };

    // Monta um usuário Administrador com senha "senha123" já cadastrado no
    // banco de teste, e devolve o AuthService pronto pra usar. Isso evita
    // repetir esse "arranjo" em cada teste da classe.
    private static async Task<IAuthService> CriarAuthServiceComUsuarioDeTesteAsync(Data.ApplicationDbContext contexto)
    {
        var perfil = new Perfil { Nome = "Administrador" };
        contexto.Perfis.Add(perfil);
        await contexto.SaveChangesAsync();

        var hasher = new PasswordHasher<Usuario>();
        var usuario = new Usuario
        {
            Nome = "Usuario de Teste",
            Email = "teste@teste.com",
            PerfilId = perfil.Id,
            Perfil = perfil
        };
        usuario.SenhaHash = hasher.HashPassword(usuario, "senha123");
        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();

        var usuarioRepositorio = new UsuarioRepository(contexto);
        var tokenService = new TokenService(Options.Create(ConfiguracaoJwtDeTeste()));
        return new AuthService(usuarioRepositorio, hasher, tokenService);
    }

    [Fact]
    public async Task Login_ComEmailESenhaCorretos_DeveDevolverToken()
    {
        // Arrange
        var contexto = ContextoDeTeste.Criar();
        var authService = await CriarAuthServiceComUsuarioDeTesteAsync(contexto);

        // Act
        var resultado = await authService.LoginAsync(new LoginDto
        {
            Email = "teste@teste.com",
            Senha = "senha123"
        });

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(resultado.Token));
        Assert.Equal("teste@teste.com", resultado.Usuario.Email);
        Assert.Equal("Administrador", resultado.Usuario.Perfil);
    }

    [Fact]
    public async Task Login_ComSenhaErrada_DeveLancarUnauthorizedAccessException()
    {
        // Arrange
        var contexto = ContextoDeTeste.Criar();
        var authService = await CriarAuthServiceComUsuarioDeTesteAsync(contexto);

        // Act + Assert: ThrowsAsync executa o código e confere se ele lança
        // exatamente a exceção esperada. É assim que testamos "deu erro certo".
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            authService.LoginAsync(new LoginDto { Email = "teste@teste.com", Senha = "senhaErrada" }));
    }

    [Fact]
    public async Task Login_ComEmailQueNaoExiste_DeveLancarUnauthorizedAccessException()
    {
        // Arrange
        var contexto = ContextoDeTeste.Criar();
        var authService = await CriarAuthServiceComUsuarioDeTesteAsync(contexto);

        // Importante: usamos a MESMA exceção pra "email não existe" e "senha errada"
        // de propósito (ver o comentário no AuthService) - por isso testamos os
        // dois casos aqui, garantindo que nenhum deles vaza informação demais.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            authService.LoginAsync(new LoginDto { Email = "naoexiste@teste.com", Senha = "qualquer" }));
    }
}
