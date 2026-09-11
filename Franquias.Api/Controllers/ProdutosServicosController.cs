using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Franquias.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProdutosServicosController : ControllerBase
{
    private readonly IProdutoServicoService _service;

    public ProdutosServicosController(IProdutoServicoService service)
    {
        _service = service;
    }

    // Ex: /api/produtosservicos?categoriaId=2&ordenarPor=preco&pagina=1&tamanhoPagina=20
    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<ProdutoServicoResponseDto>>> ObterTodos(
        [FromQuery] ProdutoServicoFiltroDto filtro) =>
        Ok(await _service.ListarAsync(filtro));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProdutoServicoResponseDto>> ObterPorId(int id) =>
        Ok(await _service.ObterPorIdAsync(id));

    // Cadastro estrutural (afeta preço/catálogo pra rede toda) - só Administrador.
    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpPost]
    public async Task<ActionResult<ProdutoServicoResponseDto>> Criar(ProdutoServicoCreateDto dto)
    {
        var produto = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = produto.Id }, produto);
    }

    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProdutoServicoResponseDto>> Atualizar(int id, ProdutoServicoCreateDto dto) =>
        Ok(await _service.AtualizarAsync(id, dto));

    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        await _service.RemoverAsync(id);
        return NoContent();
    }

    [Authorize(Roles = PerfilNomes.Administrador)]
    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<ProdutoServicoResponseDto>> AtualizarStatus(int id, ProdutoServicoAtualizarStatusDto dto) =>
        Ok(await _service.AtualizarStatusAsync(id, dto.Ativo));
}
