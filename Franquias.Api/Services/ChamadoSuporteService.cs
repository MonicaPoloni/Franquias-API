using Franquias.Api.Data;
using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Franquias.Api.Services;

public class ChamadoSuporteService : IChamadoSuporteService
{
    private readonly IRepository<ChamadoSuporte> _repository;
    private readonly IRepository<UnidadeFranqueada> _unidadeRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ApplicationDbContext _contexto;

    public ChamadoSuporteService(
        IRepository<ChamadoSuporte> repository,
        IRepository<UnidadeFranqueada> unidadeRepository,
        IUsuarioRepository usuarioRepository,
        ApplicationDbContext contexto)
    {
        _repository = repository;
        _unidadeRepository = unidadeRepository;
        _usuarioRepository = usuarioRepository;
        _contexto = contexto;
    }

    public async Task<ResultadoPaginado<ChamadoSuporteResponseDto>> ListarAsync(ChamadoSuporteFiltroDto? filtro)
    {
        filtro ??= new ChamadoSuporteFiltroDto();

        var consulta = ConsultaComIncludes();
        if (filtro.Status.HasValue)
            consulta = consulta.Where(c => c.Status == filtro.Status.Value);
        if (filtro.Prioridade.HasValue)
            consulta = consulta.Where(c => c.Prioridade == filtro.Prioridade.Value);
        if (filtro.UnidadeFranqueadaId.HasValue)
            consulta = consulta.Where(c => c.UnidadeFranqueadaId == filtro.UnidadeFranqueadaId.Value);

        consulta = consulta.OrderByDescending(c => c.DataAbertura);
        var totalRegistros = await consulta.CountAsync();
        var chamados = await consulta
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .ToListAsync();

        return new ResultadoPaginado<ChamadoSuporteResponseDto>
        {
            Itens = chamados.Select(MapearParaDto).ToList(),
            PaginaAtual = filtro.Pagina,
            TamanhoPagina = filtro.TamanhoPagina,
            TotalRegistros = totalRegistros
        };
    }

    public async Task<ChamadoSuporteResponseDto> ObterPorIdAsync(int id)
    {
        var chamado = await ConsultaComIncludes().FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Chamado {id} não encontrado.");
        return MapearParaDto(chamado);
    }

    public async Task<ChamadoSuporteResponseDto> CriarAsync(ChamadoSuporteCreateDto dto)
    {
        var unidade = await _unidadeRepository.ObterPorIdAsync(dto.UnidadeFranqueadaId)
            ?? throw new ArgumentException($"Unidade franqueada {dto.UnidadeFranqueadaId} não existe.");
        var usuario = await _usuarioRepository.ObterPorIdAsync(dto.UsuarioAberturaId)
            ?? throw new ArgumentException($"Usuário {dto.UsuarioAberturaId} não existe.");

        var chamado = new ChamadoSuporte
        {
            UnidadeFranqueadaId = dto.UnidadeFranqueadaId,
            UsuarioAberturaId = dto.UsuarioAberturaId,
            Titulo = dto.Titulo,
            Descricao = dto.Descricao,
            Prioridade = dto.Prioridade,
            Categoria = dto.Categoria,
            Status = StatusChamadoSuporte.Aberto,
            DataAbertura = DateTime.UtcNow
        };
        await _repository.AdicionarAsync(chamado);
        await _repository.SalvarAsync();

        chamado.UnidadeFranqueada = unidade;
        chamado.UsuarioAbertura = usuario;
        return MapearParaDto(chamado);
    }

    public async Task<ChamadoSuporteResponseDto> AtualizarStatusAsync(int id, ChamadoSuporteAtualizarStatusDto dto)
    {
        var chamado = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Chamado {id} não encontrado.");

        var estaEncerrando = dto.Status is StatusChamadoSuporte.Resolvido or StatusChamadoSuporte.Fechado;
        chamado.Status = dto.Status;
        chamado.DataFechamento = estaEncerrando ? (chamado.DataFechamento ?? DateTime.UtcNow) : null;

        _repository.Atualizar(chamado);
        await _repository.SalvarAsync();

        return await ObterPorIdAsync(id);
    }

    public async Task RemoverAsync(int id)
    {
        var chamado = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Chamado {id} não encontrado.");

        _repository.Remover(chamado);
        await _repository.SalvarAsync();
    }

    private IQueryable<ChamadoSuporte> ConsultaComIncludes() =>
        _contexto.ChamadosSuporte
            .Include(c => c.UnidadeFranqueada)
            .Include(c => c.UsuarioAbertura)
            .AsNoTracking();

    private static ChamadoSuporteResponseDto MapearParaDto(ChamadoSuporte chamado) => new()
    {
        Id = chamado.Id,
        UnidadeFranqueadaId = chamado.UnidadeFranqueadaId,
        UnidadeFranqueadaNome = chamado.UnidadeFranqueada.Nome,
        UsuarioAberturaId = chamado.UsuarioAberturaId,
        UsuarioAberturaNome = chamado.UsuarioAbertura.Nome,
        Titulo = chamado.Titulo,
        Descricao = chamado.Descricao,
        Categoria = chamado.Categoria,
        Status = chamado.Status,
        Prioridade = chamado.Prioridade,
        DataAbertura = chamado.DataAbertura,
        DataFechamento = chamado.DataFechamento
    };
}
