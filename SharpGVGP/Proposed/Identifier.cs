using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGVGP.Proposed
{
    public class Identifier
    {
        public double[] TransformBoard(int[,] board)
        {
            double[] toReturn = new double[board.Length];
            int i = 0;
            foreach(int v in board)
            {
                //Lookup table for Log_2(N)
                switch (v)
                {
                    case 0:
                        {
                            toReturn[i] = - 1;
                            break;
                        }
                    case 2:
                        {
                            toReturn[i] = (double)((2.0 - 11.0) / 11.0);
                            break;
                        }
                    case 4:
                        {
                            toReturn[i] = (double)((4.0 - 11.0) / 11.0);
                            break;
                        }
                    case 8:
                        {
                            toReturn[i] = (double)((6.0 - 11.0) / 11.0);
                            break;
                        }
                    case 16:
                        {
                            toReturn[i] = (double)((8.0 - 11.0) / 11.0);
                            break;
                        }
                    case 32:
                        {
                            toReturn[i] = (double)((10.0 - 11.0) / 11.0);
                            break;
                        }
                    case 64:
                        {
                            toReturn[i] = (double)((12.0 - 11.0) / 11.0);
                            break;
                        }
                    case 128:
                        {
                            toReturn[i] = (double)((14.0 - 11.0) / 11.0);
                            break;
                        }
                    case 256:
                        {
                            toReturn[i] = (double)((16.0 - 11.0) / 11.0);
                            break;
                        }
                    case 512:
                        {
                            toReturn[i] = (double)((18.0 - 11.0) / 11.0);
                            break;
                        }
                    case 1024:
                        {
                            toReturn[i] = (double)((20.0 - 11.0) / 11.0);
                            break;
                        }
                    case 2048:
                        {
                            toReturn[i] = 1;
                            break;
                        }
                    default:
                        {
                            toReturn[i] = -1;
                            break;
                        }
                }
                i++;
            }
            return toReturn;
        }
    }
}
