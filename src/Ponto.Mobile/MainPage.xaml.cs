using Ponto.Mobile.Services;

namespace Ponto.Mobile;

public partial class MainPage : ContentPage
{
    private readonly PontoApiService _apiService;

    public MainPage(PontoApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private async void OnRegistrarPontoClicked(object sender, EventArgs e)
    {
        BtnRegistrarPonto.IsEnabled = false;
        StatusLabel.TextColor = Colors.Gray;
        StatusLabel.Text = "Conectando com a API...";

        // Faz a chamada real para o backend
        var resultado = await _apiService.RegistrarPontoAsync();

        StatusLabel.TextColor = resultado.Contains("sucesso") ? Colors.Green : Colors.Red;
        StatusLabel.Text = resultado;
        BtnRegistrarPonto.IsEnabled = true;
    }
}