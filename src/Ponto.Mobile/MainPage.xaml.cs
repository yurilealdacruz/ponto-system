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

    // CORRIGIDO: Adicionado o modificador "async" para permitir o uso de "await"
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Agora o await funciona corretamente
        await VerificarAtualizacaoAsync();

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
                        // Abre o link de download do APK no navegador nativo do telemóvel
                        await Launcher.OpenAsync(new Uri(resposta.UrlDownload));
                    }
                }
            }
        }
        catch
        {
            // Se falhar ao checar (ex: sem internet), o app segue normalmente
        }
    }

    private async void OnRegistrarPontoClicked(object? sender, EventArgs e)
    {
        BtnRegistrarPonto.IsEnabled = false;
        StatusLabel.TextColor = Colors.Gray;
        StatusLabel.Text = "A obter localização..."; // Informa o utilizador

        double? latitude = null;
        double? longitude = null;

        try
        {
            // Solicita a localização com precisão média e tempo limite de 10 segundos
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location != null)
            {
                latitude = location.Latitude;
                longitude = location.Longitude;
            }
        }
        catch (FeatureNotSupportedException)
        {
            await DisplayAlert("Erro", "O GPS não é suportado neste dispositivo.", "OK");
        }
        catch (PermissionException)
        {
            await DisplayAlert("Permissão Negada", "A aplicação precisa de acesso à localização para registar o ponto.", "OK");
        }
        catch (Exception)
        {
            // Ignora falhas de GPS para não travar a app (pode registar sem localização dependendo da sua regra de negócio)
        }

        StatusLabel.Text = "A registar ponto...";

        // PASSAR A LOCALIZAÇÃO PARA O SERVIÇO DA API
        // Atenção: O seu método RegistrarPontoAsync precisa ser atualizado para receber estes parâmetros!
        var resultado = await _apiService.RegistrarPontoAsync(latitude, longitude);

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
            // Mantém o texto bruto original se falhar ao analisar o JSON
        }

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