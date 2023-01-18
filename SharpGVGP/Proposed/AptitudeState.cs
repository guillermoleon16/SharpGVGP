using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGVGP.Proposed
{
    public class AptitudeState
    {
        public double Aptitude;
        public State State;

        public AptitudeState(State state)
        {
            State = state;
            Aptitude = -1;
        }
    }
}
