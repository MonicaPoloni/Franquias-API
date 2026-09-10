using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Franquias.Api.Controllers;

/// <summary>
/// Só expõe GET e POST: movimentações de estoque são um histórico/auditoria,
/// então não faz sentido editar ou apagar uma que já aconteceu.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MovimentacoesEstoqueController : ControllerBase
{
    private readonly IMovimentacaoEstoqueService _service;

    public MovimentacoesEstoqueController(IMovimentacaoEstoqueService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<MovimentacaoEstoqueResponseDto>>> ObterTodos(
        [FromQuery] MovimentacaoEstoqueFiltroDto filtro) =>
        Ok(await _service.ListarAsync(filtro));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MovimentacaoEstoqueResponseDto>> ObterPorId(int id) =>
        Ok(await _service.ObterPorIdAsync(id));

    // Mesma regra do Estoque: quem toca o dia a dia da unidade pode lançar movimentação.
    [Authorize(Roles = $"{PerfilNomes.Administrador},{PerfilNomes.Franqueado},{PerfilNomes.Gerente}")]
    [HttpPost]
    public async Task<ActionResult<MovimentacaoEstoqueResponseDto>> Criar(MovimentacaoEstoqueCreateDto dto)
    {
        var movimentacao = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = movimentacao.Id }, movimentacao);
    }
}
