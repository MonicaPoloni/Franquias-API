namespace Franquias.Api.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public int PerfilId { get; set; }
    public Perfil Perfil { get; set; } = null!;

    public Franqueado? Franqueado { get; set; }
    public ICollection<Venda> Vendas { get; set; } = new List<Venda>();
    public ICollection<ChamadoSuporte> ChamadosAbertos { get; set; } = new List<ChamadoSuporte>();
}
