using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EightPuzzleSolver.Search.Algorithms
{
    public class BreadthLimitedSearch<TProblemState> : ISearch<TProblemState> where TProblemState : IProblemState<TProblemState>
    {
        private bool m_bIsCutoff;

        public BreadthLimitedSearch( int nLimit )
        {
            Limit = nLimit;
        }

        public int Limit
        {
            get;
        }

        /// <summary>
        /// True when search failed because the limit is reached
        /// </summary>
        public bool IsCutoff => m_bIsCutoff;

        /// <summary>
        /// Returns a list of actions from the initial state to the goal ([root, s1, s2, ..., goal]).
        /// If the goal is not found returns empty list, also sets IsCutoff to true if it was because of the limit.
        /// </summary>
        public IEnumerable<TProblemState> Search( Problem<TProblemState> kProblem, CancellationToken kCancellationToken = default( CancellationToken ) )
        {
            var kRootNode = new Node<TProblemState>( kProblem.InitialState );
            var kQueue = new Queue<Node<TProblemState>>();
            kQueue.Enqueue( kRootNode );

            m_bIsCutoff = true;

            while ( 0 < kQueue.Count && kQueue.Count < Limit )
            {
                kCancellationToken.ThrowIfCancellationRequested();

                Node<TProblemState> kNode = kQueue.Dequeue();

                if ( kProblem.IsGoalState( kNode.State ) )
                {
                    m_bIsCutoff = false;
                    return kNode.PathFromRootStates();
                }

                foreach ( Node<TProblemState> kChildNode in kNode.ExpandNode() )
                {
                    kQueue.Enqueue( kChildNode );
                }
            }

            return EmptyResult();
        }

        private IEnumerable<TProblemState> EmptyResult()
        {
            return new TProblemState[ 0 ];
        }
    }
}
