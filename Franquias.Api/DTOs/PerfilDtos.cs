using System.ComponentModel.DataAnnotations;

namespace Franquias.Api.DTOs;

public class PerfilCreateDto
{
    [Required, MaxLength(50)]
    public string Nome { get; set; } = string.Empty;
}

public class PerfilResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}
