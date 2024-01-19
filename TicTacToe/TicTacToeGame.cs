using AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe
{
    public class TicTacToeGame : IGameState
    {
        public string CurrentPlayer { get; set; }


        private const string x = "X";
        private const string o = "O";
        private GameBoard gameBoard;
        public TicTacToeGame() 
        {
            gameBoard = new GameBoard(3); 
            double randomValue = new Random().NextDouble();
            System.Diagnostics.Debug.WriteLine("Random Value: " + randomValue);
            CurrentPlayer = randomValue < 0.5 ? x : o;
        }

        public TicTacToeGame(string startingPlayer)
        {
            gameBoard = new GameBoard(3);
            if(startingPlayer != x || startingPlayer != o)
            {
                throw new ArgumentException("Invalid starting Player");
            }
            CurrentPlayer = startingPlayer;
        }

        public void SetNextPlayer()
        { 
            // if x => o else o => x
            CurrentPlayer = CurrentPlayer == x ? o : x;
        }
        #region --- Winner and Draw Checks ---
        public Boolean CheckWinner(string player)
        {
            return CheckWinnerRows(player) || CheckWinnerColumns(player) || CheckWinnerDiagonal(player);
        }

        public Boolean CheckForFullBoard()
        {
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    if (string.IsNullOrEmpty(gameBoard.getField(i, j)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        #region ---private Checker---
        private Boolean CheckWinnerRows(string player)
        {
            for (int i = 0; i < 3; i++)
            {
                if (gameBoard.getField(i, 0) == player && gameBoard.getField(i, 1) == player && gameBoard.getField(i, 2) == player) return true;
            }
            return false;
        }

        private Boolean CheckWinnerColumns(string player)
        {
            for (int i = 0; i < 3; ++i)
            {
                if (gameBoard.getField(0, i) == player && gameBoard.getField(1, i) == player && gameBoard.getField(2, i) == player) return true;
            }
            return false;
        }

        private Boolean CheckWinnerDiagonal(string player)
        {
            // check negative diagonal
            if (gameBoard.getField(0, 0) == player && gameBoard.getField(1, 1) == player && gameBoard.getField(2, 2) == player) return true;
            // check positive diagonal
            if (gameBoard.getField(0, 2) == player && gameBoard.getField(1, 1) == player && gameBoard.getField(2, 0) == player) return true;

            return false;
        }
        #endregion
        #endregion

        #region --- IGameState Implementation ---
        public uint Id
        {
            get
            {
                return 0;
            }
        }

        public List<IAction> PossibleActions
        {
            get
            {
                List<IAction> actions = new List<IAction>();
                for(int x = 0; x < 3; x++)
                {
                    for(int y = 0; y < 3; y++)
                    {
                        if(string.IsNullOrEmpty(gameBoard.getField(x,y)))
                        {
                            actions.Add(new Position { x = x, y = y});
                        }
                    }
                }
                return actions;
            }
        }
        public double ExecuteAction(IAction a)
        {
            if (a is Position position)
            {
                gameBoard.SetField(CurrentPlayer, position);
                if (CheckWinner(CurrentPlayer))
                {
                    return WINNER_REWARD;
                }
                else if (CheckWinner(CurrentPlayer == x ? o : x))
                {
                    return LOSING_REWARD;
                }
                else if (CheckForFullBoard())
                {
                    return DRAW_REWARD;
                }
                return 0;
            }
            throw new ArgumentException("Action must be a Positon");
        }
        #endregion

        #region --- Rewards ---
        double WINNER_REWARD = 1.0;
        double LOSING_REWARD = -1.0;
        double DRAW_REWARD = 0.0;
        #endregion
    }
}
