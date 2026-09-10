using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Franquias.Api.Services;

public class CategoriaService : ICategoriaService
{
    private readonly IRepository<Categoria> _repository;

    public CategoriaService(IRepository<Categoria> repository)
    {
        _repository = repository;
    }

    public async Task<ResultadoPaginado<CategoriaResponseDto>> ListarAsync(ParametrosPaginacao? paginacao)
    {
        paginacao ??= new ParametrosPaginacao();

        var consulta = _repository.Consultar().OrderBy(c => c.Nome);
        var totalRegistros = await consulta.CountAsync();
        var categorias = await consulta
            .Skip((paginacao.Pagina - 1) * paginacao.TamanhoPagina)
            .Take(paginacao.TamanhoPagina)
            .ToListAsync();

        return new ResultadoPaginado<CategoriaResponseDto>
        {
            Itens = categorias.Select(MapearParaDto).ToList(),
            PaginaAtual = paginacao.Pagina,
            TamanhoPagina = paginacao.TamanhoPagina,
            TotalRegistros = totalRegistros
        };
    }

    public async Task<CategoriaResponseDto> ObterPorIdAsync(int id)
    {
        var categoria = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Categoria {id} não encontrada.");
        return MapearParaDto(categoria);
    }

    public async Task<CategoriaResponseDto> CriarAsync(CategoriaCreateDto dto)
    {
        var categoria = new Categoria { Nome = dto.Nome, Descricao = dto.Descricao };
        await _repository.AdicionarAsync(categoria);
        await _repository.SalvarAsync();
        return MapearParaDto(categoria);
    }

    public async Task<CategoriaResponseDto> AtualizarAsync(int id, CategoriaCreateDto dto)
    {
        var categoria = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Categoria {id} não encontrada.");

        categoria.Nome = dto.Nome;
        categoria.Descricao = dto.Descricao;
        _repository.Atualizar(categoria);
        await _repository.SalvarAsync();
        return MapearParaDto(categoria);
    }

    public async Task RemoverAsync(int id)
    {
        var categoria = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Categoria {id} não encontrada.");

        _repository.Remover(categoria);
        await _repository.SalvarAsync();
    }

    private static CategoriaResponseDto MapearParaDto(Categoria categoria) => new()
    {
        Id = categoria.Id,
        Nome = categoria.Nome,
        Descricao = categoria.Descricao
    };
}
