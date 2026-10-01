using System.Net.Http.Json;

namespace Ponto.Mobile;

public partial class PainelAdminPage : ContentPage
{
    public PainelAdminPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarEquipe();
    }

    private async void OnRefreshEquipe(object? sender, EventArgs e)
    {
        await CarregarEquipe();
        RefreshEquipe.IsRefreshing = false;
    }

    private async Task CarregarEquipe()
    {
        try
        {
            using var client = new HttpClient();
            // Chama a nova rota da API
            var equipe = await client.GetFromJsonAsync<List<EquipeStatusDto>>("https://ponto-system.onrender.com/api/Funcionarios/equipe-status");

            ListaEquipe.ItemsSource = equipe;
        }
        catch (Exception)
        {
            await DisplayAlertAsync("Erro", "Não foi possível carregar a lista da equipe.", "OK");
        }
    }

    // Ação ao clicar no card de um funcionário
    private async void OnColaboradorTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is EquipeStatusDto colaborador)
        {
            // O próximo passo será criar a DetalhesColaboradorPage
            await Navigation.PushAsync(new DetalhesColaboradorPage(colaborador.Id, colaborador.Nome));
            
        }
    }
}

// DTO espelhando o que a API envia
public class EquipeStatusDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool TemAjustePendente { get; set; }
}