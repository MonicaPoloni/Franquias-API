using Franquias.Api.Data;
using Franquias.Api.Entities;

namespace Franquias.Api.Tests;

// "Semente" de dados: monta rapidinho o conjunto mínimo de entidades que os
// testes de Estoque/Venda/Cobrança/Chamado precisam ter no banco antes de
// poder testar a regra de negócio de verdade (ex: pra testar uma Venda,
// primeiro precisa existir uma Unidade Franqueada e um Produto).
public static class SementeDeDados
{
    public static async Task<UnidadeFranqueada> CriarUnidadeFranqueadaAsync(ApplicationDbContext contexto)
    {
        var franqueadora = new Franqueadora
        {
            RazaoSocial = "Rede Teste LTDA",
            NomeFantasia = "Rede Teste",
            Cnpj = "11222333000144"
        };
        var franqueado = new Franqueado { Nome = "Dono da Unidade", Cpf = "12345678901" };
        var unidade = new UnidadeFranqueada
        {
            Nome = "Unidade Teste",
            Cnpj = "11222333000225",
            Endereco = "Rua Teste, 1",
            Cidade = "Sao Paulo",
            Estado = "SP",
            Franqueadora = franqueadora,
            Franqueado = franqueado
        };

        contexto.UnidadesFranqueadas.Add(unidade);
        await contexto.SaveChangesAsync();
        return unidade;
    }

    public static async Task<ProdutoServico> CriarProdutoAsync(
        ApplicationDbContext contexto, decimal preco = 10, TipoProdutoServico tipo = TipoProdutoServico.Produto)
    {
        var categoria = new Categoria { Nome = "Categoria Teste" };
        var produto = new ProdutoServico
        {
            Nome = "Produto Teste",
            Preco = preco,
            Tipo = tipo,
            Categoria = categoria
        };

        contexto.ProdutosServicos.Add(produto);
        await contexto.SaveChangesAsync();
        return produto;
    }

    public static async Task<Usuario> CriarUsuarioAsync(ApplicationDbContext contexto, string nomePerfil = "Administrador")
    {
        var perfil = new Perfil { Nome = nomePerfil };
        var usuario = new Usuario
        {
            Nome = "Usuario Teste",
            Email = $"{Guid.NewGuid()}@teste.com",
            SenhaHash = "hash-nao-importa-para-esse-teste",
            Perfil = perfil
        };

        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();
        return usuario;
    }
}
