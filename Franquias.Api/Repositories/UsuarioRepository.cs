using Franquias.Api.Data;
using Franquias.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Franquias.Api.Repositories;

public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(ApplicationDbContext contexto) : base(contexto)
    {
    }

    public async Task<Usuario?> ObterPorEmailAsync(string email) =>
        await DbSet.Include(u => u.Perfil)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

    public async Task<bool> EmailJaExisteAsync(string email) =>
        await DbSet.AnyAsync(u => u.Email.ToLower() == email.ToLower());
}
