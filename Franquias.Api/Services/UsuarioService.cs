using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Franquias.Api.Services;

/// <summary>
/// Regras de negócio para cadastro e consulta de usuários.
/// </summary>
public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRepository<Perfil> _perfilRepository;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public UsuarioService(
        IUsuarioRepository usuarioRepository,
        IRepository<Perfil> perfilRepository,
        IPasswordHasher<Usuario> passwordHasher)
    {
        _usuarioRepository = usuarioRepository;
        _perfilRepository = perfilRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<ResultadoPaginado<UsuarioResponseDto>> ListarAsync(ParametrosPaginacao? paginacao)
    {
        paginacao ??= new ParametrosPaginacao();

        var consulta = _usuarioRepository.Consultar().Include(u => u.Perfil).OrderBy(u => u.Nome);
        var totalRegistros = await consulta.CountAsync();
        var usuarios = await consulta
            .Skip((paginacao.Pagina - 1) * paginacao.TamanhoPagina)
            .Take(paginacao.TamanhoPagina)
            .ToListAsync();

        return new ResultadoPaginado<UsuarioResponseDto>
        {
            Itens = usuarios.Select(MapearParaDto).ToList(),
            PaginaAtual = paginacao.Pagina,
            TamanhoPagina = paginacao.TamanhoPagina,
            TotalRegistros = totalRegistros
        };
    }

    public async Task<UsuarioResponseDto> ObterPorIdAsync(int id)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Usuário {id} não encontrado.");
        return MapearParaDto(usuario);
    }

    public async Task<UsuarioResponseDto> RegistrarAsync(RegistrarUsuarioDto dto)
    {
        if (await _usuarioRepository.EmailJaExisteAsync(dto.Email))
            throw new ArgumentException("Já existe um usuário cadastrado com esse e-mail.");

        var perfil = await _perfilRepository.ObterPorIdAsync(dto.PerfilId)
            ?? throw new ArgumentException($"Perfil {dto.PerfilId} não existe.");

        var usuario = new Usuario
        {
            Nome = dto.Nome,
            Email = dto.Email,
            PerfilId = dto.PerfilId,
            Perfil = perfil
        };
        usuario.SenhaHash = _passwordHasher.HashPassword(usuario, dto.Senha);

        await _usuarioRepository.AdicionarAsync(usuario);
        await _usuarioRepository.SalvarAsync();

        return MapearParaDto(usuario);
    }

    public async Task<UsuarioResponseDto> CadastrarContaPublicaAsync(CadastroPublicoDto dto)
    {
        // Cadastro público (sem estar logado) sempre cria conta como Franqueado.
        // Se deixássemos o cliente escolher o perfil aqui, seria fácil qualquer
        // pessoa se cadastrar como Administrador e ter acesso total ao sistema.
        var perfis = await _perfilRepository.ObterTodosAsync();
        var perfilFranqueado = perfis.FirstOrDefault(p => p.Nome == PerfilNomes.Franqueado)
            ?? throw new InvalidOperationException("Perfil 'Franqueado' não está cadastrado no banco.");

        var dtoComPerfilFixo = new RegistrarUsuarioDto
        {
            Nome = dto.Nome,
            Email = dto.Email,
            Senha = dto.Senha,
            PerfilId = perfilFranqueado.Id
        };
        return await RegistrarAsync(dtoComPerfilFixo);
    }

    private static UsuarioResponseDto MapearParaDto(Usuario usuario) => new()
    {
        Id = usuario.Id,
        Nome = usuario.Nome,
        Email = usuario.Email,
        Perfil = usuario.Perfil.Nome
    };
}
