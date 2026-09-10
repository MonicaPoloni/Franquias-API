namespace Franquias.Api.Entities;

public class ProdutoServico
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal Preco { get; set; }
    public TipoProdutoServico Tipo { get; set; }

    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;

    public ICollection<Estoque> Estoques { get; set; } = new List<Estoque>();
    public ICollection<ItemVenda> ItensVenda { get; set; } = new List<ItemVenda>();
}
