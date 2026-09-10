using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Franquias.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FranqueadosController : ControllerBase
{
    private readonly IFranqueadoService _service;

    public FranqueadosController(IFranqueadoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<FranqueadoResponseDto>>> ObterTodos([FromQuery] ParametrosPaginacao paginacao) =>
        Ok(await _service.ListarAsync(paginacao));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FranqueadoResponseDto>> ObterPorId(int id) =>
        Ok(await _service.ObterPorIdAsync(id));

    // Cadastro estrutural - só Administrador cria/edita/apaga franqueado.
    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpPost]
    public async Task<ActionResult<FranqueadoResponseDto>> Criar(FranqueadoCreateDto dto)
    {
        var franqueado = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = franqueado.Id }, franqueado);
    }

    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<FranqueadoResponseDto>> Atualizar(int id, FranqueadoCreateDto dto) =>
        Ok(await _service.AtualizarAsync(id, dto));

    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        await _service.RemoverAsync(id);
        return NoContent();
    }
}
