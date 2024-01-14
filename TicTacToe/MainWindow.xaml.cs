using AI.QLearning;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TicTacToe
{
    public partial class MainWindow : Window
    {
        #region --- Constructor ---
        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
            InitializeGUI();
        }
        #endregion

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
            foreach (var control in gameBoard.Children)
            {
                if (control is Button)
                {
                    // clear each Cell
                    ((Button)control).Content = String.Empty;
                }
            }
            // make a new Game entirely
            _Game = new Game();
        }
        #endregion

        #region --- Listener ---
        private void ModeSelected(object sender, RoutedEventArgs e)
        {

            SetMode(ModeSelect.SelectedIndex);
        }
        private void PlayerClicksSpace(object sender, RoutedEventArgs e)
        {
            var space = (Button)sender;
            if (!String.IsNullOrWhiteSpace(space.Content?.ToString())) return;
            space.Content = _Game.CurrentPlayer;

            var coordinates = space.Tag.ToString().Split(',');
            var x = int.Parse(coordinates[0]);
            var y = int.Parse(coordinates[1]);
            var clickedField = new Position() { x = x, y = y };
            _Game.UpateBoard(clickedField, _Game.CurrentPlayer);

            if (_Game.PlayerWin())
            {
                winnerText.Text = $"{_Game.CurrentPlayer} WINS!";
                InitializeGame();
                return;
            }
            
            if (_Game.IsDraw())
            {
                winnerText.Text = "DRAW!";
                InitializeGame();
                return;
            }

            _Game.SetNextPlayer();
        }

        private void btnNewGame_Click(object sender, RoutedEventArgs e)
        {
            InitializeGame();
            winnerText.Text = "";
        }
        #endregion

        #region --- Helper Functions ---
        private void SetMode(int Mode)
        {
            if(Mode == 0)
            {
                DiscountRateLabel.Visibility = Visibility.Hidden;
                DiscountRate.Visibility = Visibility.Hidden;
                NumIterationsLabel.Visibility = Visibility.Hidden;
                NumIterations.Visibility = Visibility.Hidden;
                Train.Visibility = Visibility.Hidden;
                Progress.Visibility = Visibility.Hidden;
            }
            else
            {
                DiscountRateLabel.Visibility = Visibility.Visible;
                DiscountRate.Visibility = Visibility.Visible;
                NumIterationsLabel.Visibility = Visibility.Visible;
                NumIterations.Visibility = Visibility.Visible;
                Train.Visibility = Visibility.Visible;
                Progress.Visibility = Visibility.Visible;
            }
            InitializeGame();
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
        private Game _Game;
        private QLearningAI LearningAI;
        #endregion
    }
}