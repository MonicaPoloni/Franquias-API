using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Franquias.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChamadosSuporteController : ControllerBase
{
    private readonly IChamadoSuporteService _service;

    public ChamadosSuporteController(IChamadoSuporteService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<ChamadoSuporteResponseDto>>> ObterTodos([FromQuery] ParametrosPaginacao paginacao) =>
        Ok(await _service.ListarAsync(paginacao));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ChamadoSuporteResponseDto>> ObterPorId(int id) =>
        Ok(await _service.ObterPorIdAsync(id));

    // Abrir chamado é liberado pra qualquer usuário logado (não tem [Authorize(Roles=...)]
    // aqui, só o [Authorize] geral da classe) - faz sentido, qualquer um da franquia
    // pode precisar pedir ajuda.
    [HttpPost]
    public async Task<ActionResult<ChamadoSuporteResponseDto>> Criar(ChamadoSuporteCreateDto dto)
    {
        var chamado = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = chamado.Id }, chamado);
    }

    // Já mudar o status (assumir, resolver, fechar) e apagar um chamado é
    // trabalho de quem atende o suporte (ou do Administrador).
    [Authorize(Roles = $"{PerfilNomes.Administrador},{PerfilNomes.Suporte}")]
    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<ChamadoSuporteResponseDto>> AtualizarStatus(int id, ChamadoSuporteAtualizarStatusDto dto) =>
        Ok(await _service.AtualizarStatusAsync(id, dto));

    [Authorize(Roles = $"{PerfilNomes.Administrador},{PerfilNomes.Suporte}")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        await _service.RemoverAsync(id);
        return NoContent();
    }
}
