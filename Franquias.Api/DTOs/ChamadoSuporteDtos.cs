using System.ComponentModel.DataAnnotations;
using Franquias.Api.Entities;

namespace Franquias.Api.DTOs;

public class ChamadoSuporteCreateDto
{
    [Required]
    public int UnidadeFranqueadaId { get; set; }

    [Required]
    public int UsuarioAberturaId { get; set; }

    [Required, MaxLength(160)]
    public string Titulo { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Descricao { get; set; } = string.Empty;

    public PrioridadeChamadoSuporte Prioridade { get; set; } = PrioridadeChamadoSuporte.Media;
    public CategoriaChamadoSuporte Categoria { get; set; } = CategoriaChamadoSuporte.Outro;
}

public class ChamadoSuporteAtualizarStatusDto
{
    [Required]
    public StatusChamadoSuporte Status { get; set; }
}

// Filtro usado em GET /api/chamadossuporte.
public class ChamadoSuporteFiltroDto : ParametrosPaginacao
{
    public StatusChamadoSuporte? Status { get; set; }
    public PrioridadeChamadoSuporte? Prioridade { get; set; }
    public int? UnidadeFranqueadaId { get; set; }
}

public class ChamadoSuporteResponseDto
{
    public int Id { get; set; }
    public int UnidadeFranqueadaId { get; set; }
    public string UnidadeFranqueadaNome { get; set; } = string.Empty;
    public int UsuarioAberturaId { get; set; }
    public string UsuarioAberturaNome { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public CategoriaChamadoSuporte Categoria { get; set; }
    public StatusChamadoSuporte Status { get; set; }
    public PrioridadeChamadoSuporte Prioridade { get; set; }
    public DateTime DataAbertura { get; set; }
    public DateTime? DataFechamento { get; set; }
}
