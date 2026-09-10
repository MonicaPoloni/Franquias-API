using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Franquias.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UnidadesFranqueadasController : ControllerBase
{
    private readonly IUnidadeFranqueadaService _service;

    public UnidadesFranqueadasController(IUnidadeFranqueadaService service)
    {
        _service = service;
    }

    // [FromQuery] pega os filtros direto da URL, tipo:
    // /api/unidadesfranqueadas?cidade=Sao Paulo&pagina=2&ordenarPor=cidade
    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<UnidadeFranqueadaResponseDto>>> ObterTodos(
        [FromQuery] UnidadeFranqueadaFiltroDto filtro) =>
        Ok(await _service.ListarAsync(filtro));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UnidadeFranqueadaResponseDto>> ObterPorId(int id) =>
        Ok(await _service.ObterPorIdAsync(id));

    // Abrir/editar/fechar uma unidade franqueada é decisão estrutural da rede -
    // só Administrador pode fazer isso.
    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpPost]
    public async Task<ActionResult<UnidadeFranqueadaResponseDto>> Criar(UnidadeFranqueadaCreateDto dto)
    {
        var unidade = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = unidade.Id }, unidade);
    }

    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<UnidadeFranqueadaResponseDto>> Atualizar(int id, UnidadeFranqueadaCreateDto dto) =>
        Ok(await _service.AtualizarAsync(id, dto));

    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        await _service.RemoverAsync(id);
        return NoContent();
    }
}
