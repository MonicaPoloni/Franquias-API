using Franquias.Api.Data;
using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Franquias.Api.Services;

public class CobrancaService : ICobrancaService
{
    private readonly IRepository<Cobranca> _repository;
    private readonly IRepository<UnidadeFranqueada> _unidadeRepository;
    private readonly ApplicationDbContext _contexto;

    public CobrancaService(
        IRepository<Cobranca> repository,
        IRepository<UnidadeFranqueada> unidadeRepository,
        ApplicationDbContext contexto)
    {
        _repository = repository;
        _unidadeRepository = unidadeRepository;
        _contexto = contexto;
    }

    public async Task<ResultadoPaginado<CobrancaResponseDto>> ListarAsync(ParametrosPaginacao? paginacao)
    {
        paginacao ??= new ParametrosPaginacao();

        // Ordena pela data de vencimento - assim quem consulta já vê primeiro
        // o que vence mais cedo (o que precisa de atenção mais urgente).
        var consulta = ConsultaComIncludes().OrderBy(c => c.DataVencimento);
        var totalRegistros = await consulta.CountAsync();
        var cobrancas = await consulta
            .Skip((paginacao.Pagina - 1) * paginacao.TamanhoPagina)
            .Take(paginacao.TamanhoPagina)
            .ToListAsync();

        return new ResultadoPaginado<CobrancaResponseDto>
        {
            Itens = cobrancas.Select(MapearParaDto).ToList(),
            PaginaAtual = paginacao.Pagina,
            TamanhoPagina = paginacao.TamanhoPagina,
            TotalRegistros = totalRegistros
        };
    }

    public async Task<CobrancaResponseDto> ObterPorIdAsync(int id)
    {
        var cobranca = await ConsultaComIncludes().FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Cobrança {id} não encontrada.");
        return MapearParaDto(cobranca);
    }

    public async Task<CobrancaResponseDto> CriarAsync(CobrancaCreateDto dto)
    {
        var unidade = await _unidadeRepository.ObterPorIdAsync(dto.UnidadeFranqueadaId)
            ?? throw new ArgumentException($"Unidade franqueada {dto.UnidadeFranqueadaId} não existe.");

        var existentes = await _repository.ObterTodosAsync();
        if (existentes.Any(c => c.UnidadeFranqueadaId == dto.UnidadeFranqueadaId && c.Competencia == dto.Competencia))
            throw new ArgumentException("Já existe uma cobrança para essa unidade nessa competência.");

        var cobranca = new Cobranca
        {
            UnidadeFranqueadaId = dto.UnidadeFranqueadaId,
            Competencia = dto.Competencia,
            FaturamentoBase = dto.FaturamentoBase,
            PercentualRoyalty = dto.PercentualRoyalty,
            ValorCobranca = CalcularValorCobranca(dto.FaturamentoBase, dto.PercentualRoyalty),
            DataVencimento = dto.DataVencimento,
            Status = StatusCobranca.Pendente
        };
        await _repository.AdicionarAsync(cobranca);
        await _repository.SalvarAsync();

        cobranca.UnidadeFranqueada = unidade;
        return MapearParaDto(cobranca);
    }

    public async Task<CobrancaResponseDto> AtualizarAsync(int id, CobrancaCreateDto dto)
    {
        var cobranca = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Cobrança {id} não encontrada.");

        if (cobranca.Status == StatusCobranca.Paga)
            throw new InvalidOperationException("Não é possível editar uma cobrança que já foi paga.");

        cobranca.FaturamentoBase = dto.FaturamentoBase;
        cobranca.PercentualRoyalty = dto.PercentualRoyalty;
        cobranca.ValorCobranca = CalcularValorCobranca(dto.FaturamentoBase, dto.PercentualRoyalty);
        cobranca.DataVencimento = dto.DataVencimento;
        _repository.Atualizar(cobranca);
        await _repository.SalvarAsync();

        return await ObterPorIdAsync(id);
    }

    public async Task<CobrancaResponseDto> MarcarComoPagaAsync(int id)
    {
        var cobranca = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Cobrança {id} não encontrada.");

        if (cobranca.Status == StatusCobranca.Paga)
            throw new InvalidOperationException("Essa cobrança já está paga.");

        cobranca.Status = StatusCobranca.Paga;
        cobranca.DataPagamento = DateTime.UtcNow;
        _repository.Atualizar(cobranca);
        await _repository.SalvarAsync();

        return await ObterPorIdAsync(id);
    }

    public async Task RemoverAsync(int id)
    {
        var cobranca = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Cobrança {id} não encontrada.");

        _repository.Remover(cobranca);
        await _repository.SalvarAsync();
    }

    private static decimal CalcularValorCobranca(decimal faturamentoBase, decimal percentualRoyalty) =>
        Math.Round(faturamentoBase * percentualRoyalty / 100m, 2);

    private IQueryable<Cobranca> ConsultaComIncludes() =>
        _contexto.Cobrancas.Include(c => c.UnidadeFranqueada).AsNoTracking();

    private static CobrancaResponseDto MapearParaDto(Cobranca cobranca) => new()
    {
        Id = cobranca.Id,
        UnidadeFranqueadaId = cobranca.UnidadeFranqueadaId,
        UnidadeFranqueadaNome = cobranca.UnidadeFranqueada.Nome,
        Competencia = cobranca.Competencia,
        FaturamentoBase = cobranca.FaturamentoBase,
        PercentualRoyalty = cobranca.PercentualRoyalty,
        ValorCobranca = cobranca.ValorCobranca,
        DataVencimento = cobranca.DataVencimento,
        DataPagamento = cobranca.DataPagamento,
        Status = cobranca.Status
    };
}
