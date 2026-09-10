namespace Franquias.Api.DTOs;

// Parâmetros que TODO GET de lista aceita: em que página estamos e quantos
// itens por página. Colocamos um limite máximo (100) pra impedir que alguém
// peça "tamanhoPagina=999999" e force o banco a devolver a tabela inteira de uma vez.
public class ParametrosPaginacao
{
    private const int TamanhoPaginaMaximo = 100;
    private int _tamanhoPagina = 10;

    public int Pagina { get; set; } = 1;

    public int TamanhoPagina
    {
        get => _tamanhoPagina;
        set => _tamanhoPagina = value > TamanhoPaginaMaximo ? TamanhoPaginaMaximo : value;
    }
}

// Envelope padrão devolvido por todo GET de lista: além dos itens da página
// atual, manda informações pra quem está consumindo a API montar uma
// navegação de páginas (1 de 5, próxima página, etc.) sem precisar adivinhar.
public class ResultadoPaginado<T>
{
    public List<T> Itens { get; set; } = new();
    public int PaginaAtual { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalRegistros { get; set; }
    public int TotalPaginas => TamanhoPagina == 0 ? 0 : (int)Math.Ceiling(TotalRegistros / (double)TamanhoPagina);
}
