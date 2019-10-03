using System;
using System.Windows.Media.Imaging;

namespace EightPuzzleSolverApp.ViewModel
{
    public class SetBoardImageEventArgs : EventArgs
    {
        public SetBoardImageEventArgs( BitmapImage kBoardImage )
        {
            BoardImage = kBoardImage;
        }

        public BitmapImage BoardImage
        {
            get; set;
        }
    }
}
