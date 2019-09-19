using System;
using System.Collections.Generic;
using System.Threading;

namespace EightPuzzleSolver.Search.Algorithms
{
    public class BreadthFirstSearch<TProblemState> : ISearch<TProblemState> where TProblemState : IProblemState<TProblemState>
    {
        public IEnumerable<TProblemState> Search( Problem<TProblemState> kProblem, CancellationToken kCancellationToken = default( CancellationToken ) )
        {
            var kLimitedSearch = new BreadthLimitedSearch<TProblemState>( Int32.MaxValue );

            IEnumerable<TProblemState> kResult = kLimitedSearch.Search( kProblem, kCancellationToken );

            if ( !kLimitedSearch.IsCutoff )
            {
                return kResult;
            }
            else
            {
                return EmptyResult();
            }
        }

        private IEnumerable<TProblemState> EmptyResult()
        {
            return new TProblemState[ 0 ];
        }
    }
}
