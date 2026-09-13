using System.ComponentModel.DataAnnotations;

namespace Ponto.Domain;

public class SolicitacaoAjuste
{
    public int Id { get; set; }
    
    public int FuncionarioId { get; set; }
    public Funcionario Funcionario { get; set; }

    // O ponto exato que o funcionário quer corrigir (opcional, caso seja apenas uma batida esquecida)
    public int? RegistroPontoId { get; set; }
    public RegistroPonto RegistroPonto { get; set; }

    public DateTime DataHoraSugerida { get; set; }
    
    [MaxLength(250)]
    public string Justificativa { get; set; } = string.Empty;
    
    // Status possíveis: "Pendente", "Aprovado", "Recusado"
    public string Status { get; set; } = "Pendente"; 
    
    public DateTime DataSolicitacao { get; set; } = DateTime.UtcNow;
}