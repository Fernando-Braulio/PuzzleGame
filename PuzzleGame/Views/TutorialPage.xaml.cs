namespace PuzzleGame.Views
{
    public partial class TutorialPage : ContentPage
    {
        public TutorialPage()
        {
            InitializeComponent();
        }

        private void OnNextPageClicked(object sender, EventArgs e)
        {
            // Adicione a página PuzzleGamePage como a página principal usando a navegação do NavigationPage
            Application.Current.MainPage = new NavigationPage(new PuzzleGamePage());
        }
    }
}