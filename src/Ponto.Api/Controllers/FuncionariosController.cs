using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ponto.Data;
using Ponto.Domain;

namespace Ponto.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FuncionariosController : ControllerBase
{
    private readonly PontoDbContext _context;

    public FuncionariosController(PontoDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CriarFuncionario(Funcionario funcionario)
    {
        // Valida se o cargo informado realmente existe no banco de dados
        var cargoExiste = await _context.Cargos.AnyAsync(c => c.Id == funcionario.CargoId);
        if (!cargoExiste)
        {
            return BadRequest(new { mensagem = "O cargo informado não existe." });
        }

        _context.Funcionarios.Add(funcionario);
        await _context.SaveChangesAsync();

        return Ok(funcionario);
    }

    [HttpGet]
    public async Task<IActionResult> ListarFuncionarios()
    {
        // Traz a lista de funcionários junto com os dados do Cargo correspondente (.Include)
        var funcionarios = await _context.Funcionarios
            .Include(f => f.Cargo)
            .ToListAsync();

            
        return Ok(funcionarios);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        // Aqui você busca no banco pelo CPF ou ID e valida a senha
        var funcionario = await _context.Funcionarios
            .FirstOrDefaultAsync(f => (f.Id.ToString() == dto.Identificador || f.Cpf == dto.Identificador) && f.Ativo);

        if (funcionario == null || funcionario.Senha != dto.Senha)
        {
            return Unauthorized(new { mensagem = "Credenciais inválidas ou funcionário inativo." });
        }

        return Ok(funcionario);
    }
}