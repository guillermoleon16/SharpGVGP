using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGVGP
{ 
    /// <summary>
    /// This class allows the building of a Neural Network with a general topology.
    /// </summary>
    public class NeuralNetwork
    {
        /// <summary>
        /// Types of Neurons available for the Network.
        /// </summary>
        public enum NeuronType : byte
        {
            /// <summary>
            /// Input neuron. It only admits one input and comes from the medium.
            /// </summary>
            Input,
            /// <summary>
            /// Output neurons. They provide the output of the system.
            /// </summary>
            Output,
            /// <summary>
            /// Hidden neurons. They may have several inputs but only one output.
            /// </summary>
            Hidden,
            Bias,
            Unused
        }

        /// <summary>
        /// Neuron units of the Network.
        /// </summary>
        protected Neuron[] Neurons;
        /// <summary>
        /// Subset of Neurons which are inputs for the Network
        /// </summary>
        protected List<Neuron> Inputs;
        /// <summary>
        /// Subset of Neurons which are outputs for the Network.
        /// </summary>
        protected List<Neuron> Outputs;
        /// <summary>
        /// Subset of Neurons which are biases in the network
        /// </summary>
        protected List<Neuron> Biases;
        /// <summary>
        /// Total number of connections on the Network.
        /// </summary>
        public ulong ConnectionCount { get; }
        /// <summary>
        /// Total number of neurons of the Network.
        /// </summary>
        public ulong NeuronCount { get; }
        /// <summary>
        ///Indicates if the Network was built correctly and all of the Neurons
        ///can calculate an output.
        /// </summary>
        public bool IsValid { get; }

        public delegate double ActivationFunction(double x);

        private ActivationFunction Activation;

        #region Constructors
        /// <summary>
        /// Get an instance of a Neural Network with custom parameters.
        /// </summary>
        /// <param name="connectionCount">Number of connections</param>
        /// <param name="neuronCount">Number of neurons</param>
        /// <param name="connections">Array with all of the connections</param>
        /// <param name="neuronTypes">Array with all of the neuron types</param>
        public NeuralNetwork(ulong connectionCount, ulong neuronCount, Connection[] connections, NeuronType[] neuronTypes)
        {
            Activation = Sigmoid;
            ConnectionCount = connectionCount;
            NeuronCount = neuronCount;
            Neurons = new Neuron[NeuronCount];
            Inputs = new List<Neuron>();
            Outputs = new List<Neuron>();
            Biases = new List<Neuron>();
            IsValid = true;
            List<NeuronInput>[] inputs = new List<NeuronInput>[neuronCount];
            for (ulong i = 0; i < neuronCount; i++)
            {
                Neuron node = new Neuron(i);
                Neurons[i] = node;
                if (neuronTypes[i] == NeuronType.Input)
                {
                    Inputs.Add(node);
                }
                else
                {
                    if (neuronTypes[i] == NeuronType.Output)
                    {
                        Outputs.Add(node);
                    }
                    else
                    {
                        if (neuronTypes[i] == NeuronType.Bias)
                        {
                            Biases.Add(node);
                        }
                    }
                }
                inputs[i] = new List<NeuronInput>();
            }
            for (ulong i = 0; i < connectionCount; i++)
            {
                if ((connections[i].Source < 0) ||
                    (connections[i].Source > neuronCount) ||
                    (connections[i].Destination < 0) ||
                    (connections[i].Destination > neuronCount))
                {
                    IsValid = false;
                    return;
                }
                else
                {
                    inputs[connections[i].Destination].Add(new NeuronInput(connections[i].Weight,
                        Neurons[connections[i].Source]));
                }
            }
            for (ulong i = 0; i < NeuronCount; i++)
            {
                Neurons[i].Inputs = inputs[i];
            }
            //Check Recurrency
            ulong level = 1;
            ulong[] levels = new ulong[neuronCount];
            Queue<Neuron> pending = new Queue<Neuron>();
            ulong N = 0;
            ulong Ncur = 0;
            foreach (Neuron n in Outputs)
            {
                levels[n.ID] = level;
                foreach (NeuronInput ni in n.Inputs)
                {
                    Ncur++;
                    pending.Enqueue(ni.Source);
                }
            }
            N = Ncur;
            Ncur = 0;
            level++;
            while (pending.Count > 0)
            {
                while (N > 0)
                {
                    Neuron n = pending.Dequeue();
                    if (levels[n.ID] == 0)
                    {
                        levels[n.ID] = level;
                        if ((!(neuronTypes[n.ID] == NeuronType.Input)) &&
                            (!(neuronTypes[n.ID] == NeuronType.Bias)))
                        {
                            foreach (NeuronInput ni in n.Inputs)
                            {
                                if (levels[ni.Source.ID] == 0)
                                {
                                    pending.Enqueue(ni.Source);
                                    Ncur++;
                                }
                                else
                                {
                                    if (levels[ni.Source.ID] == level)
                                    {
                                        if (ni.Source.ID == n.ID)
                                        {
                                            ni.IsRecurrent = true;
                                        }
                                    }
                                    else
                                    {
                                        ni.IsRecurrent = true;
                                    }
                                }
                            }
                        }
                    }
                    N--;
                }
                level++;
                N = Ncur;
                Ncur = 0;
            }
            //Check Validity
            bool flag = true;
            foreach (Neuron n in Outputs)
            {
                if (n.Inputs.Count > 0)
                {
                    foreach (NeuronInput ni in n.Inputs)
                    {
                        flag = flag && CheckValidity(ni);
                    }
                }
                else
                {
                    flag = false;
                    break;
                }
            }
            IsValid = flag;
        }
        /// <summary>
        /// Get an instance of a Neural Network with custom parameters.
        /// </summary>
        /// <param name="connectionCount">Number of connections</param>
        /// <param name="neuronCount">Number of neurons</param>
        /// <param name="connections">Array with all of the connections</param>
        /// <param name="neuronTypes">Array with all of the neuron types</param>
        /// <param name="activationFunction">Activation function of the Neural Network</param>
        public NeuralNetwork(ulong connectionCount, ulong neuronCount, Connection[] connections, NeuronType[] neuronTypes, ActivationFunction activationFunction)
        {
            Activation = activationFunction;
            ConnectionCount = connectionCount;
            NeuronCount = neuronCount;
            Neurons = new Neuron[NeuronCount];
            Inputs = new List<Neuron>();
            Outputs = new List<Neuron>();
            Biases = new List<Neuron>();
            IsValid = true;
            List<NeuronInput>[] inputs = new List<NeuronInput>[neuronCount];
            for (ulong i = 0; i < neuronCount; i++)
            {
                Neuron node = new Neuron(i);
                Neurons[i] = node;
                if (neuronTypes[i] == NeuronType.Input)
                {
                    Inputs.Add(node);
                }
                else
                {
                    if (neuronTypes[i] == NeuronType.Output)
                    {
                        Outputs.Add(node);
                    }
                    else
                    {
                        if (neuronTypes[i] == NeuronType.Bias)
                        {
                            Biases.Add(node);
                        }
                    }
                }
                inputs[i] = new List<NeuronInput>();
            }
            for (ulong i = 0; i < connectionCount; i++)
            {
                if ((connections[i].Source < 0) ||
                    (connections[i].Source > neuronCount) ||
                    (connections[i].Destination < 0) ||
                    (connections[i].Destination > neuronCount))
                {
                    IsValid = false;
                    return;
                }
                else
                {
                    inputs[connections[i].Destination].Add(new NeuronInput(connections[i].Weight,
                        Neurons[connections[i].Source]));
                }
            }
            for (ulong i = 0; i < NeuronCount; i++)
            {
                Neurons[i].Inputs = inputs[i];
            }
            //Check Recurrency
            ulong level = 1;
            ulong[] levels = new ulong[neuronCount];
            Queue<Neuron> pending = new Queue<Neuron>();
            ulong N = 0;
            ulong Ncur = 0;
            foreach (Neuron n in Outputs)
            {
                levels[n.ID] = level;
                foreach (NeuronInput ni in n.Inputs)
                {
                    Ncur++;
                    pending.Enqueue(ni.Source);
                }
            }
            N = Ncur;
            Ncur = 0;
            level++;
            while (pending.Count > 0)
            {
                while (N > 0)
                {
                    Neuron n = pending.Dequeue();
                    if (levels[n.ID] == 0)
                    {
                        levels[n.ID] = level;
                        if ((!(neuronTypes[n.ID]==NeuronType.Input))&&
                            (!(neuronTypes[n.ID] == NeuronType.Bias)))
                        {
                            foreach (NeuronInput ni in n.Inputs)
                            {
                                if (levels[ni.Source.ID] == 0)
                                {
                                    pending.Enqueue(ni.Source);
                                    Ncur++;
                                }
                                else
                                {
                                    if (levels[ni.Source.ID] == level)
                                    {
                                        if (ni.Source.ID == n.ID)
                                        {
                                            ni.IsRecurrent = true;
                                        }
                                    }
                                    else
                                    {
                                        ni.IsRecurrent = true;
                                    }
                                }
                            }
                        }
                    }
                    N--;
                }
                level++;
                N = Ncur;
                Ncur = 0;
            }
            //Check Validity
            bool flag = true;
            foreach (Neuron n in Outputs)
            {
                if (n.Inputs.Count > 0)
                {
                    foreach (NeuronInput ni in n.Inputs)
                    {
                        flag = flag && CheckValidity(ni);
                    }
                }
                else
                {
                    flag = false;
                    break;
                }
            }
            IsValid = flag;
        }

        #endregion Constructors
        /// <summary>
        /// Checks if an input connection of the neuron is valid.
        /// </summary>
        /// <param name="input">Neuron connection to be checked.</param>
        /// <returns>Returns if the connection is valid</returns>
        private bool CheckValidity(NeuronInput input)
        {
            //Check if the connection is to an input node.
            bool flag = Inputs.Exists(x => x.ID == input.Source.ID);
            if (flag)
            {
                return true;
            }
            else
            {
                if (input.Source.Inputs.Count > 0)
                {
                    // Recurrent call to explore all of the subsequent connections
                    flag = true;
                    bool notrecurrent = false;
                    foreach (NeuronInput ni in input.Source.Inputs)
                    {
                        //If a recurrrent connection is found, it is ignored.
                        if (ni.IsRecurrent == false)
                        {
                            notrecurrent = true;
                            flag = flag && CheckValidity(ni);
                        }                        
                    }
                    return flag && notrecurrent;
                }
                else
                {
                    return false;
                }                
            }
        }

        public class Neuron
        {
            public ulong ID { get; }
            public List<NeuronInput> Inputs { get; set; }
            public double Output { get; set; }
            public double LastOutput { get; set; }
            public bool Ready { get; set; }

            public Neuron(ulong id)
            {
                ID = id;
                Inputs = new List<NeuronInput>();
                Output = 0;
                LastOutput = 0;
                Ready = false;
            }
        }

        public class NeuronInput
        {
            public double Weight { get; set; }
            public bool IsRecurrent { get; set; }
            public Neuron Source { get; set; }

            public NeuronInput(double weight, Neuron source)
            {
                Weight = weight;
                IsRecurrent = false;
                Source = source;
            }
        }

        public class Connection
        {
            public ulong Source { get; set; }
            public ulong Destination { get; set; }
            public double Weight { get; set; }
            public bool IsRecurrent { get; set; }

            public Connection(ulong source, ulong destination, double weight)
            {
                Source = source;
                Destination = destination;
                Weight = weight;
                IsRecurrent = false;
            }
        }

        /// <summary>
        /// Generates properties por a completely convolutional Neural Network
        /// </summary>
        public class ConvolutionalProperties
        {
            public ulong ConnectionCount { get; }
            public ulong NeuronCount { get; }

            public Connection[] Connections { get; set; }
            public NeuronType[] Neurons { get; }

            public ConvolutionalProperties(ulong nInputs, ulong nOutputs, ulong nLayers, ulong[] nNeurons, bool randomWeights)
            {
                CryptoRandom r = new CryptoRandom();
                ulong id = 0;
                ulong[] lastLayer = new ulong[nInputs];
                ulong lastSize = nInputs;
                NeuronCount = nInputs + nOutputs;
                ConnectionCount = 0;
                if (nLayers > 0)
                {
                    for (ulong i = 0; i < nLayers; i++)
                    {
                        NeuronCount += nNeurons[i];
                        if (i == 0)
                        {
                            ConnectionCount += nNeurons[i] * nInputs;
                        }
                        else
                        {
                            ConnectionCount += nNeurons[i] * nNeurons[i - 1];
                        }
                    }
                    ConnectionCount += nOutputs * nNeurons[nLayers - 1];
                }
                else
                {
                    ConnectionCount += nOutputs * nInputs;
                }         
                Neurons = new NeuronType[NeuronCount];
                Connections = new Connection[ConnectionCount];
                for (ulong i = 0; i < nInputs; i++)
                {
                    lastLayer[i] = id;
                    Neurons[id] = NeuronType.Input;
                    id++;
                }
                ulong conn = 0;
                //For layer i
                for (ulong i = 0; i < nLayers; i++)
                {
                    //For every neuron in layer i
                    for (ulong j = 0; j < nNeurons[i]; j++)
                    {
                        //Connect all neurons from the last layer
                        for (ulong k = 0; k < lastSize; k++)
                        {
                            if (randomWeights)
                            {
                                Connections[conn] = new Connection(lastLayer[k], id, 2*r.DoubleValue-1);
                                r.NewDoubleValue();
                            }
                            else
                            {
                                Connections[conn] = new Connection(lastLayer[k], id, 1);
                            }
                            conn++;
                        }
                        Neurons[id] = NeuronType.Hidden;
                        id++;
                    }
                    //Store layer i in lastLayer
                    lastSize = nNeurons[i];
                    lastLayer = new ulong[lastSize];
                    ulong idcopy = id - 1;
                    for (ulong k = lastSize - 1; k > 0; k--)
                    {
                        lastLayer[k] = idcopy;
                        idcopy--;
                    }
                    lastLayer[0] = idcopy;
                    idcopy--;
                }
                //Connect the output layer
                for (ulong i = 0; i < nOutputs; i++)
                {
                    for (ulong k = 0; k < lastSize; k++)
                    {
                        if (randomWeights)
                        {
                            Connections[conn] = new Connection(lastLayer[k], id, 2 * r.DoubleValue - 1);
                            r.NewDoubleValue();
                        }
                        else
                        {
                            Connections[conn] = new Connection(lastLayer[k], id, 1);
                        }
                        conn++;
                    }
                    Neurons[id] = NeuronType.Output;
                    id++;
                }
            }
        }

        public bool Feed(double[] inputs, ulong[] targets, ulong size)
        {
            if (IsValid)
            {
                for (ulong i = 0; i < NeuronCount; i++)
                {
                    Neurons[i].Ready = false;
                }
                for (ulong i = 0; i < size; i++)
                {
                    Neuron n = Inputs.Find(x => x.ID == targets[i]);
                    n.Output = Activation(inputs[i]);
                    n.Ready = true;
                }
                foreach(Neuron n in Biases)
                {
                    n.Output = 1;
                    n.Ready = true;
                }
                Queue<ulong> pending = new Queue<ulong>();
                for (ulong i = 0; i < NeuronCount; i++)
                {
                    if (!Neurons[i].Ready)
                    {
                        pending.Enqueue(i);
                    }
                }
                while (pending.Count > 0)
                {
                    ulong i = pending.Dequeue();
                    bool pendingFlag = true;
                    double tempInput = 0;
                    foreach (NeuronInput n in Neurons[i].Inputs)
                    {
                        if (n.IsRecurrent)
                        {
                            tempInput += n.Source.LastOutput*n.Weight;
                        }
                        else
                        {
                            pendingFlag = pendingFlag && n.Source.Ready;
                            tempInput += n.Source.Output*n.Weight;
                        }
                    }
                    if (!pendingFlag)
                    {
                        pending.Enqueue(i);
                    }
                    else
                    {
                        Neurons[i].Output = Activation(tempInput);
                        Neurons[i].Ready = true;
                    }
                }
                for (ulong i = 0; i < NeuronCount; i++)
                {
                    Neurons[i].LastOutput = Neurons[i].Output;
                }
                return true;
            }
            else
            {
                return false;
            }            
        }

        public double[] GetOutputs()
        {
            double[] ToReturn = new double[Outputs.Count];
            if (IsValid)
            {
                ulong i = 0;
                foreach (Neuron n in Outputs)
                {
                    ToReturn[i] = n.Output;
                    i++;
                }
            }
            else
            {
                for (int i = 0; i < Outputs.Count; i++)
                {
                    ToReturn[i] = -2;
                }
            }
            return ToReturn;
        }

        public double Sigmoid(double x)
        {
            return 1 / (1+Math.Exp(-x));            
        }
    }
}
