using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Franquias.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CobrancasController : ControllerBase
{
    private readonly ICobrancaService _service;

    public CobrancasController(ICobrancaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<CobrancaResponseDto>>> ObterTodos([FromQuery] ParametrosPaginacao paginacao) =>
        Ok(await _service.ListarAsync(paginacao));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CobrancaResponseDto>> ObterPorId(int id) =>
        Ok(await _service.ObterPorIdAsync(id));

    // Cobrança de royalty é área financeira sensível - só Administrador mexe aqui.
    // Franqueado/Gerente só consultam (GET), pra saber quanto devem, mas não
    // conseguem criar, editar ou "se dar baixa" sozinhos numa cobrança.
    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpPost]
    public async Task<ActionResult<CobrancaResponseDto>> Criar(CobrancaCreateDto dto)
    {
        var cobranca = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = cobranca.Id }, cobranca);
    }

    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CobrancaResponseDto>> Atualizar(int id, CobrancaCreateDto dto) =>
        Ok(await _service.AtualizarAsync(id, dto));

    /// <summary>Ação específica (não é o CRUD padrão) para dar baixa no pagamento.</summary>
    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpPost("{id:int}/pagar")]
    public async Task<ActionResult<CobrancaResponseDto>> Pagar(int id) =>
        Ok(await _service.MarcarComoPagaAsync(id));

    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        await _service.RemoverAsync(id);
        return NoContent();
    }
}
