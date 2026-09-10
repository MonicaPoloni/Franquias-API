using System.ComponentModel.DataAnnotations;
using Franquias.Api.Entities;

namespace Franquias.Api.DTOs;

public class MovimentacaoEstoqueCreateDto
{
    [Required]
    public int EstoqueId { get; set; }

    [Required]
    public TipoMovimentacaoEstoque Tipo { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")]
    public int Quantidade { get; set; }

    [MaxLength(250)]
    public string? Observacao { get; set; }

    /// <summary>Só faz sentido para movimentações do tipo Entrada.</summary>
    public int? FornecedorId { get; set; }
}

// Filtro usado em GET /api/movimentacoesestoque. EstoqueId continua opcional
// (pra quem quiser ver só o histórico de um item de estoque específico).
public class MovimentacaoEstoqueFiltroDto : ParametrosPaginacao
{
    public int? EstoqueId { get; set; }
}

public class MovimentacaoEstoqueResponseDto
{
    public int Id { get; set; }
    public int EstoqueId { get; set; }
    public string ProdutoNome { get; set; } = string.Empty;
    public TipoMovimentacaoEstoque Tipo { get; set; }
    public int Quantidade { get; set; }
    public DateTime Data { get; set; }
    public string? Observacao { get; set; }
    public int? FornecedorId { get; set; }
    public string? FornecedorNome { get; set; }
    public int QuantidadeAtualAposMovimentacao { get; set; }
}
