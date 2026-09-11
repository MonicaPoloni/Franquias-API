using Franquias.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Franquias.Api.Configurations;

public class ChamadoSuporteConfiguration : IEntityTypeConfiguration<ChamadoSuporte>
{
    public void Configure(EntityTypeBuilder<ChamadoSuporte> builder)
    {
        builder.ToTable("ChamadosSuporte");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Titulo).IsRequired().HasMaxLength(160);
        builder.Property(c => c.Descricao).IsRequired().HasMaxLength(2000);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.Prioridade).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.Categoria).HasConversion<string>().HasMaxLength(20).HasDefaultValue(CategoriaChamadoSuporte.Outro);

        builder.HasOne(c => c.UnidadeFranqueada)
            .WithMany(u => u.ChamadosSuporte)
            .HasForeignKey(c => c.UnidadeFranqueadaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.UsuarioAbertura)
            .WithMany(u => u.ChamadosAbertos)
            .HasForeignKey(c => c.UsuarioAberturaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
