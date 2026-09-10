using Franquias.Api.Entities;

namespace Franquias.Api.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiraEm) GerarToken(Usuario usuario);
}
