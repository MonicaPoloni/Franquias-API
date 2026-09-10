using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Franquias.Api.Services;

public class PerfilService : IPerfilService
{
    private readonly IRepository<Perfil> _repository;

    public PerfilService(IRepository<Perfil> repository)
    {
        _repository = repository;
    }

    public async Task<ResultadoPaginado<PerfilResponseDto>> ListarAsync(ParametrosPaginacao? paginacao)
    {
        paginacao ??= new ParametrosPaginacao();

        var consulta = _repository.Consultar().OrderBy(p => p.Nome);
        var totalRegistros = await consulta.CountAsync();
        var perfis = await consulta
            .Skip((paginacao.Pagina - 1) * paginacao.TamanhoPagina)
            .Take(paginacao.TamanhoPagina)
            .ToListAsync();

        return new ResultadoPaginado<PerfilResponseDto>
        {
            Itens = perfis.Select(MapearParaDto).ToList(),
            PaginaAtual = paginacao.Pagina,
            TamanhoPagina = paginacao.TamanhoPagina,
            TotalRegistros = totalRegistros
        };
    }

    public async Task<PerfilResponseDto> ObterPorIdAsync(int id)
    {
        var perfil = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Perfil {id} não encontrado.");
        return MapearParaDto(perfil);
    }

    public async Task<PerfilResponseDto> CriarAsync(PerfilCreateDto dto)
    {
        var perfil = new Perfil { Nome = dto.Nome };
        await _repository.AdicionarAsync(perfil);
        await _repository.SalvarAsync();
        return MapearParaDto(perfil);
    }

    public async Task<PerfilResponseDto> AtualizarAsync(int id, PerfilCreateDto dto)
    {
        var perfil = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Perfil {id} não encontrado.");

        perfil.Nome = dto.Nome;
        _repository.Atualizar(perfil);
        await _repository.SalvarAsync();
        return MapearParaDto(perfil);
    }

    public async Task RemoverAsync(int id)
    {
        var perfil = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Perfil {id} não encontrado.");

        _repository.Remover(perfil);
        await _repository.SalvarAsync();
    }

    private static PerfilResponseDto MapearParaDto(Perfil perfil) => new()
    {
        Id = perfil.Id,
        Nome = perfil.Nome
    };
}
