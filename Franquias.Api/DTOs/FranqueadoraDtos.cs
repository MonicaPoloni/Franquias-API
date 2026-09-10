using System.ComponentModel.DataAnnotations;

namespace Franquias.Api.DTOs;

public class FranqueadoraCreateDto
{
    [Required, MaxLength(160)]
    public string RazaoSocial { get; set; } = string.Empty;

    [Required, MaxLength(160)]
    public string NomeFantasia { get; set; } = string.Empty;

    [Required, MaxLength(14)]
    public string Cnpj { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telefone { get; set; }

    [MaxLength(160), EmailAddress]
    public string? Email { get; set; }

    public DateTime? DataFundacao { get; set; }
}

public class FranqueadoraResponseDto
{
    public int Id { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string NomeFantasia { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public DateTime? DataFundacao { get; set; }
}
