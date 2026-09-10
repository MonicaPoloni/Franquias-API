using Franquias.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Franquias.Api.Configurations;

public class ItemVendaConfiguration : IEntityTypeConfiguration<ItemVenda>
{
    public void Configure(EntityTypeBuilder<ItemVenda> builder)
    {
        builder.ToTable("ItensVenda");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.PrecoUnitario).HasColumnType("decimal(18,2)");
        builder.Ignore(i => i.Subtotal);

        builder.HasOne(i => i.Venda)
            .WithMany(v => v.Itens)
            .HasForeignKey(i => i.VendaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.ProdutoServico)
            .WithMany(p => p.ItensVenda)
            .HasForeignKey(i => i.ProdutoServicoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
