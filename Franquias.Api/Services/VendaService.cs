using Franquias.Api.Data;
using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Franquias.Api.Services;

/// <summary>
/// Orquestra a criação de uma venda: valida tudo primeiro, calcula os valores,
/// dá baixa no estoque dos produtos vendidos e só então salva tudo de uma vez.
/// Usa o ApplicationDbContext diretamente porque essa operação mexe em várias
/// tabelas ao mesmo tempo (Venda, ItemVenda, Estoque, MovimentacaoEstoque).
/// </summary>
public class VendaService : IVendaService
{
    private readonly ApplicationDbContext _contexto;

    public VendaService(ApplicationDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<ResultadoPaginado<VendaResponseDto>> ListarAsync(VendaFiltroDto? filtro)
    {
        filtro ??= new VendaFiltroDto();

        var consulta = ConsultaComIncludes();

        if (filtro.UnidadeFranqueadaId.HasValue)
            consulta = consulta.Where(v => v.UnidadeFranqueadaId == filtro.UnidadeFranqueadaId.Value);
        if (filtro.DataInicio.HasValue)
            consulta = consulta.Where(v => v.DataVenda >= filtro.DataInicio.Value);
        if (filtro.DataFim.HasValue)
            consulta = consulta.Where(v => v.DataVenda <= filtro.DataFim.Value);

        // Vendas mais recentes primeiro - é o jeito mais natural de olhar
        // um histórico de vendas.
        consulta = consulta.OrderByDescending(v => v.DataVenda);

        var totalRegistros = await consulta.CountAsync();
        var vendas = await consulta
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .ToListAsync();

        return new ResultadoPaginado<VendaResponseDto>
        {
            Itens = vendas.Select(MapearParaDto).ToList(),
            PaginaAtual = filtro.Pagina,
            TamanhoPagina = filtro.TamanhoPagina,
            TotalRegistros = totalRegistros
        };
    }

    public async Task<VendaResponseDto> ObterPorIdAsync(int id)
    {
        var venda = await ConsultaComIncludes().FirstOrDefaultAsync(v => v.Id == id)
            ?? throw new KeyNotFoundException($"Venda {id} não encontrada.");
        return MapearParaDto(venda);
    }

    public async Task<VendaResponseDto> CriarAsync(VendaCreateDto dto)
    {
        var unidade = await _contexto.UnidadesFranqueadas.FindAsync(dto.UnidadeFranqueadaId)
            ?? throw new ArgumentException($"Unidade franqueada {dto.UnidadeFranqueadaId} não existe.");
        var usuario = await _contexto.Usuarios.FindAsync(dto.UsuarioId)
            ?? throw new ArgumentException($"Usuário {dto.UsuarioId} não existe.");

        var itensVenda = new List<ItemVenda>();
        var baixasDeEstoque = new List<(Estoque Estoque, int Quantidade)>();

        foreach (var itemDto in dto.Itens)
        {
            var produto = await _contexto.ProdutosServicos.FindAsync(itemDto.ProdutoServicoId)
                ?? throw new ArgumentException($"Produto/Serviço {itemDto.ProdutoServicoId} não existe.");

            itensVenda.Add(new ItemVenda
            {
                ProdutoServicoId = produto.Id,
                Quantidade = itemDto.Quantidade,
                PrecoUnitario = produto.Preco
            });

            // Serviços não têm estoque físico, então só produtos geram baixa.
            if (produto.Tipo == TipoProdutoServico.Produto)
            {
                var estoque = await _contexto.Estoques.FirstOrDefaultAsync(e =>
                    e.UnidadeFranqueadaId == dto.UnidadeFranqueadaId && e.ProdutoServicoId == produto.Id)
                    ?? throw new ArgumentException(
                        $"Produto '{produto.Nome}' não possui estoque cadastrado nessa unidade.");

                if (estoque.QuantidadeAtual < itemDto.Quantidade)
                    throw new ArgumentException(
                        $"Estoque insuficiente de '{produto.Nome}': disponível {estoque.QuantidadeAtual}, solicitado {itemDto.Quantidade}.");

                baixasDeEstoque.Add((estoque, itemDto.Quantidade));
            }
        }

        var venda = new Venda
        {
            UnidadeFranqueadaId = dto.UnidadeFranqueadaId,
            UsuarioId = dto.UsuarioId,
            FormaPagamento = dto.FormaPagamento,
            DataVenda = DateTime.UtcNow,
            ValorTotal = itensVenda.Sum(i => i.Quantidade * i.PrecoUnitario),
            Itens = itensVenda
        };
        await _contexto.Vendas.AddAsync(venda);

        foreach (var (estoque, quantidade) in baixasDeEstoque)
        {
            estoque.QuantidadeAtual -= quantidade;
            estoque.UltimaAtualizacao = DateTime.UtcNow;

            await _contexto.MovimentacoesEstoque.AddAsync(new MovimentacaoEstoque
            {
                EstoqueId = estoque.Id,
                Tipo = TipoMovimentacaoEstoque.Saida,
                Quantidade = quantidade,
                QuantidadeResultante = estoque.QuantidadeAtual,
                Data = DateTime.UtcNow,
                Observacao = "Baixa automática gerada por venda"
            });
        }

        // Tudo (venda + itens + baixas de estoque + movimentações) é gravado numa
        // única transação: ou tudo funciona, ou nada é salvo.
        await _contexto.SaveChangesAsync();

        venda.UnidadeFranqueada = unidade;
        venda.Usuario = usuario;
        foreach (var item in venda.Itens)
            item.ProdutoServico ??= (await _contexto.ProdutosServicos.FindAsync(item.ProdutoServicoId))!;

        return MapearParaDto(venda);
    }

    private IQueryable<Venda> ConsultaComIncludes() =>
        _contexto.Vendas
            .Include(v => v.UnidadeFranqueada)
            .Include(v => v.Usuario)
            .Include(v => v.Itens).ThenInclude(i => i.ProdutoServico)
            .AsNoTracking();

    private static VendaResponseDto MapearParaDto(Venda venda) => new()
    {
        Id = venda.Id,
        UnidadeFranqueadaId = venda.UnidadeFranqueadaId,
        UnidadeFranqueadaNome = venda.UnidadeFranqueada.Nome,
        UsuarioId = venda.UsuarioId,
        UsuarioNome = venda.Usuario.Nome,
        DataVenda = venda.DataVenda,
        ValorTotal = venda.ValorTotal,
        FormaPagamento = venda.FormaPagamento,
        Itens = venda.Itens.Select(i => new ItemVendaResponseDto
        {
            ProdutoServicoId = i.ProdutoServicoId,
            ProdutoNome = i.ProdutoServico.Nome,
            Quantidade = i.Quantidade,
            PrecoUnitario = i.PrecoUnitario,
            Subtotal = i.Subtotal
        }).ToList()
    };
}
