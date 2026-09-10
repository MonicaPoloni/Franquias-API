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

    public async Task<ResultadoPaginado<ChamadoSuporteResponseDto>> ListarAsync(ParametrosPaginacao? paginacao)
    {
        paginacao ??= new ParametrosPaginacao();

        var consulta = ConsultaComIncludes().OrderByDescending(c => c.DataAbertura);
        var totalRegistros = await consulta.CountAsync();
        var chamados = await consulta
            .Skip((paginacao.Pagina - 1) * paginacao.TamanhoPagina)
            .Take(paginacao.TamanhoPagina)
            .ToListAsync();

        return new ResultadoPaginado<ChamadoSuporteResponseDto>
        {
            Itens = chamados.Select(MapearParaDto).ToList(),
            PaginaAtual = paginacao.Pagina,
            TamanhoPagina = paginacao.TamanhoPagina,
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
        Status = chamado.Status,
        Prioridade = chamado.Prioridade,
        DataAbertura = chamado.DataAbertura,
        DataFechamento = chamado.DataFechamento
    };
}
