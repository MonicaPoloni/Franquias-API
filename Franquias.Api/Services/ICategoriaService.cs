using Franquias.Api.DTOs;

namespace Franquias.Api.Services;

public interface ICategoriaService
{
    Task<ResultadoPaginado<CategoriaResponseDto>> ListarAsync(ParametrosPaginacao? paginacao);
    Task<CategoriaResponseDto> ObterPorIdAsync(int id);
    Task<CategoriaResponseDto> CriarAsync(CategoriaCreateDto dto);
    Task<CategoriaResponseDto> AtualizarAsync(int id, CategoriaCreateDto dto);
    Task RemoverAsync(int id);
}
