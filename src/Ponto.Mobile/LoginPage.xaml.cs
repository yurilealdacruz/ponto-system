using System.Net.Http.Json; // <-- Adicione isso no topo

namespace Ponto.Mobile;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string identificador = IdentificadorEntry.Text;
        string senha = SenhaEntry.Text;

        if (string.IsNullOrWhiteSpace(identificador) || string.IsNullOrWhiteSpace(senha))
        {
            await DisplayAlert("Erro", "Preencha todos os campos", "OK");
            return;
        }

        try
        {
            // URL da sua API rodando no Windows
            string apiUrl = "https://localhost:7185/api/Funcionarios/login";

            // Ignorar validação de SSL de desenvolvimento (apenas para teste local)
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            using var client = new HttpClient(handler);

            var loginData = new { Identificador = identificador, Senha = senha };

            var response = await client.PostAsJsonAsync(apiUrl, loginData);

            if (response.IsSuccessStatusCode)
            {
                // Se a API retornar 200 OK, redireciona para a tela principal (AppShell carrega a MainPage)
                Application.Current.MainPage = new AppShell();
            }
            else
            {
                await DisplayAlert("Acesso Negado", "CPF/ID ou senha incorretos.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro de Conexão", $"Não foi possível conectar na API: {ex.Message}", "OK");
        }
    }
}