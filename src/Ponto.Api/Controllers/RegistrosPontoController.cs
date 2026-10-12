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


    [HttpGet("admin/relatorio-pdf")]
    public async Task<IActionResult> GerarRelatorioPdfAdmin([FromQuery] int funcionarioId, [FromQuery] int mes, [FromQuery] int ano)
    {
        var funcionario = await _context.Funcionarios.FindAsync(funcionarioId);

        if (funcionario == null)
            return NotFound(new { mensagem = "Funcionário não encontrado." });

        // Busca todos os registros do mês/ano informados
        var registros = await _context.RegistrosPonto
            .Where(r => r.FuncionarioId == funcionarioId
                     && r.DataHoraOficial.Month == mes
                     && r.DataHoraOficial.Year == ano)
            .OrderBy(r => r.DataHoraOficial)
            .ToListAsync();

        // Agrupa as batidas por dia para montar as 4 colunas da folha de ponto
        var diasTrabalhados = registros
            .GroupBy(r => r.DataHoraOficial.Date)
            .Select(grupo => {
                var pontos = grupo.OrderBy(p => p.DataHoraOficial).ToList();
                return new LinhaPdfDia
                {
                    Data = grupo.Key,
                    Ent1 = pontos.Count > 0 ? pontos[0].DataHoraOficial.ToString("HH:mm") : "-",
                    Sai1 = pontos.Count > 1 ? pontos[1].DataHoraOficial.ToString("HH:mm") : "-",
                    Ent2 = pontos.Count > 2 ? pontos[2].DataHoraOficial.ToString("HH:mm") : "-",
                    Sai2 = pontos.Count > 3 ? pontos[3].DataHoraOficial.ToString("HH:mm") : "-"
                };
            }).ToList();

        var pdfService = new RelatorioPdfService();
        var pdfBytes = pdfService.GerarRelatorioMensal(funcionario.Nome, $"{mes:D2}/{ano}", diasTrabalhados);

        string nomeArquivo = $"Relatorio_Ponto_{funcionario.Nome.Replace(" ", "_")}_{mes:D2}_{ano}.pdf";
        return File(pdfBytes, "application/pdf", nomeArquivo);
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