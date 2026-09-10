using Franquias.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Franquias.Api.Configurations;

public class EstoqueConfiguration : IEntityTypeConfiguration<Estoque>
{
    public void Configure(EntityTypeBuilder<Estoque> builder)
    {
        builder.ToTable("Estoques");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.UnidadeFranqueadaId, e.ProdutoServicoId }).IsUnique();

        builder.HasOne(e => e.UnidadeFranqueada)
            .WithMany(u => u.Estoques)
            .HasForeignKey(e => e.UnidadeFranqueadaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ProdutoServico)
            .WithMany(p => p.Estoques)
            .HasForeignKey(e => e.ProdutoServicoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
