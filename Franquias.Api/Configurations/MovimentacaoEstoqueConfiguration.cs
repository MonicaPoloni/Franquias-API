using Franquias.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Franquias.Api.Configurations;

public class MovimentacaoEstoqueConfiguration : IEntityTypeConfiguration<MovimentacaoEstoque>
{
    public void Configure(EntityTypeBuilder<MovimentacaoEstoque> builder)
    {
        builder.ToTable("MovimentacoesEstoque");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Observacao).HasMaxLength(250);

        builder.HasOne(m => m.Estoque)
            .WithMany(e => e.Movimentacoes)
            .HasForeignKey(m => m.EstoqueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Fornecedor)
            .WithMany(f => f.Movimentacoes)
            .HasForeignKey(m => m.FornecedorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
