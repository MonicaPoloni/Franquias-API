using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Franquias.Api.Services;

public class FranqueadoService : IFranqueadoService
{
    private readonly IRepository<Franqueado> _repository;
    private readonly IUsuarioRepository _usuarioRepository;

    public FranqueadoService(IRepository<Franqueado> repository, IUsuarioRepository usuarioRepository)
    {
        _repository = repository;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<ResultadoPaginado<FranqueadoResponseDto>> ListarAsync(ParametrosPaginacao? paginacao)
    {
        paginacao ??= new ParametrosPaginacao();

        var consulta = _repository.Consultar().OrderBy(f => f.Nome);
        var totalRegistros = await consulta.CountAsync();
        var franqueados = await consulta
            .Skip((paginacao.Pagina - 1) * paginacao.TamanhoPagina)
            .Take(paginacao.TamanhoPagina)
            .ToListAsync();

        return new ResultadoPaginado<FranqueadoResponseDto>
        {
            Itens = franqueados.Select(MapearParaDto).ToList(),
            PaginaAtual = paginacao.Pagina,
            TamanhoPagina = paginacao.TamanhoPagina,
            TotalRegistros = totalRegistros
        };
    }

    public async Task<FranqueadoResponseDto> ObterPorIdAsync(int id)
    {
        var franqueado = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Franqueado {id} não encontrado.");
        return MapearParaDto(franqueado);
    }

    public async Task<FranqueadoResponseDto> CriarAsync(FranqueadoCreateDto dto)
    {
        await GarantirCpfDisponivelAsync(dto.Cpf);
        if (dto.UsuarioId.HasValue)
            await GarantirUsuarioValidoAsync(dto.UsuarioId.Value);

        var franqueado = new Franqueado
        {
            Nome = dto.Nome,
            Cpf = dto.Cpf,
            Email = dto.Email,
            Telefone = dto.Telefone,
            UsuarioId = dto.UsuarioId
        };
        await _repository.AdicionarAsync(franqueado);
        await _repository.SalvarAsync();
        return MapearParaDto(franqueado);
    }

    public async Task<FranqueadoResponseDto> AtualizarAsync(int id, FranqueadoCreateDto dto)
    {
        var franqueado = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Franqueado {id} não encontrado.");

        if (!string.Equals(franqueado.Cpf, dto.Cpf, StringComparison.OrdinalIgnoreCase))
            await GarantirCpfDisponivelAsync(dto.Cpf);
        if (dto.UsuarioId.HasValue && dto.UsuarioId != franqueado.UsuarioId)
            await GarantirUsuarioValidoAsync(dto.UsuarioId.Value);

        franqueado.Nome = dto.Nome;
        franqueado.Cpf = dto.Cpf;
        franqueado.Email = dto.Email;
        franqueado.Telefone = dto.Telefone;
        franqueado.UsuarioId = dto.UsuarioId;
        _repository.Atualizar(franqueado);
        await _repository.SalvarAsync();
        return MapearParaDto(franqueado);
    }

    public async Task RemoverAsync(int id)
    {
        var franqueado = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Franqueado {id} não encontrado.");

        _repository.Remover(franqueado);
        await _repository.SalvarAsync();
    }

    private async Task GarantirCpfDisponivelAsync(string cpf)
    {
        var existentes = await _repository.ObterTodosAsync();
        if (existentes.Any(f => f.Cpf == cpf))
            throw new ArgumentException($"Já existe um franqueado cadastrado com o CPF {cpf}.");
    }

    private async Task GarantirUsuarioValidoAsync(int usuarioId)
    {
        _ = await _usuarioRepository.ObterPorIdAsync(usuarioId)
            ?? throw new ArgumentException($"Usuário {usuarioId} não existe.");

        var franqueados = await _repository.ObterTodosAsync();
        if (franqueados.Any(f => f.UsuarioId == usuarioId))
            throw new ArgumentException($"Usuário {usuarioId} já está vinculado a outro franqueado.");
    }

    private static FranqueadoResponseDto MapearParaDto(Franqueado franqueado) => new()
    {
        Id = franqueado.Id,
        Nome = franqueado.Nome,
        Cpf = franqueado.Cpf,
        Email = franqueado.Email,
        Telefone = franqueado.Telefone,
        UsuarioId = franqueado.UsuarioId
    };
}
