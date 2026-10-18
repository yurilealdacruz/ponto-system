using Ponto.Mobile.Services;
using System.Net.Http.Json;
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

    private async Task VerificarAtualizacaoAsync()
    {
        try
        {
            using var client = new HttpClient();
            string url = "https://ponto-system.onrender.com/api/versao";

            var resposta = await client.GetFromJsonAsync<VersaoDto>(url);

            if (resposta != null)
            {
                // Defina a versão atual instalada no seu app (exemplo: 1)
                int versaoInstalada = 1;

                if (resposta.VersaoCodigo > versaoInstalada)
                {
                    bool atualizar = await DisplayAlert(
                        "Nova Atualização Disponível!",
                        $"Foi lançada a versão {resposta.VersaoNome} do Ponto. Deseja atualizar agora?",
                        "Sim", "Mais tarde");

                    if (atualizar)
                    {
                        // Abre o link de download do APK no navegador nativo do telemóvel para descarregar e instalar
                        await Launcher.OpenAsync(new Uri(resposta.UrlDownload));
                    }
                }
            }
        }
        catch
        {
            // Se falhar ao checar (ex: sem internet), o app segue normalmente sem incomodar o utilizador
        }
    }


    private async void OnRegistrarPontoClicked(object? sender, EventArgs e)
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


    public class VersaoDto
    {
        public int VersaoCodigo { get; set; }
        public string VersaoNome { get; set; } = string.Empty;
        public string UrlDownload { get; set; } = string.Empty;
        public bool Obrigatorio { get; set; }
    }

}