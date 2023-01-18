using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Neuro.Learning;
using Accord.Neuro;
using System.Threading;

namespace SharpGVGP.Proposed
{
    public class Executer
    {
        public readonly int NInputs, NOutputs;
        public readonly int[] NLayerNeurons;
        public readonly double LearningRate;
        private double[] LastInput;
        private Random RNG;
        private List<ActivationNetwork> NetworkBank;
        private List<BackPropagationLearning> TeacherBank;
        public int ActiveNetwork;
        public int NNetworks { get; private set; }
        public double[] LastOutput { get; private set; }

        public Executer(int[] nHiddenLayerNeurons, int nInputs, int nOutputs, double learningRate)
        {
            LearningRate = learningRate;
            NLayerNeurons = new int[nHiddenLayerNeurons.Length + 1];
            Array.Copy(nHiddenLayerNeurons, NLayerNeurons, nHiddenLayerNeurons.Length);
            NLayerNeurons[nHiddenLayerNeurons.Length] = nOutputs;
            NInputs = nInputs;
            NOutputs = nOutputs;
            RNG = new Random();
            NetworkBank = new List<ActivationNetwork>();
            TeacherBank = new List<BackPropagationLearning>();
            NewNetwork();
            ActiveNetwork = 0;
            LastInput = new double[nInputs];
        }

        public int NewNetwork()
        {
            ActivationNetwork NN = new ActivationNetwork(new SigmoidFunction(), NInputs, NLayerNeurons);
            BackPropagationLearning Teacher = new BackPropagationLearning(NN)
            {
                LearningRate = LearningRate
            };
            NN.Randomize();
            NetworkBank.Add(NN);
            TeacherBank.Add(Teacher);
            NNetworks = NetworkBank.Count;
            return NNetworks - 1;
        }

        public double[] GetOutputs(double[] inputs)
        {
            Array.Copy(inputs, LastInput, NInputs);
            LastOutput = NetworkBank[ActiveNetwork].Compute(inputs);
            return LastOutput;
        }

        public void Reinforce(double reinfValue, bool[] supressAmplify)
        {
            double[] output = LastOutput;
            for (int i = 0; i < supressAmplify.Length; i++)
            {
                if (supressAmplify[i])
                {
                    if (reinfValue < 0)
                    {
                        output[i] = 0;
                    }
                    else
                    {
                        output[i] = 1;
                    }
                }
            }
            if (reinfValue > 0)
            {
                TeacherBank[ActiveNetwork].Run(LastInput, output);
            }
            else
            {
                TeacherBank[ActiveNetwork].Run(LastInput, output);
                bool flag = false;
                while (!flag)
                {
                    double[] temp = NetworkBank[ActiveNetwork].Compute(LastInput);
                    int k = 0;
                    for (int i = 0; i < temp.Length; i++)
                    {
                        if (temp[i] > temp[k])
                        {
                            k = i;
                        }
                    }
                    if (!supressAmplify[k])
                    {
                        flag = true;
                    }
                    else
                    {
                        TeacherBank[ActiveNetwork].Run(LastInput, output);
                    }
                }
            }
        }

        public bool ChangeNetwork(int newNetwork)
        {
            if (newNetwork >= NNetworks)
            {
                return false;
            }
            else
            {
                ActiveNetwork = newNetwork;
                return true;
            }
        }

        public async void LearnAptitudes(List<AptitudeState> aptitudes)
        {
            List<AptitudeState> aptitudes_copy = new List<AptitudeState>(aptitudes);
            await Task.Factory.StartNew(() => ReinforcePaths(aptitudes_copy, ActiveNetwork));
            aptitudes_copy.Clear();
        }

        public async void LearnAptitudes(List<AptitudeState> aptitudes, int target)
        {
            List<AptitudeState> aptitudes_copy = new List<AptitudeState>(aptitudes);
            await Task.Factory.StartNew(() => ReinforcePaths(aptitudes_copy, target));
            aptitudes_copy.Clear();
        }

        private void ReinforcePaths(List<AptitudeState> aptitudes, int target)
        {
            List<AptitudeState> optimalPath = new List<AptitudeState>
            {
                aptitudes[aptitudes.Count - 1]
            };
            bool flag = true;
            while (flag)
            {
                AptitudeState temp;
                AptitudeState toAdd = new AptitudeState(null);
                double maxAptitude = -1;
                flag = false;
                foreach (State s in optimalPath[optimalPath.Count - 1].State.Links)
                {
                    if (s != null)
                    {
                        flag = true;
                        temp = aptitudes.Find(x => x.State.Equals(s));
                        if (temp.Aptitude >= maxAptitude)
                        {
                            maxAptitude = temp.Aptitude;
                            toAdd = temp;
                        }
                    }
                }
                if (flag)
                {
                    optimalPath.Add(toAdd);
                }
            }
            double[][] inputs = new double[optimalPath.Count][];
            double[][] outputs = new double[optimalPath.Count][];
            int i = 0;
            foreach (AptitudeState ap in optimalPath)
            {
                inputs[i] = new double[NInputs];
                Array.Copy(ap.State.Board, inputs[i], NInputs);
                int k = 0;
                for (int j = 0; j < NOutputs; j++)
                {
                    if (ap.State.Links[j] != null)
                    {
                        k = j;
                    }
                }
                outputs[i] = new double[NOutputs];
                outputs[i][k] = 1;
                i++;
            }
            double error = 0;
            double prevError;
            do
            {
                prevError = error;
                error = TeacherBank[target].RunEpoch(inputs, outputs);
            } while (Math.Abs(error - prevError) / error > 0.1);
        }
    }
}
