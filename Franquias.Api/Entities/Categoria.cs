namespace Franquias.Api.Entities;

public class Categoria
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }

    public ICollection<ProdutoServico> Produtos { get; set; } = new List<ProdutoServico>();
}
