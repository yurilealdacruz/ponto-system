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
        // 1. Validações de seleção de mês e ano
        if (PickerMes.SelectedIndex == -1 || PickerAno.SelectedIndex == -1)
        {
            await DisplayAlertAsync("Atenção", "Selecione o mês e o ano desejados.", "OK");
            return;
        }

        int mes = PickerMes.SelectedIndex + 1; // 1 a 12
        int ano = int.Parse(PickerAno.SelectedItem.ToString());

        // Supondo que você tenha a variável do ID do funcionário nesta página (ex: _funcionarioId)
        int funcionarioId = _funcionarioId;

        string url = $"https://ponto-system.onrender.com/api/RegistrosPonto/admin/relatorio-pdf?funcionarioId={funcionarioId}&mes={mes}&ano={ano}";

        try
        {
            using var client = new HttpClient();
            byte[] pdfBytes = await client.GetByteArrayAsync(url);

            string nomeArquivo = $"Relatorio_Ponto_{funcionarioId}_{mes}_{ano}.pdf";
            string caminhoLocal = Path.Combine(FileSystem.CacheDirectory, nomeArquivo);

            await File.WriteAllBytesAsync(caminhoLocal, pdfBytes);

            // Abre o menu nativo de compartilhar/salvar/abrir PDF
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

    private async Task CarregarDados()
    {
        try
        {
            using var client = new HttpClient();

            // 1. CARREGA O HISTÓRICO (Reaproveita RegistroPontoResponse e DiaTrabalho do HistoricoPage)
            string urlHistorico = $"https://ponto-system.onrender.com/api/RegistrosPonto/funcionario/{_funcionarioId}";
            var registros = await client.GetFromJsonAsync<List<RegistroPontoResponse>>(urlHistorico);

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
                        return new DiaTrabalho
                        {
                            DataOriginal = grupo.Key,
                            DataFormatada = grupo.Key.ToString("dd/MM/yyyy"),
                            Ponto1 = pontos.Count > 0 ? pontos[0].DataHoraOficial.ToString("HH:mm") : "-",
                            Ponto2 = pontos.Count > 1 ? pontos[1].DataHoraOficial.ToString("HH:mm") : "-",
                            Ponto3 = pontos.Count > 2 ? pontos[2].DataHoraOficial.ToString("HH:mm") : "-",
                            Ponto4 = pontos.Count > 3 ? pontos[3].DataHoraOficial.ToString("HH:mm") : "-"
                        };
                    })
                    .OrderByDescending(d => d.DataOriginal)
                    .ToList();

                ListaHistorico.ItemsSource = dadosAgrupados;
            }

            // 2. VERIFICA SE HÁ PENDÊNCIAS
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

// Declarada apenas ela aqui no final para suprir a falta no projeto!
public class SolicitacaoPendenteDto
{
    public int Id { get; set; }
    public string FuncionarioNome { get; set; } = string.Empty;
    public string? FuncionarioCpf { get; set; }
    public DateTime DataHoraSugerida { get; set; }
    public string Justificativa { get; set; } = string.Empty;
}