using Franquias.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Franquias.Api.Configurations;

public class ProdutoServicoConfiguration : IEntityTypeConfiguration<ProdutoServico>
{
    public void Configure(EntityTypeBuilder<ProdutoServico> builder)
    {
        builder.ToTable("ProdutosServicos");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nome).IsRequired().HasMaxLength(160);
        builder.Property(p => p.Descricao).HasMaxLength(250);
        builder.Property(p => p.Preco).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Tipo).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(p => p.Categoria)
            .WithMany(c => c.Produtos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
