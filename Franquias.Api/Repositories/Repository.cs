using Franquias.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Franquias.Api.Repositories;

/// <summary>
/// Implementação genérica de IRepository&lt;T&gt; usando o Entity Framework Core.
/// Serve para qualquer entidade que não precise de consultas especiais
/// (ex: Categoria, Fornecedor, Franqueadora).
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext Contexto;
    protected readonly DbSet<T> DbSet;

    public Repository(ApplicationDbContext contexto)
    {
        Contexto = contexto;
        DbSet = contexto.Set<T>();
    }

    public async Task<IEnumerable<T>> ObterTodosAsync() => await DbSet.AsNoTracking().ToListAsync();

    public IQueryable<T> Consultar() => DbSet.AsNoTracking();

    public async Task<T?> ObterPorIdAsync(int id) => await DbSet.FindAsync(id);

    public async Task AdicionarAsync(T entidade) => await DbSet.AddAsync(entidade);

    public void Atualizar(T entidade) => DbSet.Update(entidade);

    public void Remover(T entidade) => DbSet.Remove(entidade);

    public async Task<bool> SalvarAsync() => await Contexto.SaveChangesAsync() > 0;
}
