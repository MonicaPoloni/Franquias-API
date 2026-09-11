using Franquias.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Franquias.Api.Data;

// Só entra em ação quando o banco está vazio - por isso a checagem logo no
// início. Sem isso, quem clona o projeto do zero só teria os 4 Perfis (que já
// vêm de uma Migration) e nenhum outro dado pra testar filtros, relatórios e
// regras de negócio. Esse método é chamado uma vez, na subida da API (ver
// Program.cs).
public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext contexto)
    {
        if (await contexto.UnidadesFranqueadas.AnyAsync())
            return;

        var hasher = new PasswordHasher<Usuario>();

        var perfilAdmin = await contexto.Perfis.FirstAsync(p => p.Nome == PerfilNomes.Administrador);
        var perfilGerente = await contexto.Perfis.FirstAsync(p => p.Nome == PerfilNomes.Gerente);
        var perfilSuporte = await contexto.Perfis.FirstAsync(p => p.Nome == PerfilNomes.Suporte);

        var admin = new Usuario { Nome = "Ana Admin", Email = "ana@teste.com", PerfilId = perfilAdmin.Id, Perfil = perfilAdmin };
        admin.SenhaHash = hasher.HashPassword(admin, "123456");

        var gerente = new Usuario { Nome = "Marcia Gerente", Email = "marcia@teste.com", PerfilId = perfilGerente.Id, Perfil = perfilGerente };
        gerente.SenhaHash = hasher.HashPassword(gerente, "123456");

        var suporte = new Usuario { Nome = "Rafael Suporte", Email = "rafael@teste.com", PerfilId = perfilSuporte.Id, Perfil = perfilSuporte };
        suporte.SenhaHash = hasher.HashPassword(suporte, "123456");

        contexto.Usuarios.AddRange(admin, gerente, suporte);

        var franqueadora = new Franqueadora
        {
            RazaoSocial = "Rede Sabor Brasil LTDA",
            NomeFantasia = "Rede Sabor",
            Cnpj = "11222333000144",
            Telefone = "1140028922",
            Email = "contato@redesabor.com.br",
            DataFundacao = new DateTime(2018, 5, 1)
        };

        var franqueadoCarlos = new Franqueado { Nome = "Carlos Souza", Cpf = "12345678901", Email = "carlos@redesabor.com.br", Telefone = "11988887777" };
        var franqueadoFernanda = new Franqueado { Nome = "Fernanda Lima", Cpf = "98765432100", Email = "fernanda@redesabor.com.br", Telefone = "21988886666" };

        var unidadeCentro = new UnidadeFranqueada
        {
            Nome = "Unidade Centro", Cnpj = "11222333000225", Endereco = "Rua Augusta, 100",
            Cidade = "São Paulo", Estado = "SP", Franqueadora = franqueadora, Franqueado = franqueadoCarlos,
            DataInauguracao = new DateTime(2023, 3, 10)
        };
        var unidadeZonaSul = new UnidadeFranqueada
        {
            Nome = "Unidade Zona Sul", Cnpj = "11222333000306", Endereco = "Av. Ibirapuera, 500",
            Cidade = "São Paulo", Estado = "SP", Franqueadora = franqueadora, Franqueado = franqueadoCarlos,
            DataInauguracao = new DateTime(2023, 8, 22)
        };
        var unidadeCopacabana = new UnidadeFranqueada
        {
            Nome = "Unidade Copacabana", Cnpj = "11222333000487", Endereco = "Av. Atlântica, 900",
            Cidade = "Rio de Janeiro", Estado = "RJ", Franqueadora = franqueadora, Franqueado = franqueadoFernanda,
            DataInauguracao = new DateTime(2024, 1, 15)
        };
        contexto.UnidadesFranqueadas.AddRange(unidadeCentro, unidadeZonaSul, unidadeCopacabana);

        var categoriaAlimentos = new Categoria { Nome = "Alimentos", Descricao = "Comidas em geral" };
        var categoriaBebidas = new Categoria { Nome = "Bebidas", Descricao = "Bebidas em geral" };
        var categoriaServicos = new Categoria { Nome = "Serviços", Descricao = "Serviços prestados pela unidade" };

        var hamburguer = new ProdutoServico { Nome = "Hambúrguer Artesanal", Descricao = "Pão, carne 180g, queijo e salada", Preco = 28.90m, Tipo = TipoProdutoServico.Produto, Categoria = categoriaAlimentos };
        var batata = new ProdutoServico { Nome = "Batata Frita", Descricao = "Porção de batata frita", Preco = 15.00m, Tipo = TipoProdutoServico.Produto, Categoria = categoriaAlimentos };
        var refrigerante = new ProdutoServico { Nome = "Refrigerante Lata", Descricao = "Lata 350ml", Preco = 7.00m, Tipo = TipoProdutoServico.Produto, Categoria = categoriaBebidas };
        var suco = new ProdutoServico { Nome = "Suco Natural", Descricao = "Suco de fruta natural 400ml", Preco = 10.00m, Tipo = TipoProdutoServico.Produto, Categoria = categoriaBebidas };
        var montagemEvento = new ProdutoServico { Nome = "Montagem de Evento", Descricao = "Serviço de montagem de buffet para eventos", Preco = 350.00m, Tipo = TipoProdutoServico.Servico, Categoria = categoriaServicos };
        contexto.ProdutosServicos.AddRange(hamburguer, batata, refrigerante, suco, montagemEvento);

        var fornecedor = new Fornecedor { RazaoSocial = "Distribuidora Alimentos SP LTDA", Cnpj = "22333444000155", Telefone = "1133334444", Email = "vendas@distribuidorasp.com.br" };
        contexto.Fornecedores.Add(fornecedor);

        // Precisa salvar aqui pra gerar os Ids antes de criar Estoque/Venda,
        // que dependem desses registros já existirem no banco.
        await contexto.SaveChangesAsync();

        contexto.Estoques.AddRange(
            new Estoque { UnidadeFranqueada = unidadeCentro, ProdutoServico = hamburguer, QuantidadeAtual = 50, QuantidadeMinima = 10 },
            new Estoque { UnidadeFranqueada = unidadeCentro, ProdutoServico = batata, QuantidadeAtual = 40, QuantidadeMinima = 10 },
            new Estoque { UnidadeFranqueada = unidadeCentro, ProdutoServico = refrigerante, QuantidadeAtual = 100, QuantidadeMinima = 20 },
            // Esse aqui fica de propósito abaixo do mínimo, pra já aparecer
            // pronto no relatório de estoque crítico.
            new Estoque { UnidadeFranqueada = unidadeCentro, ProdutoServico = suco, QuantidadeAtual = 8, QuantidadeMinima = 15 },
            new Estoque { UnidadeFranqueada = unidadeZonaSul, ProdutoServico = hamburguer, QuantidadeAtual = 30, QuantidadeMinima = 10 },
            new Estoque { UnidadeFranqueada = unidadeZonaSul, ProdutoServico = refrigerante, QuantidadeAtual = 60, QuantidadeMinima = 20 },
            new Estoque { UnidadeFranqueada = unidadeCopacabana, ProdutoServico = hamburguer, QuantidadeAtual = 25, QuantidadeMinima = 10 },
            new Estoque { UnidadeFranqueada = unidadeCopacabana, ProdutoServico = refrigerante, QuantidadeAtual = 45, QuantidadeMinima = 20 }
        );

        // Todas as vendas de exemplo caem no mês atual, de propósito - assim os
        // relatórios de faturamento/royalty já mostram algo sem precisar
        // informar intervalo de datas.
        var anoAtual = DateTime.UtcNow.Year;
        var mesAtual = DateTime.UtcNow.Month;

        var vendaCentro1 = CriarVenda(unidadeCentro, admin, new DateTime(anoAtual, mesAtual, 3), FormaPagamento.Pix,
            (hamburguer, 2), (refrigerante, 2));
        var vendaCentro2 = CriarVenda(unidadeCentro, gerente, new DateTime(anoAtual, mesAtual, 12), FormaPagamento.CartaoCredito,
            (hamburguer, 1), (batata, 1));
        var vendaZonaSul = CriarVenda(unidadeZonaSul, gerente, new DateTime(anoAtual, mesAtual, 7), FormaPagamento.Dinheiro,
            (hamburguer, 3));
        var vendaCopacabana = CriarVenda(unidadeCopacabana, admin, new DateTime(anoAtual, mesAtual, 18), FormaPagamento.Pix,
            (refrigerante, 4));

        contexto.Vendas.AddRange(vendaCentro1, vendaCentro2, vendaZonaSul, vendaCopacabana);

        // Cobrança de royalty da Unidade Centro sobre o que ela faturou esse
        // mês (vendaCentro1 + vendaCentro2) - o mesmo cálculo que o
        // CobrancaService faria de verdade.
        var faturamentoCentro = vendaCentro1.ValorTotal + vendaCentro2.ValorTotal;
        var percentualRoyalty = 8m;
        contexto.Cobrancas.Add(new Cobranca
        {
            UnidadeFranqueada = unidadeCentro,
            Competencia = new DateOnly(anoAtual, mesAtual, 1),
            FaturamentoBase = faturamentoCentro,
            PercentualRoyalty = percentualRoyalty,
            ValorCobranca = Math.Round(faturamentoCentro * percentualRoyalty / 100m, 2),
            DataVencimento = new DateTime(anoAtual, mesAtual, 1).AddMonths(1),
            Status = StatusCobranca.Pendente
        });

        contexto.ChamadosSuporte.AddRange(
            new ChamadoSuporte
            {
                UnidadeFranqueada = unidadeCentro,
                UsuarioAbertura = gerente,
                Titulo = "Impressora de cupom não funciona",
                Descricao = "A impressora térmica parou de imprimir os cupons de venda.",
                Categoria = CategoriaChamadoSuporte.TI,
                Prioridade = PrioridadeChamadoSuporte.Alta,
                Status = StatusChamadoSuporte.Aberto
            },
            new ChamadoSuporte
            {
                UnidadeFranqueada = unidadeZonaSul,
                UsuarioAbertura = admin,
                Titulo = "Aumento do estoque mínimo de refrigerante",
                Descricao = "Solicitação para aumentar o estoque mínimo de 20 para 30 unidades.",
                Categoria = CategoriaChamadoSuporte.Operacional,
                Prioridade = PrioridadeChamadoSuporte.Media,
                Status = StatusChamadoSuporte.Resolvido,
                DataFechamento = DateTime.UtcNow
            }
        );

        await contexto.SaveChangesAsync();
    }

    private static Venda CriarVenda(
        UnidadeFranqueada unidade, Usuario usuario, DateTime data, FormaPagamento formaPagamento,
        params (ProdutoServico Produto, int Quantidade)[] itens)
    {
        var itensVenda = itens
            .Select(i => new ItemVenda { ProdutoServico = i.Produto, Quantidade = i.Quantidade, PrecoUnitario = i.Produto.Preco })
            .ToList();

        return new Venda
        {
            UnidadeFranqueada = unidade,
            Usuario = usuario,
            DataVenda = data,
            FormaPagamento = formaPagamento,
            ValorTotal = itensVenda.Sum(i => i.Quantidade * i.PrecoUnitario),
            Itens = itensVenda
        };
    }
}
