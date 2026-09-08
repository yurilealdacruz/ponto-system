namespace Ponto.Domain;

public class Funcionario
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public bool Ativo { get; set; }
    public int CargoId { get; set; }
    public Cargo? Cargo { get; set; }
    public string Cpf { get; set; }
    public string Senha { get; set; }
}