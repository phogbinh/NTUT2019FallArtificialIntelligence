using GalaSoft.MvvmLight.Threading;
using System.Windows;

namespace EightPuzzleSolverApp
{
    public partial class App : Application
    {
        static App()
        {
            DispatcherHelper.Initialize();
        }
    }
}
