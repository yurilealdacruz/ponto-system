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

    private async void OnExportarPdfClicked(object sender, EventArgs e)
    {
        try
        {
            // Variáveis de exemplo (você pegará isso do picker ou do contexto atual)
            int mesAtual = DateTime.Now.Month;
            int anoAtual = DateTime.Now.Year;

            // Pega o ID real do usuário logado no celular
            int funcionarioId = Preferences.Default.Get("FuncionarioId", 0);
            if (funcionarioId == 0) return;

            // URL corrigida apontando para o seu Render e para o Controller correto
            string baseUrl = "https://ponto-system.onrender.com";
            string url = $"{baseUrl}/api/RegistrosPonto/funcionario/{funcionarioId}/pdf?mes={mesAtual}&ano={anoAtual}";

            using var client = new HttpClient();

            byte[] pdfBytes = await client.GetByteArrayAsync(url);

            // Define o caminho no cache do celular
            string nomeArquivo = $"Controlo_Assiduidade_{mesAtual}_{anoAtual}.pdf";
            string caminhoLocal = Path.Combine(FileSystem.CacheDirectory, nomeArquivo);

            // Salva o PDF no aparelho
            await File.WriteAllBytesAsync(caminhoLocal, pdfBytes);

            // Chama a tela nativa do Android/iOS para compartilhar (WhatsApp, Email, etc)
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Enviar Relatório de Ponto",
                File = new ShareFile(caminhoLocal)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível exportar o PDF: {ex.Message}", "OK");
        }
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
            if (funcionarioId == 0)
            {
                await DisplayAlert("Aviso", "ID do funcionário não encontrado nas preferências.", "OK");
                return;
            }

            string apiUrl = $"https://ponto-system.onrender.com/api/RegistrosPonto/funcionario/{funcionarioId}";

            using var client = new HttpClient();

            // Faz o GET e pega o texto bruto para sabermos exatamente o que a API respondeu
            string jsonResponse = await client.GetStringAsync(apiUrl);

            // Se a resposta vier vazia ou apenas com []
            if (string.IsNullOrWhiteSpace(jsonResponse) || jsonResponse == "[]")
            {
                await DisplayAlert("Depuração", $"A API retornou vazio para o FuncionarioId: {funcionarioId}", "OK");
                return;
            }

            var registros = System.Text.Json.JsonSerializer.Deserialize<List<RegistroPontoResponse>>(jsonResponse, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (registros != null && registros.Any())
            {
                foreach (var r in registros)
                {
                    r.DataHoraOficial = DateTime.SpecifyKind(r.DataHoraOficial, DateTimeKind.Utc).ToLocalTime();
                }

                var dadosAgrupados = registros
                    .OrderBy(r => r.DataHoraOficial)
                    .GroupBy(r => r.DataHoraOficial.Date)
                    .Select(grupo => {
                        var pontosDoDia = grupo.ToList();
                        return new DiaTrabalho
                        {
                            DataOriginal = grupo.Key,
                            DataFormatada = grupo.Key.ToString("dd/MM/yyyy"),
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
            else
            {
                await DisplayAlert("Depuração", $"Nenhum registo encontrado na lista para o ID: {funcionarioId}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro de Diagnóstico", $"Detalhe: {ex.Message}", "OK");
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

