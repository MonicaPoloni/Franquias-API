namespace Franquias.Api.DTOs;

// Filtro comum aos relatórios que fazem sentido "por período" (faturamento,
// ranking, royalties, produtos mais vendidos). Ambas as datas são opcionais -
// se não vier nenhuma, o relatório considera o histórico inteiro.
public class FiltroPeriodoDto
{
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
}

public class FaturamentoPorUnidadeDto
{
    public int UnidadeFranqueadaId { get; set; }
    public string UnidadeFranqueadaNome { get; set; } = string.Empty;
    public decimal TotalFaturado { get; set; }
    public int QuantidadeVendas { get; set; }
}

public class RankingUnidadeDto
{
    public int Posicao { get; set; }
    public int UnidadeFranqueadaId { get; set; }
    public string UnidadeFranqueadaNome { get; set; } = string.Empty;
    public decimal TotalFaturado { get; set; }
}

public class TotalRoyaltiesDto
{
    public decimal TotalCobrado { get; set; }
    public decimal TotalPago { get; set; }
    public decimal TotalPendente { get; set; }
    public int QuantidadeCobrancas { get; set; }
}

public class RoyaltiesPorUnidadeDto
{
    public int UnidadeFranqueadaId { get; set; }
    public string UnidadeFranqueadaNome { get; set; } = string.Empty;
    public decimal TotalCobrado { get; set; }
    public decimal TotalPago { get; set; }
    public decimal TotalPendente { get; set; }
    public int QuantidadeCobrancas { get; set; }
}

public class ProdutoMaisVendidoDto
{
    public int ProdutoServicoId { get; set; }
    public string ProdutoNome { get; set; } = string.Empty;
    public int QuantidadeVendida { get; set; }
    public decimal TotalFaturado { get; set; }
}

public class EstoqueCriticoDto
{
    public int EstoqueId { get; set; }
    public string UnidadeFranqueadaNome { get; set; } = string.Empty;
    public string ProdutoNome { get; set; } = string.Empty;
    public int QuantidadeAtual { get; set; }
    public int QuantidadeMinima { get; set; }
}

public class ChamadosPorStatusDto
{
    public string Status { get; set; } = string.Empty;
    public int Quantidade { get; set; }
}
