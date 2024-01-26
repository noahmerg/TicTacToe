using AI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
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
        private int gameBoardSize;
        private Position[] allActions;
        public bool gameHasEnded { get; private set; }

        public TicTacToeGame() 
        {
            gameBoardSize = 3;
            gameHasEnded = false;
            gameBoard = new GameBoard(gameBoardSize);
            // set x as startingplayer
            CurrentPlayer = x;
            // add Positions to allActions so we dont need to create new Objects which would destroy the algorithm (yes I did this and didnt notice)
            allActions = new Position[gameBoardSize * gameBoardSize];
            allActions[0] = new Position { Name = "Top Left", x = 0, y = 0};
            allActions[1] = new Position { Name = "Top Middle", x = 1, y = 0 };
            allActions[2] = new Position { Name = "Top Right", x = 2, y = 0 };
            allActions[3] = new Position { Name = "Middle Left", x = 0, y = 1 };
            allActions[4] = new Position { Name = "Middle Middle", x = 1, y = 1 };
            allActions[5] = new Position { Name = "Middle Right", x = 2, y = 1 };
            allActions[6] = new Position { Name = "Bottom Left", x = 0, y = 2 };
            allActions[7] = new Position { Name = "Bottom Middle", x = 1, y = 2 };
            allActions[8] = new Position { Name = "Bottom Right", x = 2, y = 2 };
        }

        public void SetNextPlayer()
        { 
            // if x => o else o => x
            CurrentPlayer = CurrentPlayer == x ? o : x;
        }

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

        #region --- Winner and Draw Checks ---
        public Boolean CheckWinner(string player)
        { 
            // the winner check is based on counting player fields for each row/column and diagonal
            // if one of them equals 3 it means the player has won
            int diagPos = 0;
            int diagNeg = 0;
            for(int i = 0; i < gameBoardSize; i++)
            {
                int row = 0;
                int col = 0;
                for (int j = 0; j < gameBoardSize; j++)
                {
                    if (GetField(i, j) == player) row++;
                    if (GetField(j, i) == player) col++;
                }
                if (row == gameBoardSize || col == gameBoardSize) return true;
                if(GetField(i, i) == player) diagNeg++;
                if(GetField(i, 2 - i) == player) diagPos++;
            }
            if (diagPos == gameBoardSize || diagNeg == gameBoardSize) return true;
            return false;
            // i know this could be changed to return (diagPos == gameBoardSize || diagNeg == gameBoardSize) but this makes it easier for me
        }

        // if any field is null or empty return false | its that simple
        public bool CheckForFullBoard()
        {
            for (int i = 0; i < gameBoardSize; ++i)
            {
                for (int j = 0; j < gameBoardSize; ++j)
                {
                    if (string.IsNullOrEmpty(gameBoard.GetField(i, j)))
                    { 
                        return false;
                    }
                }
            }
            return true;
        }

        // function counts the player and opponent symbols in a row/column/diagonal and checks
        // if the player count in a row/column/diagonal equals gameBoardSize (3) - 1 and 
        // if the opponent count in that row/column/diagonal equals 0 
        // means the player could win with his next move
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

            for (int i = 0; i < gameBoardSize; i++)
            {
                if (gameBoard.GetField(i, yPos) == player) playerRowCount++;
                else if (gameBoard.GetField(i, yPos) == opponent) oppRowCount++;
                if (gameBoard.GetField(xPos, i) == player) playerColumnCount++;
                else if (gameBoard.GetField(xPos, i) == opponent) oppColumnCount++;
                if (gameBoard.GetField(i, i) == player) playerNegDiagonalCount++;
                else if (gameBoard.GetField(i, i) == opponent) oppNegDiagonalCount++;
                if (gameBoard.GetField(i, 2 - i) == player) playerPosDiagonalCount++;
                else if (gameBoard.GetField(i, 2 - i) == opponent) oppPosDiagonalCount++;
            }

            if (playerRowCount == gameBoardSize - 1 && oppRowCount == 0) return true;
            else if (playerColumnCount == gameBoardSize - 1 && oppColumnCount == 0) return true;
            else if (playerPosDiagonalCount == gameBoardSize - 1 && oppPosDiagonalCount == 0) return true;
            else if (playerNegDiagonalCount == gameBoardSize - 1 && oppNegDiagonalCount == 0) return true;
            else return false;
        }
        #endregion

        #region --- IGameState Implementation ---
        public uint Id
        {
            get
            {
                uint id = 0;

                for (int x = 0; x < gameBoardSize; x++)
                {
                    for (int y = 0; y < gameBoardSize; y++)
                    {
                        // 2 bits / cell = 18 Bits in total at maximum
                        id <<= 2;

                        string cellState = GetField(x, y);

                        if (cellState == "X")
                        {
                            id |= 0b01; // x = 01
                        }
                        else if (cellState == "O")
                        {
                            id |= 0b10; // o = 10
                        }
                        // "" = 00
                    }
                }
                return id;
            }
        }

        public List<IAction> PossibleActions
        {
            get
            {
                List<IAction> actions = new List<IAction>();
                // iterate over each field row for row
                for( int i = 0; i < gameBoardSize; i++)
                {
                    for (int j = 0; j < gameBoardSize; j++)
                    {
                        if(String.IsNullOrEmpty(GetField(i, j)))
                        {
                            // actions are 0-9 starting at top left row for row
                            actions.Add(allActions[i + gameBoardSize * j]);
                        }
                    }
                }
                return actions;
            }
        }
        public double ExecuteAction(IAction a)
        {
            if (!(a is Position))
            {
                throw new ArgumentException("Invalid Action");
            }
            Position position =  a as Position;
            int xPos = position.x;
            int yPos = position.y;
            string opponent = CurrentPlayer == x ? o : x;
            double reward = 0.0;

            // set the field for the gameboard
            gameBoard.SetField(CurrentPlayer, xPos, yPos);

            // check if the move resulted in two in a row/column/diagonal without enemy tile in the way
            // by checking if the opponent of the opponent (which is the player) could win
            if (CheckForPossibleWin(CurrentPlayer, xPos, yPos))
            {
                reward = 0.8;
            }
            // cecks if the enemy could win after this operation of if this move blocked it
            if (CheckForPossibleWin(opponent, xPos, yPos))
            {
                reward = -1.0;
            }
            // checks if the move resulted in winning
            if (CheckWinner(CurrentPlayer))
            {
                // when the player won the game finished
                gameHasEnded = true;
                // best possible reward for winning
                reward = 1.0;
            }
            // checks if the board is full
            if (CheckForFullBoard())
            {
                // full board = game is ended
                gameHasEnded = true;
                // no reward for drawing
            }
            if (!gameHasEnded)
            {
                // if the game hasnt ended yet set next player
                SetNextPlayer();
            }
            // return the reward
            return reward;
       
        }
        #endregion
        #region --- Public Execute Function For Manual Playing
        public double TryExecuteAction(int x, int y)
        { 
            return ExecuteAction(allActions[x + y * gameBoardSize]);
        }
        #endregion
    }
}
