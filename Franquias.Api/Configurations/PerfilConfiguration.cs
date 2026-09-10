using Franquias.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Franquias.Api.Configurations;

public class PerfilConfiguration : IEntityTypeConfiguration<Perfil>
{
    public void Configure(EntityTypeBuilder<Perfil> builder)
    {
        builder.ToTable("Perfis");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nome).IsRequired().HasMaxLength(50);
        builder.HasIndex(p => p.Nome).IsUnique();

        builder.HasData(
            new Perfil { Id = 1, Nome = "Administrador" },
            new Perfil { Id = 2, Nome = "Franqueado" },
            new Perfil { Id = 3, Nome = "Gerente" },
            new Perfil { Id = 4, Nome = "Suporte" }
        );
    }
}
