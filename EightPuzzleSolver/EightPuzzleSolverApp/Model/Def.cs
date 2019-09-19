using System.ComponentModel;

namespace EightPuzzleSolverApp.Model
{
    public enum EAlgorithm
    {
        [Description( "A*" )]
        A_STAR,
        [Description( "Recursive Best-First Search" )]
        RECURSIVE_BEST_FIRST_SEARCH,
        [Description( "Iterative Deepening Search" )]
        ITERATIVE_DEEPENING_SEARCH,
        [Description( "Breadth-First Search" )]
        BREADTH_FIRST_SEARCH
    }

    public enum EHeuristicFunction
    {
        [Description( "Without heuristic (h(x) = 0)" )]
        NO_HEURISTIC,
        [Description( "Manhattan Distance" )]
        MANHATTAN_DISTANCE
    }
}
