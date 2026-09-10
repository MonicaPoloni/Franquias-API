namespace Franquias.Api.Entities;

/// <summary>
/// Pessoa responsável por uma ou mais unidades franqueadas.
/// </summary>
public class Franqueado
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }

    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public ICollection<UnidadeFranqueada> UnidadesFranqueadas { get; set; } = new List<UnidadeFranqueada>();
}
