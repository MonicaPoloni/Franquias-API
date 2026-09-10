using Franquias.Api.Data;
using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Franquias.Api.Services;

public class UnidadeFranqueadaService : IUnidadeFranqueadaService
{
    private readonly IRepository<UnidadeFranqueada> _repository;
    private readonly IRepository<Franqueadora> _franqueadoraRepository;
    private readonly IRepository<Franqueado> _franqueadoRepository;
    private readonly ApplicationDbContext _contexto;

    public UnidadeFranqueadaService(
        IRepository<UnidadeFranqueada> repository,
        IRepository<Franqueadora> franqueadoraRepository,
        IRepository<Franqueado> franqueadoRepository,
        ApplicationDbContext contexto)
    {
        _repository = repository;
        _franqueadoraRepository = franqueadoraRepository;
        _franqueadoRepository = franqueadoRepository;
        _contexto = contexto;
    }

    public async Task<ResultadoPaginado<UnidadeFranqueadaResponseDto>> ListarAsync(UnidadeFranqueadaFiltroDto? filtro)
    {
        filtro ??= new UnidadeFranqueadaFiltroDto();

        var consulta = _contexto.UnidadesFranqueadas
            .Include(u => u.Franqueadora)
            .Include(u => u.Franqueado)
            .AsNoTracking()
            .AsQueryable();

        // Cada filtro só entra na consulta se a pessoa realmente informou ele.
        // Usamos Contains (tipo um "LIKE %texto%") pra achar por nome parcial,
        // não só nome exato - é mais parecido com uma busca de verdade.
        if (!string.IsNullOrWhiteSpace(filtro.Nome))
            consulta = consulta.Where(u => u.Nome.Contains(filtro.Nome));
        if (!string.IsNullOrWhiteSpace(filtro.Cidade))
            consulta = consulta.Where(u => u.Cidade.Contains(filtro.Cidade));
        if (!string.IsNullOrWhiteSpace(filtro.Cnpj))
            consulta = consulta.Where(u => u.Cnpj.Contains(filtro.Cnpj));

        consulta = filtro.OrdenarPor?.ToLower() switch
        {
            "cidade" => consulta.OrderBy(u => u.Cidade),
            "cnpj" => consulta.OrderBy(u => u.Cnpj),
            _ => consulta.OrderBy(u => u.Nome)
        };

        var totalRegistros = await consulta.CountAsync();
        var unidades = await consulta
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .ToListAsync();

        return new ResultadoPaginado<UnidadeFranqueadaResponseDto>
        {
            Itens = unidades.Select(MapearParaDto).ToList(),
            PaginaAtual = filtro.Pagina,
            TamanhoPagina = filtro.TamanhoPagina,
            TotalRegistros = totalRegistros
        };
    }

    public async Task<UnidadeFranqueadaResponseDto> ObterPorIdAsync(int id)
    {
        var unidade = await _contexto.UnidadesFranqueadas
            .Include(u => u.Franqueadora)
            .Include(u => u.Franqueado)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new KeyNotFoundException($"Unidade franqueada {id} não encontrada.");
        return MapearParaDto(unidade);
    }

    public async Task<UnidadeFranqueadaResponseDto> CriarAsync(UnidadeFranqueadaCreateDto dto)
    {
        var franqueadora = await ObterFranqueadoraOuFalharAsync(dto.FranqueadoraId);
        var franqueado = await ObterFranqueadoOuFalharAsync(dto.FranqueadoId);

        var unidade = new UnidadeFranqueada
        {
            Nome = dto.Nome,
            Cnpj = dto.Cnpj,
            Endereco = dto.Endereco,
            Cidade = dto.Cidade,
            Estado = dto.Estado,
            Telefone = dto.Telefone,
            DataInauguracao = dto.DataInauguracao,
            FranqueadoraId = dto.FranqueadoraId,
            FranqueadoId = dto.FranqueadoId
        };
        await _repository.AdicionarAsync(unidade);
        await _repository.SalvarAsync();

        unidade.Franqueadora = franqueadora;
        unidade.Franqueado = franqueado;
        return MapearParaDto(unidade);
    }

    public async Task<UnidadeFranqueadaResponseDto> AtualizarAsync(int id, UnidadeFranqueadaCreateDto dto)
    {
        var unidade = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Unidade franqueada {id} não encontrada.");

        var franqueadora = await ObterFranqueadoraOuFalharAsync(dto.FranqueadoraId);
        var franqueado = await ObterFranqueadoOuFalharAsync(dto.FranqueadoId);

        unidade.Nome = dto.Nome;
        unidade.Cnpj = dto.Cnpj;
        unidade.Endereco = dto.Endereco;
        unidade.Cidade = dto.Cidade;
        unidade.Estado = dto.Estado;
        unidade.Telefone = dto.Telefone;
        unidade.DataInauguracao = dto.DataInauguracao;
        unidade.FranqueadoraId = dto.FranqueadoraId;
        unidade.FranqueadoId = dto.FranqueadoId;
        _repository.Atualizar(unidade);
        await _repository.SalvarAsync();

        unidade.Franqueadora = franqueadora;
        unidade.Franqueado = franqueado;
        return MapearParaDto(unidade);
    }

    public async Task RemoverAsync(int id)
    {
        var unidade = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Unidade franqueada {id} não encontrada.");

        _repository.Remover(unidade);
        await _repository.SalvarAsync();
    }

    private async Task<Franqueadora> ObterFranqueadoraOuFalharAsync(int id) =>
        await _franqueadoraRepository.ObterPorIdAsync(id)
            ?? throw new ArgumentException($"Franqueadora {id} não existe.");

    private async Task<Franqueado> ObterFranqueadoOuFalharAsync(int id) =>
        await _franqueadoRepository.ObterPorIdAsync(id)
            ?? throw new ArgumentException($"Franqueado {id} não existe.");

    private static UnidadeFranqueadaResponseDto MapearParaDto(UnidadeFranqueada unidade) => new()
    {
        Id = unidade.Id,
        Nome = unidade.Nome,
        Cnpj = unidade.Cnpj,
        Endereco = unidade.Endereco,
        Cidade = unidade.Cidade,
        Estado = unidade.Estado,
        Telefone = unidade.Telefone,
        DataInauguracao = unidade.DataInauguracao,
        FranqueadoraId = unidade.FranqueadoraId,
        FranqueadoraNome = unidade.Franqueadora.NomeFantasia,
        FranqueadoId = unidade.FranqueadoId,
        FranqueadoNome = unidade.Franqueado.Nome
    };
}
