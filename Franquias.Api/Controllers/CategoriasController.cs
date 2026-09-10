using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Franquias.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaService _service;

    public CategoriasController(ICategoriaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<CategoriaResponseDto>>> ObterTodos([FromQuery] ParametrosPaginacao paginacao) =>
        Ok(await _service.ListarAsync(paginacao));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoriaResponseDto>> ObterPorId(int id) =>
        Ok(await _service.ObterPorIdAsync(id));

    // Cadastro estrutural (usado por todo o sistema) - só Administrador cria/edita/apaga.
    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpPost]
    public async Task<ActionResult<CategoriaResponseDto>> Criar(CategoriaCreateDto dto)
    {
        var categoria = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = categoria.Id }, categoria);
    }

    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoriaResponseDto>> Atualizar(int id, CategoriaCreateDto dto) =>
        Ok(await _service.AtualizarAsync(id, dto));

    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        await _service.RemoverAsync(id);
        return NoContent();
    }
}
