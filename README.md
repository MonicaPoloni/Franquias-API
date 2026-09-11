# Sistema de Gestão de Franquias

API REST em C# (ASP.NET Core Web API) para gerenciar uma rede de franquias:
franqueadora, unidades franqueadas, franqueados/responsáveis, catálogo de
produtos/serviços, estoque, vendas, royalties, fornecedores, chamados de
suporte e relatórios/indicadores gerenciais.

Trabalho acadêmico da disciplina de Desenvolvimento Back-end.

## Tecnologias utilizadas

- C# / .NET 8
- ASP.NET Core Web API
- Entity Framework Core + SQLite
- Autenticação JWT (JSON Web Token)
- Swagger / OpenAPI (Swashbuckle)
- xUnit + EF Core InMemory (testes automatizados)

## Arquitetura

Controller → Service → Repository, com DTOs para entrada/saída, injeção de
dependência, tratamento global de exceções e Migrations do EF Core.

```
Franquias.Api/
├── Controllers/
├── Entities/
├── DTOs/
├── Services/
├── Repositories/
├── Data/
├── Migrations/
├── Configurations/
└── Program.cs
Franquias.Api.Tests/
```

## Requisitos para executar

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) instalado

Não é necessário instalar nenhum banco de dados separado - o projeto usa
SQLite, que roda a partir de um único arquivo local (`franquias.db`).

## Como rodar a API

```bash
cd Franquias.Api
dotnet run
```

Na primeira vez que a API sobe, ela já:
1. Aplica automaticamente todas as Migrations (cria o banco `franquias.db` e as tabelas).
2. Popula o banco com dados de exemplo (unidades, produtos, estoque, vendas, uma cobrança de royalty e chamados de suporte), caso ainda esteja vazio.

Não é preciso rodar nenhum comando manual do Entity Framework - só `dotnet run`.

A API sobe em `http://localhost:5126` (a porta pode variar conforme o
`launchSettings.json`). O Swagger fica disponível em:

```
http://localhost:5126/swagger
```

## Como testar

1. Abra o Swagger no navegador.
2. Faça login em `POST /api/Auth/login` com um dos usuários de exemplo abaixo.
3. Copie o `token` da resposta.
4. Clique em **Authorize** (cadeado no topo da página) e cole **só o token**, sem escrever "Bearer" na frente - o próprio Swagger já adiciona esse prefixo.
5. Explore os demais endpoints.

### Usuários de exemplo (criados pelo seed automático)

| E-mail | Senha | Perfil |
|---|---|---|
| ana@teste.com | 123456 | Administrador |
| marcia@teste.com | 123456 | Gerente |
| rafael@teste.com | 123456 | Suporte |

## Executando os testes automatizados

```bash
dotnet test
```

## Principais decisões técnicas

- **JWT** para autenticação stateless, com o perfil do usuário embutido no token e usado para autorização por `Roles`.
- **SQLite** pela simplicidade de configuração, adequada ao escopo do trabalho. O provider do EF Core para SQLite não suporta `Sum()`/ordenação direto no SQL sobre colunas `decimal`; isso é contornado trazendo os valores para memória antes de somar/ordenar (ver `RelatorioService`, `CobrancaService`, `ProdutoServicoService`).
- **Ativação/inativação** (em vez de exclusão física) para Usuário, Unidade Franqueada, Produto/Serviço e Fornecedor - preserva o histórico de vendas, estoque e chamados desses registros.
- **Royalty calculado automaticamente**: o faturamento usado para calcular a cobrança de royalty não é digitado manualmente - é somado a partir das vendas reais da unidade no mês da competência informada.
- Documentação completa dos endpoints, entidades e regras de negócio está no relatório entregue em separado (`.docx`), que inclui também o diagrama entidade-relacionamento.
