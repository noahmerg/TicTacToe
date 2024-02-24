using AI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Xaml;

namespace TicTacToe
{
    public class TicTacToeGame : IGameState
    {
        public string CurrentPlayer { get; set; }

        private const string x = "X";
        private const string o = "O";
        private GameBoard gameBoard;
        private Position[] allActions;
        public bool gameHasEnded { get; private set; }
        public double PossibleWinReward { get; set; } = 0;
        public double PossibleLossReward { get; set; } = 0;
        public double WinReward { get; set; } = 0;
        public double DrawReward { get; set; } = 0;

        public TicTacToeGame() 
        {
            gameHasEnded = false;
            gameBoard = new GameBoard(3);
            // set x as startingplayer
            CurrentPlayer = x;
            // add Positions to allActions so we dont need to create new Objects which would destroy the algorithm (yes I did this and didnt notice)
            allActions = new Position[9];
            allActions[0] = new Position { Name = "Top Left", x = 0, y = 0};
            allActions[1] = new Position { Name = "Top Middle", x = 1, y = 0 };
            allActions[2] = new Position { Name = "Top Right", x = 2, y = 0 };
            allActions[3] = new Position { Name = "Middle Left", x = 0, y = 1 };
            allActions[4] = new Position { Name = "Middle Middle", x = 1, y = 1 };
            allActions[5] = new Position { Name = "Middle Right", x = 2, y = 1 };
            allActions[6] = new Position { Name = "Bottom Left", x = 0, y = 2 };
            allActions[7] = new Position { Name = "Bottom Middle", x = 1, y = 2 };
            allActions[8] = new Position { Name = "Bottom Right", x = 2, y = 2 };

            /*      x0 x1 x2
             *   y0 [] [] []
             *   y1 [] [] []
             *   y2 [] [] []
             */
        }

        // x => o OR o => x
        public void SetNextPlayer()
        { 
            CurrentPlayer = CurrentPlayer == x ? o : x;
        }

        // Resets gameBoard and currentPlayer
        public void Reset()
        {
            gameBoard.Reset();
            gameHasEnded = false;
            // set x as startingplayer
            CurrentPlayer = x;
        }

        public String GetField(int x, int y) 
        {
            return gameBoard.GetField(x, y);
        }

        /* 
         * checks if a specific player has won by counting the player fields for each column
         * if one of the counter is at 3, that means the player has won
         */
        #region --- Winner and Draw Checks ---
        public Boolean CheckWinner(string player)
        { 
            int diagPos = 0;
            int diagNeg = 0;
            for(int i = 0; i < 3; i++)
            {
                int row = 0;
                int col = 0;
                for (int j = 0; j < 3; j++)
                {
                    if (GetField(i, j) == player) row++;
                    if (GetField(j, i) == player) col++;
                }
                if (row == 3 || col == 3) return true;
                if(GetField(i, i) == player) diagNeg++;
                if(GetField(i, 2 - i) == player) diagPos++;
            }
            if (diagPos == 3 || diagNeg == 3) return true;
            return false;
        }

        // if any field is null or empty return false
        public bool CheckForFullBoard()
        {
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    if (string.IsNullOrEmpty(gameBoard.GetField(i, j)))
                    { 
                        return false;
                    }
                }
            }
            return true;
        }

        /*
         * function counts the player and opponent symbols in a row/column/diagonal and checks
         * if the player count in a row/column/diagonal equals gameBoardSize (3) - 1 and 
         * if the opponent count in that row/column/diagonal equals 0 
         * means the player could win with his next move
        */
        private bool CheckForPossibleWin(string player, int xPos, int yPos)
        {
            string opponent = player == x ? o : x;

            int playerRowCount = 0;
            int playerColumnCount = 0;

            int oppRowCount = 0;
            int oppColumnCount = 0;

            int playerPosDiagonalCount = 0;
            int oppPosDiagonalCount = 0;

            int playerNegDiagonalCount = 0;
            int oppNegDiagonalCount = 0;

            for (int i = 0; i < 3; i++)
            {
                if (gameBoard.GetField(i, yPos) == player) playerRowCount++;
                else if (gameBoard.GetField(i, yPos) == opponent) oppRowCount++;
                if (gameBoard.GetField(xPos, i) == player) playerColumnCount++;
                else if (gameBoard.GetField(xPos, i) == opponent) oppColumnCount++;
                // only check for diagonal if the placed tile even is on a diagonal
                if(xPos == yPos || xPos == 2 - yPos)
                {
                    if (gameBoard.GetField(i, i) == player) playerNegDiagonalCount++;
                    else if (gameBoard.GetField(i, i) == opponent) oppNegDiagonalCount++;
                    if (gameBoard.GetField(i, 2 - i) == player) playerPosDiagonalCount++;
                    else if (gameBoard.GetField(i, 2 - i) == opponent) oppPosDiagonalCount++;
                }
            }

            if (playerRowCount == 2 && oppRowCount == 0) return true;
            else if (playerColumnCount == 2 && oppColumnCount == 0) return true;
            else if (playerPosDiagonalCount == 2 && oppPosDiagonalCount == 0) return true;
            else if (playerNegDiagonalCount == 2 && oppNegDiagonalCount == 0) return true;
            else return false;
        }

        /*
         * Is basically the opposite of CheckForPossibleWin(...). 
         * However this method check it for the opponent and for all rows/columns/diagonals
        */
        private bool CheckForPossibleLoss(string player)
        {
            string opponent = player == x ? o : x;


            int playerPosDiagonalCount = 0;
            int oppPosDiagonalCount = 0;

            int playerNegDiagonalCount = 0;
            int oppNegDiagonalCount = 0;

            for (int i = 0; i < 3; i++)
            {
                if (gameBoard.GetField(i, i) == player) playerNegDiagonalCount++;
                else if (gameBoard.GetField(i, i) == opponent) oppNegDiagonalCount++;
                if (gameBoard.GetField(i, 2 - i) == player) playerPosDiagonalCount++;
                else if (gameBoard.GetField(i, 2 - i) == opponent) oppPosDiagonalCount++;

                int playerRowCount = 0;
                int playerColumnCount = 0;

                int oppRowCount = 0;
                int oppColumnCount = 0;

                for (int j = 0; j < 3; j++)
                {
                    if (gameBoard.GetField(i, j) == player) playerRowCount++;
                    else if (gameBoard.GetField(i, j) == opponent) oppRowCount++;
                    if (gameBoard.GetField(j, i) == player) playerColumnCount++;
                    else if (gameBoard.GetField(j, i) == opponent) oppColumnCount++;
                }
                if (playerRowCount == 0 && oppRowCount == 2) return true;
                else if (playerColumnCount == 0 && oppColumnCount == 2) return true;
            }
            if (playerPosDiagonalCount == 0 && oppPosDiagonalCount == 2) return true;
            else if (playerNegDiagonalCount == 0 && oppNegDiagonalCount == 2) return true;
            else return false;
        }
        #endregion

        #region --- IGameState Implementation ---
        // each cell is represented by a 2-bit pair
        // X = 01, O = 10, EMPTY = 00
        public uint Id
        {
            get
            {
                uint id = 0;

                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        id <<= 2;

                        string cellState = GetField(x, y);

                        if (cellState == "X")
                        {
                            id |= 0b01;
                        }
                        else if (cellState == "O")
                        {
                            id |= 0b10;
                        }
                    }
                }
                return id;
            }
        }

        // If one cell is empty it can be added to a list of possible actions, this list is then returned
        public List<IAction> PossibleActions
        {
            get
            {
                List<IAction> actions = new List<IAction>();
                // iterate over each field row for row
                for( int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if(String.IsNullOrEmpty(GetField(i, j)))
                        {
                            // actions are 0-9 starting at top left row for row
                            actions.Add(allActions[i + 3 * j]);
                        }
                    }
                }
                return actions;
            }
        }

        // after executing a Position several check methods are called, which determine the reward for this move, which is returned after
        public double ExecuteAction(IAction a)
        {
            if (!(a is Position))
            {
                throw new ArgumentException("Invalid Action");
            }

            Position position =  a as Position;
            int xPos = position.x;
            int yPos = position.y;

            double reward = 0.0;

            gameBoard.SetField(CurrentPlayer, xPos, yPos);

            if (CheckForPossibleWin(CurrentPlayer, xPos, yPos))
            {
                reward = PossibleWinReward;
            }
            if (CheckForPossibleLoss(CurrentPlayer))
            {
                reward = PossibleLossReward;
            }
            if (CheckWinner(CurrentPlayer))
            {
                gameHasEnded = true;
                reward = WinReward;
            }
            if (CheckForFullBoard())
            {
                gameHasEnded = true;
                reward = DrawReward;
            }
            // Debug.WriteLine(CurrentPlayer + " " + reward + " (" + Id + ")");
            if (!gameHasEnded)
            {
                // if the game hasnt ended yet set next player
                SetNextPlayer();
            }
            return reward;
       
        }
        #endregion
        #region --- Public Execute Function For Manual Playing
        // Function inspired by code provided by Professor Cristof Rezk-Salama (CRS | C.Rezk-Salama@hochschule-trier.de)
        // this function can be used for manual playing
        public double TryExecuteAction(int x, int y)
        { 
            return ExecuteAction(allActions[x + y * 3]);
        }
        #endregion
    }
}
