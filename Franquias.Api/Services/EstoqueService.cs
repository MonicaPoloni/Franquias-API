using Franquias.Api.Data;
using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Franquias.Api.Services;

public class EstoqueService : IEstoqueService
{
    private readonly IRepository<Estoque> _repository;
    private readonly IRepository<UnidadeFranqueada> _unidadeRepository;
    private readonly IRepository<ProdutoServico> _produtoRepository;
    private readonly ApplicationDbContext _contexto;

    public EstoqueService(
        IRepository<Estoque> repository,
        IRepository<UnidadeFranqueada> unidadeRepository,
        IRepository<ProdutoServico> produtoRepository,
        ApplicationDbContext contexto)
    {
        _repository = repository;
        _unidadeRepository = unidadeRepository;
        _produtoRepository = produtoRepository;
        _contexto = contexto;
    }

    public async Task<ResultadoPaginado<EstoqueResponseDto>> ListarAsync(ParametrosPaginacao? paginacao)
    {
        paginacao ??= new ParametrosPaginacao();

        var consulta = ConsultaComIncludes().OrderBy(e => e.Id);
        var totalRegistros = await consulta.CountAsync();
        var estoques = await consulta
            .Skip((paginacao.Pagina - 1) * paginacao.TamanhoPagina)
            .Take(paginacao.TamanhoPagina)
            .ToListAsync();

        return new ResultadoPaginado<EstoqueResponseDto>
        {
            Itens = estoques.Select(MapearParaDto).ToList(),
            PaginaAtual = paginacao.Pagina,
            TamanhoPagina = paginacao.TamanhoPagina,
            TotalRegistros = totalRegistros
        };
    }

    public async Task<EstoqueResponseDto> ObterPorIdAsync(int id)
    {
        var estoque = await ConsultaComIncludes().FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new KeyNotFoundException($"Estoque {id} não encontrado.");
        return MapearParaDto(estoque);
    }

    public async Task<EstoqueResponseDto> CriarAsync(EstoqueCreateDto dto)
    {
        var unidade = await _unidadeRepository.ObterPorIdAsync(dto.UnidadeFranqueadaId)
            ?? throw new ArgumentException($"Unidade franqueada {dto.UnidadeFranqueadaId} não existe.");
        var produto = await _produtoRepository.ObterPorIdAsync(dto.ProdutoServicoId)
            ?? throw new ArgumentException($"Produto/Serviço {dto.ProdutoServicoId} não existe.");

        var existentes = await _repository.ObterTodosAsync();
        if (existentes.Any(e => e.UnidadeFranqueadaId == dto.UnidadeFranqueadaId && e.ProdutoServicoId == dto.ProdutoServicoId))
            throw new ArgumentException("Já existe um registro de estoque para esse produto nessa unidade.");

        var estoque = new Estoque
        {
            UnidadeFranqueadaId = dto.UnidadeFranqueadaId,
            ProdutoServicoId = dto.ProdutoServicoId,
            QuantidadeAtual = dto.QuantidadeInicial,
            QuantidadeMinima = dto.QuantidadeMinima,
            UltimaAtualizacao = DateTime.UtcNow
        };
        await _repository.AdicionarAsync(estoque);
        await _repository.SalvarAsync();

        estoque.UnidadeFranqueada = unidade;
        estoque.ProdutoServico = produto;
        return MapearParaDto(estoque);
    }

    public async Task<EstoqueResponseDto> AtualizarAsync(int id, EstoqueUpdateDto dto)
    {
        var estoque = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Estoque {id} não encontrado.");

        estoque.QuantidadeMinima = dto.QuantidadeMinima;
        _repository.Atualizar(estoque);
        await _repository.SalvarAsync();
        return await ObterPorIdAsync(id);
    }

    public async Task RemoverAsync(int id)
    {
        var estoque = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Estoque {id} não encontrado.");

        _repository.Remover(estoque);
        await _repository.SalvarAsync();
    }

    private IQueryable<Estoque> ConsultaComIncludes() =>
        _contexto.Estoques
            .Include(e => e.UnidadeFranqueada)
            .Include(e => e.ProdutoServico)
            .AsNoTracking();

    private static EstoqueResponseDto MapearParaDto(Estoque estoque) => new()
    {
        Id = estoque.Id,
        UnidadeFranqueadaId = estoque.UnidadeFranqueadaId,
        UnidadeFranqueadaNome = estoque.UnidadeFranqueada.Nome,
        ProdutoServicoId = estoque.ProdutoServicoId,
        ProdutoNome = estoque.ProdutoServico.Nome,
        QuantidadeAtual = estoque.QuantidadeAtual,
        QuantidadeMinima = estoque.QuantidadeMinima,
        UltimaAtualizacao = estoque.UltimaAtualizacao
    };
}
