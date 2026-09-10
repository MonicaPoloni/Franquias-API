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
}

public class ChamadoSuporteAtualizarStatusDto
{
    [Required]
    public StatusChamadoSuporte Status { get; set; }
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
    public StatusChamadoSuporte Status { get; set; }
    public PrioridadeChamadoSuporte Prioridade { get; set; }
    public DateTime DataAbertura { get; set; }
    public DateTime? DataFechamento { get; set; }
}
