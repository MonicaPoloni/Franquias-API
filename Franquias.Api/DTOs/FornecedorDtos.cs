using System.ComponentModel.DataAnnotations;

namespace Franquias.Api.DTOs;

public class FornecedorCreateDto
{
    [Required, MaxLength(160)]
    public string RazaoSocial { get; set; } = string.Empty;

    [Required, MaxLength(14)]
    public string Cnpj { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telefone { get; set; }

    [MaxLength(160), EmailAddress]
    public string? Email { get; set; }
}

// Filtro usado em GET /api/fornecedores.
public class FornecedorFiltroDto : ParametrosPaginacao
{
    public string? Nome { get; set; }
    public string? Cnpj { get; set; }
    public bool? Ativo { get; set; }
}

public class FornecedorResponseDto
{
    public int Id { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public bool Ativo { get; set; }
}

/// <summary>Corpo usado para ativar ou inativar um fornecedor (PUT /api/fornecedores/{id}/status).</summary>
public class FornecedorAtualizarStatusDto
{
    public bool Ativo { get; set; }
}
