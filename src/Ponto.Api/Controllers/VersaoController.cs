using Microsoft.AspNetCore.Mvc;

namespace Ponto.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VersaoController : ControllerBase
{
    [HttpGet]
    public IActionResult GetVersaoAtual()
    {
        // Defina aqui a versão atual da sua aplicação e o link direto para baixar o APK atualizado
        return Ok(new
        {
            VersaoCodigo = 2, // Incremente este número sempre que gerar um APK novo
            VersaoNome = "1.1.0",
            UrlDownload = "https://www.mediafire.com/file/kdra3gztir5lf45/pontosystem.apk/file", // Link onde o APK estará hospedado
            Obrigatorio = false
        });
    }
}