using Franquias.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Franquias.Api.Configurations;

public class UnidadeFranqueadaConfiguration : IEntityTypeConfiguration<UnidadeFranqueada>
{
    public void Configure(EntityTypeBuilder<UnidadeFranqueada> builder)
    {
        builder.ToTable("UnidadesFranqueadas");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Nome).IsRequired().HasMaxLength(160);
        builder.Property(u => u.Cnpj).IsRequired().HasMaxLength(14);
        builder.Property(u => u.Endereco).IsRequired().HasMaxLength(250);
        builder.Property(u => u.Cidade).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Estado).IsRequired().HasMaxLength(2);
        builder.Property(u => u.Telefone).HasMaxLength(20);
        builder.HasIndex(u => u.Cnpj).IsUnique();

        builder.HasOne(u => u.Franqueadora)
            .WithMany(f => f.UnidadesFranqueadas)
            .HasForeignKey(u => u.FranqueadoraId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Franqueado)
            .WithMany(f => f.UnidadesFranqueadas)
            .HasForeignKey(u => u.FranqueadoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
