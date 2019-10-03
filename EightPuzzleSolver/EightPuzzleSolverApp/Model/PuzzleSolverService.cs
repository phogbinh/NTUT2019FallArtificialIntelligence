using EightPuzzleSolver.EightPuzzle;
using EightPuzzleSolver.Search;
using EightPuzzleSolver.Search.Algorithms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EightPuzzleSolverApp.Model
{
    public class PuzzleSolverService : IPuzzleSolverService
    {
        public async Task<SolutionSearchResult> SolveAsync( Board initialBoard, EAlgorithm algorithm, EHeuristicFunction heuristicFunction, CancellationToken cancellationToken )
        {
            return await Task.Run( () =>
            {
                return Solve( initialBoard, algorithm, heuristicFunction, cancellationToken );
            }, cancellationToken );
        }

        public SolutionSearchResult Solve( Board kInitialBoard, EAlgorithm eAlgorithmOption, EHeuristicFunction eHeuristicFunctionOption, CancellationToken kCancellationToken )
        {
            EightPuzzleProblem kProblem = new EightPuzzleProblem( kInitialBoard );

            ISearch<EightPuzzleState> kSearch = CreateSearch( kInitialBoard, eAlgorithmOption, eHeuristicFunctionOption );

            Stopwatch kStopwatch = new Stopwatch();
            kStopwatch.Start();
            List<EightPuzzleState> kResult = kSearch.Search( kProblem, kCancellationToken ).ToList();
            kStopwatch.Stop();

            return new SolutionSearchResult( kResult.Any(), kResult, kStopwatch.Elapsed );
        }

        private ISearch<EightPuzzleState> CreateSearch( Board kInitialBoard, EAlgorithm eAlgorithmOption, EHeuristicFunction eHeuristicFunctionOption )
        {
            switch ( eAlgorithmOption )
            {
                case EAlgorithm.A_STAR:
                    return new AStarSearch<EightPuzzleState>( GetHeuristicFunction( kInitialBoard, eHeuristicFunctionOption ) );

                case EAlgorithm.RECURSIVE_BEST_FIRST_SEARCH:
                    return new RecursiveBestFirstSearch<EightPuzzleState>( GetHeuristicFunction( kInitialBoard, eHeuristicFunctionOption ) );

                case EAlgorithm.ITERATIVE_DEEPENING_SEARCH:
                    return new IterativeDeepeningSearch<EightPuzzleState>();

                case EAlgorithm.BREADTH_FIRST_SEARCH:
                    return new BreadthFirstSearch<EightPuzzleState>();

                default:
                    throw new ArgumentOutOfRangeException( nameof( eAlgorithmOption ), eAlgorithmOption, null );

            }
        }

        private IHeuristicFunction<EightPuzzleState> GetHeuristicFunction( Board kInitialBoard, EHeuristicFunction eHeuristicFunctionOption )
        {
            Board kGoalBoard = Board.CreateGoalBoard( kInitialBoard.RowCount, kInitialBoard.ColumnCount );

            switch ( eHeuristicFunctionOption )
            {
                case EHeuristicFunction.NO_HEURISTIC:
                    return new NoHeuristicFunction<EightPuzzleState>();

                case EHeuristicFunction.MANHATTAN_DISTANCE:
                    return new ManhattanHeuristicFunction( kGoalBoard );

                default:
                    throw new ArgumentOutOfRangeException( nameof( eHeuristicFunctionOption ), eHeuristicFunctionOption, null );

            }
        }
    }
}