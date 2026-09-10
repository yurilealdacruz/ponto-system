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
            // URL da sua API rodando no Render
            string apiUrl = "https://ponto-system.onrender.com/api/Funcionarios/login";

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
                // 1. Lê os dados do funcionário que a API devolveu
                var funcionario = await response.Content.ReadFromJsonAsync<FuncionarioResponse>();

                if (funcionario != null)
                {
                    // 2. Salva o ID e o Nome de forma segura no aparelho
                    Preferences.Default.Set("FuncionarioId", funcionario.Id);
                    Preferences.Default.Set("FuncionarioNome", funcionario.Nome);
                }

                // 3. Redireciona para a tela principal (AppShell carrega a MainPage)
                Application.Current.MainPage = new AppShell();
            }
            else
            {
                await DisplayAlert("Acesso Negado", "CPF ou senha incorretos.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro de Conexão", $"Não foi possível conectar com o servidor {ex.Message}", "OK");
        }
    }
}

// Classe auxiliar para receber os dados do JSON da API
public class FuncionarioResponse
{
    public int Id { get; set; }
    public string Nome { get; set; }
}