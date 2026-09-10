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
        if (filtro.Ativo.HasValue)
            consulta = consulta.Where(u => u.Ativo == filtro.Ativo.Value);

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
        // O banco já tem um índice único de CNPJ (então uma segunda unidade com
        // o mesmo CNPJ nunca seria salva), mas sem essa checagem aqui na frente
        // o erro que voltaria pro cliente seria um 500 genérico do banco, em vez
        // de um 400 com mensagem clara - por isso validamos antes de tentar salvar.
        await GarantirCnpjDisponivelAsync(dto.Cnpj);

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

        // Só precisa checar o CNPJ de novo se ele realmente mudou - senão a
        // unidade ia "colidir com ela mesma" e nunca conseguiria ser editada.
        if (!string.Equals(unidade.Cnpj, dto.Cnpj, StringComparison.OrdinalIgnoreCase))
            await GarantirCnpjDisponivelAsync(dto.Cnpj);

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

    public async Task<UnidadeFranqueadaResponseDto> AtualizarStatusAsync(int id, bool ativo)
    {
        var unidade = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Unidade franqueada {id} não encontrada.");

        unidade.Ativo = ativo;
        _repository.Atualizar(unidade);
        await _repository.SalvarAsync();

        return await ObterPorIdAsync(id);
    }

    private async Task GarantirCnpjDisponivelAsync(string cnpj)
    {
        var existentes = await _repository.ObterTodosAsync();
        if (existentes.Any(u => u.Cnpj == cnpj))
            throw new ArgumentException($"Já existe uma unidade franqueada cadastrada com o CNPJ {cnpj}.");
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
        Ativo = unidade.Ativo,
        FranqueadoraId = unidade.FranqueadoraId,
        FranqueadoraNome = unidade.Franqueadora.NomeFantasia,
        FranqueadoId = unidade.FranqueadoId,
        FranqueadoNome = unidade.Franqueado.Nome
    };
}
