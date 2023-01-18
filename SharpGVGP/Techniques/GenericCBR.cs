using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace SharpGVGP
{
    /// <summary>
    /// Class that allows the implementation of CBR controller with online revise.
    /// </summary>
    /// <typeparam name="TCase">Type of the input current case.</typeparam>
    /// <typeparam name="TSolution">Type of the output solution.</typeparam>
    /// <typeparam name="TMetric">Type of the evaluation/ranking metric</typeparam>
    public class GenericCBR<TCase,TSolution,TMetric>: Threader
    {
        private delegate TSolution Retrieve(TCase CurrentCase);

        private List<Case> CaseBase;
        private ConcurrentQueue<Feedback> FeedbackQueue;

        /// <summary>
        /// This process takes feedback and updates the case base.
        /// </summary>
        public override void ObjectTask()
        {
            while (!FeedbackQueue.IsEmpty)
            {

            }
        }



        private class Feedback
        {
            public TCase DeliveredCase{ get;}
            public TSolution ProposedSolution{ get;}
            public TMetric ObtainedMetric { get;}

            public Feedback(TCase deliveredCase, TSolution proposedSolution, TMetric obtainedMetric)
            {
                DeliveredCase = deliveredCase;
                ProposedSolution = proposedSolution;
                ObtainedMetric = obtainedMetric;
            }
        }

        private class Case
        {
            private TCase SavedCase { get; }
            private TMetric Performace { get; }
            private TSolution Solution { get; }
            private long ID { get; }

            public Case(TCase savedCase, TMetric performace, TSolution solution, long iD)
            {
                SavedCase = savedCase;
                Performace = performace;
                Solution = solution;
                ID = iD;
            }
        }
    }
}
