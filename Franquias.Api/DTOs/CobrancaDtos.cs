using System.ComponentModel.DataAnnotations;
using Franquias.Api.Entities;

namespace Franquias.Api.DTOs;

public class CobrancaCreateDto
{
    [Required]
    public int UnidadeFranqueadaId { get; set; }

    [Required]
    public DateOnly Competencia { get; set; }

    [Range(0, double.MaxValue)]
    public decimal FaturamentoBase { get; set; }

    [Range(0, 100)]
    public decimal PercentualRoyalty { get; set; }

    [Required]
    public DateTime DataVencimento { get; set; }
}

public class CobrancaResponseDto
{
    public int Id { get; set; }
    public int UnidadeFranqueadaId { get; set; }
    public string UnidadeFranqueadaNome { get; set; } = string.Empty;
    public DateOnly Competencia { get; set; }
    public decimal FaturamentoBase { get; set; }
    public decimal PercentualRoyalty { get; set; }
    public decimal ValorCobranca { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public StatusCobranca Status { get; set; }
}
