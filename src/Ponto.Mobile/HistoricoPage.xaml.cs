using System.Net.Http.Json;

namespace Ponto.Mobile;

public partial class HistoricoPage : ContentPage
{
    public HistoricoPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarHistorico();
    }

    // Adicionei a interrogação (object? sender) para agradar o compilador do .NET 10
    private async void OnRefresh(object? sender, EventArgs e)
    {
        await CarregarHistorico();
        AtualizarView.IsRefreshing = false;
    }

    private async Task CarregarHistorico()
    {
        try
        {
            int funcionarioId = Preferences.Default.Get("FuncionarioId", 0);
            if (funcionarioId == 0) return;

            string apiUrl = $"https://ponto-system.onrender.com/api/RegistrosPonto/funcionario/{funcionarioId}";

            using var client = new HttpClient();
            var registros = await client.GetFromJsonAsync<List<RegistroPontoResponse>>(apiUrl);

            if (registros != null && registros.Any())
            {
                var dadosAgrupados = registros
                    .OrderBy(r => r.DataHoraOficial)
                    .GroupBy(r => r.DataHoraOficial.ToLocalTime().Date)
                    .Select(grupo => {
                        var pontosDoDia = grupo.ToList();
                        return new DiaTrabalho
                        {
                            DataFormatada = grupo.Key.ToString("ddd, dd MMM"),
                            Ponto1 = pontosDoDia.Count > 0 ? pontosDoDia[0].DataHoraOficial.ToLocalTime().ToString("HH:mm") : "-",
                            Ponto2 = pontosDoDia.Count > 1 ? pontosDoDia[1].DataHoraOficial.ToLocalTime().ToString("HH:mm") : "-",
                            Ponto3 = pontosDoDia.Count > 2 ? pontosDoDia[2].DataHoraOficial.ToLocalTime().ToString("HH:mm") : "-",
                            Ponto4 = pontosDoDia.Count > 3 ? pontosDoDia[3].DataHoraOficial.ToLocalTime().ToString("HH:mm") : "-"
                        };
                    })
                    .OrderByDescending(d => d.DataFormatada)
                    .ToList();

                ListaHistorico.ItemsSource = dadosAgrupados;
            }
        }
        catch (Exception) // Removi o "ex" daqui para não dar warning de variável não usada
        {
            // O .NET 10 pede para usar DisplayAlertAsync agora
            await DisplayAlertAsync("Erro", "Não foi possível carregar o histórico.", "OK");
        }
    }
}

public class RegistroPontoResponse
{
    public DateTime DataHoraOficial { get; set; }
}

public class DiaTrabalho
{
    // O = string.Empty evita o warning de "propriedade não pode ser nula"
    public string DataFormatada { get; set; } = string.Empty;
    public string Ponto1 { get; set; } = string.Empty;
    public string Ponto2 { get; set; } = string.Empty;
    public string Ponto3 { get; set; } = string.Empty;
    public string Ponto4 { get; set; } = string.Empty;
}