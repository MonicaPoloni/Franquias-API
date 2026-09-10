using System.ComponentModel.DataAnnotations;

namespace Franquias.Api.DTOs;

/// <summary>
/// Dados para criar um usuário com um perfil específico (ex: Gerente, Suporte,
/// ou até outro Administrador). Só um Administrador logado pode usar isso
/// (ver UsuariosController) - por isso o PerfilId fica livre aqui.
/// </summary>
public class RegistrarUsuarioDto
{
    [Required, MaxLength(120)]
    public string Nome { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(160)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Senha { get; set; } = string.Empty;

    [Required]
    public int PerfilId { get; set; }
}

/// <summary>
/// Dados do cadastro público (tela de "criar conta"). Não tem PerfilId de propósito:
/// se deixássemos a pessoa escolher o perfil aqui, qualquer um poderia se cadastrar
/// como Administrador. Toda conta criada por esse caminho vira Franqueado.
/// </summary>
public class CadastroPublicoDto
{
    [Required, MaxLength(120)]
    public string Nome { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(160)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Senha { get; set; } = string.Empty;
}

/// <summary>Dados que o cliente envia para fazer login.</summary>
public class LoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Senha { get; set; } = string.Empty;
}

/// <summary>O que a API devolve depois de um cadastro ou login (sem a senha!).</summary>
public class UsuarioResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Perfil { get; set; } = string.Empty;
}

/// <summary>Resposta do login: o token JWT e quando ele expira.</summary>
public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
    public UsuarioResponseDto Usuario { get; set; } = null!;
}
