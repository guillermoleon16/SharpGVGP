using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SharpGVGP
{
    /// <summary>
    /// This class provides generating more reliable random <c>int</c>
    /// and <c>double</c> values using the <c>RNGCryptoServiceProvider</c> class.
    /// </summary>
    class CryptoRandom
    {
        /// <summary>
        /// The last generated random <c>double</c> value.
        /// </summary>
        public double DoubleValue { get; private set; }
        /// <summary>
        /// The last generated random <c>int</c> value.
        /// </summary>
        public int IntValue { get; private set; }

        private Random RNG;

        /// <summary>
        /// Creates a new instance of the <c>CryptoRandom</c> class, generating two
        /// initial random values for <c>DoubleValue</c> and <c>IntValue</c>.
        /// </summary>
        public CryptoRandom()
        {
            RNG = new Random();
            NewDoubleValue();
            NewIntValue();
        }

        /// <summary>
        /// Creates a new instance of the <c>CryptoRandom</c> class, generating two
        /// initial random values for <c>DoubleValue</c> and an <c>IntValue</c>
        /// between two values.
        /// </summary>
        /// <param name="min">Minimum value for <c>IntValue</c></param>
        /// <param name="max">Maximum value for <c>IntValue</c></param>
        public CryptoRandom(int min, int max)
        {
            RNG = new Random();
            NewDoubleValue();
            NewIntValue(min, max);
        }

        public double NewDoubleValue()
        {
            this.DoubleValue = RNG.NextDouble();
            return this.DoubleValue;
        }

        public int NewIntValue()
        {
            this.IntValue = RNG.Next();
            return this.IntValue;
        }

        public int NewIntValue(int min, int max)
        {
            this.IntValue = RNG.Next(min,max+1);
            return this.IntValue;
        }
    }
}
