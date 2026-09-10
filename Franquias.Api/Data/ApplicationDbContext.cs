using Franquias.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Franquias.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Perfil> Perfis => Set<Perfil>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Franqueadora> Franqueadoras => Set<Franqueadora>();
    public DbSet<Franqueado> Franqueados => Set<Franqueado>();
    public DbSet<UnidadeFranqueada> UnidadesFranqueadas => Set<UnidadeFranqueada>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<ProdutoServico> ProdutosServicos => Set<ProdutoServico>();
    public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();
    public DbSet<Estoque> Estoques => Set<Estoque>();
    public DbSet<MovimentacaoEstoque> MovimentacoesEstoque => Set<MovimentacaoEstoque>();
    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<ItemVenda> ItensVenda => Set<ItemVenda>();
    public DbSet<Cobranca> Cobrancas => Set<Cobranca>();
    public DbSet<ChamadoSuporte> ChamadosSuporte => Set<ChamadoSuporte>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
