using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGVGP
{
    public class NEATGenotype
    {
        public ulong ConnectionCount { get; private set; }
        public ulong EnabledConnectionCount { get; private set; }
        public ulong NeuronCount { get; private set; }
        public NEATConnection[] Connections { get; private set; }
        public NeuralNetwork.Connection[] EnabledConnections { get; private set; }
        public NeuralNetwork.NeuronType[] UsedNeurons { get; private set; }
        public NeuralNetwork.NeuronType[] Neurons { get; private set; }
        public double Fitness { get; private set; }
        public bool HasFitness { get; private set; }
        public bool HasBias { get; private set; }

        public NEATGenotype(NEATGenotype reference)
        {
            ConnectionCount = reference.ConnectionCount;
            EnabledConnectionCount = reference.EnabledConnectionCount;
            NeuronCount = reference.NeuronCount;
            Connections = reference.Connections;
            EnabledConnections = reference.EnabledConnections;
            UsedNeurons = reference.UsedNeurons;
            Neurons = reference.Neurons;
            Fitness = 0;
            HasFitness = false;
            HasBias = reference.HasBias;
        }

        public NEATGenotype(ulong maxNeuron,ulong connectionCount,
            NEATConnection[] connections, NeuralNetwork.NeuronType[] neurons)
        {
            Fitness = 0;
            HasFitness = false;
            ConnectionCount = connectionCount;
            Connections = connections;
            NeuronCount = 0;
            HasBias = false;
            for (ulong i = 0; i < maxNeuron; i++)
            {
                if (neurons[i] != NeuralNetwork.NeuronType.Unused)
                {
                    NeuronCount++;
                    if (neurons[i] == NeuralNetwork.NeuronType.Bias)
                    {
                        HasBias = true;
                    }
                }                    
            }
            ulong k = 0;
            UsedNeurons = new NeuralNetwork.NeuronType[NeuronCount];
            for (ulong i = 0; i < maxNeuron; i++)
            {
                if (neurons[i] != NeuralNetwork.NeuronType.Unused)
                {
                    UsedNeurons[k] = neurons[i];
                    k++;
                }
            }
            Neurons = neurons;
            UpdateEnabledConnections();
        }

        public NEATGenotype(ulong maxNeuron, ulong connectionCount,
            NEATConnection[] connections, NeuralNetwork.NeuronType[] neurons, double fitness)
        {
            Fitness = fitness;
            HasFitness = true;
            ConnectionCount = connectionCount;
            NeuronCount = 0;
            Connections = connections;
            Neurons = neurons;
            HasBias = false;
            for (ulong i = 0; i < maxNeuron; i++)
            {
                if (neurons[i] != NeuralNetwork.NeuronType.Unused)
                {
                    NeuronCount++;
                    if (neurons[i] == NeuralNetwork.NeuronType.Bias)
                    {
                        HasBias = true;
                    }
                }
            }
            ulong k = 0;
            UsedNeurons = new NeuralNetwork.NeuronType[NeuronCount];
            for (ulong i = 0; i < maxNeuron; i++)
            {
                if (neurons[i] != NeuralNetwork.NeuronType.Unused)
                {
                    UsedNeurons[k] = neurons[i];
                    k++;
                }
            }
            UpdateEnabledConnections();
        }

        private void UpdateEnabledConnections()
        {
            EnabledConnectionCount = 0;
            for (ulong i = 0; i < ConnectionCount; i++)
            {
                if (Connections[i].Enabled)
                {
                    EnabledConnectionCount++;
                }
            }
            EnabledConnections = new NeuralNetwork.Connection[EnabledConnectionCount];
            int k = 0;
            for (ulong i = 0; i < ConnectionCount; i++)
            {
                if (Connections[i].Enabled)
                {
                    EnabledConnections[k] = Connections[i];
                    k++;
                }
            }
        }

        public bool SetFitness(double fitness)
        {
            if (HasFitness)
            {
                return false;
            }
            else
            {
                Fitness = fitness;
                HasFitness = true;
                return true;
            }
        }

        public class NEATConnection: NeuralNetwork.Connection
        {
            public bool Enabled { get; set; }
            public ulong InnovationNumber { get; set; }

            public NEATConnection(ulong source, ulong destination, double weight,
                bool enabled, ulong innovationNumber):base(source,destination,weight)
            {
                Enabled = enabled;
                InnovationNumber = innovationNumber;
            }
        }

        public ulong AddBias()
        {
            NeuronCount++;
            NeuralNetwork.NeuronType[] temp = new NeuralNetwork.NeuronType[NeuronCount];
            Array.Copy(Neurons, temp, (int)NeuronCount - 1);
            temp[NeuronCount - 1] = NeuralNetwork.NeuronType.Bias;
            Neurons = temp;
            return NeuronCount - 1;
        }

        public void AddConnection(ulong source, ulong target,double weight, ulong innovation)
        {
            ConnectionCount++;
            EnabledConnectionCount++;
            NEATConnection[] temp = new NEATConnection[ConnectionCount];
            Array.Copy(Connections, temp, (int)ConnectionCount - 1);
            temp[ConnectionCount - 1] = new NEATConnection(source, target, weight, true, innovation);
            Connections = temp;
            UpdateEnabledConnections();
        }

        public bool ContainsLink(ulong source, ulong target)
        {
            bool flag = false;
            for (ulong i = 0; i < ConnectionCount; i++)
            {                
                if((Connections[i].Source == source)&&
                    (Connections[i].Destination == target))
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        public bool HasHidden()
        {
            bool flag = false;
            for(ulong i = 0; i < (ulong)NeuronCount; i++)
            {
                if (Neurons[i] == NeuralNetwork.NeuronType.Hidden)
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        public void SplitConnection(ulong connectionID,ulong currInn)
        {
            NeuronCount++;
            NeuralNetwork.NeuronType[] temp2 = new NeuralNetwork.NeuronType[NeuronCount];
            Array.Copy(Neurons, temp2, (int)NeuronCount - 1);
            temp2[NeuronCount - 1] = NeuralNetwork.NeuronType.Hidden;
            Neurons = temp2;
            ulong newID = NeuronCount - 1;
            Connections[connectionID].Enabled = false;
            ConnectionCount++;            
            NEATConnection[] temp = new NEATConnection[ConnectionCount+1];
            Array.Copy(Connections, temp, (int)ConnectionCount - 1);
            temp[ConnectionCount - 1] = new NEATConnection(Connections[ConnectionCount-1].Source,
                newID, Connections[ConnectionCount - 1].Weight, true, currInn+1);
            ConnectionCount++;
            temp[ConnectionCount - 1] = new NEATConnection(newID,
                Connections[ConnectionCount - 1].Destination, Connections[ConnectionCount - 1].Weight, true, currInn + 2);
            Connections = temp;
        }
    }
}