using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe
{
    public class Game
    {
        public string CurrentPlayer { get; set; }
        private const string x = "X";
        private const string o = "O";
        // private board to keep track of the GUI Board
        private string[,] Board;

        public Game() 
        {
            Board = new string[3,3];
            CurrentPlayer = x;
        }

        public void SetNextPlayer()
        {
            // if x => o else o => x
            CurrentPlayer = CurrentPlayer == x ? o : x;
        }

        public bool PlayerWin()
        {
            for(int i = 0; i < 3; ++i)
            {
                // check for rows
                if (Board[i, 0] == CurrentPlayer && Board[i, 1] == CurrentPlayer && Board[i, 2] == CurrentPlayer) return true;
                // check columns
                if (Board[0, i] == CurrentPlayer && Board[1, i] == CurrentPlayer && Board[2, i] == CurrentPlayer) return true;
            }
            // check negative diagonal
            if (Board[0, 0] == CurrentPlayer && Board[1, 1] == CurrentPlayer && Board[2, 2] == CurrentPlayer) return true;
            // check positive diagonal
            if (Board[0, 2] == CurrentPlayer && Board[1, 1] == CurrentPlayer && Board[2, 0] == CurrentPlayer) return true;
            // otherwise return true
            return false;
        }

        public bool IsDraw()
        {
            for(int i = 0; i <3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    if (string.IsNullOrEmpty(Board[i,j])) 
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal void UpateBoard(Position clickedField, string player)
        {
            // keep String Array up to date with actual board
            Board[clickedField.x, clickedField.y] = player;
        }
    }
}
