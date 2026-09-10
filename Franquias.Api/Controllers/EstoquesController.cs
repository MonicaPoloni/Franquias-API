using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Franquias.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EstoquesController : ControllerBase
{
    private readonly IEstoqueService _service;

    public EstoquesController(IEstoqueService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<EstoqueResponseDto>>> ObterTodos([FromQuery] ParametrosPaginacao paginacao) =>
        Ok(await _service.ListarAsync(paginacao));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EstoqueResponseDto>> ObterPorId(int id) =>
        Ok(await _service.ObterPorIdAsync(id));

    // Quem mexe no dia a dia da unidade (Administrador, Franqueado ou Gerente)
    // pode cadastrar/editar/apagar estoque. Suporte não mexe aqui.
    [Authorize(Roles = $"{PerfilNomes.Administrador},{PerfilNomes.Franqueado},{PerfilNomes.Gerente}")]
    [HttpPost]
    public async Task<ActionResult<EstoqueResponseDto>> Criar(EstoqueCreateDto dto)
    {
        var estoque = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = estoque.Id }, estoque);
    }

    [Authorize(Roles = $"{PerfilNomes.Administrador},{PerfilNomes.Franqueado},{PerfilNomes.Gerente}")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<EstoqueResponseDto>> Atualizar(int id, EstoqueUpdateDto dto) =>
        Ok(await _service.AtualizarAsync(id, dto));

    [Authorize(Roles = $"{PerfilNomes.Administrador},{PerfilNomes.Franqueado},{PerfilNomes.Gerente}")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        await _service.RemoverAsync(id);
        return NoContent();
    }
}
