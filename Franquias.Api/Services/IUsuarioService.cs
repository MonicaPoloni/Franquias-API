using Franquias.Api.DTOs;

namespace Franquias.Api.Services;

public interface IUsuarioService
{
    Task<ResultadoPaginado<UsuarioResponseDto>> ListarAsync(ParametrosPaginacao? paginacao);
    Task<UsuarioResponseDto> ObterPorIdAsync(int id);
    Task<UsuarioResponseDto> RegistrarAsync(RegistrarUsuarioDto dto);
    Task<UsuarioResponseDto> CadastrarContaPublicaAsync(CadastroPublicoDto dto);
    Task<UsuarioResponseDto> AtualizarStatusAsync(int id, bool ativo);
}
