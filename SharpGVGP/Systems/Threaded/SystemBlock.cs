using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGVGP
{
    /// <summary>
    /// This class allows for construction of the blocks of the agent's system.
    /// To use an instance of this class, the <c>Start()</c> method must be called.
    /// To get and input data, the <c>In</c> and <c>Out</c> methods must be used.
    /// </summary>
    /// <typeparam name="TInput">Input type of the block</typeparam>
    /// <typeparam name="TOutput">Output type of the block</typeparam>
    /// <seealso cref="Threader"/>
    public class SystemBlock<TInput, TOutput> : Threader
    {
        /// <summary>
        /// Process to do with the inputs
        /// </summary>
        /// <param name="input">Value of the input signal</param>
        /// <returns>Returns a <c>TOutput</c> value.</returns>
        public delegate TOutput ProcessInput(TInput input);
        
        ProcessInput H;

        /// <summary>
        /// Primary input buffer of the block
        /// </summary>
        protected ConcurrentQueue<TInput> InputBuffer;

        /// <summary>
        /// Primary output buffer of the block
        /// </summary>
        protected ConcurrentQueue<TOutput> OutputBuffer;

        /// <summary>
        /// Get an instance of <c>SystemBlock</c>
        /// </summary>
        /// <param name="Process">Delegate of the process to execute for the
        /// inputs</param>
        /// <seealso cref="ProcessInput"/>
        public SystemBlock(ProcessInput Process)
        {
            this.H = Process;
            InputBuffer = new ConcurrentQueue<TInput>();
            OutputBuffer = new ConcurrentQueue<TOutput>();
        }

        /// <summary>
        /// Get an instance of <c>SystemBlock</c>
        /// </summary>
        public SystemBlock()
        {
            InputBuffer = new ConcurrentQueue<TInput>();
            OutputBuffer = new ConcurrentQueue<TOutput>();
        }

        /// <summary>
        /// Set the process to be made to the input.
        /// </summary>
        /// <param name="Process">Process to do with the input</param>
        protected void SetProcess(ProcessInput Process)
        {
            this.H = Process;
        }

        /// <summary>
        /// Insert a new value to the input buffer.
        /// </summary>
        /// <param name="input"><c>TInput</c> value to insert in the queue.</param>
        public void In(TInput input)
        {
            while (InputBuffer.Count > 0)
            {
                InputBuffer.TryDequeue(out TInput V);
            }      
            InputBuffer.Enqueue(input);
        }

        /// <summary>
        /// Get the last output of the block.
        /// </summary>
        /// <param name="output">Last <c>TOutput</c> value calculated.</param>
        public bool Out(out TOutput output)
        {
            bool flag = false;
            output = default(TOutput);
            while(OutputBuffer.Count>0)
            {
                flag = OutputBuffer.TryDequeue(out TOutput TempOutput);
                output = TempOutput;
            }            
            return flag;
        }
        
        /// <summary>
        /// Peeks the last output of the block. Doesn't dequeue it.
        /// </summary>
        /// <param name="output">Last value on the output queue.</param>
        /// <returns>Result state of the operation.</returns>
        public bool PeekOut(out TOutput output)
        {
            bool flag = false;
            flag = OutputBuffer.TryPeek(out output);
            return flag;
        }

        /// <summary>
        /// Task executed by the block every cycle.
        /// </summary>
        public override void ObjectTask()
        {
            TInput input = default(TInput);
            while (!InputBuffer.TryDequeue(out input)) ;
            TOutput output = this.H(input);
            if (output != null)
            {
                OutputBuffer.Enqueue(output);
            }
        }
    }
}
