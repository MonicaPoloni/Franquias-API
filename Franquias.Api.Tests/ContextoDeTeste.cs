using Franquias.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Franquias.Api.Tests;

// Usamos o banco "InMemory" do EF Core pra testar sem precisar de um arquivo
// .db de verdade. Cada teste chama Criar() e ganha um banco novo e vazio -
// o nome (Guid) é sempre diferente, então um teste nunca enxerga dados
// deixados por outro teste.
//
// Atenção: o InMemory não valida tudo igual a um banco relacional de verdade
// (por exemplo, ele não recusa CNPJ duplicado sozinho, tipo o SQLite faria).
// Por isso os testes daqui checam as regras que o nosso C# aplica (nos
// Services), não as regras que só existem no banco.
public static class ContextoDeTeste
{
    public static ApplicationDbContext Criar()
    {
        var opcoes = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(opcoes);
    }
}
