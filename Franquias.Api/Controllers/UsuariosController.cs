using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Franquias.Api.Controllers;

// So o Administrador pode ver e criar usuarios por aqui. Isso e diferente do
// POST /api/auth/registrar (que e publico e so cria Franqueado): esse endpoint
// e a unica forma de criar Gerente, Suporte ou outro Administrador no sistema.
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = PerfilNomes.Administrador)]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _service;

    public UsuariosController(IUsuarioService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<UsuarioResponseDto>>> ObterTodos([FromQuery] ParametrosPaginacao paginacao) =>
        Ok(await _service.ListarAsync(paginacao));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioResponseDto>> ObterPorId(int id) =>
        Ok(await _service.ObterPorIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<UsuarioResponseDto>> Criar(RegistrarUsuarioDto dto)
    {
        var usuario = await _service.RegistrarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = usuario.Id }, usuario);
    }
}
