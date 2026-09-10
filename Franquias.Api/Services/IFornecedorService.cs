using Franquias.Api.DTOs;

namespace Franquias.Api.Services;

public interface IFornecedorService
{
    Task<ResultadoPaginado<FornecedorResponseDto>> ListarAsync(ParametrosPaginacao? paginacao);
    Task<FornecedorResponseDto> ObterPorIdAsync(int id);
    Task<FornecedorResponseDto> CriarAsync(FornecedorCreateDto dto);
    Task<FornecedorResponseDto> AtualizarAsync(int id, FornecedorCreateDto dto);
    Task RemoverAsync(int id);
}
