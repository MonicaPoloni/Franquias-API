namespace Franquias.Api.Entities;

public class MovimentacaoEstoque
{
    public int Id { get; set; }

    public int EstoqueId { get; set; }
    public Estoque Estoque { get; set; } = null!;

    public TipoMovimentacaoEstoque Tipo { get; set; }
    public int Quantidade { get; set; }

    /// <summary>Quantidade que o estoque ficou logo depois desta movimentação (registro histórico).</summary>
    public int QuantidadeResultante { get; set; }
    public DateTime Data { get; set; } = DateTime.UtcNow;
    public string? Observacao { get; set; }

    public int? FornecedorId { get; set; }
    public Fornecedor? Fornecedor { get; set; }
}
