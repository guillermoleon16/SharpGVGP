using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpGVGP
{
    /// <summary>
    /// This class stipulates the methods necessary for using classes which perform
    /// threading tasks during gameplay as the namespace may manage them.
    /// </summary>
    public abstract class Threader
    {
        private Thread ActiveThread;

        //0 for false, 1 for true
        private long PauseFlag;
        private long StepCount;

        /// <summary>
        /// Gets an instance of <c>Threader</c>
        /// </summary>
        public Threader()
        {
            PauseFlag = 0;
            StepCount = 0;
        }

        /// <summary>
        /// Begins execution of the thread.
        /// </summary>
        /// <returns>Status of the operation</returns>
        public virtual bool Start()
        {
            try
            {
                if (ActiveThread == null)
                {
                    ActiveThread = new Thread(PausableThread);
                }
                if (!ActiveThread.IsAlive)
                {
                    ActiveThread = new Thread(PausableThread);
                    ActiveThread.Start();
                }
                return true;
            }
            catch (Exception Ex)
            {
                Console.WriteLine
                        ("Exception catched on start block:  {0}",
                        Ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Stops execution of the thread.
        /// </summary>
        /// <returns>Status of the operation</returns>
        public virtual bool Stop()
        {
            try
            {
                if (ActiveThread != null)
                {
                    if (ActiveThread.IsAlive)
                    {
                        ActiveThread.Abort();
                    }
                }
                return true;
            }
            catch (Exception Ex)
            {
                Console.WriteLine
                        ("Exception catched on stop block:  {0}",
                        Ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Toggles the pause of the execution of the 
        /// object's thread after finishing the current loop.
        /// </summary>
        /// <returns>Returns the status of the operation.</returns>
        public bool TogglePause()
        {
            bool RetrievedFlag = false;
            while (!PauseStatus(out RetrievedFlag)) ;
            return SetPause(!RetrievedFlag);
        }

        /// <summary>
        /// Allow a single execution cycle for the thread.
        /// </summary>
        /// <returns>Returns the status of the operation.</returns>
        public bool SetStep()
        {
            int RetrievedSteps = 0;
            while (!PendingSteps(out RetrievedSteps)) ;
            return AddStep();
        }

        private bool AddStep()
        {
            try
            {
                Interlocked.Increment(ref StepCount);
                return true;
            }
            catch (Exception Ex)
            {
                Console.WriteLine("Error adding step to the block");
                Console.WriteLine(Ex.Message);
                return false;
            }
        }

        private bool RemoveStep()
        {
            try
            {
                Interlocked.Decrement(ref StepCount);
                return true;
            }
            catch (Exception Ex)
            {
                Console.WriteLine("Error removing step from the block");
                Console.WriteLine(Ex.Message);
                return false;
            }
        }

        /// <summary>
        /// The threading task the object has to do.
        /// Must be implemented by the user.
        /// DO NOT USE <c>while(true)</c> ON THE IMPLEMENTATION
        /// </summary>
        abstract public void ObjectTask();

        /// <summary>
        /// Execute the object task as long as the thread hasn't been paused.
        /// </summary>
        private void PausableThread()
        {
            while (true)
            {
                if (PauseStatus(out bool flag))
                {
                    if (!flag)
                    {
                        ObjectTask();
                    }
                    else
                    {
                        if(PendingSteps(out int i))
                        {
                            if (i > 0)
                            {
                                ObjectTask();
                                RemoveStep();
                            }
                        }
                        Thread.Sleep(1000);
                    }
                }
                else
                {
                    Console.WriteLine("Error at flag read");
                }
            }
        }

        /// <summary>
        /// Get the current status of the pause flag.
        /// </summary>
        /// <param name="Status">Variable where the current status of the
        /// pause flag will be returned.</param>
        /// <returns>Success state of the operation</returns>
        public bool PauseStatus(out bool Status)
        {
            try
            {
                if (0 == Interlocked.Read(ref PauseFlag))
                {
                    Status = false;
                }
                else
                {
                    Status = true;
                }
                return true;
            }
            catch(Exception Ex)
            {
                Console.Write(Ex.Message);
                Status = false;
                return false;
            }            
        }

        /// <summary>
        /// Get the steps required before pausing again.
        /// </summary>
        /// <param name="Steps">Variable where the step count will be
        /// returned.</param>
        /// <returns>Success state of the operation</returns>
        public bool PendingSteps(out int Steps)
        {
            try
            {
                Steps = (int)Interlocked.Read(ref StepCount);
                return true;
            }
            catch (Exception Ex)
            {
                Console.Write(Ex.Message);
                Steps = 0;
                return false;
            }
        }

        /// <summary>
        /// Sets the pause flag to the desired value.
        /// </summary>
        /// <param name="NewStatus">New pause flag value.</param>
        /// <returns>Success state of the operation.</returns>
        protected bool SetPause(bool NewStatus)
        {
            try
            {
                if (NewStatus)
                {
                    Interlocked.Exchange(ref PauseFlag, 1);
                }
                else
                {
                    Interlocked.Exchange(ref PauseFlag, 0);
                }
                return true;
            }
            catch(Exception Ex)
            {
                Console.Write(Ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Disposes safely of any instance of <c>Threader</c>
        /// </summary>
        ~Threader()
        {
            Stop();
        }
    }
}
