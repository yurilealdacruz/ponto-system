using Ponto.Mobile.Services;
using System.Text.Json;

namespace Ponto.Mobile;

public partial class MainPage : ContentPage
{
    private readonly PontoApiService _apiService;

    public MainPage(PontoApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Puxa o nome que foi salvo no login (se não achar nada, usa "Colaborador")
        string nomeFuncionario = Preferences.Default.Get("FuncionarioNome", "Colaborador");
        
        // Atualiza a interface
        SaudacaoLabel.Text = $"Olá, {nomeFuncionario}!";
    }

    private async void OnRegistrarPontoClicked(object sender, EventArgs e)
    {
        BtnRegistrarPonto.IsEnabled = false;
        StatusLabel.TextColor = Colors.Gray;
        StatusLabel.Text = "Registrando ponto...";

        // Faz a chamada real para o backend
        var resultado = await _apiService.RegistrarPontoAsync();

        try
        {
            if (resultado.Trim().StartsWith("{"))
            {
                using var jsonDoc = JsonDocument.Parse(resultado);
                if (jsonDoc.RootElement.TryGetProperty("mensagem", out var mensagemElement))
                {
                    resultado = mensagemElement.GetString() ?? resultado;
                }
            }
        }
        catch
        {
            // Se falhar ao processar o JSON (ex: erro de servidor), mantém o texto bruto original
        }

        // StringComparison.OrdinalIgnoreCase garante que vai identificar a palavra "sucesso" mesmo se vier com letra maiúscula
        StatusLabel.TextColor = resultado.Contains("sucesso", StringComparison.OrdinalIgnoreCase) ? Colors.Green : Colors.Red;
        StatusLabel.Text = resultado;
        
        BtnRegistrarPonto.IsEnabled = true;
    }
}