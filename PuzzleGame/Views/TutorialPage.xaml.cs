namespace PuzzleGame.Views
{
    public partial class TutorialPage : ContentPage
    {
        public TutorialPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private void OnNextPageClicked(object sender, EventArgs e)
        {
            Application.Current.MainPage = new NavigationPage(new PuzzleGamePage());
        }
    }
}