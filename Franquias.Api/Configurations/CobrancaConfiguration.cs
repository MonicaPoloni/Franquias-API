using Franquias.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Franquias.Api.Configurations;

public class CobrancaConfiguration : IEntityTypeConfiguration<Cobranca>
{
    public void Configure(EntityTypeBuilder<Cobranca> builder)
    {
        builder.ToTable("Cobrancas");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.FaturamentoBase).HasColumnType("decimal(18,2)");
        builder.Property(c => c.PercentualRoyalty).HasColumnType("decimal(5,2)");
        builder.Property(c => c.ValorCobranca).HasColumnType("decimal(18,2)");
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(c => new { c.UnidadeFranqueadaId, c.Competencia }).IsUnique();

        builder.HasOne(c => c.UnidadeFranqueada)
            .WithMany(u => u.Cobrancas)
            .HasForeignKey(c => c.UnidadeFranqueadaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
