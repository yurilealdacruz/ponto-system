using System.Net.Http.Json;

namespace Ponto.Mobile.Services;

public class PontoApiService
{
    private readonly HttpClient _httpClient;

    public PontoApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        // Substitua pela porta real que você copiou do Swagger
        _httpClient.BaseAddress = new Uri("https://ponto-system.onrender.com/");
    }

    public async Task<string> RegistrarPontoAsync()
    {
        try
        {
            // Pega dinamicamente o ID do usuário que fez o login no app
            int funcionarioId = Preferences.Default.Get("FuncionarioId", 0);
            if (funcionarioId == 0)
                return "Erro: Nenhum funcionário logado encontrado.";

            var dadosPonto = new
            {
                FuncionarioId = funcionarioId,
                Latitude = 0.0, // Se estiver a usar geolocalização real, pode passar aqui
                Longitude = 0.0,
                GPSMockado = false
            };

            var response = await _httpClient.PostAsJsonAsync("api/RegistrosPonto", dadosPonto);

            if (response.IsSuccessStatusCode)
                return $"Ponto registrado com sucesso às {DateTime.Now:HH:mm:ss}";

            // Lendo o erro real gerado pela API (como a trava de 4 pontos)
            var erroDetalhe = await response.Content.ReadAsStringAsync();
            return $"{erroDetalhe}";
        }
        catch (Exception ex)
        {
            return $"Falha na conexão: {ex.Message}";
        }
    }
}