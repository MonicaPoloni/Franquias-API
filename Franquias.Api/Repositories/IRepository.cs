namespace Franquias.Api.Repositories;

/// <summary>
/// Contrato genérico de acesso a dados: define as operações básicas que
/// qualquer entidade (Categoria, Fornecedor, etc.) pode usar para ler e gravar no banco.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> ObterTodosAsync();

    // Devolve uma consulta (ainda não executada) em vez de uma lista pronta.
    // Isso deixa o Service montar filtro/ordenação/paginação em cima dela, e só
    // então o EF Core manda pro banco UM SELECT já filtrado - em vez de trazer
    // a tabela inteira pra memória pra só depois cortar um pedaço.
    IQueryable<T> Consultar();

    Task<T?> ObterPorIdAsync(int id);
    Task AdicionarAsync(T entidade);
    void Atualizar(T entidade);
    void Remover(T entidade);
    Task<bool> SalvarAsync();
}
