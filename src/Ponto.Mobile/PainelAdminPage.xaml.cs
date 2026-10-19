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
            var equipe = await client.GetFromJsonAsync<List<EquipeStatusDto>>("https://ponto-system.onrender.com/api/Funcionarios/equipe-status");

            ListaEquipe.ItemsSource = equipe;
        }
        catch (Exception)
        {
            await DisplayAlert("Erro", "Não foi possível carregar a lista da equipe.", "OK");
        }
    }

    private async void OnColaboradorTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is EquipeStatusDto colaborador)
        {
            await Navigation.PushAsync(new DetalhesColaboradorPage(colaborador.Id, colaborador.Nome));
        }
    }

    // AÇÃO: Desativar Funcionário (Soft Delete)
    private async void OnDesativarClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is int funcionarioId)
        {
            bool confirmar = await DisplayAlert("Confirmação", "Deseja realmente desativar este funcionário? Ele perderá o acesso ao sistema, mas o histórico será preservado.", "Sim", "Não");

            if (!confirmar) return;

            try
            {
                using var client = new HttpClient();
                string url = $"https://ponto-system.onrender.com/api/Funcionarios/{funcionarioId}/desativar";

                // Envia a requisição PATCH para desativar
                var request = new HttpRequestMessage(HttpMethod.Patch, url);
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Sucesso", "Funcionário desativado com sucesso.", "OK");
                    await CarregarEquipe(); // Atualiza a lista
                }
                else
                {
                    await DisplayAlert("Erro", "Não foi possível desativar o funcionário.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Falha de conexão: {ex.Message}", "OK");
            }
        }
    }

    // AÇÃO: Criar Novo Funcionário (Prompt simples ou navegação)
    private async void OnNovoFuncionarioClicked(object? sender, EventArgs e)
    {
        // Solicita os dados básicos para cadastro rápido
        string nome = await DisplayPromptAsync("Novo Funcionário", "Digite o nome completo:");
        if (string.IsNullOrWhiteSpace(nome)) return;

        string cpf = await DisplayPromptAsync("Novo Funcionário", "Digite o CPF (apenas números):");
        if (string.IsNullOrWhiteSpace(cpf)) return;

        string senha = await DisplayPromptAsync("Novo Funcionário", "Defina uma senha de acesso:");
        if (string.IsNullOrWhiteSpace(senha)) return;

        try
        {
            var novoFuncionario = new
            {
                Nome = nome,
                Cpf = cpf,
                Senha = senha,
                CargoId = 2, // Cargo comum / colaborador
                Ativo = true
            };

            using var client = new HttpClient();
            string url = "https://ponto-system.onrender.com/api/Funcionarios";

            var response = await client.PostAsJsonAsync(url, novoFuncionario);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Sucesso", "Funcionário cadastrado com sucesso!", "OK");
                await CarregarEquipe();
            }
            else
            {
                string erro = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Erro", $"Não foi possível criar: {erro}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha de conexão: {ex.Message}", "OK");
        }
    }
    public class EquipeStatusDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool TemAjustePendente { get; set; }
    }
}