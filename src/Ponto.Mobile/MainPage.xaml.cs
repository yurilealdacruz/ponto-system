namespace Ponto.Mobile
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnRegistrarPontoClicked(object sender, EventArgs e)
        {
            // Mais para frente, é aqui que faremos a chamada HTTP para a sua Ponto.Api!
            StatusLabel.Text = $"Ponto registrado às {DateTime.Now:HH:mm:ss}";
        }
    }
}