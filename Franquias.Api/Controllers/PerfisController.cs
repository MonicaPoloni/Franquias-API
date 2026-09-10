using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Franquias.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PerfisController : ControllerBase
{
    private readonly IPerfilService _service;

    public PerfisController(IPerfilService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<PerfilResponseDto>>> ObterTodos([FromQuery] ParametrosPaginacao paginacao) =>
        Ok(await _service.ListarAsync(paginacao));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PerfilResponseDto>> ObterPorId(int id) =>
        Ok(await _service.ObterPorIdAsync(id));

    // Criar/editar/apagar um Perfil mexe na estrutura de permissões do sistema
    // inteiro, então só o Administrador pode fazer isso. Consultar (GET acima)
    // continua liberado pra qualquer usuário logado.
    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpPost]
    public async Task<ActionResult<PerfilResponseDto>> Criar(PerfilCreateDto dto)
    {
        var perfil = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = perfil.Id }, perfil);
    }

    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<PerfilResponseDto>> Atualizar(int id, PerfilCreateDto dto) =>
        Ok(await _service.AtualizarAsync(id, dto));

    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        await _service.RemoverAsync(id);
        return NoContent();
    }
}
