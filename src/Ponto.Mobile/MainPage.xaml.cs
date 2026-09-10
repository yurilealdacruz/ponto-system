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

        StatusLabel.TextColor = resultado.Contains("sucesso") ? Colors.Green : Colors.Red;
        StatusLabel.Text = resultado;
        BtnRegistrarPonto.IsEnabled = true;
    }
}