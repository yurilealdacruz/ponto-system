using System.Net.Http.Json;

namespace Ponto.Mobile;

public partial class SolicitarAjustePage : ContentPage
{
    public SolicitarAjustePage(DateTime dataSelecionada)
    {
        InitializeComponent();

        // Preenche o campo de data automaticamente com o dia em que o utilizador clicou
        DataAjustePicker.Date = dataSelecionada;
    }

    private async void OnCancelarClicked(object? sender, EventArgs e)
    {
        // Fecha o pop-up sem fazer nada
        await Navigation.PopModalAsync();
    }

    private async void OnEnviarClicked(object? sender, EventArgs e)
    {
        // 1. Valida se o utilizador selecionou qual o ponto a ajustar
        if (PickerTipoPonto.SelectedIndex == -1)
        {
            await DisplayAlertAsync("Atenção", "Por favor, selecione qual batida (Entrada 1, Saída 1, etc.) deseja ajustar.", "OK");
            return;
        }

        // 2. Valida a justificativa
        if (string.IsNullOrWhiteSpace(JustificativaEntry.Text))
        {
            await DisplayAlertAsync("Aviso", "Por favor, preencha uma justificativa para os Recursos Humanos.", "OK");
            return;
        }

        BtnEnviarSolicitacao.IsEnabled = false;
        BtnEnviarSolicitacao.Text = "A enviar...";

        try
        {
            int funcionarioId = Preferences.Default.Get("FuncionarioId", 0);

            // 3. Junta a data do DatePicker com a hora do TimePicker numa única variável
            // 3. Junta a data do DatePicker com a hora do TimePicker numa única variável
            DateTime dataSugerida = DataAjustePicker.Date ?? DateTime.Today;
            TimeSpan horaSugerida = HoraAjustePicker.Time ?? TimeSpan.Zero;
            DateTime dataHoraSugerida = dataSugerida.Add(horaSugerida);

            // 4. Obtém o valor do Picker selecionado
            string tipoPontoSelecionado = PickerTipoPonto.SelectedItem.ToString();

            // 5. Monta o objeto final
            var solicitacao = new
            {
                FuncionarioId = funcionarioId,
                RegistroPontoId = (int?)null,
                DataHoraSugerida = dataHoraSugerida,
                TipoBatida = tipoPontoSelecionado, // O novo campo incluído aqui
                Justificativa = JustificativaEntry.Text
            };

            using var client = new HttpClient();
            var response = await client.PostAsJsonAsync("https://ponto-system.onrender.com/api/SolicitacoesAjuste", solicitacao);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlertAsync("Sucesso", "Solicitação enviada para análise dos Recursos Humanos!", "OK");
                await Navigation.PopModalAsync();
            }
            else
            {
                await DisplayAlertAsync("Erro", "Falha ao enviar a solicitação. Tente novamente.", "OK");
            }
        }
        catch (Exception)
        {
            await DisplayAlertAsync("Erro", "Erro de ligação ao servidor.", "OK");
        }
        finally
        {
            BtnEnviarSolicitacao.IsEnabled = true;
            BtnEnviarSolicitacao.Text = "Enviar Solicitação";
        }
    }
}