using Franquias.Api.Entities;

namespace Franquias.Api.Repositories;

/// <summary>
/// Além do CRUD genérico, o repositório de Usuario sabe buscar por e-mail
/// (necessário para o login) já trazendo o Perfil junto.
/// </summary>
public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task<bool> EmailJaExisteAsync(string email);
}
