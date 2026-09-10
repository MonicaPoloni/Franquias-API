using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Franquias.Api.Services;

public class FornecedorService : IFornecedorService
{
    private readonly IRepository<Fornecedor> _repository;

    public FornecedorService(IRepository<Fornecedor> repository)
    {
        _repository = repository;
    }

    public async Task<ResultadoPaginado<FornecedorResponseDto>> ListarAsync(ParametrosPaginacao? paginacao)
    {
        paginacao ??= new ParametrosPaginacao();

        var consulta = _repository.Consultar().OrderBy(f => f.RazaoSocial);
        var totalRegistros = await consulta.CountAsync();
        var fornecedores = await consulta
            .Skip((paginacao.Pagina - 1) * paginacao.TamanhoPagina)
            .Take(paginacao.TamanhoPagina)
            .ToListAsync();

        return new ResultadoPaginado<FornecedorResponseDto>
        {
            Itens = fornecedores.Select(MapearParaDto).ToList(),
            PaginaAtual = paginacao.Pagina,
            TamanhoPagina = paginacao.TamanhoPagina,
            TotalRegistros = totalRegistros
        };
    }

    public async Task<FornecedorResponseDto> ObterPorIdAsync(int id)
    {
        var fornecedor = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Fornecedor {id} não encontrado.");
        return MapearParaDto(fornecedor);
    }

    public async Task<FornecedorResponseDto> CriarAsync(FornecedorCreateDto dto)
    {
        await GarantirCnpjDisponivelAsync(dto.Cnpj);

        var fornecedor = new Fornecedor
        {
            RazaoSocial = dto.RazaoSocial,
            Cnpj = dto.Cnpj,
            Telefone = dto.Telefone,
            Email = dto.Email
        };
        await _repository.AdicionarAsync(fornecedor);
        await _repository.SalvarAsync();
        return MapearParaDto(fornecedor);
    }

    public async Task<FornecedorResponseDto> AtualizarAsync(int id, FornecedorCreateDto dto)
    {
        var fornecedor = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Fornecedor {id} não encontrado.");

        if (!string.Equals(fornecedor.Cnpj, dto.Cnpj, StringComparison.OrdinalIgnoreCase))
            await GarantirCnpjDisponivelAsync(dto.Cnpj);

        fornecedor.RazaoSocial = dto.RazaoSocial;
        fornecedor.Cnpj = dto.Cnpj;
        fornecedor.Telefone = dto.Telefone;
        fornecedor.Email = dto.Email;
        _repository.Atualizar(fornecedor);
        await _repository.SalvarAsync();
        return MapearParaDto(fornecedor);
    }

    public async Task RemoverAsync(int id)
    {
        var fornecedor = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Fornecedor {id} não encontrado.");

        _repository.Remover(fornecedor);
        await _repository.SalvarAsync();
    }

    private async Task GarantirCnpjDisponivelAsync(string cnpj)
    {
        var existentes = await _repository.ObterTodosAsync();
        if (existentes.Any(f => f.Cnpj == cnpj))
            throw new ArgumentException($"Já existe um fornecedor cadastrado com o CNPJ {cnpj}.");
    }

    private static FornecedorResponseDto MapearParaDto(Fornecedor fornecedor) => new()
    {
        Id = fornecedor.Id,
        RazaoSocial = fornecedor.RazaoSocial,
        Cnpj = fornecedor.Cnpj,
        Telefone = fornecedor.Telefone,
        Email = fornecedor.Email
    };
}
