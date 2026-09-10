using Franquias.Api.Data;
using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Franquias.Api.Services;

public class MovimentacaoEstoqueService : IMovimentacaoEstoqueService
{
    private readonly IRepository<MovimentacaoEstoque> _repository;
    private readonly IRepository<Estoque> _estoqueRepository;
    private readonly IRepository<Fornecedor> _fornecedorRepository;
    private readonly ApplicationDbContext _contexto;

    public MovimentacaoEstoqueService(
        IRepository<MovimentacaoEstoque> repository,
        IRepository<Estoque> estoqueRepository,
        IRepository<Fornecedor> fornecedorRepository,
        ApplicationDbContext contexto)
    {
        _repository = repository;
        _estoqueRepository = estoqueRepository;
        _fornecedorRepository = fornecedorRepository;
        _contexto = contexto;
    }

    public async Task<ResultadoPaginado<MovimentacaoEstoqueResponseDto>> ListarAsync(MovimentacaoEstoqueFiltroDto? filtro)
    {
        filtro ??= new MovimentacaoEstoqueFiltroDto();

        var consulta = ConsultaComIncludes();
        if (filtro.EstoqueId.HasValue)
            consulta = consulta.Where(m => m.EstoqueId == filtro.EstoqueId.Value);

        // Movimentações mais recentes primeiro, tipo um extrato.
        consulta = consulta.OrderByDescending(m => m.Data);

        var totalRegistros = await consulta.CountAsync();
        var movimentacoes = await consulta
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .ToListAsync();

        return new ResultadoPaginado<MovimentacaoEstoqueResponseDto>
        {
            Itens = movimentacoes.Select(MapearParaDto).ToList(),
            PaginaAtual = filtro.Pagina,
            TamanhoPagina = filtro.TamanhoPagina,
            TotalRegistros = totalRegistros
        };
    }

    public async Task<MovimentacaoEstoqueResponseDto> ObterPorIdAsync(int id)
    {
        var movimentacao = await ConsultaComIncludes().FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Movimentação {id} não encontrada.");
        return MapearParaDto(movimentacao);
    }

    public async Task<MovimentacaoEstoqueResponseDto> CriarAsync(MovimentacaoEstoqueCreateDto dto)
    {
        var estoque = await _estoqueRepository.ObterPorIdAsync(dto.EstoqueId)
            ?? throw new ArgumentException($"Estoque {dto.EstoqueId} não existe.");

        if (dto.FornecedorId.HasValue)
        {
            _ = await _fornecedorRepository.ObterPorIdAsync(dto.FornecedorId.Value)
                ?? throw new ArgumentException($"Fornecedor {dto.FornecedorId} não existe.");
        }

        switch (dto.Tipo)
        {
            case TipoMovimentacaoEstoque.Entrada:
                estoque.QuantidadeAtual += dto.Quantidade;
                break;
            case TipoMovimentacaoEstoque.Saida:
                if (estoque.QuantidadeAtual < dto.Quantidade)
                    throw new ArgumentException(
                        $"Estoque insuficiente: disponível {estoque.QuantidadeAtual}, solicitado {dto.Quantidade}.");
                estoque.QuantidadeAtual -= dto.Quantidade;
                break;
            case TipoMovimentacaoEstoque.Ajuste:
                estoque.QuantidadeAtual = dto.Quantidade;
                break;
        }
        estoque.UltimaAtualizacao = DateTime.UtcNow;
        _estoqueRepository.Atualizar(estoque);

        var movimentacao = new MovimentacaoEstoque
        {
            EstoqueId = dto.EstoqueId,
            Tipo = dto.Tipo,
            Quantidade = dto.Quantidade,
            QuantidadeResultante = estoque.QuantidadeAtual,
            Observacao = dto.Observacao,
            FornecedorId = dto.FornecedorId,
            Data = DateTime.UtcNow
        };
        await _repository.AdicionarAsync(movimentacao);

        // Uma única chamada de SaveChangesAsync grava a movimentação E a atualização do
        // estoque juntas, como uma coisa só: se algo falhar, nada fica salvo pela metade.
        await _repository.SalvarAsync();

        // Recarrega com os relacionamentos (Estoque -> ProdutoServico, Fornecedor)
        // já incluídos, para montar a resposta sem erro de referência nula.
        return await ObterPorIdAsync(movimentacao.Id);
    }

    private IQueryable<MovimentacaoEstoque> ConsultaComIncludes() =>
        _contexto.MovimentacoesEstoque
            .Include(m => m.Estoque).ThenInclude(e => e.ProdutoServico)
            .Include(m => m.Fornecedor)
            .AsNoTracking();

    private static MovimentacaoEstoqueResponseDto MapearParaDto(MovimentacaoEstoque movimentacao) => new()
    {
        Id = movimentacao.Id,
        EstoqueId = movimentacao.EstoqueId,
        ProdutoNome = movimentacao.Estoque.ProdutoServico.Nome,
        Tipo = movimentacao.Tipo,
        Quantidade = movimentacao.Quantidade,
        Data = movimentacao.Data,
        Observacao = movimentacao.Observacao,
        FornecedorId = movimentacao.FornecedorId,
        FornecedorNome = movimentacao.Fornecedor?.RazaoSocial,
        QuantidadeAtualAposMovimentacao = movimentacao.QuantidadeResultante
    };
}
