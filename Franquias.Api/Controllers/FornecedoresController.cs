using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Franquias.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FornecedoresController : ControllerBase
{
    private readonly IFornecedorService _service;

    public FornecedoresController(IFornecedorService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<FornecedorResponseDto>>> ObterTodos([FromQuery] ParametrosPaginacao paginacao) =>
        Ok(await _service.ListarAsync(paginacao));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FornecedorResponseDto>> ObterPorId(int id) =>
        Ok(await _service.ObterPorIdAsync(id));

    // Cadastro estrutural - só Administrador cria/edita/apaga fornecedor.
    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpPost]
    public async Task<ActionResult<FornecedorResponseDto>> Criar(FornecedorCreateDto dto)
    {
        var fornecedor = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = fornecedor.Id }, fornecedor);
    }

    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<FornecedorResponseDto>> Atualizar(int id, FornecedorCreateDto dto) =>
        Ok(await _service.AtualizarAsync(id, dto));

    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        await _service.RemoverAsync(id);
        return NoContent();
    }
}
