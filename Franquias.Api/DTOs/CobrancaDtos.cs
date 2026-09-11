using System.ComponentModel.DataAnnotations;
using Franquias.Api.Entities;

namespace Franquias.Api.DTOs;

// Repare que não tem campo de FaturamentoBase aqui: o valor não é digitado por
// quem cria a cobrança, e sim somado automaticamente a partir das vendas da
// unidade naquele mês (ver CobrancaService) - assim o royalty sempre bate com
// o que realmente foi vendido, sem depender de alguém preencher certo.
public class CobrancaCreateDto
{
    [Required]
    public int UnidadeFranqueadaId { get; set; }

    [Required]
    public DateOnly Competencia { get; set; }

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
