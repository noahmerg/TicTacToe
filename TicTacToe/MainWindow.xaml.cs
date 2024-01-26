using AI;
using AI.QLearning;
using System.Diagnostics;
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
            InitiializeGame();
            InitializeAI();
            InitializeTimer();
            InitializeGUI();
        }
        #endregion

        #region --- Init Functions ---
        private void InitiializeGame()
        {
            Game = new TicTacToeGame();
        }
        private void InitializeAI()
        {
            AI_y = new QLearningAI()
            {
                GameState = Game,
            };
            AI_x = new QLearningAI()
            {
                GameState = Game,
            };
        }
        private void InitializeTimer()
        {
            if (null != Timer)
            {
                Timer.Stop();
            }
            else
            {
                Timer = new DispatcherTimer();
                Timer.Tick += LearnStep;
            }
        }
        private void InitializeGUI()
        {
            ModeSelect.Items.Clear();
            ModeSelect.Items.Add("Player vs AI");
            ModeSelect.Items.Add("AI vs AI");
            ModeSelect.SelectedIndex = 0;
            SetMode(0);
        }
        #endregion

        #region --- Update functions
        private void UpdateProgessBar()
        {
            Progress.Value = CurrentIteration * 100 / (MaxNumIterations);
        }

        private void UpdateButtons()
        {
            foreach (var control in GameBoard.Children)
            {
                if (control is Button button)
                {
                    button.Content = Game.GetField(Grid.GetColumn(button), Grid.GetRow(button));
                }
            }
        }
        #endregion

        #region --- AI functions ---
        private void StartTimer(double seconds)
        {
            Timer.Stop();
            Timer.Interval = TimeSpan.FromSeconds(seconds);
            Timer.Start();
        }
        private async void LearnStep(object sender, EventArgs e)
        {
            QLearningAI currAI = Game.CurrentPlayer == "X" ? AI_x : AI_y;

            UpdateAIValues();

            currAI.Learn(1);

            CurrentIteration++;
            UpdateButtons();
            UpdateProgessBar();

            // if Game Has Ended
            Debug.WriteLine(Game.gameHasEnded);
            if (Game.gameHasEnded)
            {
                Reset();
            }
        }

        private void UpdateAIValues()
        {
            int LearnPhase = MaxNumIterations / 4;

            if (CurrentIteration < LearnPhase)
            {
                AI_x.LearningRate = 0.5;
                AI_x.ExplorationRate = 1.0;
                AI_y.LearningRate = 0.5;
                AI_y.ExplorationRate = 1.0;
            }
            else if (CurrentIteration < 2 * LearnPhase)
            {
                AI_x.LearningRate = 0.4;
                AI_x.ExplorationRate = 0.7;
                AI_y.LearningRate = 0.4;
                AI_y.ExplorationRate = 0.7;
            }
            else if (CurrentIteration < 3 * LearnPhase)
            {
                AI_x.LearningRate = 0.3;
                AI_x.ExplorationRate = 0.5;
                AI_y.LearningRate = 0.3;
                AI_y.ExplorationRate = 0.5;
            }
            else if (CurrentIteration < 4 * LearnPhase)
            {
                AI_x.LearningRate = 0.2;
                AI_x.ExplorationRate = 0.3;
                AI_y.LearningRate = 0.2;
                AI_y.ExplorationRate = 0.3;
            }
            else
            {
                AI_x.ExplorationRate = 0.0;
                AI_x.LearningRate = 0.0;
                AI_y.ExplorationRate = 0.0;
                AI_y.LearningRate = 0.0;
                Timer.Interval = TimeSpan.FromSeconds(0.5);
            }
        }
        #endregion


        #region --- Button Listener ---
        private void ButtonListener(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            if (!String.IsNullOrWhiteSpace(button.Content?.ToString())) return;

            var row = Grid.GetRow(button);
            var column = Grid.GetColumn(button);

            Game.TryExecuteAction(column, row);

            if (!Game.gameHasEnded) AI_y.Learn(1);
            if (Game.gameHasEnded) Reset();
            UpdateButtons();
        }
        private void StartTraining(object sender, RoutedEventArgs e)
        {
            CurrentIteration = 0;
            uint n = UInt32.Parse(NumIterations.Text);
            MaxNumIterations = (int)n;
            double d = Double.Parse(DiscountRate.Text);
            AI_x.DiscountRate = (double)d;
            AI_y.DiscountRate = (double)d;
            StartTimer(0.0001);
        }
        private void ModeSelected(object sender, RoutedEventArgs e)
        {
            SetMode(ModeSelect.SelectedIndex);
            Reset();
        }
        private void btnNewGame_Click(object sender, RoutedEventArgs e)
        {
            winnerText.Text = "";
            Reset();
        }
        #endregion

        #region --- Helper Functions ---

        private void Reset()
        {
            ClearButtons();
            Game.Reset();
            UpdateButtons();
        }
        
        private void SetMode(int modeIndex)
        {
            if(modeIndex == 0) // Player vs AI doesnt need Discount, Num Iterations, Train, etc.
            {
                Timer.Stop();
                DiscountRateLabel.Visibility = Visibility.Hidden;
                DiscountRate.Visibility = Visibility.Hidden;
                DiscountRate.IsEnabled = false;
                NumIterationsLabel.Visibility = Visibility.Hidden;
                NumIterations.Visibility = Visibility.Hidden;
                NumIterations.IsEnabled = false;
                Train.Visibility = Visibility.Hidden;
                Progress.Visibility = Visibility.Hidden;
                enableButtons();
            }
            else if (modeIndex == 1)// AI vs AI (training)
            {
                DiscountRateLabel.Visibility = Visibility.Visible;
                DiscountRate.Visibility = Visibility.Visible;
                DiscountRate.IsEnabled = true;
                NumIterationsLabel.Visibility = Visibility.Visible;
                NumIterations.Visibility = Visibility.Visible;
                NumIterations.IsEnabled = true;
                Train.Visibility = Visibility.Visible;
                Progress.Visibility = Visibility.Visible;
                disableButtons();
            }
        }

        private void enableButtons()
        {
            foreach (var control in GameBoard.Children)
            {
                if (control is Button btn)
                {
                    btn.IsEnabled = true;
                }
            }
        }

        private void disableButtons()
        {
            foreach (var control in GameBoard.Children)
            {
                if (control is Button btn)
                {
                    // clear each Cell
                    btn.IsEnabled = false;
                }
            }
        }
        public void ClearButtons()
        {
            foreach (var control in GameBoard.Children)
            {
                if (control is Button btn)
                {
                    // clear each Cell
                    btn.Content = String.Empty;
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
        private int MaxNumIterations = 100000;
        private int CurrentIteration = 0;

        private QLearningAI AI_x;
        private QLearningAI AI_y;
        private DispatcherTimer Timer;
        private TicTacToeGame Game;
        #endregion
    }
}