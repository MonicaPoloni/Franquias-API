namespace Franquias.Api.Entities;

/// <summary>
/// Cobrança de royalty referente a uma competência (mês/ano) de uma unidade franqueada.
/// </summary>
public class Cobranca
{
    public int Id { get; set; }

    public int UnidadeFranqueadaId { get; set; }
    public UnidadeFranqueada UnidadeFranqueada { get; set; } = null!;

    public DateOnly Competencia { get; set; }
    public decimal FaturamentoBase { get; set; }
    public decimal PercentualRoyalty { get; set; }
    public decimal ValorCobranca { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public StatusCobranca Status { get; set; } = StatusCobranca.Pendente;
}
