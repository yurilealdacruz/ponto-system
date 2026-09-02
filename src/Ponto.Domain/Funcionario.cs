namespace Ponto.Domain;

public class Funcionario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    
    // Relação com o Cargo
    public int CargoId { get; set; }
    public Cargo? Cargo { get; set; }
}