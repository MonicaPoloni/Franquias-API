using Franquias.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Franquias.Api.Configurations;

public class VendaConfiguration : IEntityTypeConfiguration<Venda>
{
    public void Configure(EntityTypeBuilder<Venda> builder)
    {
        builder.ToTable("Vendas");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.ValorTotal).HasColumnType("decimal(18,2)");
        builder.Property(v => v.FormaPagamento).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(v => v.UnidadeFranqueada)
            .WithMany(u => u.Vendas)
            .HasForeignKey(v => v.UnidadeFranqueadaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Usuario)
            .WithMany(u => u.Vendas)
            .HasForeignKey(v => v.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
