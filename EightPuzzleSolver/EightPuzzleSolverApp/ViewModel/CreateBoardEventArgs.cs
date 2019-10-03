using EightPuzzleSolver.EightPuzzle;
using System;

namespace EightPuzzleSolverApp.ViewModel
{
    public class CreateBoardEventArgs : EventArgs
    {
        public CreateBoardEventArgs()
        {

        }

        public CreateBoardEventArgs( Board board )
        {
            Board = board;
        }

        public Board Board
        {
            get; set;
        }
    }
}
