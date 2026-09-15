using System.Net.Http.Json;

namespace Ponto.Mobile;

public partial class PainelAdminPage : ContentPage
{
    public PainelAdminPage()
    {
        InitializeComponent();
    }

    // Carrega a lista automaticamente sempre que o ADM abrir esta aba
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarPendentes();
    }

    private async void OnRefreshPendentes(object? sender, EventArgs e)
    {
        await CarregarPendentes();
        RefreshPendentes.IsRefreshing = false; // Para a animação de carregamento
    }

    private async Task CarregarPendentes()
    {
        try
        {
            using var client = new HttpClient();
            var pendentes = await client.GetFromJsonAsync<List<SolicitacaoPendenteDto>>("https://ponto-system.onrender.com/api/SolicitacoesAjuste/pendentes");
            
            ListaPendentes.ItemsSource = pendentes;
        }
        catch (Exception)
        {
            await DisplayAlertAsync("Erro", "Não foi possível carregar as solicitações.", "OK");
        }
    }

    private async void OnAprovarClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int solicitacaoId)
        {
            bool confirmar = await DisplayAlertAsync("Confirmação", "Deseja realmente aprovar e inserir este ajuste de ponto?", "Sim", "Cancelar");
            if (!confirmar) return;

            btn.IsEnabled = false;
            btn.Text = "Aprovando...";

            try
            {
                using var client = new HttpClient();
                // Chama a rota de aprovação que altera a tabela RegistrosPonto
                var response = await client.PostAsync($"https://ponto-system.onrender.com/api/SolicitacoesAjuste/{solicitacaoId}/aprovar", null);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlertAsync("Sucesso", "Ponto ajustado com sucesso!", "OK");
                    await CarregarPendentes(); // Atualiza a lista, o card aprovado vai sumir
                }
                else
                {
                    await DisplayAlertAsync("Erro", "Falha ao aprovar a solicitação.", "OK");
                    btn.IsEnabled = true;
                    btn.Text = "Aprovar Ajuste";
                }
            }
            catch (Exception)
            {
                await DisplayAlertAsync("Erro", "Erro de conexão com o servidor.", "OK");
                btn.IsEnabled = true;
                btn.Text = "Aprovar Ajuste";
            }
        }
    }
}

// Objeto para espelhar a resposta do back-end
public class SolicitacaoPendenteDto
{
    public int Id { get; set; }
    public string FuncionarioNome { get; set; } = string.Empty;
    public string FuncionarioCpf { get; set; } = string.Empty;
    public DateTime DataHoraSugerida { get; set; }
    public string Justificativa { get; set; } = string.Empty;
    public DateTime DataSolicitacao { get; set; }
}