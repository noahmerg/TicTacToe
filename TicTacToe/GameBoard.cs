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

        public GameBoard(int x, int y) 
        {
            gameBoard = new string[x, y];
        }

        public void SetField(string player, Position position) 
        {
            if (gameBoard == null)
            {
                throw new ArgumentException("No Gameboard");
            }
            if (!string.IsNullOrEmpty(gameBoard[position.x, position.y]))
            {
                throw new ArgumentException($"Field is already set with {getField(position.x, position.y)} on Gameboard");
            }
            gameBoard[position.x, position.y] = player;
            
        }

        public string getField(int x, int y)
        {
            if (gameBoard == null)
            {
                throw new ArgumentException("No Gameboard");
            }
            return gameBoard[x, y];
        }
    }
}
