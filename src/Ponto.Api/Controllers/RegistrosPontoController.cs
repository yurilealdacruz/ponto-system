using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ponto.Data;
using Ponto.Domain;

namespace Ponto.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrosPontoController : ControllerBase
{
    private readonly PontoDbContext _context;

    public RegistrosPontoController(PontoDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> BaterPonto([FromBody] RegistroPontoDto dto)
    {
        // 1. Valida se o funcionário existe e está ativo
        var funcionario = await _context.Funcionarios.FindAsync(dto.FuncionarioId);
        if (funcionario == null || !funcionario.Ativo)
        {
            return BadRequest(new { mensagem = "Funcionário não encontrado ou inativo." });
        }

        // Busca o último ponto batido por este funcionário
        var ultimoPonto = await _context.RegistrosPonto
            .Where(r => r.FuncionarioId == dto.FuncionarioId)
            .OrderByDescending(r => r.DataHoraOficial)
            .FirstOrDefaultAsync();

        // Se houver um ponto anterior, verifica a diferença de tempo
        if (ultimoPonto != null)
        {
            var tempoPassado = DateTime.UtcNow - ultimoPonto.DataHoraOficial;
            if (tempoPassado.TotalMinutes < 5) // Trava de 5 minutos
            {
                return BadRequest(new { mensagem = "Você já bateu o ponto recentemente. Aguarde alguns minutos." });
            }
        }

        // ==========================================================
        // NOVO: TRAVA DE 4 PONTOS POR DIA
        // ==========================================================
        var dataHoje = DateTime.UtcNow.Date;

        int totalPontosHoje = await _context.RegistrosPonto
            .Where(r => r.FuncionarioId == dto.FuncionarioId
                     && r.DataHoraOficial.Date == dataHoje)
            .CountAsync();

        if (totalPontosHoje >= 4)
        {
            return BadRequest(new { mensagem = "Limite atingido: Você já registrou 4 pontos no dia de hoje." });
        }
        // ==========================================================

        // 2. Segurança: A data e hora são geradas pelo servidor, nunca pelo celular do usuário
        var novoPonto = new RegistroPonto
        {
            FuncionarioId = dto.FuncionarioId,
            DataHoraOficial = DateTime.UtcNow,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            GPSMockado = dto.GPSMockado
        };

        _context.RegistrosPonto.Add(novoPonto);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensagem = "Ponto registrado com sucesso!",
            dataHora = novoPonto.DataHoraOficial,
            funcionario = funcionario.Nome
        });
    }

    [HttpGet]
    public async Task<IActionResult> ListarRegistros()
    {
        var registros = await _context.RegistrosPonto
            .Include(r => r.Funcionario)
            .ThenInclude(f => f.Cargo)
            .ToListAsync();

        return Ok(registros);
    }

    // NOVA ROTA INSERIDA AQUI:
    // Rota: GET api/RegistrosPonto/funcionario/1
    [HttpGet("funcionario/{funcionarioId}")]
    public async Task<IActionResult> GetHistoricoFuncionario(int funcionarioId)
    {
        // Busca os pontos apenas do funcionário logado, ordenando do mais recente para o mais antigo
        var registros = await _context.RegistrosPonto
            .Where(r => r.FuncionarioId == funcionarioId)
            .OrderByDescending(r => r.DataHoraOficial)
            .ToListAsync();

        return Ok(registros);
    }


    [HttpGet("funcionario/{id}/pdf")]
    public async Task<IActionResult> ExportarPdfMes(int id, [FromQuery] int mes, [FromQuery] int ano)
    {
        // 1. Busque o funcionário no banco
        var funcionario = await _context.Funcionarios.FindAsync(id);

        // 2. Busque os registros desse mês específico
        var registros = await _context.RegistrosPonto
            .Where(r => r.FuncionarioId == id && r.DataHoraOficial.Month == mes && r.DataHoraOficial.Year == ano)
            .OrderBy(r => r.DataHoraOficial)
            .ToListAsync();

        // 3. Gere o arquivo
        var pdfService = new RelatorioPdfService();
        var pdfBytes = pdfService.GerarRelatorioMensal(funcionario.Nome, $"{mes:D2}/{ano}", registros);

        // 4. Retorne como download
        return File(pdfBytes, "application/pdf", $"Controlo_Assiduidade_{funcionario.Nome.Replace(" ", "_")}_{mes}_{ano}.pdf");
    }
}

// Objeto auxiliar (DTO) para receber os dados do app mobile com segurança
public class RegistroPontoDto
{
    public int FuncionarioId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool GPSMockado { get; set; }
}