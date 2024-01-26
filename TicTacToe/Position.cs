using AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe
{
    // basic implementation of 2D coordinates as an IAction
    public class Position : IAction
    {
        public int x {  get; set; }

        public int y { get; set; }

        public string Name { get; set; }
    }
}
