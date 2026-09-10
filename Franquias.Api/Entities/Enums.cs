namespace Franquias.Api.Entities;

public enum TipoProdutoServico
{
    Produto = 1,
    Servico = 2
}

public enum TipoMovimentacaoEstoque
{
    Entrada = 1,
    Saida = 2,
    Ajuste = 3
}

public enum FormaPagamento
{
    Dinheiro = 1,
    CartaoDebito = 2,
    CartaoCredito = 3,
    Pix = 4
}

public enum StatusCobranca
{
    Pendente = 1,
    Paga = 2,
    Atrasada = 3,
    Cancelada = 4
}

public enum StatusChamadoSuporte
{
    Aberto = 1,
    EmAndamento = 2,
    Resolvido = 3,
    Fechado = 4
}

public enum PrioridadeChamadoSuporte
{
    Baixa = 1,
    Media = 2,
    Alta = 3,
    Urgente = 4
}
