namespace Franquias.Api.Entities;

public class UnidadeFranqueada
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public DateTime? DataInauguracao { get; set; }
    public bool Ativo { get; set; } = true;

    public int FranqueadoraId { get; set; }
    public Franqueadora Franqueadora { get; set; } = null!;

    public int FranqueadoId { get; set; }
    public Franqueado Franqueado { get; set; } = null!;

    public ICollection<Estoque> Estoques { get; set; } = new List<Estoque>();
    public ICollection<Venda> Vendas { get; set; } = new List<Venda>();
    public ICollection<Cobranca> Cobrancas { get; set; } = new List<Cobranca>();
    public ICollection<ChamadoSuporte> ChamadosSuporte { get; set; } = new List<ChamadoSuporte>();
}
