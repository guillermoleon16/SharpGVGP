using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpGVGP.Systems.Async
{
    public class AsyncSystemBlock<TIn,TOut>
    {
        #region Public attributes
        public delegate TOut ProcessInput(TIn input);
        #endregion

        #region Private attributes
        //0 for false, 1 for true;
        protected long Ready;
        protected TOut Output;
        private ProcessInput H;
        protected CancellationTokenSource cts;
        #endregion

        #region Constructors
        public AsyncSystemBlock()
        {
            Ready = 0;
            Output = default(TOut);
        }

        public AsyncSystemBlock(ProcessInput process)
        {
            Ready = 0;
            Output = default(TOut);
            H = process;
        }
        #endregion

        public void SetProcess(ProcessInput process)
        {
            H = process;
        }

        public TOut GetOutput()
        {
            Ready = 0;
            return Output;            
        }

        public bool CheckReady()
        {
            long flag = Interlocked.Read(ref Ready);
            if (flag == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool BeginProcess(TIn input)
        {
            if (1 == Interlocked.Exchange(ref Ready, 0)) //Block was sleeping
            {
                this.Ready = 0;
                cts = new CancellationTokenSource();
                try
                {
                    WorkAsync(input, cts.Token);
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
        
        public bool StopProcess()
        {
            try
            {
                if (cts != null)
                {
                    cts.Cancel();
                    Console.WriteLine("Block stopped");
                }
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error at stopping block.");
                Console.WriteLine(ex.Message);
                return false;
            }            
        }

        protected async void WorkAsync(TIn input,CancellationToken token)
        {
            await Task.Factory.StartNew(() => {
                Output = H(input);
                this.Ready = 1;
            });            
        }
    }
}
