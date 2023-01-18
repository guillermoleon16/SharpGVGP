using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGVGP.Proposed
{
    public class DefinitiveDecisor
    {
        public Supervisor SV;
        public Executer EX;
        public Planner PL;
        public int LastMove;
        public int LastNetwork;
        public int PossibleMoves;
        public double EXLearningRate;
        public double PLLearningRate;
        public double LastReward;

        public DefinitiveDecisor(int nMoves, int nInputs, double EXLearning, double PLLearning)
        {

            EXLearningRate = EXLearning;
            PLLearningRate = PLLearning; 
            SV = new Supervisor();
            //EX = new Executer(new int[] {nInputs, nInputs, (int)Math.Ceiling(nInputs / 2.0), (int)Math.Ceiling(nInputs / 4.0) }, nInputs, nMoves, EXLearningRate);
            //PL = new Planner(new int[] {nInputs, (int)Math.Ceiling(nInputs / 2.0), (int)Math.Ceiling(nInputs / 2.0), (int)Math.Ceiling(nInputs / 4.0) }, nInputs, PLLearningRate);
            EX = new Executer(new int[] {(int)Math.Ceiling(nInputs / 2.0)}, nInputs, nMoves, EXLearningRate);
            PL = new Planner(new int[] {(int)Math.Ceiling(nInputs / 2.0)}, nInputs, PLLearningRate);
            LastMove = 0;
            LastNetwork = 0;
            PossibleMoves = nMoves;
        }

        public void Feedback(double reward)
        {
            LastReward = reward;
            bool[] supressAmplify = new bool[PossibleMoves];
            if (reward < 0)
            {
                for (int i = 0; i < PossibleMoves; i++)
                {
                    if(SV.LastState.Links[i]!= null)
                    {
                        if (SV.LastState.Links[i].Equals(SV.LastState.Links[i]))
                        {
                            supressAmplify[i] = true;
                        }
                    }                    
                }
            }
            supressAmplify[LastMove] = true;
            EX.Reinforce(reward,supressAmplify);
        }

        public void EndGame(bool win)
        {
            
            SV.GamePaths(win);
            PL.LearnAptitudes(SV.Aptitudes);
            EX.LearnAptitudes(SV.Aptitudes);
            SV.ResetAll();
        }

        public Move FirstMove(double[] inputs)
        {
            EX.ChangeNetwork(PL.Reset(inputs));
            LastNetwork = PL.ActiveNetwork;
            SV.AddState(new State(inputs,PossibleMoves), Move.DOWN);
            double[] outputs = EX.GetOutputs(inputs);
            int max = 0;
            // Look for max output
            for (int i = 1; i < outputs.Length; i++)
            {
                if (outputs[i] > outputs[max])
                {
                    max = i;
                }
            }
            LastMove = max;
            return (Move)LastMove;
        }

        public Move GetMove(double[] inputs)
        {
            if(SV.AddState(new State(inputs, PossibleMoves), (Move)LastMove))
            {
                Feedback(-1);
            }
            else
            {
                Feedback(1);
            }
            int plout = PL.GetOutput(inputs);
            if(LastNetwork != plout)
            {
                while (!EX.ChangeNetwork(plout))
                {
                    EX.NewNetwork();
                }
                SV.GamePaths(true);
                SV.Aptitudes[0].Aptitude = 0;
                PL.LearnAptitudes(SV.Aptitudes,LastNetwork);
                SV.GamePaths(false);
                EX.LearnAptitudes(SV.Aptitudes,LastNetwork);
                SV.ResetAll();
                SV.AddState(new State(inputs, PossibleMoves), Move.DOWN);
                LastNetwork = PL.ActiveNetwork;
            }
            double[] outputs = EX.GetOutputs(inputs);
            int max = 0;
            // Look for max output
            for (int i = 1; i < outputs.Length; i++)
            {
                if(outputs[i] > outputs[max])
                {
                    max = i;
                }
            }
            LastMove = max;
            return (Move) LastMove;
        }
    }
}
