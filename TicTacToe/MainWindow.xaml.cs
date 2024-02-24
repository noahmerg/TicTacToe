using AI;
using AI.QLearning;
using System.Collections;
using System.Collections.Generic;
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
            Game.PossibleWinReward = Double.Parse(PossibleWinReward.Text);
            Game.PossibleLossReward = Double.Parse(PossibleLossReward.Text);
            Game.WinReward = Double.Parse(WinReward.Text);
            Game.DrawReward = Double.Parse(DrawReward.Text);
        }
        private void InitializeAI()
        {
            AI_o = new QLearningAI()
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
            ModeSelect.Items.Add("Player vs Player");
            ModeSelect.SelectedIndex = 0;
            SetMode(0);
        }
        #endregion

        #region --- 1functions
        private void UpdateProgessBar()
        {
            Progress.Value = CurrentIteration * 100 / (MaxNumIterations);
        }
        private void UpdateStatistics()
        {
            int TotalGames = NumberOfDraws + NumberOfOWins + NumberOfXWins;
            if (TotalGames == 0) return;
            if(Mode == 0)
            {
                xWinsLabel.Content = $"Player1 (X) Wins: {NumberOfXWins}";
                oWinsLabel.Content = $"AI2 (O) Wins: {NumberOfOWins}";
            }
            else if(Mode == 1)
            {
                xWinsLabel.Content = $"AI1 (X) Wins: {NumberOfXWins}";
                oWinsLabel.Content = $"AI2 (O) Wins: {NumberOfOWins}";
            }
            else if (Mode == 2)
            {
                xWinsLabel.Content = $"Player1 (X) Wins: {NumberOfXWins}";
                oWinsLabel.Content = $"Player2 (O) Wins: {NumberOfOWins}";
            }
            drawsLabel.Content = $"Draws: {NumberOfDraws}";
            totalLabel.Content = $"Total: {TotalGames}";
            xWins.Value = (NumberOfXWins * 100 / TotalGames);
            draws.Value = NumberOfDraws * 100 / TotalGames + xWins.Value;
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
            QLearningAI currAI = Game.CurrentPlayer == "X" ? AI_x : AI_o;

            UpdateAIValues();

            if (Game.CurrentPlayer == "X" || AI2Random.IsChecked == false)
            {
                currAI.Learn(1);
            }
            else
            {
                executeRandomAction();
            }
            
            CurrentIteration++;
            UpdateButtons();
            UpdateProgessBar();

            // if Game Has Ended
            if (Game.gameHasEnded)
            {
                string winnerString;
                if(Game.CheckWinner("X"))
                {
                    winnerString = "X Wins";
                    NumberOfXWins++;
                }
                else if (Game.CheckWinner("O"))
                {
                    winnerString = "O Wins";
                    NumberOfOWins++;
                }
                else
                {
                    winnerString = "Draw";
                    NumberOfDraws++;
                }
                if (finishedAiTraining)
                {
                    winnerText.Text = winnerString;
                }
                UpdateStatistics();
                Reset();
            }
        }

        // Function inspired by code provided by Professor Cristof Rezk-Salama (CRS | C.Rezk-Salama@hochschule-trier.de)
        private void UpdateAIValues()
        {
            int LearnPhase = MaxNumIterations / 4;

            if (CurrentIteration < LearnPhase)
            {
                AI_x.LearningRate = 0.6;
                AI_x.ExplorationRate = 1.0;
                AI_o.LearningRate = 0.6;
                AI_o.ExplorationRate = 1.0;
            }
            else if (CurrentIteration < 2 * LearnPhase)
            {
                AI_x.LearningRate = 0.5;
                AI_x.ExplorationRate = 0.5;
                AI_o.LearningRate = 0.5;
                AI_o.ExplorationRate = 0.5;
            }
            else if (CurrentIteration < 3 * LearnPhase)
            {
                AI_x.LearningRate = 0.3;
                AI_x.ExplorationRate = 0.3;
                AI_o.LearningRate = 0.3;
                AI_o.ExplorationRate = 0.3;
            }
            else if (CurrentIteration < 4 * LearnPhase)
            {
                AI_x.LearningRate = 0.2;
                AI_x.ExplorationRate = 0.1;
                AI_o.LearningRate = 0.2;
                AI_o.ExplorationRate = 0.1;
            }
            else
            {
                finishedAiTraining = true;
                AI_x.ExplorationRate = 0.0;
                AI_x.LearningRate = 0.0;
                AI_o.ExplorationRate = 0.0;
                AI_o.LearningRate = 0.0;
                Timer.Interval = TimeSpan.FromSeconds(0.5);
            }
        }

        private void executeRandomAction()
        {
            Random Rand = new Random();
            List<IAction> possibleActions = Game.PossibleActions;
            Position randAction = (Position)possibleActions[Rand.Next() % Game.PossibleActions.Count];
            Game.TryExecuteAction(randAction.x, randAction.y);
        }
        #endregion


        #region --- Button Listener ---
        // listens on every button for input
        // on input the game trys to execute the spefici action
        // if the game hasnt end yet, AI has a turn
        // if game has end, text is updated and the board is resetted after a short delay
        private async void ButtonListener(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            if (!String.IsNullOrWhiteSpace(button.Content?.ToString())) return;

            var row = Grid.GetRow(button);
            var column = Grid.GetColumn(button);

            Game.TryExecuteAction(column, row);

            if (Mode == 0 && !Game.gameHasEnded) AI_o.Learn(1);
            UpdateButtons();
            if (Mode == 2 && !Game.gameHasEnded) return;
            if (Game.gameHasEnded)
            {
                if (Game.CheckWinner("X"))
                {
                    winnerText.Text = "X Wins";
                    NumberOfXWins++;
                }
                else if (Game.CheckWinner("O"))
                {
                    winnerText.Text = "O Wins";
                    NumberOfOWins++;
                }
                else
                {
                    winnerText.Text = "Draw";
                    NumberOfDraws++;
                }
                disableButtons();
                UpdateStatistics();
                await Task.Delay(1000);
                enableButtons();
                Reset();
            }
            UpdateButtons();
        }
        private void StartTraining(object sender, RoutedEventArgs e)
        {
            CurrentIteration = 0;
            uint n = UInt32.Parse(NumIterations.Text);
            MaxNumIterations = (int)n;
            double d = Double.Parse(DiscountRate.Text);
            AI_x.DiscountRate = (double)d;
            AI_o.DiscountRate = (double)d;
            StartTimer(0.0001);
            winnerText.Text = "";
            finishedAiTraining = false;
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
        private void PossibleWinReward_TextChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                double d0 = Double.Parse(tb.Text);
                double d1 = Math.Min(Math.Max(d0, -1.0), 1.0);
                if (d0 != d1)
                {
                    tb.Text = "" + d1;
                }
                if (Game != null) Game.PossibleWinReward = d1;
            }
        }

        private void PossibleLossReward_TextChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                double d0 = Double.Parse(tb.Text);
                double d1 = Math.Min(Math.Max(d0, -1.0), 1.0);
                if (d0 != d1)
                {
                    tb.Text = "" + d1;
                }
                if(Game != null) Game.PossibleLossReward = d1;
            }
        }

        private void WinReward_TextChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                double d0 = Double.Parse(tb.Text);
                double d1 = Math.Min(Math.Max(d0, -1.0), 1.0);
                if (d0 != d1)
                {
                    tb.Text = "" + d1;
                }
                if(Game != null) Game.WinReward = d1;
            }
        }

        private void DrawReward_TextChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                double d0 = Double.Parse(tb.Text);
                double d1 = Math.Min(Math.Max(d0, -1.0), 1.0);
                if (d0 != d1)
                {
                    tb.Text = "" + d1;
                }
                if (Game != null) Game.DrawReward = d1;
            }
        }
        #endregion

        #region --- Helper Functions ---
        private void Reset()
        {
            Game.Reset();
            ClearButtons();
        }
        
        private void SetMode(int modeIndex)
        {
            if (modeIndex == 0) // Player vs AI
            {
                winnerText.Text = String.Empty;
                Timer.Stop();
                DiscountRateLabel.Visibility = Visibility.Hidden;
                DiscountRate.Visibility = Visibility.Hidden;
                DiscountRate.IsEnabled = false;
                NumIterationsLabel.Visibility = Visibility.Hidden;
                NumIterations.Visibility = Visibility.Hidden;
                NumIterations.IsEnabled = false;
                Train.Visibility = Visibility.Hidden;
                Progress.Visibility = Visibility.Hidden;
                AI2Random.Visibility = Visibility.Visible;
                PossibleWinRewardLabel.Visibility = Visibility.Visible;
                PossibleWinReward.Visibility = Visibility.Visible;
                PossibleLossRewardLabel.Visibility = Visibility.Visible;
                PossibleLossReward.Visibility = Visibility.Visible;
                PossibleWinReward.Visibility = Visibility.Visible;
                WinRewardLabel.Visibility = Visibility.Visible;
                WinReward.Visibility = Visibility.Visible;
                DrawRewardLabel.Visibility = Visibility.Visible;
                DrawReward.Visibility = Visibility.Visible;
                enableButtons();
                resetStatistics();
                xWinsLabel.Content = $"Player1 (X) Wins: {NumberOfXWins}";
                oWinsLabel.Content = $"AI2 (O) Wins: {NumberOfOWins}";
                Mode = 0;
            }
            else if (modeIndex == 1)// AI vs AI
            {
                winnerText.Text = String.Empty;
                DiscountRateLabel.Visibility = Visibility.Visible;
                DiscountRate.Visibility = Visibility.Visible;
                DiscountRate.IsEnabled = true;
                NumIterationsLabel.Visibility = Visibility.Visible;
                NumIterations.Visibility = Visibility.Visible;
                NumIterations.IsEnabled = true;
                Train.Visibility = Visibility.Visible;
                Progress.Visibility = Visibility.Visible;
                AI2Random.Visibility = Visibility.Visible;
                PossibleWinRewardLabel.Visibility = Visibility.Visible;
                PossibleWinReward.Visibility = Visibility.Visible;
                PossibleLossRewardLabel.Visibility = Visibility.Visible;
                PossibleLossReward.Visibility = Visibility.Visible;
                PossibleWinReward.Visibility = Visibility.Visible;
                WinRewardLabel.Visibility = Visibility.Visible;
                WinReward.Visibility = Visibility.Visible;
                DrawRewardLabel.Visibility = Visibility.Visible;
                DrawReward.Visibility = Visibility.Visible;
                disableButtons();
                resetStatistics();
                Mode = 1;
                xWinsLabel.Content = $"AI1 (X) Wins: {NumberOfXWins}";
                oWinsLabel.Content = $"AI2 (O) Wins: {NumberOfOWins}";
            }
            else if (modeIndex == 2) // Player vs Player
            {
                winnerText.Text = String.Empty;
                Timer.Stop();
                DiscountRateLabel.Visibility = Visibility.Hidden;
                DiscountRate.Visibility = Visibility.Hidden;
                DiscountRate.IsEnabled = false;
                NumIterationsLabel.Visibility = Visibility.Hidden;
                NumIterations.Visibility = Visibility.Hidden;
                NumIterations.IsEnabled = false;
                Train.Visibility = Visibility.Hidden;
                Progress.Visibility = Visibility.Hidden;
                AI2Random.Visibility = Visibility.Hidden;
                PossibleWinRewardLabel.Visibility = Visibility.Hidden;
                PossibleWinReward.Visibility = Visibility.Hidden;
                PossibleLossRewardLabel.Visibility= Visibility.Hidden;
                PossibleLossReward.Visibility = Visibility.Hidden;
                WinRewardLabel.Visibility = Visibility.Hidden;
                WinReward.Visibility = Visibility.Hidden;
                DrawRewardLabel.Visibility = Visibility.Hidden;
                DrawReward.Visibility = Visibility.Hidden;
                enableButtons();
                resetStatistics();
                Mode = 2;
                xWinsLabel.Content = $"Player1 (X) Wins: {NumberOfXWins}";
                oWinsLabel.Content = $"Player2 (O) Wins: {NumberOfOWins}";
            }
        }

        private void resetStatistics()
        {
            NumberOfXWins = 0;
            NumberOfOWins = 0;
            NumberOfDraws = 0;  
            xWinsLabel.Content = $"X Wins: 0";
            oWinsLabel.Content = $"O Wins: 0";
            drawsLabel.Content = $"Draws: 0";
            totalLabel.Content = $"Total: 0";
            xWins.Value = 33;
            draws.Value = 66;
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
                    btn.Content = String.Empty;
                }
            }
        }

        // Original code provided by Professor Cristof Rezk-Salama (CRS | C.Rezk-Salama@hochschule-trier.de)
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

        private void ValidateNegativePositiveOne(object sender, RoutedEventArgs e)
        {
            if(sender is TextBox tb)
            {
                double d0 = Double.Parse(tb.Text);
                double d1 = d0;
                if (d1 < -1.0) d1 = -1.0;
                if (d1 > 1.0) d1 = 1.0;
                if (d0 != d1) tb.Text = "" + d1;
            }
        }

        // Original code provided by Professor Cristof Rezk-Salama (CRS | C.Rezk-Salama@hochschule-trier.de)
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
        private bool finishedAiTraining = false;

        private int NumberOfXWins = 0;
        private int NumberOfOWins = 0;
        private int NumberOfDraws = 0;

        private int Mode = 0;

        private QLearningAI AI_x;
        private QLearningAI AI_o;
        private DispatcherTimer Timer;
        private TicTacToeGame Game;
        #endregion

    }
}