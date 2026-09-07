using System.Net.Http.Json;

namespace Ponto.Mobile.Services;

public class PontoApiService
{
    private readonly HttpClient _httpClient;

    public PontoApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        // Substitua pela porta real que você copiou do Swagger
        _httpClient.BaseAddress = new Uri("https://localhost:7185/");
    }

    public async Task<string> RegistrarPontoAsync()
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/RegistrosPonto", new { FuncionarioId = 1 });

            if (response.IsSuccessStatusCode)
                return $"Ponto batido com sucesso às {DateTime.Now:HH:mm:ss}";

            // Lendo o erro real gerado pela API
            var erroDetalhe = await response.Content.ReadAsStringAsync();
            return $"Detalhe: {erroDetalhe}";
        }
        catch (Exception ex)
        {
            return $"Falha na conexão: {ex.Message}";
        }
    }
}