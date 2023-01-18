using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Neuro.Learning;
using Accord.Neuro;

namespace SharpGVGP.Proposed
{
    public class Planner
    {
        public readonly int NInputs, NOutputs;
        public readonly int[] NLayerNeurons;
        public readonly double LearningRate;
        private double[] LastInput;
        private Random RNG;
        private List<ActivationNetwork> NetworkBank;
        private List<BackPropagationLearning> TeacherBank;
        public int ActiveNetwork;
        public double LastAptitude;
        public int NNetworks { get; private set; }

        public Planner(int[] nHiddenLayerNeurons, int nInputs, double learningRate)
        {
            LearningRate = learningRate;
            NLayerNeurons = new int[nHiddenLayerNeurons.Length + 1];
            Array.Copy(nHiddenLayerNeurons, NLayerNeurons, nHiddenLayerNeurons.Length);
            NLayerNeurons[nHiddenLayerNeurons.Length] = 1;
            NInputs = nInputs;
            NOutputs = 1;
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

        public int Reset(double[] inputs)
        {
            ActiveNetwork = 0;
            double[] output = NetworkBank[ActiveNetwork].Compute(inputs);
            LastAptitude = output[0];
            return ActiveNetwork;
        }

        public int GetOutput(double[] inputs)
        {
            Array.Copy(inputs, LastInput, NInputs);
            double[] output = NetworkBank[ActiveNetwork].Compute(inputs);
            LastAptitude = output[0];
            if (output[0] < 0.9)
            {
                double maxAptitude = LastAptitude;
                int maxNetwork = ActiveNetwork;
                if (RNG.NextDouble() > output[0])
                {
                    //Change triggered
                    int i = 0;

                    bool flag = false;
                    while ((i < NNetworks) && (!flag))
                    {
                        if (i != ActiveNetwork)
                        {
                            output = NetworkBank[i].Compute(inputs);
                            if (RNG.NextDouble() < output[0])
                            {
                                flag = true;
                                maxAptitude = output[0];
                                maxNetwork = i;
                            }
                        }
                        i++;
                    }
                    if (!flag)
                    {
                        // All networks are below the threshold
                        ActiveNetwork = NewNetwork();
                        LastAptitude = 1;
                    }
                    else
                    {
                        ActiveNetwork = maxNetwork;
                        LastAptitude = maxAptitude;
                    }
                }
            }
            return ActiveNetwork;
        }

        public void Reinforce(double reinfValue)
        {/*
            if(reinfValue < 0)
            {
                TeacherBank[ActiveNetwork].Run(LastInput, new double[] { 0 });
            }        */
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
            double[][] inputs = new double[aptitudes.Count][];
            double[][] outputs = new double[aptitudes.Count][];
            int i = 0;
            foreach (AptitudeState ap in aptitudes)
            {
                inputs[i] = new double[NInputs];
                Array.Copy(ap.State.Board, inputs[i], NInputs);
                outputs[i] = new double[] { ap.Aptitude };
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
