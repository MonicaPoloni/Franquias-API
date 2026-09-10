using System.ComponentModel.DataAnnotations;

namespace Franquias.Api.DTOs;

public class FranqueadoCreateDto
{
    [Required, MaxLength(120)]
    public string Nome { get; set; } = string.Empty;

    [Required, MaxLength(11)]
    public string Cpf { get; set; } = string.Empty;

    [MaxLength(160), EmailAddress]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Telefone { get; set; }

    /// <summary>Opcional: vincula este franqueado a um usuário já cadastrado, para ele poder logar.</summary>
    public int? UsuarioId { get; set; }
}

public class FranqueadoResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public int? UsuarioId { get; set; }
}
