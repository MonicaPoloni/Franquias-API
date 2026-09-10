using System.ComponentModel.DataAnnotations;

namespace Franquias.Api.DTOs;

public class UnidadeFranqueadaCreateDto
{
    [Required, MaxLength(160)]
    public string Nome { get; set; } = string.Empty;

    [Required, MaxLength(14)]
    public string Cnpj { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string Endereco { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Cidade { get; set; } = string.Empty;

    [Required, MaxLength(2)]
    public string Estado { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telefone { get; set; }

    public DateTime? DataInauguracao { get; set; }

    [Required]
    public int FranqueadoraId { get; set; }

    [Required]
    public int FranqueadoId { get; set; }
}

// Filtro usado em GET /api/unidadesfranqueadas. Todos os campos são opcionais -
// se vier em branco, esse filtro simplesmente não é aplicado.
public class UnidadeFranqueadaFiltroDto : ParametrosPaginacao
{
    public string? Nome { get; set; }
    public string? Cidade { get; set; }
    public string? Cnpj { get; set; }

    // Se não vier nada, a listagem mostra ativas E inativas juntas.
    public bool? Ativo { get; set; }

    // Aceita "nome", "cidade" ou "cnpj". Qualquer outro valor (ou vazio) usa "nome".
    public string? OrdenarPor { get; set; }
}

public class UnidadeFranqueadaResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public DateTime? DataInauguracao { get; set; }
    public bool Ativo { get; set; }
    public int FranqueadoraId { get; set; }
    public string FranqueadoraNome { get; set; } = string.Empty;
    public int FranqueadoId { get; set; }
    public string FranqueadoNome { get; set; } = string.Empty;
}

/// <summary>Corpo usado para ativar ou inativar uma unidade (PUT /api/unidadesfranqueadas/{id}/status).</summary>
public class UnidadeFranqueadaAtualizarStatusDto
{
    public bool Ativo { get; set; }
}
