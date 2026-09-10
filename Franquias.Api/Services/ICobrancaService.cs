using Franquias.Api.DTOs;

namespace Franquias.Api.Services;

public interface ICobrancaService
{
    Task<ResultadoPaginado<CobrancaResponseDto>> ListarAsync(ParametrosPaginacao? paginacao);
    Task<CobrancaResponseDto> ObterPorIdAsync(int id);
    Task<CobrancaResponseDto> CriarAsync(CobrancaCreateDto dto);
    Task<CobrancaResponseDto> AtualizarAsync(int id, CobrancaCreateDto dto);
    Task<CobrancaResponseDto> MarcarComoPagaAsync(int id);
    Task RemoverAsync(int id);
}
