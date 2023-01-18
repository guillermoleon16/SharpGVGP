using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpGVGP.Utils
{
    /// <summary>
    /// Metrics provided by the DataProvider class. </summary>
    public enum MetricTypes : byte
    {
        /// <summary>
        /// 
        /// </summary>
        Times,
        /// <summary>
        /// 
        /// </summary>
        Performances,
        /// <summary>
        /// 
        /// </summary>
        Coverages
    }

    /// <summary>
    /// This class allows for threadsafe metric exchange between the agent and the UI.
    /// </summary>
    public class DataProvider
    {
        /// <summary>
        /// Metric list available for exchange
        /// </summary>
        private ConcurrentQueue<double> Coverages, Times, Performances;

        /// <summary>
        /// Creates an instance of the DataProvider class, with its queues empty and
        /// ready to use.
        /// </summary>
        public DataProvider()
        {
            ResetData();
        }

        /// <summary>
        /// Empties all metric queues available.
        /// </summary>
        public void ResetData()
        {
            Coverages = new ConcurrentQueue<double>();
            Times = new ConcurrentQueue<double>();
            Performances = new ConcurrentQueue<double>();
        }

        /// <summary>
        /// Adds a coverage value to the <c>Coverages</c> queue.
        /// </summary>
        /// <param name="coverage">
        /// Coverage value to be added to the queue.</param>
        /// <param name="time">
        /// Time value to be added to the queue.</param>
        /// <returns>
        /// Returns an indicator of the success of the operation</returns>
        public bool AddCoverage(double coverage,double time)
        {
            try
            {
                Coverages.Enqueue(coverage);
                Times.Enqueue(time);
                return true;
            } catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Adds a performance value to the Performances queue.
        /// </summary>
        /// <param name="item">
        /// Value to be added to the queue.</param>
        /// <returns>
        /// Returns an indicator of the success of the operation</returns>
        public bool AddPerformance(double item)
        {
            try
            {
                Performances.Enqueue(item);
                return true;
            }
            catch (Exception Ex) { Console.Write(Ex.Message); return false; }
        }

        /// <summary>
        /// Dequeues all the elements stored on the Coverages and Times queues
        /// as a List of pairs [Coverage, Time]. 
        /// </summary>
        /// <returns>
        /// List of previously stored coverage and times values</returns>
        public List<double[]> GetCoverages()
        {
            List<double[]> ToReturn = new List<double[]>();
            while ((!Times.IsEmpty) && (!Coverages.IsEmpty))
            {
                double[] temp = new double[2];
                if((Times.TryDequeue(out temp[1]))&&(Coverages.TryDequeue(out temp[0])))
                {
                    ToReturn.Add(temp);
                }
            }
            List<double> t;
            if (!Times.IsEmpty)
            {
                t = EmptyQueue(MetricTypes.Times);
            }
            if (!Coverages.IsEmpty)
            {
                t = EmptyQueue(MetricTypes.Coverages);
            }
            return ToReturn;
        }

        /// <summary>
        /// Dequeues all the elements stored on the Performances queue as a List. 
        /// </summary>
        /// <returns>
        /// List of previously stored performance values</returns>
        public List<double> GetPerformances()
        {
            return EmptyQueue(MetricTypes.Performances);
        }

        /// <summary>
        /// Dequeues the contents to the desired metric queue as a List. 
        /// </summary>
        /// <param name="name">
        /// Metric to be extracted</param>
        /// <seealso cref="MetricTypes"/>
        /// <returns>
        /// List of previously stored metric values</returns>
        private List<double> EmptyQueue(MetricTypes name)
        {
            ConcurrentQueue<double> cb;
            if (name == MetricTypes.Times)
            { cb = Times; }
            else if (name == MetricTypes.Performances)
            { cb = Performances; }
            else if (name == MetricTypes.Coverages)
            { cb = Coverages; }
            else
            { return null; }
            List<double> ToReturn = new List<double>();
            while (!cb.IsEmpty)
            {
                if (cb.TryDequeue(out double item))
                {
                    ToReturn.Add(item);
                }
            }
            return ToReturn;
        }
    }
}
