using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Repositories;
using Franquias.Api.Services;

namespace Franquias.Api.Tests;

// Esses testes cobrem a regra de "CNPJ duplicado" que existe em vários
// Services (Fornecedor, Franqueadora, Franqueado). Testamos só o Fornecedor
// como representante - a lógica dos outros é igual, então testar todos
// seria só repetir o mesmo teste com nomes diferentes.
public class FornecedorServiceTests
{
    private static IFornecedorService CriarServico(Data.ApplicationDbContext contexto) =>
        new FornecedorService(new Repository<Fornecedor>(contexto));

    [Fact]
    public async Task CriarAsync_ComCnpjNovo_DeveSalvarFornecedor()
    {
        // Arrange
        var contexto = ContextoDeTeste.Criar();
        var servico = CriarServico(contexto);

        // Act
        var fornecedor = await servico.CriarAsync(new FornecedorCreateDto
        {
            RazaoSocial = "Distribuidora ABC",
            Cnpj = "11222333000144"
        });

        // Assert
        Assert.NotEqual(0, fornecedor.Id);
        Assert.Equal("Distribuidora ABC", fornecedor.RazaoSocial);
    }

    [Fact]
    public async Task CriarAsync_ComCnpjJaCadastrado_DeveLancarArgumentException()
    {
        // Arrange: já existe um fornecedor com esse CNPJ
        var contexto = ContextoDeTeste.Criar();
        var servico = CriarServico(contexto);
        await servico.CriarAsync(new FornecedorCreateDto
        {
            RazaoSocial = "Distribuidora ABC",
            Cnpj = "11222333000144"
        });

        // Act + Assert: tenta cadastrar outro fornecedor com o MESMO CNPJ
        await Assert.ThrowsAsync<ArgumentException>(() => servico.CriarAsync(new FornecedorCreateDto
        {
            RazaoSocial = "Outra Empresa",
            Cnpj = "11222333000144"
        }));
    }

    [Fact]
    public async Task AtualizarStatusAsync_DeveInativarEDepoisReativarOFornecedor()
    {
        // Arrange: fornecedor nasce ativo por padrão
        var contexto = ContextoDeTeste.Criar();
        var servico = CriarServico(contexto);
        var fornecedor = await servico.CriarAsync(new FornecedorCreateDto
        {
            RazaoSocial = "Distribuidora ABC",
            Cnpj = "11222333000144"
        });
        Assert.True(fornecedor.Ativo);

        // Act: inativa
        var inativado = await servico.AtualizarStatusAsync(fornecedor.Id, false);
        Assert.False(inativado.Ativo);

        // Act: reativa
        var reativado = await servico.AtualizarStatusAsync(fornecedor.Id, true);
        Assert.True(reativado.Ativo);
    }
}
