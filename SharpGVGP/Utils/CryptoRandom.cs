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
        public double DoubleValue { get; set; }
        /// <summary>
        /// The last generated random <c>int</c> value.
        /// </summary>
        public double IntValue { get; set; }

        /// <summary>
        /// Creates a new instance of the <c>CryptoRandom</c> class, generating two
        /// initial random values for <c>DoubleValue</c> and <c>IntValue</c>.
        /// </summary>
        public CryptoRandom()
        {
            using (RNGCryptoServiceProvider p = new RNGCryptoServiceProvider())
            {
                Random r = new Random(p.GetHashCode());
                this.DoubleValue = r.NextDouble();
                this.IntValue = r.Next();
            }
        }

        public CryptoRandom(int min, int max)
        {
            using (RNGCryptoServiceProvider p = new RNGCryptoServiceProvider())
            {
                Random r = new Random(p.GetHashCode());
                this.DoubleValue = r.NextDouble();
                this.IntValue = r.Next(min,max+1);
            }
        }

        public void NewDoubleValue()
        {
            using (RNGCryptoServiceProvider p = new RNGCryptoServiceProvider())
            {
                Random r = new Random(p.GetHashCode());
                this.DoubleValue = r.NextDouble();
                p.Dispose();
            }
        }

        public void NewIntValue()
        {
            using (RNGCryptoServiceProvider p = new RNGCryptoServiceProvider())
            {
                Random r = new Random(p.GetHashCode());
                this.IntValue = r.Next();
                p.Dispose();
            }
        }

        public void NewIntValue(int min, int max)
        {
            using (RNGCryptoServiceProvider p = new RNGCryptoServiceProvider())
            {
                Random r = new Random(p.GetHashCode());
                this.IntValue = r.Next(min,max+1);
                p.Dispose();
            }
        }
    }
}
