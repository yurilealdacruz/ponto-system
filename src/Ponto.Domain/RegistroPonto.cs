namespace Ponto.Domain;

public class RegistroPonto
{
    public int Id { get; set; }
    public DateTime DataHoraOficial { get; set; } // Horário pego pelo Servidor (Inviolável)
    public double Latitude { get; set; }  // GPS vindo do celular
    public double Longitude { get; set; } // GPS vindo do celular
    public bool GPSMockado { get; set; }  // Se o celular acusou "Localização Falsa"
    
    // Relação com o Funcionário
    public int FuncionarioId { get; set; }
    public Funcionario? Funcionario { get; set; }
}