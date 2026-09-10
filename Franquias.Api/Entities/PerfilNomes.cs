namespace Franquias.Api.Entities;

// Esses nomes precisam ser EXATAMENTE iguais aos nomes gravados na tabela Perfis
// (ver o HasData em PerfilConfiguration.cs). Uso essa classe em vez de escrever
// a string direto em cada [Authorize(Roles = "...")], porque se eu digitar
// errado em um lugar só, o compilador não avisa (é só uma string) e a permissão
// simplesmente não funciona, sem erro nenhum. Assim, erro de digitação vira
// erro de compilação.
public static class PerfilNomes
{
    public const string Administrador = "Administrador";
    public const string Franqueado = "Franqueado";
    public const string Gerente = "Gerente";
    public const string Suporte = "Suporte";
}
