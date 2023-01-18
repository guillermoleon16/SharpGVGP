using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGVGP.Proposed
{
    public class Supervisor
    {
        public State LastState;
        public List<State> States;
        public List<AptitudeState> Aptitudes;

        public Supervisor()
        {
            States = new List<State>();
            Aptitudes = new List<AptitudeState>();
            LastState = null;
        }

        public void ResetAll()
        {
            States.Clear();
            Aptitudes.Clear();
            LastState = null;
        }

        public bool AddState(State s, Move m)
        {
            if(LastState == null)
            {
                LastState = new State(s);
                States.Add(LastState);
                return false;
            }
            else
            {
                bool toReturn = true;
                State res = States.Find(x => x.Equals(s));
                if(res == null)
                {
                    res = new State(s);
                    toReturn = false;
                    States.Add(res);
                }                
                LastState.Links[(int) m] = res;
                LastState = res;
                return toReturn;
            }

        }

        public void GamePaths(bool win)
        {
            List<State> Visited = new List<State>();
            Queue<State> Pending = new Queue<State>();
            Pending.Enqueue(States[0]);
            while (Pending.Count > 0)
            {
                State N = Pending.Dequeue();
                Visited.Add(N);
                for (int i = 0; i < 4; i++)
                {
                    if (N.Links[i] != null)
                    {
                        State result = Visited.Find(x => x.Equals(N.Links[i]));
                        if (result != null)
                        {
                            N.Links[i] = null;
                        }
                        else
                        {
                            Pending.Enqueue(N.Links[i]);
                        }
                    }
                }
            }
            Stack<AptitudeState> temp = new Stack<AptitudeState>();
            temp.Push(new AptitudeState(States[0]));
            AssignAptitude(temp,win);
        }

        private void AssignAptitude(Stack<AptitudeState> stack,bool win)
        {
            bool flag = false;
            for (int i = 0; i < 4; i++)
            {
                if (stack.Peek().State.Links[i] != null)
                {
                    flag = true;
                    stack.Push(new AptitudeState(stack.Peek().State.Links[i]));
                    AssignAptitude(stack,win);
                }
            }
            if (!flag)
            {
                double N = (double)(stack.Count - 1);
                foreach (AptitudeState ap in stack)
                {
                    if (win)
                    {
                        ap.Aptitude = 1;
                    }
                    else
                    {
                        double temp = 1.0 + ((N / (1.0 - (double)(stack.Count))));
                        if (temp > ap.Aptitude)
                        {
                            ap.Aptitude = temp;
                        }
                    }                    
                    N--;
                }
            }
            Aptitudes.Add(stack.Pop());
        }
    }
}
