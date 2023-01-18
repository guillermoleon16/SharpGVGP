using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpGVGP.Systems.Async
{
    class AsyncTwoInputBlock<TIn1,TIn2,TOut>: AsyncSystemBlock<TIn1,TOut>
    {
        new public delegate TOut ProcessInput(TIn1 input1, TIn2 input2);
        private ProcessInput H;

        #region Constructors
        public AsyncTwoInputBlock()
        {
            Ready = 0;
            Output = default(TOut);
        }

        public AsyncTwoInputBlock(ProcessInput process)
        {
            Ready = 0;
            Output = default(TOut);
            H = process;
        }
        #endregion

        public bool BeginProcess(TIn1 input1, TIn2 input2)
        {
            if (1 == Interlocked.Exchange(ref Ready, 0)) //Block was sleeping
            {
                this.Ready = 0;
                cts = new CancellationTokenSource();
                try
                {
                    WorkAsync(input1, input2, cts.Token);
                    cts.Dispose();
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("A block was stopped.");
                }
                return true;
            }
            else //Block was processing an input
            {
                Console.WriteLine("A block was already working.");
                return false;
            }
        }

        protected async void WorkAsync(TIn1 input1, TIn2 input2
            , CancellationToken token)
        {
            await Task.Factory.StartNew(() => {
                Output = H(input1, input2);
                this.Ready = 1;
            });
        }
    }
}
