using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ponto.Data;
using Ponto.Domain;

namespace Ponto.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CargosController : ControllerBase
{
    private readonly PontoDbContext _context;

    // Injeção de dependência do banco de dados
    public CargosController(PontoDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CriarCargo(Cargo cargo)
    {
        _context.Cargos.Add(cargo);
        await _context.SaveChangesAsync();
        return Ok(cargo); // Retorna 200 OK com os dados do cargo criado
    }

    [HttpGet]
    public async Task<IActionResult> ListarCargos()
    {
        var cargos = await _context.Cargos.ToListAsync();
        return Ok(cargos);
    }
}