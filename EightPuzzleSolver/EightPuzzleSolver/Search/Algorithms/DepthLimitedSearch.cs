using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EightPuzzleSolver.Search.Algorithms
{
    public class DepthLimitedSearch<TProblemState> : ISearch<TProblemState>
                                        where TProblemState : IProblemState<TProblemState>
    {
        private bool _isCutoff;

        public DepthLimitedSearch( int limit )
        {
            Limit = limit;
        }

        public int Limit
        {
            get;
        }

        /// <summary>
        /// True when search failed because the limit is reached
        /// </summary>
        public bool IsCutoff => _isCutoff;

        /// <summary>
        /// Returns a list of actions from the initial state to the goal ([root, s1, s2, ..., goal]).
        /// If the goal is not found returns empty list, also sets IsCutoff to true if it was because of the limit.
        /// </summary>
        public IEnumerable<TProblemState> Search( Problem<TProblemState> kProblem, CancellationToken kCancellationToken = default( CancellationToken ) )
        {
            return RecursiveDls( new Node<TProblemState>( kProblem.InitialState ), kProblem, Limit, out _isCutoff, kCancellationToken );
        }

        private IEnumerable<TProblemState> RecursiveDls( Node<TProblemState> kNode,
                                                         Problem<TProblemState> kProblem,
                                                         int nLimit,
                                                         out bool bIsCutoff,
                                                         CancellationToken kCancellationToken )
        {
            bIsCutoff = false;

            kCancellationToken.ThrowIfCancellationRequested();

            if ( kProblem.IsGoalState( kNode.State ) )
            {
                return kNode.PathFromRootStates();
            }
            else if ( nLimit == 0 )
            {
                bIsCutoff = true;
                return EmptyResult();
            }
            else
            {
                bool bCutoffOccurred = false;

                foreach ( Node<TProblemState> kChildNode in kNode.ExpandNode() )
                {
                    bool bIsChildCutoff;

                    IEnumerable<TProblemState> kResult = RecursiveDls( kChildNode, kProblem, nLimit - 1, out bIsChildCutoff, kCancellationToken );

                    if ( bIsChildCutoff )
                    {
                        bCutoffOccurred = true;
                    }
                    else if ( kResult.Any() ) // success
                    {
                        return kResult;
                    }
                }

                bIsCutoff = bCutoffOccurred;
                return EmptyResult();
            }
        }

        private IEnumerable<TProblemState> EmptyResult()
        {
            return new TProblemState[ 0 ];
        }
    }
}
