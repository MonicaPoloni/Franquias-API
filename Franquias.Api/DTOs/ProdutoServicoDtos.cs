using System.ComponentModel.DataAnnotations;
using Franquias.Api.Entities;

namespace Franquias.Api.DTOs;

public class ProdutoServicoCreateDto
{
    [Required, MaxLength(160)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Descricao { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Preco { get; set; }

    [Required]
    public TipoProdutoServico Tipo { get; set; }

    [Required]
    public int CategoriaId { get; set; }
}

// Filtro usado em GET /api/produtosservicos.
public class ProdutoServicoFiltroDto : ParametrosPaginacao
{
    public int? CategoriaId { get; set; }
    public string? Nome { get; set; }

    // Se não vier nada, mostra ativos e inativos juntos.
    public bool? Ativo { get; set; }

    // Aceita "nome" ou "preco". Qualquer outro valor (ou vazio) usa "nome".
    public string? OrdenarPor { get; set; }
}

public class ProdutoServicoResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal Preco { get; set; }
    public TipoProdutoServico Tipo { get; set; }
    public bool Ativo { get; set; }
    public int CategoriaId { get; set; }
    public string CategoriaNome { get; set; } = string.Empty;
}

/// <summary>Corpo usado para ativar ou inativar um produto/serviço (PUT /api/produtosservicos/{id}/status).</summary>
public class ProdutoServicoAtualizarStatusDto
{
    public bool Ativo { get; set; }
}
