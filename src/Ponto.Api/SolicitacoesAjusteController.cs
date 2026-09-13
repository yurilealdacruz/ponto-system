using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ponto.Data;
using Ponto.Domain;

namespace Ponto.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SolicitacoesAjusteController : ControllerBase
{
    private readonly PontoDbContext _context;

    public SolicitacoesAjusteController(PontoDbContext context)
    {
        _context = context;
    }

    // 1. COLABORADOR: Cria um pedido de ajuste
    [HttpPost]
    public async Task<IActionResult> CriarSolicitacao([FromBody] SolicitacaoAjusteDto dto)
    {
        var solicitacao = new SolicitacaoAjuste
        {
            FuncionarioId = dto.FuncionarioId,
            RegistroPontoId = dto.RegistroPontoId,
            DataHoraSugerida = dto.DataHoraSugerida.ToUniversalTime(),
            Justificativa = dto.Justificativa,
            Status = "Pendente",
            DataSolicitacao = DateTime.UtcNow
        };

        _context.SolicitacoesAjuste.Add(solicitacao);
        await _context.SaveChangesAsync();

        return Ok(new { mensagem = "Solicitação enviada para análise." });
    }

    // 2. ADMINISTRADOR: Lista todos os pedidos pendentes com CPF e Nome
    [HttpGet("pendentes")]
    public async Task<IActionResult> GetPendentes()
    {
        var pendentes = await _context.SolicitacoesAjuste
            .Include(s => s.Funcionario)
            .Where(s => s.Status == "Pendente")
            .Select(s => new 
            {
                s.Id,
                FuncionarioNome = s.Funcionario.Nome,
                FuncionarioCpf = s.Funcionario.Cpf,
                s.DataHoraSugerida,
                s.Justificativa,
                s.DataSolicitacao
            })
            .ToListAsync();

        return Ok(pendentes);
    }

    // 3. ADMINISTRADOR: Aprova o pedido e altera o relógio real
    [HttpPost("{id}/aprovar")]
    public async Task<IActionResult> AprovarSolicitacao(int id)
    {
        var solicitacao = await _context.SolicitacoesAjuste.FindAsync(id);
        if (solicitacao == null || solicitacao.Status != "Pendente")
            return BadRequest(new { mensagem = "Solicitação inválida ou já processada." });

        // Se o funcionário enviou o ID de um ponto que já existia, atualizamos ele
        if (solicitacao.RegistroPontoId.HasValue)
        {
            var pontoOriginal = await _context.RegistrosPonto.FindAsync(solicitacao.RegistroPontoId.Value);
            if (pontoOriginal != null)
            {
                pontoOriginal.DataHoraOficial = solicitacao.DataHoraSugerida;
            }
        }
        else 
        {
            // Se ele esqueceu de bater, criamos um ponto novo do zero
            var novoPonto = new RegistroPonto
            {
                FuncionarioId = solicitacao.FuncionarioId,
                DataHoraOficial = solicitacao.DataHoraSugerida,
                Latitude = 0,
                Longitude = 0,
                GPSMockado = false
            };
            _context.RegistrosPonto.Add(novoPonto);
        }

        solicitacao.Status = "Aprovado";
        await _context.SaveChangesAsync();

        return Ok(new { mensagem = "Ajuste aprovado e ponto atualizado com sucesso." });
    }
}

// Objeto para receber os dados do MAUI
public class SolicitacaoAjusteDto
{
    public int FuncionarioId { get; set; }
    public int? RegistroPontoId { get; set; }
    public DateTime DataHoraSugerida { get; set; }
    public string Justificativa { get; set; } = string.Empty;
}