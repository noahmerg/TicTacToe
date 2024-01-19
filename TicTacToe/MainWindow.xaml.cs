using AI;
using AI.QLearning;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

// clean up code structure, seems like everything is calling everything as if its all a big band-aid solution
namespace TicTacToe
{
    public partial class MainWindow : Window
    {
        #region --- Constructor ---
        public MainWindow()
        {
            InitializeComponent();
            InitializeGUI();
            InitializeGame();
            StartGame();
        }
        #endregion

        private void StartGame()
        {
            if (Mode == 0)
            {
                StartPlayerVsAi();
            }
            else if (Mode == 1)
            {
                StartAiVsAi();
            }
        }

        private void StartPlayerVsAi()
        {
            if (TicTacToe.CurrentPlayer == "O")
            {
                makeAIMove();
            }
        }
        private void StartAiVsAi()
        {

        }

        #region --- Init Functions ---
        private void InitializeGUI()
        {
            ModeSelect.Items.Clear();
            ModeSelect.Items.Add("Player vs AI");
            ModeSelect.Items.Add("AI vs AI");
            ModeSelect.SelectedIndex = 0;
            SetMode(0);
        }

        private void InitializeGame()
        {
            clearButtons();
            // make a new Game entirely
            TicTacToe = new TicTacToeGame();
        }
        #endregion

        #region --- Listener ---
        // Method gets called everytime a "button" is clicked
        private void PlayerClicksSpace(object sender, RoutedEventArgs e)
        {
            var space = (Button)sender;
            if (!String.IsNullOrWhiteSpace(space.Content?.ToString())) return;
            space.Content = TicTacToe.CurrentPlayer;

            var coordinates = space.Tag.ToString().Split(',');
            var x = int.Parse(coordinates[0]);
            var y = int.Parse(coordinates[1]);
            var clickedField = new Position() { x = x, y = y };
            TicTacToe.ExecuteAction(clickedField);

            // Check if a player has won
            if (TicTacToe.CheckWinner(TicTacToe.CurrentPlayer))
            {
                winnerText.Text = $"{TicTacToe.CurrentPlayer} WINS!";
                InitializeGame();
                StartGame();
                return;
            }
            // or if it's a draw
            else if (TicTacToe.CheckForFullBoard())
            {
                winnerText.Text = "DRAW!";
                InitializeGame();
                StartGame();
                return;
            }

            TicTacToe.SetNextPlayer();
            if (TicTacToe.CurrentPlayer == "O")
            {
                makeAIMove();
            }
        }

        private void ModeSelected(object sender, RoutedEventArgs e)
        {
            SetMode(ModeSelect.SelectedIndex);
        }

        // when clicked on new Game a new game is init (might need to be changed later for AI Reasons)
        private void btnNewGame_Click(object sender, RoutedEventArgs e)
        {
            InitializeGame();
            winnerText.Text = "";
            StartGame();
        }
        #endregion

        #region --- Helper Functions ---
        private async void makeAIMove()
        {
            disableButtons();
            List<IAction> possibleActions = TicTacToe.PossibleActions;
            Position randomAction = (Position) possibleActions[new Random().Next(possibleActions.Count)];
            foreach (var control in gameBoard.Children)
            {
                if (control is Button button)
                {
                    if (button.Tag != null && button.Tag.ToString() == $"{randomAction.x},{randomAction.y}")
                    {
                        await Task.Delay(1000);
                        button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                        break;
                    }
                }
            }
            enableButtons();
        }

        private void SetMode(int modeIndex)
        {
            if(modeIndex == 0) // Player vs AI doesnt need Discount, Num Iterations, Train, etc.
            {
                Mode = 0;
                DiscountRateLabel.Visibility = Visibility.Hidden;
                DiscountRate.Visibility = Visibility.Hidden;
                NumIterationsLabel.Visibility = Visibility.Hidden;
                NumIterations.Visibility = Visibility.Hidden;
                Train.Visibility = Visibility.Hidden;
                Progress.Visibility = Visibility.Hidden;
                enableButtons();
            }
            else // AI vs AI (training)
            {
                Mode = 1;
                DiscountRateLabel.Visibility = Visibility.Visible;
                DiscountRate.Visibility = Visibility.Visible;
                NumIterationsLabel.Visibility = Visibility.Visible;
                NumIterations.Visibility = Visibility.Visible;
                Train.Visibility = Visibility.Visible;
                Progress.Visibility = Visibility.Visible;
                disableButtons();
            }
            InitializeGame();
        }
        private void enableButtons()
        {
            foreach (var control in gameBoard.Children)
            {
                if (control is Button)
                {
                    ((Button)control).IsEnabled = true;
                }
            }
        }

        private void disableButtons()
        {
            foreach (var control in gameBoard.Children)
            {
                if (control is Button)
                {
                    // clear each Cell
                    ((Button)control).IsEnabled = false;
                }
            }
        }
        private void clearButtons()
        {
            foreach (var control in gameBoard.Children)
            {
                if (control is Button)
                {
                    // clear each Cell
                    ((Button)control).Content = String.Empty;
                }
            }
        }
        private void ValidateZeroOne(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                double d0 = Double.Parse(tb.Text);
                double d1 = d0;
                if (d1 < 0.0001) d1 = 0.0001;
                if (d1 > 0.9999) d1 = 0.9999;
                if (d0 != d1) tb.Text = "" + d1;
            }
        }

        private void ValidatePositiveInt(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                bool ok = UInt32.TryParse(tb.Text, out uint d0);
                if (!ok) d0 = 1000;
                tb.Text = "" + d0;
            }
        }
        #endregion

        #region --- Private Member ---
        private TicTacToeGame TicTacToe;
        private int Mode;
        #endregion
    }
}