using System.ComponentModel.DataAnnotations;
using Franquias.Api.Entities;

namespace Franquias.Api.DTOs;

public class ItemVendaCreateDto
{
    [Required]
    public int ProdutoServicoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")]
    public int Quantidade { get; set; }
}

public class VendaCreateDto
{
    [Required]
    public int UnidadeFranqueadaId { get; set; }

    [Required]
    public int UsuarioId { get; set; }

    [Required]
    public FormaPagamento FormaPagamento { get; set; }

    [Required, MinLength(1, ErrorMessage = "A venda precisa ter pelo menos um item.")]
    public List<ItemVendaCreateDto> Itens { get; set; } = new();
}

// Filtro usado em GET /api/vendas.
public class VendaFiltroDto : ParametrosPaginacao
{
    public int? UnidadeFranqueadaId { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
}

public class ItemVendaResponseDto
{
    public int ProdutoServicoId { get; set; }
    public string ProdutoNome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class VendaResponseDto
{
    public int Id { get; set; }
    public int UnidadeFranqueadaId { get; set; }
    public string UnidadeFranqueadaNome { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public DateTime DataVenda { get; set; }
    public decimal ValorTotal { get; set; }
    public FormaPagamento FormaPagamento { get; set; }
    public List<ItemVendaResponseDto> Itens { get; set; } = new();
}
