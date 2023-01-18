using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGVGP.Proposed
{
    public class State
    {
        public double[] Board;
        public State[] Links;

        public State(State s)
        {
            Board = new double[s.Board.Length];
            Array.Copy(s.Board, Board, s.Board.Length);
            Links = new State[s.Links.Length];
        }

        public State(double[] board,int nMoves)
        {
            Board = new double[board.Length];
            Array.Copy(board, Board, board.Length);
            Links = new State[nMoves];
        }

        public State(double[,] board, int nMoves)
        {
            Board = MatrixToVector(board);
            Links = new State[nMoves];
        }

        public bool Equals(State s)
        {
            bool toReturn = true;
            int i = 0;
            while (toReturn && i < Board.Length)
            {
                if (Board[i] != s.Board[i])
                {
                    toReturn = false;
                }
                i++;
            }
            return toReturn;
        } 

        public static double[] MatrixToVector(double[,] board)
        {
            double[] toReturn = new double[board.Length];
            int i = 0;
            foreach(double v in board)
            {
                toReturn[i] = v;
                i++;
            }
            return toReturn;
        }
    }
}
