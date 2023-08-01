using System.Collections.ObjectModel;
using System.Windows.Input;
using PuzzleGame.Models;
using PuzzleGame.Views;

namespace PuzzleGame.ViewModel
{
    public class OnboardingViewModel : BaseViewModel
	{
        #region public variables
        public ObservableCollection<Onboarding> Items
        {
            get => items;
            set => SetProperty(ref items, value);
        }
        public string NextButtonText
        {
            get => nextButtonText;
            set => SetProperty(ref nextButtonText, value);
        }
        public string SkipButtonText
        {
            get => skipButtonText;
            set => SetProperty(ref skipButtonText, value);
        }
        public int Position
        {
            get => position;
            set
            {
                if (SetProperty(ref position, value))
                {
                    UpdateNextButtonText();
                }
            }
        }

        public ICommand NextCommand { get; private set; }
        public ICommand SkipCommand { get; private set; }
        #endregion

        #region private variables
        private ObservableCollection<Onboarding> items;

        private string nextButtonText;
        private string skipButtonText;
        private int position;

        private string Skip = "SAIR";
        private string Next = "PRÓXIMO";
        private string GotIt = "CONTINUAR";
        #endregion

        public OnboardingViewModel()
        {
            SetNextButtonText(Next);
            SetSkipButtonText(Skip);
            OnBoarding();
            LaunchNextCommand();
            SkipCommand = new Command(ExitOnBoarding);
        }

        private void SetNextButtonText(string nextButtonText) => NextButtonText = nextButtonText;
        private void SetSkipButtonText(string skipButtonText) => SkipButtonText = skipButtonText;
        private bool LastPositionReached() => Position == Items.Count - 1;

        private void UpdateNextButtonText()
        {
            if (LastPositionReached())
                SetNextButtonText(GotIt);
            else
                SetNextButtonText(Next);
        }

        private void OnBoarding()
        {
            Items = new ObservableCollection<Onboarding>
            {
                new Onboarding
                {
                    Title = "Bem-vindo",
                    Content = "Movimente as peças do quebra-cabeça para reorganizá-las em ordem crescente. \n O objetivo é colocar as peças em ordem crescente, deixando o espaço vazio no último lugar.",
                    ImageUrl = ""
                },
                new Onboarding
                {
                    Title = "",
                    Content = "Clique nos botões adjacentes ao espaço vazio para movimentar as peças. \n Quando todas as peças estiverem em ordem, você completou o nível!",
                    ImageUrl = "",
                    NotTitle = false
                },
                new Onboarding
                {
                    Title = "",
                    Content = "Você pode aumentar o nível para desafiar-se ainda mais.",
                    ImageUrl = "",
                    NotTitle = false
                }
            };
        }

        private void LaunchNextCommand()
        {
            NextCommand = new Command(() =>
            {
                if (LastPositionReached())
                    ExitOnBoarding();
                else
                    MoveToNextPosition();
            });
        }

        private static void ExitOnBoarding()
        {
            Application.Current.MainPage = new NavigationPage(new PuzzleGamePage());
        }

        private void MoveToNextPosition()
        {
            var nextPosition = ++Position;
            Position = nextPosition;
        }
    }
}