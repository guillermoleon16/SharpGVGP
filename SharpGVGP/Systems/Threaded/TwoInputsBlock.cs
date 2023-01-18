using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGVGP
{
    /// <summary>
    /// Allows for two inputs on the same single output System block.
    /// It is important to know that this implementation doesn't 
    /// Dequeue the elements on the input buffer. 
    /// The declared <c>ProcessInput</c> must be able to distinguish
    /// if the received input is still the same of a new one, or to not fail
    /// when any of the inputs doesn´t change.
    /// </summary>
    /// <typeparam name="TIn1">Type of the primary input</typeparam>
    /// <typeparam name="TIn2">Type of the secondary input</typeparam>
    /// <typeparam name="TOut">Type of the output</typeparam>
    public class TwoInputsBlock<TIn1,TIn2,TOut>:SystemBlock<TIn1,TOut>
    {
        private ConcurrentQueue<TIn2> SecondBuffer;

        /// <summary>
        /// Process to do with the inputs
        /// </summary>
        /// <param name="input1">Primary input of the block</param>
        /// <param name="input2">Secondary input of the block</param>
        /// <returns>Returns a <c>TOut</c> value.</returns>
        new public delegate TOut ProcessInput(TIn1 input1, TIn2 input2);

        ProcessInput H;

        /// <summary>
        /// Get an instance of <c>TwoInputsBlock</c>.
        /// </summary>
        /// <param name="Process">Process to do with the inputs</param>
        public TwoInputsBlock(ProcessInput Process)
        {
            this.H = Process;
            SecondBuffer = new ConcurrentQueue<TIn2>();
        }

        /// <summary>
        /// Process made to the input signals.
        /// </summary>
        /// <param name="Process"></param>
        protected void SetProcess(ProcessInput Process)
        {
            this.H = Process;
        }

        /// <summary>
        /// Gets an instance of a two input <c>SystemBlock</c>.
        /// Must call SetProcess before using Start.
        /// </summary>
        public TwoInputsBlock()
        {
            SecondBuffer = new ConcurrentQueue<TIn2>();
        }

        /// <summary>
        /// Insert a new value to the secondary input buffer.
        /// </summary>
        /// <param name="input"><c>TInput</c> value to insert in the queue.</param>
        public void In2(TIn2 input)
        {
            while (SecondBuffer.Count > 0)
            {
                SecondBuffer.TryDequeue(out TIn2 V);
            }
            SecondBuffer.Enqueue(input);
        }

        /// <summary>
        /// Task executed by the block every cycle.
        /// </summary>
        public override void ObjectTask()
        {
            TIn1 input1 = default(TIn1);
            TIn2 input2 = default(TIn2);
            while (!InputBuffer.TryPeek(out input1)) ;
            while (!SecondBuffer.TryPeek(out input2)) ;
            TOut output = this.H(input1, input2);
            if (output != null)
            {
                OutputBuffer.Enqueue(output);
            }            
        }
    }
}
