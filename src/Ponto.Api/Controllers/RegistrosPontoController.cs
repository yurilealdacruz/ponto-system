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
}

// Objeto auxiliar (DTO) para receber os dados do app mobile com segurança
public class RegistroPontoDto
{
    public int FuncionarioId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool GPSMockado { get; set; }
}