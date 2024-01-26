using Accessibility;
using AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe
{
    class GameBoard
    {
        readonly string[,] gameBoard;

        public GameBoard(int i)
        {
            gameBoard = new string[i,i];
        }

        public void SetField(string player, int x, int y) 
        {
            if (gameBoard == null)
            {
                throw new ArgumentException("No Gameboard");
            }
            if (!string.IsNullOrEmpty(gameBoard[x, y]))
            {
                throw new ArgumentException($"Field is already set with {GetField(x, y)} on Gameboard");
            }
            gameBoard[x, y] = player;
            
        }

        public string GetField(int x, int y)
        {
            if (gameBoard == null)
            {
                throw new ArgumentException("No Gameboard");
            }
            return gameBoard[x, y];
        }

        public void Reset()
        {
            if(gameBoard == null)
            {
                throw new ArgumentException("No Gameboard to Reset");
            }
            for (int i = 0; i < gameBoard.GetLength(0); i++)
            {
                for (int j = 0; j < gameBoard.GetLength(1); j++)
                {
                    gameBoard[i, j] = String.Empty;
                }
            }
        }
    }
}
