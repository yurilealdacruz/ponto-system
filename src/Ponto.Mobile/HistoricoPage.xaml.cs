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

    private async void OnSolicitarAjusteClicked(object? sender, EventArgs e)
    {
        // NOVO: Verifica o dia do mês atual
        int diaAtual = DateTime.Now.Day;

        // Se for antes do dia 5 ou depois do dia 25, bloqueia.
        if (diaAtual < 5 || diaAtual > 25)
        {
            await DisplayAlertAsync("Acesso Bloqueado", "Os ajustes de ponto só podem ser solicitados entre os dias 05 e 25 de cada mês.", "OK");
            return; // Interrompe o código aqui, não abre a tela
        }

        // Se estiver no prazo, continua com o código original para abrir o modal
        if (sender is ImageButton button && button.CommandParameter is DateTime dataSelecionada)
        {
            await Navigation.PushModalAsync(new SolicitarAjustePage(dataSelecionada));
        }
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
                // 1. CORREÇÃO DO FUSO HORÁRIO
                foreach (var r in registros)
                {
                    // Força o C# a entender que a data do banco é UTC e converte para a hora do celular (-3h)
                    r.DataHoraOficial = DateTime.SpecifyKind(r.DataHoraOficial, DateTimeKind.Utc).ToLocalTime();
                }

                // 2. AGRUPAMENTO E EXIBIÇÃO
                var dadosAgrupados = registros
                    .OrderBy(r => r.DataHoraOficial)
                    .GroupBy(r => r.DataHoraOficial.Date) // Como já convertemos, o dia agora está correto
                    .Select(grupo => {
                        var pontosDoDia = grupo.ToList();
                        return new DiaTrabalho
                        {
                            DataOriginal = grupo.Key,
                            DataFormatada = grupo.Key.ToString("dd/MM/yyyy"),
                            // Removemos os ToLocalTime() daqui porque já fizemos no foreach
                            Ponto1 = pontosDoDia.Count > 0 ? pontosDoDia[0].DataHoraOficial.ToString("HH:mm") : "-",
                            Ponto2 = pontosDoDia.Count > 1 ? pontosDoDia[1].DataHoraOficial.ToString("HH:mm") : "-",
                            Ponto3 = pontosDoDia.Count > 2 ? pontosDoDia[2].DataHoraOficial.ToString("HH:mm") : "-",
                            Ponto4 = pontosDoDia.Count > 3 ? pontosDoDia[3].DataHoraOficial.ToString("HH:mm") : "-"
                        };
                    })
                    .OrderByDescending(d => d.DataOriginal)
                    .ToList();

                ListaHistorico.ItemsSource = dadosAgrupados;
            }
        }
        catch (Exception)
        {
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
    public DateTime DataOriginal { get; set; }
    public string DataFormatada { get; set; } = string.Empty;
    public string Ponto1 { get; set; } = string.Empty;
    public string Ponto2 { get; set; } = string.Empty;
    public string Ponto3 { get; set; } = string.Empty;
    public string Ponto4 { get; set; } = string.Empty;
}

