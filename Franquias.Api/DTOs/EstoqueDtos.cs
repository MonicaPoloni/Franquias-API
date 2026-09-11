using System.ComponentModel.DataAnnotations;

namespace Franquias.Api.DTOs;

public class EstoqueCreateDto
{
    [Required]
    public int UnidadeFranqueadaId { get; set; }

    [Required]
    public int ProdutoServicoId { get; set; }

    [Range(0, int.MaxValue)]
    public int QuantidadeInicial { get; set; }

    [Range(0, int.MaxValue)]
    public int QuantidadeMinima { get; set; }
}

/// <summary>Só permite alterar o mínimo de alerta; a quantidade atual muda apenas via movimentação.</summary>
public class EstoqueUpdateDto
{
    [Range(0, int.MaxValue)]
    public int QuantidadeMinima { get; set; }
}

// Filtro usado em GET /api/estoques.
public class EstoqueFiltroDto : ParametrosPaginacao
{
    public int? UnidadeFranqueadaId { get; set; }
}

public class EstoqueResponseDto
{
    public int Id { get; set; }
    public int UnidadeFranqueadaId { get; set; }
    public string UnidadeFranqueadaNome { get; set; } = string.Empty;
    public int ProdutoServicoId { get; set; }
    public string ProdutoNome { get; set; } = string.Empty;
    public int QuantidadeAtual { get; set; }
    public int QuantidadeMinima { get; set; }
    public DateTime UltimaAtualizacao { get; set; }
}
