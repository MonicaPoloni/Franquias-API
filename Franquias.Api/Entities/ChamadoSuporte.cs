namespace Franquias.Api.Entities;

public class ChamadoSuporte
{
    public int Id { get; set; }

    public int UnidadeFranqueadaId { get; set; }
    public UnidadeFranqueada UnidadeFranqueada { get; set; } = null!;

    public int UsuarioAberturaId { get; set; }
    public Usuario UsuarioAbertura { get; set; } = null!;

    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public CategoriaChamadoSuporte Categoria { get; set; } = CategoriaChamadoSuporte.Outro;
    public StatusChamadoSuporte Status { get; set; } = StatusChamadoSuporte.Aberto;
    public PrioridadeChamadoSuporte Prioridade { get; set; } = PrioridadeChamadoSuporte.Media;
    public DateTime DataAbertura { get; set; } = DateTime.UtcNow;
    public DateTime? DataFechamento { get; set; }
}
