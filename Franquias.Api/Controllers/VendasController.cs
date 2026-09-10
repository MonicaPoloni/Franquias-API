using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Franquias.Api.Controllers;

/// <summary>Só GET e POST: uma venda concluída não deve ser editada ou apagada.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VendasController : ControllerBase
{
    private readonly IVendaService _service;

    public VendasController(IVendaService service)
    {
        _service = service;
    }

    // Ex: /api/vendas?dataInicio=2026-09-01&dataFim=2026-09-30&unidadeFranqueadaId=1
    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<VendaResponseDto>>> ObterTodos([FromQuery] VendaFiltroDto filtro) =>
        Ok(await _service.ListarAsync(filtro));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VendaResponseDto>> ObterPorId(int id) =>
        Ok(await _service.ObterPorIdAsync(id));

    // Quem registra uma venda é quem trabalha na unidade no dia a dia.
    [Authorize(Roles = $"{PerfilNomes.Administrador},{PerfilNomes.Franqueado},{PerfilNomes.Gerente}")]
    [HttpPost]
    public async Task<ActionResult<VendaResponseDto>> Criar(VendaCreateDto dto)
    {
        var venda = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = venda.Id }, venda);
    }
}
