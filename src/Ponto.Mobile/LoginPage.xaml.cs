using System.Net.Http.Json;

namespace Ponto.Mobile;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        string identificador = IdentificadorEntry.Text;
        string senha = SenhaEntry.Text;

        if (string.IsNullOrWhiteSpace(identificador) || string.IsNullOrWhiteSpace(senha))
        {
            await DisplayAlertAsync("Erro", "Preencha todos os campos", "OK");
            return;
        }

        try
        {
            string apiUrl = "https://ponto-system.onrender.com/api/Funcionarios/login";

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            using var client = new HttpClient(handler);

            var loginData = new { Identificador = identificador, Senha = senha };
            var response = await client.PostAsJsonAsync(apiUrl, loginData);

            if (response.IsSuccessStatusCode)
            {
                var funcionario = await response.Content.ReadFromJsonAsync<FuncionarioResponse>();

                if (funcionario != null)
                {
                    Preferences.Default.Set("FuncionarioId", funcionario.Id);
                    Preferences.Default.Set("FuncionarioNome", funcionario.Nome);

                    var shell = new AppShell();

                    if (funcionario.CargoId == 1) 
                    {
                        Preferences.Default.Set("IsAdmin", true);
                        shell.TabAdministracao.IsVisible = true;
                    }
                    else
                    {
                        Preferences.Default.Set("IsAdmin", false);
                    }

                    if (Application.Current != null && Application.Current.Windows.Count > 0)
                    {
                        Application.Current.Windows[0].Page = shell;
                    }
                }
            }
            else
            {
                await DisplayAlertAsync("Acesso Negado", "CPF ou senha incorretos.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro de Conexão", $"Não foi possível conectar com o servidor {ex.Message}", "OK");
        }
    }
}

public class FuncionarioResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int CargoId { get; set; }
}