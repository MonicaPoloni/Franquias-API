using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Franquias.Api.Services;

public class FranqueadoraService : IFranqueadoraService
{
    private readonly IRepository<Franqueadora> _repository;

    public FranqueadoraService(IRepository<Franqueadora> repository)
    {
        _repository = repository;
    }

    public async Task<ResultadoPaginado<FranqueadoraResponseDto>> ListarAsync(ParametrosPaginacao? paginacao)
    {
        paginacao ??= new ParametrosPaginacao();

        var consulta = _repository.Consultar().OrderBy(f => f.NomeFantasia);
        var totalRegistros = await consulta.CountAsync();
        var franqueadoras = await consulta
            .Skip((paginacao.Pagina - 1) * paginacao.TamanhoPagina)
            .Take(paginacao.TamanhoPagina)
            .ToListAsync();

        return new ResultadoPaginado<FranqueadoraResponseDto>
        {
            Itens = franqueadoras.Select(MapearParaDto).ToList(),
            PaginaAtual = paginacao.Pagina,
            TamanhoPagina = paginacao.TamanhoPagina,
            TotalRegistros = totalRegistros
        };
    }

    public async Task<FranqueadoraResponseDto> ObterPorIdAsync(int id)
    {
        var franqueadora = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Franqueadora {id} não encontrada.");
        return MapearParaDto(franqueadora);
    }

    public async Task<FranqueadoraResponseDto> CriarAsync(FranqueadoraCreateDto dto)
    {
        await GarantirCnpjDisponivelAsync(dto.Cnpj);

        var franqueadora = new Franqueadora
        {
            RazaoSocial = dto.RazaoSocial,
            NomeFantasia = dto.NomeFantasia,
            Cnpj = dto.Cnpj,
            Telefone = dto.Telefone,
            Email = dto.Email,
            DataFundacao = dto.DataFundacao
        };
        await _repository.AdicionarAsync(franqueadora);
        await _repository.SalvarAsync();
        return MapearParaDto(franqueadora);
    }

    public async Task<FranqueadoraResponseDto> AtualizarAsync(int id, FranqueadoraCreateDto dto)
    {
        var franqueadora = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Franqueadora {id} não encontrada.");

        if (!string.Equals(franqueadora.Cnpj, dto.Cnpj, StringComparison.OrdinalIgnoreCase))
            await GarantirCnpjDisponivelAsync(dto.Cnpj);

        franqueadora.RazaoSocial = dto.RazaoSocial;
        franqueadora.NomeFantasia = dto.NomeFantasia;
        franqueadora.Cnpj = dto.Cnpj;
        franqueadora.Telefone = dto.Telefone;
        franqueadora.Email = dto.Email;
        franqueadora.DataFundacao = dto.DataFundacao;
        _repository.Atualizar(franqueadora);
        await _repository.SalvarAsync();
        return MapearParaDto(franqueadora);
    }

    public async Task RemoverAsync(int id)
    {
        var franqueadora = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Franqueadora {id} não encontrada.");

        _repository.Remover(franqueadora);
        await _repository.SalvarAsync();
    }

    private async Task GarantirCnpjDisponivelAsync(string cnpj)
    {
        var existentes = await _repository.ObterTodosAsync();
        if (existentes.Any(f => f.Cnpj == cnpj))
            throw new ArgumentException($"Já existe uma franqueadora cadastrada com o CNPJ {cnpj}.");
    }

    private static FranqueadoraResponseDto MapearParaDto(Franqueadora franqueadora) => new()
    {
        Id = franqueadora.Id,
        RazaoSocial = franqueadora.RazaoSocial,
        NomeFantasia = franqueadora.NomeFantasia,
        Cnpj = franqueadora.Cnpj,
        Telefone = franqueadora.Telefone,
        Email = franqueadora.Email,
        DataFundacao = franqueadora.DataFundacao
    };
}
