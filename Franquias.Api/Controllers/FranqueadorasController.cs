using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Franquias.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FranqueadorasController : ControllerBase
{
    private readonly IFranqueadoraService _service;

    public FranqueadorasController(IFranqueadoraService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<FranqueadoraResponseDto>>> ObterTodos([FromQuery] ParametrosPaginacao paginacao) =>
        Ok(await _service.ListarAsync(paginacao));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FranqueadoraResponseDto>> ObterPorId(int id) =>
        Ok(await _service.ObterPorIdAsync(id));

    // Cadastro estrutural - só Administrador cria/edita/apaga franqueadora.
    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpPost]
    public async Task<ActionResult<FranqueadoraResponseDto>> Criar(FranqueadoraCreateDto dto)
    {
        var franqueadora = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = franqueadora.Id }, franqueadora);
    }

    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<FranqueadoraResponseDto>> Atualizar(int id, FranqueadoraCreateDto dto) =>
        Ok(await _service.AtualizarAsync(id, dto));

    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        await _service.RemoverAsync(id);
        return NoContent();
    }
}
