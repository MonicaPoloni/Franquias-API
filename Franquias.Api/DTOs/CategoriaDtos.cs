using System.ComponentModel.DataAnnotations;

namespace Franquias.Api.DTOs;

public class CategoriaCreateDto
{
    [Required, MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Descricao { get; set; }
}

public class CategoriaResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}
