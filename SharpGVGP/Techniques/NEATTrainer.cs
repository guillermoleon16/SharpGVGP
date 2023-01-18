using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGVGP.Techniques
{
    public class NEATTrainer
    {
        public ulong MaxNodes { get; private set; }
        public ulong Generation { get; private set; }
        public ulong Inputs { get; private set; }
        public ulong Outputs { get; private set; }
        public double MaxFitness { get; private set; }
        public long MaxPopulation { get; private set; }
        public Probabilities Rates { get; private set; }
        public ulong Innovation { get; private set; }
        private List<Species> GenePool;
        public double DeltaExcess { get; private set; }
        public double DeltaDisjoint { get; private set; }
        public double DeltaWeights { get; private set; }
        public double DeltaThreshold { get; private set; }
        public double DeltaN { get; private set; }
        public double PerturbStep { get; private set; }
        public double MaxAbsWeight { get; private set; }
        public long StaleSpecies { get; private set; }
        Random RNG;
        private List<InnovationOcurrence> InnovationHistory;


        public NEATTrainer(ulong inputs, ulong outputs, long population, Probabilities probabilities,
            double deltaExcess, double deltaDisjoint, double deltaWeights,
            double deltaThreshold, double maxAbsWeight, long staleSpecies,
            ulong maxNodes,double deltaN)
        {
            DeltaN = deltaN;
            Inputs = inputs;
            Outputs = outputs;
            Generation = 0;
            MaxFitness = -1;
            MaxNodes = maxNodes;
            MaxPopulation = population;
            Rates = probabilities;
            DeltaExcess = deltaExcess;
            DeltaDisjoint = deltaDisjoint;
            DeltaWeights = deltaWeights;
            DeltaThreshold = deltaThreshold;
            MaxAbsWeight = maxAbsWeight;
            StaleSpecies = staleSpecies;
            Innovation = 0;
            GenePool = new List<Species>();
            RNG = new Random();
            InnovationHistory = new List<InnovationOcurrence>();
        }

        private NEATGenotype BasicGenotype()
        {
            NeuralNetwork.ConvolutionalProperties properties =
                new NeuralNetwork.ConvolutionalProperties(Inputs, Outputs,
                0, new ulong[] { 0 }, true);
            NEATGenotype.NEATConnection[] conns = new NEATGenotype.NEATConnection[properties.ConnectionCount];
            for(ulong i = 0; i < properties.ConnectionCount; i++)
            {
                conns[i] = new NEATGenotype.NEATConnection(
                    properties.Connections[i].Source,
                    properties.Connections[i].Destination,
                    properties.Connections[i].Weight,
                    true,
                    i);
            }
            Innovation = properties.ConnectionCount;
            return new NEATGenotype(properties.NeuronCount, properties.ConnectionCount,
                conns, properties.Neurons);
        }

        public void InitializePool()
        {
            for(int i = 0; i < MaxPopulation; i++)
            {
                AddToSpecies(BasicGenotype());
            }
        }

        public void ReportFitness(NEATGenotype g, double fitness)
        {
            NEATGenotype gene=null;
            foreach (Species s in GenePool)
            {
                gene = s.Genes.Find(x=>x==g);
                if (gene != null)
                {
                    break;
                }
            }
            gene.SetFitness(fitness);
        }

        public NEATGenotype GetNextGenotype()
        {
            bool found = false;
            NEATGenotype toUse = null;
            foreach(Species s in GenePool)
            {
                foreach(NEATGenotype g in s.Genes)
                {
                    if ((!g.HasFitness)&&(!found))
                    {
                        toUse = g;
                        found = true;
                    }
                }
            }
            if (found)
            {
                return toUse;
            }
            else
            {
                NewGeneration();
                found = false;
                toUse = null;
                foreach (Species s in GenePool)
                {
                    foreach (NEATGenotype g in s.Genes)
                    {
                        if ((!g.HasFitness) && (!found))
                        {
                            toUse = g;
                            found = true;
                        }
                    }
                }
                return toUse;
            }
            
        }

        public class Probabilities
        {
            public double MutateConnection { get; set; }
            public double Perturb { get; set; }
            public double Crossover { get; set; }
            public double LinkMutation { get; set; }
            public double NodeMutation { get; set; }
            public double BiasMutation { get; set; }
            public double EnableMutation { get; set; }
            public double DisableMutation { get; set; }
        }

        private class Species
        {
            public List<NEATGenotype> Genes;
            public double TopFitness;
            public double AverageFitness;
            public long Staleness;

            public Species(double topFitness, double averageFitness, long staleness)
            {
                TopFitness = topFitness;
                AverageFitness = averageFitness;
                Staleness = staleness;
                Genes = new List<NEATGenotype>();
            }
        }

        private class InnovationOcurrence
        {
            public ulong source, destination, number;
        }

        private void SortSpecies()
        {
            foreach(Species s in GenePool)
            {
                s.Genes.Sort((x, y) => x.Fitness.CompareTo(y.Fitness));
            }
        }

        private void Cull(bool LetOne)
        {
            foreach (Species s in GenePool)
            {
                s.Genes.Sort((x,y)=>x.Fitness.CompareTo(y.Fitness));
                if (LetOne)
                {
                    s.Genes.RemoveRange(0, s.Genes.Count - 1);
                }
                else
                {
                    s.Genes.RemoveRange(0, 
                        (int)Math.Floor((double)(s.Genes.Count / 2)));
                }
            }            
        }

        private double TotalAverageFitness()
        {
            double toReturn = 0;
            foreach(Species s in GenePool)
            {
                toReturn += s.AverageFitness;
            }
            return toReturn;
        }

        private void CalculateAverageFitness()
        {
            foreach(Species s in GenePool)
            {
                double av = 0;
                foreach(NEATGenotype g in s.Genes)
                {
                    av += g.Fitness;
                }
                s.AverageFitness = av / s.Genes.Count;
            }
        }

        private void RemoveStale()
        {
            foreach (Species s in GenePool)
            {
                s.Genes.Sort((x, y) => x.Fitness.CompareTo(y.Fitness));
                double temptop = s.Genes.ElementAt(s.Genes.Count - 1).Fitness;
                if (s.TopFitness > temptop)
                {
                    s.Staleness++;
                }
                else
                {
                    s.TopFitness = temptop;
                    s.Staleness = 0;
                }
                if (s.Staleness >= StaleSpecies)
                {
                    GenePool.Remove(s);
                }
            }
        }

        private void RemoveWeak()
        {
            double total = TotalAverageFitness();
            foreach (Species s in GenePool)
            {
                if((s.AverageFitness/total)*MaxPopulation<1)
                {
                    GenePool.Remove(s);
                }
            }
        }

        private ulong MaxInnovation(NEATGenotype parent1, NEATGenotype parent2)
        {
            ulong max = 0;
            for(ulong i = 0; i < parent1.ConnectionCount; i++)
            {
                if (parent1.Connections[i].InnovationNumber > max)
                {
                    max = parent1.Connections[i].InnovationNumber;
                }
            }
            for (ulong i = 0; i < parent2.ConnectionCount; i++)
            {
                if (parent1.Connections[i].InnovationNumber > max)
                {
                    max = parent2.Connections[i].InnovationNumber;
                }
            }
            return max;
        }

        private NEATGenotype Crossover(NEATGenotype parent1, NEATGenotype parent2)
        {
            NEATGenotype child;
            List<NEATGenotype.NEATConnection> ToUse = new List<NEATGenotype.NEATConnection>();
            if (parent1.Fitness == parent2.Fitness)
            {
                NEATGenotype.NEATConnection[] genes1 = parent1.Connections;
                NEATGenotype.NEATConnection[] genes2 = parent2.Connections;
                Array.Sort(genes1, (x, y) => x.InnovationNumber.CompareTo(y.InnovationNumber));
                Array.Sort(genes2, (x, y) => x.InnovationNumber.CompareTo(y.InnovationNumber));
                ulong i = 0;
                ulong j = 0;
                while ((i < parent1.ConnectionCount) && (j<parent2.ConnectionCount))
                {
                    if (genes1[i].InnovationNumber == genes2[j].InnovationNumber)
                    {
                        if (RNG.Next(0, 2) == 1)
                        {
                            ToUse.Add(genes1[i]);
                        }
                        else
                        {
                            ToUse.Add(genes2[j]);
                        }
                        i++;
                        j++;
                    }
                    else
                    {
                        if(genes1[i].InnovationNumber > genes2[j].InnovationNumber)
                        {
                            if (RNG.Next(0, 2) == 1)
                            {
                                ToUse.Add(genes2[j]);
                            }
                            j++;
                        }
                        else
                        {
                            if (RNG.Next(0, 2) == 1)
                            {
                                ToUse.Add(genes1[i]);
                            }
                            i++;
                        }
                    }
                }
                if (j == parent2.ConnectionCount)
                {
                    while (i < parent1.ConnectionCount)
                    {
                        if (RNG.Next(0, 2) == 1)
                        {
                            ToUse.Add(genes1[i]);
                        }
                        i++;
                    }
                }
                else
                {
                    while (j < parent2.ConnectionCount)
                    {
                        if (RNG.Next(0, 2) == 1)
                        {
                            ToUse.Add(genes2[j]);
                        }
                        j++;
                    }
                }
            }
            else
            {
                if (parent2.Fitness > parent1.Fitness)
                {
                    NEATGenotype tempg = parent1;
                    parent1 = parent2;
                    parent2 = tempg;
                }
                NEATGenotype.NEATConnection[] genes1 = parent1.Connections;
                NEATGenotype.NEATConnection[] genes2 = parent2.Connections;
                Array.Sort(genes1, (x, y) => x.InnovationNumber.CompareTo(y.InnovationNumber));
                Array.Sort(genes2, (x, y) => x.InnovationNumber.CompareTo(y.InnovationNumber));
                ulong i = 0;
                ulong j = 0;
                while ((i < parent1.ConnectionCount) && (j < parent2.ConnectionCount))
                {
                    if (genes1[i].InnovationNumber == genes2[j].InnovationNumber)
                    {
                        if (RNG.Next(0, 2) == 1)
                        {
                            ToUse.Add(genes1[i]);
                        }
                        else
                        {
                            ToUse.Add(genes2[j]);
                        }
                        i++;
                        j++;
                    }
                    else
                    {
                        if (genes1[i].InnovationNumber > genes2[j].InnovationNumber)
                        {
                            j++;
                        }
                        else
                        {
                            ToUse.Add(genes1[i]);
                            i++;
                        }
                    }
                }
                if (j == parent2.ConnectionCount)
                {
                    while (i < parent1.ConnectionCount)
                    {
                        if (RNG.Next(0, 2) == 1)
                        {
                            ToUse.Add(genes1[i]);
                        }
                        i++;
                    }
                }
            }

            Queue<NeuralNetwork.NeuronType> neurons = new Queue<NeuralNetwork.NeuronType>();
            List<ulong> ids = new List<ulong>();
            NEATGenotype.NEATConnection[] connections = new NEATGenotype.NEATConnection[ToUse.Count];
            ulong k = 0;
            foreach(NEATGenotype.NEATConnection c in ToUse)
            {
                connections[k] = c;
                k++;
                if(!ids.Exists(x => x == c.Source))
                {
                    if (c.Source >= parent2.NeuronCount)
                    {
                        neurons.Enqueue(parent1.Neurons[c.Source]);
                    }
                    else
                    {
                        neurons.Enqueue(parent2.Neurons[c.Source]);
                    }
                    ids.Add(c.Source);
                }
                if (!ids.Exists(x => x == c.Destination))
                {
                    if (c.Destination >= parent2.NeuronCount)
                    {
                        neurons.Enqueue(parent1.Neurons[c.Destination]);
                    }
                    else
                    {
                        neurons.Enqueue(parent2.Neurons[c.Destination]);
                    }
                    ids.Add(c.Destination);
                }
            }
            ulong maxid = ids.Max();
            NeuralNetwork.NeuronType[] neuron = new NeuralNetwork.NeuronType[maxid+1];
            for (ulong i = 0; i < maxid; i++)
            {
                neuron[i] = NeuralNetwork.NeuronType.Unused;
            }
            foreach (ulong id in ids)
            {
                neuron[id] = neurons.Dequeue();
            }
            child = new NEATGenotype(maxid, (ulong)ToUse.Count, connections, neuron);
            return child;
        }

        private void Mutate(NEATGenotype genome)
        {
            if(RNG.NextDouble()<Rates.MutateConnection)
            {
                PointMutate(genome);
            }

            if (RNG.NextDouble() < Rates.LinkMutation)
            {
                while(!LinkMutate(genome,false));
            }

            if(RNG.NextDouble() < Rates.BiasMutation)
            {
                while(!LinkMutate(genome, true));
            }

            if (RNG.NextDouble() < Rates.NodeMutation)
            {
                NeuronMutate(genome);
            }
        }

        private void NeuronMutate(NEATGenotype genome)
        {
            ulong target;
            do
            {
                target = (ulong)RNG.Next(0, (int)genome.ConnectionCount);
            } while ((!genome.Connections[target].Enabled)||
            (genome.Neurons[genome.Connections[target].Source]==NeuralNetwork.NeuronType.Unused)||
            (genome.Neurons[genome.Connections[target].Destination] == NeuralNetwork.NeuronType.Unused));
            genome.SplitConnection(target, Innovation);
            Innovation++;
            InnovationHistory.Add(new InnovationOcurrence
            {
                source = genome.Connections[target].Source,
                destination = genome.NeuronCount - 1,
                number = Innovation
            });
            Innovation++;
            InnovationHistory.Add(new InnovationOcurrence
            {
                source = genome.NeuronCount - 1,
                destination = genome.Connections[target].Destination,
                number = Innovation
            });
        }

        private bool LinkMutate(NEATGenotype genome, bool v)
        {
            bool success = false;
            ulong neuron1=0;
            ulong neuron2 = 0;
            if (!v)
            {
                do
                {
                    neuron1 = (ulong)RNG.Next(0, (int)genome.NeuronCount);
                } while ((genome.Neurons[neuron1] == NeuralNetwork.NeuronType.Bias)||
                (genome.Neurons[neuron1] == NeuralNetwork.NeuronType.Unused));
            }
            else
            {
                if (genome.HasBias)
                {
                    for(ulong i = 0;i<genome.NeuronCount; i++)
                    {
                        if (genome.Neurons[i] == NeuralNetwork.NeuronType.Bias)
                        {
                            neuron1 = i;
                            break;
                        }
                    }
                }
                else
                {
                    neuron1 = genome.AddBias();
                }
            }
            
            if (genome.Neurons[neuron1] == NeuralNetwork.NeuronType.Input)
            {
                do
                {
                    neuron2 = (ulong)RNG.Next(0, (int)genome.NeuronCount);
                } while ((neuron1 == neuron2)||
                (genome.Neurons[neuron2] == NeuralNetwork.NeuronType.Input)||
                (genome.Neurons[neuron1] == NeuralNetwork.NeuronType.Bias) ||
                (genome.Neurons[neuron1] == NeuralNetwork.NeuronType.Unused));
            }
            else if (genome.Neurons[neuron1] == NeuralNetwork.NeuronType.Output)
            {
                do
                {
                    neuron2 = (ulong)RNG.Next(0, (int)genome.NeuronCount);
                } while ((genome.Neurons[neuron2] != NeuralNetwork.NeuronType.Hidden)||
                (neuron1==neuron2));
            }
            else if (genome.Neurons[neuron1] == NeuralNetwork.NeuronType.Bias)
            {
                do
                {
                    neuron2 = (ulong)RNG.Next(0, (int)genome.NeuronCount);
                } while ((neuron1 == neuron2) ||
                (genome.Neurons[neuron2] != NeuralNetwork.NeuronType.Input) ||
                (genome.Neurons[neuron1] != NeuralNetwork.NeuronType.Bias) ||
                (genome.Neurons[neuron1] == NeuralNetwork.NeuronType.Unused));
            }
            else
            {
                do
                {
                    neuron2 = (ulong)RNG.Next(0, (int)genome.NeuronCount);
                } while ((genome.Neurons[neuron2] == NeuralNetwork.NeuronType.Input) ||
                (genome.Neurons[neuron1] == NeuralNetwork.NeuronType.Bias) ||
                (genome.Neurons[neuron1] == NeuralNetwork.NeuronType.Unused));
            }
            int Inn = 0;bool flag = false;
            while ((Inn < InnovationHistory.Count)&&(!flag))
            {
                if((InnovationHistory.ElementAt(Inn).source==neuron1)&&
                    (InnovationHistory.ElementAt(Inn).destination == neuron2))
                {
                    flag = true;
                }
                Inn++;
            }
            if (flag)
            {
                if (!genome.ContainsLink(neuron1, neuron2))
                {
                    genome.AddConnection(neuron1, neuron2, RNG.NextDouble(), (ulong)Inn-1);
                    success = true;
                }
            }
            else
            {
                Innovation++;
                genome.AddConnection(neuron1, neuron2, RNG.NextDouble(), Innovation);
                InnovationHistory.Add(new InnovationOcurrence
                {
                    source = neuron1,
                    destination = neuron2,
                    number = Innovation
                });
                success = true;
            }
            return success;
        }

        private void PointMutate(NEATGenotype genome)
        {
            for(ulong i = 0; i < genome.ConnectionCount; i++)
            {
                if (RNG.NextDouble() < Rates.Perturb)
                {
                    genome.Connections[i].Weight +=
                        (2 * RNG.NextDouble() - 1) * PerturbStep;
                    if (Math.Abs(genome.Connections[i].Weight) > MaxAbsWeight)
                    {
                        genome.Connections[i].Weight = MaxAbsWeight * Math.Sign(genome.Connections[i].Weight);
                    }
                }
                else
                {
                    genome.Connections[i].Weight = (2 * RNG.NextDouble() - 1) * MaxAbsWeight;
                }
            }
        }

        private void NewGeneration()
        {
            Console.WriteLine("New Generation required ");
            Cull(false);
            Console.WriteLine("Half species culled ");
            RemoveStale();
            Console.WriteLine("Stale removed ");
            CalculateAverageFitness();
            Console.WriteLine("Average fitness calculated ");
            RemoveWeak();
            Console.WriteLine("Weak species removed ");
            Queue<NEATGenotype> children = new Queue<NEATGenotype>();
            double sum = TotalAverageFitness();
            foreach (Species s in GenePool)
            {
                int breed = (int) Math.Floor((s.AverageFitness / sum) * MaxPopulation) - 1;
                for(int i = 0; i < breed; i++)
                {
                    children.Enqueue(BreedChild(s));
                }
            }
            Cull(true);
            Console.WriteLine("Culled to one ");
            while (children.Count + GenePool.Count < MaxPopulation)
            {
                children.Enqueue(BreedChild(GenePool.ElementAt(RNG.Next(0, GenePool.Count))));
            }
            foreach(NEATGenotype g in children)
            {
                AddToSpecies(g);
            }
            Generation++;
            //Save backup
        }

        private void AddToSpecies(NEATGenotype g)
        {
            bool foundSpecies = false;
            foreach(Species s in GenePool)
            {
                if ((!foundSpecies) && (SameSpecies(g, s.Genes.ElementAt(0))))
                {
                    s.Genes.Add(g);
                    foundSpecies = true;
                }
            }
            if (!foundSpecies)
            {
                Species s = new Species(0, 0, 0);
                s.Genes.Add(g);
                GenePool.Add(s);
            }
        }

        private bool SameSpecies(NEATGenotype g1, NEATGenotype g2)
        {
            DisjointExcessWeights(g1, g2, out double deltaDisjoint,
                out double deltaExcess, out double deltaWeights);
            double delta = (DeltaDisjoint * deltaDisjoint / DeltaN) +
                (DeltaExcess * deltaExcess / DeltaN) + 
                deltaWeights * DeltaWeights;
            return delta < DeltaThreshold;
        }

        private NEATGenotype BreedChild(Species species)
        {
            NEATGenotype child;
            if (RNG.NextDouble() < Rates.Crossover)
            {
                Console.WriteLine("Crossover ocurred. ");
                NEATGenotype g1 = species.Genes.ElementAt(RNG.Next(0, species.Genes.Count-1));
                NEATGenotype g2 = species.Genes.ElementAt(RNG.Next(0, species.Genes.Count-1));
                child = Crossover(g1, g2);
            }
            else
            {
                Console.WriteLine("Crossover didn't ocurr ");
                NEATGenotype g = species.Genes.ElementAt(RNG.Next(0, species.Genes.Count-1));
                child = new NEATGenotype(g);
            }

            Mutate(child);
            return child;
        }

        private void DisjointExcessWeights(NEATGenotype parent1, NEATGenotype parent2, out double disjoint, out double excess, out double weights)
        {
            NEATGenotype.NEATConnection[] genes1 = parent1.Connections;
            NEATGenotype.NEATConnection[] genes2 = parent2.Connections;
            Array.Sort(genes1, (x, y) => x.InnovationNumber.CompareTo(y.InnovationNumber));
            Array.Sort(genes2, (x, y) => x.InnovationNumber.CompareTo(y.InnovationNumber));
            ulong i = 0;
            ulong j = 0;
            weights = 0;
            disjoint = 0;
            excess = 0;
            double matching = 0;
            while ((i < parent1.ConnectionCount) && (j < parent2.ConnectionCount))
            {
                if (genes1[i].InnovationNumber == genes2[j].InnovationNumber)
                {
                    weights = Math.Abs(genes1[i].Weight - genes2[j].Weight);
                    matching++;
                    i++;
                    j++;
                }
                else
                {
                    disjoint++;

                    if (genes1[i].InnovationNumber > genes2[j].InnovationNumber)
                    {
                        j++;
                    }
                    else
                    {
                        i++;
                    }
                }
            }
            if (j == parent2.ConnectionCount)
            {
                while (i < parent1.ConnectionCount)
                {
                    excess++;
                    i++;
                }
            }
            else
            {
                while (j < parent2.ConnectionCount)
                {
                    excess++;
                    j++;
                }
            }
            weights = weights / matching;
        }
    }
}
