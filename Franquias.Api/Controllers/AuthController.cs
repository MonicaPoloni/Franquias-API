using Franquias.Api.DTOs;
using Franquias.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Franquias.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUsuarioService _usuarioService;

    public AuthController(IAuthService authService, IUsuarioService usuarioService)
    {
        _authService = authService;
        _usuarioService = usuarioService;
    }

    /// <summary>
    /// Cadastro público (rota sem login). Sempre cria a conta como Franqueado -
    /// quem precisar de um perfil diferente (Gerente, Suporte, Administrador)
    /// precisa ser cadastrado por um Administrador em POST /api/usuarios.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("registrar")]
    [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<UsuarioResponseDto>> Registrar(CadastroPublicoDto dto)
    {
        var usuario = await _usuarioService.CadastrarContaPublicaAsync(dto);
        return CreatedAtAction(nameof(Registrar), new { id = usuario.Id }, usuario);
    }

    /// <summary>Autentica um usuário e devolve o token JWT (rota pública).</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var resultado = await _authService.LoginAsync(dto);
        return Ok(resultado);
    }
}
