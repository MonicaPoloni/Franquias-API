namespace Franquias.Api.Entities;

public class Franqueadora
{
    public int Id { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string NomeFantasia { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public DateTime? DataFundacao { get; set; }

    public ICollection<UnidadeFranqueada> UnidadesFranqueadas { get; set; } = new List<UnidadeFranqueada>();
}
