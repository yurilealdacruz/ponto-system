using System.Net.Http.Json;

namespace Ponto.Mobile;

public partial class DetalhesColaboradorPage : ContentPage
{
    private readonly int _funcionarioId;
    private int _solicitacaoIdPendente;

    public DetalhesColaboradorPage(int funcionarioId, string nomeFuncionario)
    {
        InitializeComponent();
        _funcionarioId = funcionarioId;
        Title = nomeFuncionario;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarDados();
    }

    private async void OnGerarPdfAdminClicked(object sender, EventArgs e)
    {
        if (PickerMes.SelectedIndex == -1 || PickerAno.SelectedIndex == -1)
        {
            await DisplayAlertAsync("Atenção", "Selecione o mês e o ano desejados.", "OK");
            return;
        }

        int mes = PickerMes.SelectedIndex + 1; // 1 a 12
        int ano = int.Parse(PickerAno.SelectedItem.ToString());

        int funcionarioId = _funcionarioId;
        string url = $"https://ponto-system.onrender.com/api/RegistrosPonto/admin/relatorio-pdf?funcionarioId={funcionarioId}&mes={mes}&ano={ano}";

        try
        {
            using var client = new HttpClient();
            byte[] pdfBytes = await client.GetByteArrayAsync(url);

            string nomeArquivo = $"Relatorio_Ponto_{funcionarioId}_{mes}_{ano}.pdf";
            string caminhoLocal = Path.Combine(FileSystem.CacheDirectory, nomeArquivo);

            await File.WriteAllBytesAsync(caminhoLocal, pdfBytes);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Enviar Relatório de Ponto",
                File = new ShareFile(caminhoLocal)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", $"Falha ao baixar o PDF: {ex.Message}", "OK");
        }
    }

    private async void OnAbrirMapaDiaClicked(object sender, EventArgs e)
    {
        var button = sender as Button;

        // Atualizado para usar o novo nome da classe
        var diaTrabalho = button?.CommandParameter as DiaTrabalhoAdmin;

        if (diaTrabalho == null) return;

        double? lat = diaTrabalho.Lat1 ?? diaTrabalho.Lat2 ?? diaTrabalho.Lat3 ?? diaTrabalho.Lat4;
        double? lon = diaTrabalho.Lon1 ?? diaTrabalho.Lon2 ?? diaTrabalho.Lon3 ?? diaTrabalho.Lon4;

        if (lat == null || lon == null || lat == 0)
        {
            await DisplayAlert("Sem Localização", "Não há dados de GPS registados para este dia.", "OK");
            return;
        }

        var location = new Location(lat.Value, lon.Value);
        var options = new MapLaunchOptions { Name = $"Ponto de {Title} em {diaTrabalho.DataFormatada}" };

        try
        {
            await Map.Default.OpenAsync(location, options);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível abrir o mapa: {ex.Message}", "OK");
        }
    }

    private async Task CarregarDados()
    {
        try
        {
            using var client = new HttpClient();

            string urlHistorico = $"https://ponto-system.onrender.com/api/RegistrosPonto/funcionario/{_funcionarioId}";

            // Atualizado para usar o novo nome da classe
            var registros = await client.GetFromJsonAsync<List<RegistroPontoAdminResponse>>(urlHistorico);

            if (registros != null)
            {
                foreach (var r in registros)
                {
                    r.DataHoraOficial = DateTime.SpecifyKind(r.DataHoraOficial, DateTimeKind.Utc).ToLocalTime();
                }

                var dadosAgrupados = registros
                    .OrderBy(r => r.DataHoraOficial)
                    .GroupBy(r => r.DataHoraOficial.Date)
                    .Select(grupo => {
                        var pontos = grupo.ToList();

                        // Atualizado para usar o novo nome da classe
                        return new DiaTrabalhoAdmin
                        {
                            DataOriginal = grupo.Key,
                            DataFormatada = grupo.Key.ToString("dd/MM/yyyy"),

                            Ponto1 = pontos.Count > 0 ? pontos[0].DataHoraOficial.ToString("HH:mm") : "-",
                            Lat1 = pontos.Count > 0 ? pontos[0].Latitude : null,
                            Lon1 = pontos.Count > 0 ? pontos[0].Longitude : null,

                            Ponto2 = pontos.Count > 1 ? pontos[1].DataHoraOficial.ToString("HH:mm") : "-",
                            Lat2 = pontos.Count > 1 ? pontos[1].Latitude : null,
                            Lon2 = pontos.Count > 1 ? pontos[1].Longitude : null,

                            Ponto3 = pontos.Count > 2 ? pontos[2].DataHoraOficial.ToString("HH:mm") : "-",
                            Lat3 = pontos.Count > 2 ? pontos[2].Latitude : null,
                            Lon3 = pontos.Count > 2 ? pontos[2].Longitude : null,

                            Ponto4 = pontos.Count > 3 ? pontos[3].DataHoraOficial.ToString("HH:mm") : "-",
                            Lat4 = pontos.Count > 3 ? pontos[3].Latitude : null,
                            Lon4 = pontos.Count > 3 ? pontos[3].Longitude : null
                        };
                    })
                    .OrderByDescending(d => d.DataOriginal)
                    .ToList();

                ListaHistorico.ItemsSource = dadosAgrupados;
            }

            var pendentes = await client.GetFromJsonAsync<List<SolicitacaoPendenteDto>>("https://ponto-system.onrender.com/api/SolicitacoesAjuste/pendentes");
            var ajusteReal = pendentes?.FirstOrDefault(p => p.FuncionarioNome == Title);

            if (ajusteReal != null)
            {
                _solicitacaoIdPendente = ajusteReal.Id;
                LabelDataSugerida.Text = $"Data Sugerida: {ajusteReal.DataHoraSugerida:dd/MM/yyyy HH:mm}";
                LabelJustificativa.Text = $"Motivo: {ajusteReal.Justificativa}";
                ContainerPendencia.IsVisible = true;
            }
            else
            {
                ContainerPendencia.IsVisible = false;
            }
        }
        catch (Exception)
        {
            await DisplayAlertAsync("Erro", "Falha ao carregar os dados.", "OK");
        }
    }

    private async void OnAprovarClicked(object? sender, EventArgs e)
    {
        bool confirmar = await DisplayAlertAsync("Confirmação", "Aprovar este ajuste?", "Sim", "Cancelar");
        if (!confirmar) return;

        BtnAprovar.IsEnabled = false;
        BtnAprovar.Text = "Aprovando...";

        try
        {
            using var client = new HttpClient();
            var response = await client.PostAsync($"https://ponto-system.onrender.com/api/SolicitacoesAjuste/{_solicitacaoIdPendente}/aprovar", null);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlertAsync("Sucesso", "Ponto ajustado!", "OK");
                ContainerPendencia.IsVisible = false;
                await CarregarDados();
            }
            else
            {
                await DisplayAlertAsync("Erro", "Falha ao aprovar.", "OK");
                BtnAprovar.IsEnabled = true;
                BtnAprovar.Text = "Aprovar Ajuste";
            }
        }
        catch (Exception)
        {
            await DisplayAlertAsync("Erro", "Erro de conexão.", "OK");
            BtnAprovar.IsEnabled = true;
            BtnAprovar.Text = "Aprovar Ajuste";
        }
    }
}

// --- CLASSES AUXILIARES ÚNICAS (Nomes alterados para evitar conflitos) ---

public class SolicitacaoPendenteDto
{
    public int Id { get; set; }
    public string FuncionarioNome { get; set; } = string.Empty;
    public string? FuncionarioCpf { get; set; }
    public DateTime DataHoraSugerida { get; set; }
    public string Justificativa { get; set; } = string.Empty;
}

public class RegistroPontoAdminResponse
{
    public DateTime DataHoraOficial { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class DiaTrabalhoAdmin
{
    public DateTime DataOriginal { get; set; }
    public string DataFormatada { get; set; } = string.Empty;
    public string Ponto1 { get; set; } = string.Empty;
    public string Ponto2 { get; set; } = string.Empty;
    public string Ponto3 { get; set; } = string.Empty;
    public string Ponto4 { get; set; } = string.Empty;

    public double? Lat1 { get; set; }
    public double? Lon1 { get; set; }
    public double? Lat2 { get; set; }
    public double? Lon2 { get; set; }
    public double? Lat3 { get; set; }
    public double? Lon3 { get; set; }
    public double? Lat4 { get; set; }
    public double? Lon4 { get; set; }
}