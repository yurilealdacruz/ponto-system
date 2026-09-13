using System.Net.Http.Json;


namespace Ponto.Mobile;

public partial class SolicitarAjustePage : ContentPage
{
    public SolicitarAjustePage(DateTime dataSelecionada)
    {
        InitializeComponent();
        
        // Preenche o campo de data automaticamente com o dia que o usuário clicou
        DataAjustePicker.Date = dataSelecionada;
    }

    private async void OnCancelarClicked(object? sender, EventArgs e)
    {
        // Fecha o pop-up sem fazer nada
        await Navigation.PopModalAsync();
    }

    private async void OnEnviarClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(JustificativaEntry.Text))
        {
            await DisplayAlertAsync("Aviso", "Por favor, preencha uma justificativa para o RH.", "OK");
            return;
        }

        BtnEnviarSolicitacao.IsEnabled = false;
        BtnEnviarSolicitacao.Text = "Enviando...";

        try
        {
            int funcionarioId = Preferences.Default.Get("FuncionarioId", 0);
            
            // Junta a data do DatePicker com a hora do TimePicker em uma única variável
            DateTime dataSugerida = DataAjustePicker.Date ?? DateTime.Today;
			TimeSpan horaSugerida = HoraAjustePicker.Time ?? TimeSpan.Zero;
			DateTime dataHoraSugerida = dataSugerida.Add(horaSugerida);

            var solicitacao = new
            {
                FuncionarioId = funcionarioId,
                RegistroPontoId = (int?)null, // Nulo indica que não estamos alterando um ID específico, mas sim o dia
                DataHoraSugerida = dataHoraSugerida,
                Justificativa = JustificativaEntry.Text
            };

            using var client = new HttpClient();
            var response = await client.PostAsJsonAsync("https://ponto-system.onrender.com/api/SolicitacoesAjuste", solicitacao);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlertAsync("Sucesso", "Solicitação enviada para análise do RH!", "OK");
                await Navigation.PopModalAsync();
            }
            else
            {
                await DisplayAlertAsync("Erro", "Falha ao enviar a solicitação. Tente novamente.", "OK");
            }
        }
        catch (Exception)
        {
            await DisplayAlertAsync("Erro", "Erro de conexão com o servidor.", "OK");
        }
        finally
        {
            BtnEnviarSolicitacao.IsEnabled = true;
            BtnEnviarSolicitacao.Text = "Enviar Solicitação";
        }
    }
}