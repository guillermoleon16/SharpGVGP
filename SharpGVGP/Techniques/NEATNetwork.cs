using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGVGP
{
    public class NEATNetwork: NeuralNetwork
    {
        public NEATGenotype Genotype;

        public NEATNetwork(NEATGenotype geneticEncoding):
            base(geneticEncoding.EnabledConnectionCount,
                geneticEncoding.NeuronCount,
                geneticEncoding.EnabledConnections,
                geneticEncoding.UsedNeurons)
        {
            Genotype = geneticEncoding;
        }
    }
}
