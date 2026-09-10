using Franquias.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Franquias.Api.Configurations;

public class FranqueadoConfiguration : IEntityTypeConfiguration<Franqueado>
{
    public void Configure(EntityTypeBuilder<Franqueado> builder)
    {
        builder.ToTable("Franqueados");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Nome).IsRequired().HasMaxLength(120);
        builder.Property(f => f.Cpf).IsRequired().HasMaxLength(11);
        builder.Property(f => f.Email).HasMaxLength(160);
        builder.Property(f => f.Telefone).HasMaxLength(20);
        builder.HasIndex(f => f.Cpf).IsUnique();
        builder.HasIndex(f => f.UsuarioId).IsUnique();

        builder.HasOne(f => f.Usuario)
            .WithOne(u => u.Franqueado)
            .HasForeignKey<Franqueado>(f => f.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
