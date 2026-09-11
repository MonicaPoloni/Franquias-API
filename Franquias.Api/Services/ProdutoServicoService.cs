using Franquias.Api.Data;
using Franquias.Api.DTOs;
using Franquias.Api.Entities;
using Franquias.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Franquias.Api.Services;

public class ProdutoServicoService : IProdutoServicoService
{
    private readonly IRepository<ProdutoServico> _repository;
    private readonly IRepository<Categoria> _categoriaRepository;
    private readonly ApplicationDbContext _contexto;

    public ProdutoServicoService(
        IRepository<ProdutoServico> repository,
        IRepository<Categoria> categoriaRepository,
        ApplicationDbContext contexto)
    {
        _repository = repository;
        _categoriaRepository = categoriaRepository;
        _contexto = contexto;
    }

    public async Task<ResultadoPaginado<ProdutoServicoResponseDto>> ListarAsync(ProdutoServicoFiltroDto? filtro)
    {
        filtro ??= new ProdutoServicoFiltroDto();

        var consulta = _contexto.ProdutosServicos
            .Include(p => p.Categoria)
            .AsNoTracking()
            .AsQueryable();

        if (filtro.CategoriaId.HasValue)
            consulta = consulta.Where(p => p.CategoriaId == filtro.CategoriaId.Value);
        if (!string.IsNullOrWhiteSpace(filtro.Nome))
            consulta = consulta.Where(p => p.Nome.Contains(filtro.Nome));
        if (filtro.Ativo.HasValue)
            consulta = consulta.Where(p => p.Ativo == filtro.Ativo.Value);

        // O SQLite não sabe ordenar direto por uma coluna "decimal" (mesma
        // limitação do Sum() que já vimos nos relatórios). O truque aqui é
        // converter pra "double" só na hora de ordenar - o valor salvo no
        // banco continua decimal, isso não afeta o cálculo do preço.
        consulta = filtro.OrdenarPor?.ToLower() switch
        {
            "preco" => consulta.OrderBy(p => (double)p.Preco),
            _ => consulta.OrderBy(p => p.Nome)
        };

        var totalRegistros = await consulta.CountAsync();
        var produtos = await consulta
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .ToListAsync();

        return new ResultadoPaginado<ProdutoServicoResponseDto>
        {
            Itens = produtos.Select(MapearParaDto).ToList(),
            PaginaAtual = filtro.Pagina,
            TamanhoPagina = filtro.TamanhoPagina,
            TotalRegistros = totalRegistros
        };
    }

    public async Task<ProdutoServicoResponseDto> ObterPorIdAsync(int id)
    {
        var produto = await _contexto.ProdutosServicos
            .Include(p => p.Categoria)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Produto/Serviço {id} não encontrado.");
        return MapearParaDto(produto);
    }

    public async Task<ProdutoServicoResponseDto> CriarAsync(ProdutoServicoCreateDto dto)
    {
        var categoria = await ObterCategoriaOuFalharAsync(dto.CategoriaId);

        var produto = new ProdutoServico
        {
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            Preco = dto.Preco,
            Tipo = dto.Tipo,
            CategoriaId = dto.CategoriaId
        };
        await _repository.AdicionarAsync(produto);
        await _repository.SalvarAsync();

        produto.Categoria = categoria;
        return MapearParaDto(produto);
    }

    public async Task<ProdutoServicoResponseDto> AtualizarAsync(int id, ProdutoServicoCreateDto dto)
    {
        var produto = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Produto/Serviço {id} não encontrado.");

        var categoria = await ObterCategoriaOuFalharAsync(dto.CategoriaId);

        produto.Nome = dto.Nome;
        produto.Descricao = dto.Descricao;
        produto.Preco = dto.Preco;
        produto.Tipo = dto.Tipo;
        produto.CategoriaId = dto.CategoriaId;
        _repository.Atualizar(produto);
        await _repository.SalvarAsync();

        produto.Categoria = categoria;
        return MapearParaDto(produto);
    }

    public async Task RemoverAsync(int id)
    {
        var produto = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Produto/Serviço {id} não encontrado.");

        _repository.Remover(produto);
        await _repository.SalvarAsync();
    }

    public async Task<ProdutoServicoResponseDto> AtualizarStatusAsync(int id, bool ativo)
    {
        var produto = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Produto/Serviço {id} não encontrado.");

        produto.Ativo = ativo;
        _repository.Atualizar(produto);
        await _repository.SalvarAsync();

        return await ObterPorIdAsync(id);
    }

    private async Task<Categoria> ObterCategoriaOuFalharAsync(int id) =>
        await _categoriaRepository.ObterPorIdAsync(id)
            ?? throw new ArgumentException($"Categoria {id} não existe.");

    private static ProdutoServicoResponseDto MapearParaDto(ProdutoServico produto) => new()
    {
        Id = produto.Id,
        Nome = produto.Nome,
        Descricao = produto.Descricao,
        Preco = produto.Preco,
        Tipo = produto.Tipo,
        Ativo = produto.Ativo,
        CategoriaId = produto.CategoriaId,
        CategoriaNome = produto.Categoria.Nome
    };
}
