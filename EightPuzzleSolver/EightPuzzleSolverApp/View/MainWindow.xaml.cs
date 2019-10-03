using EightPuzzleSolver.EightPuzzle;
using EightPuzzleSolverApp.ViewModel;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace EightPuzzleSolverApp.View
{
    public partial class MainWindow : Window
    {
        private class Tile
        {
            private Border m_kBorder;
            private readonly TextBlock m_kTextBlock;

            public Tile( Border kBorder )
            {
                m_kBorder = kBorder;
                m_kTextBlock = ( TextBlock ) kBorder.Child;
            }

            public void SetText( string strText )
            {
                m_kTextBlock.Text = strText;
            }

            public void SetVisibility( Visibility eVisibility )
            {
                m_kBorder.Visibility = eVisibility;
            }

            public void Move( MoveDirection kDirection, Action kCallbackFunc, int nDurationMs = 900 )
            {
                var kDuration = new Duration( TimeSpan.FromMilliseconds( nDurationMs ) );

                bool bIsHorizontalMove = kDirection.ColumnChange != 0;
                int nMoveDistance = bIsHorizontalMove ? kDirection.ColumnChange : kDirection.RowChange;

                var kTransform = new TranslateTransform();
                m_kBorder.RenderTransform = kTransform;

                var kAnimation = new DoubleAnimation( nMoveDistance * TILE_SIZE, kDuration );
                kAnimation.Completed += ( s, e ) =>
                {
                    m_kBorder.RenderTransform = null;
                    kCallbackFunc();
                };
                kTransform.BeginAnimation( bIsHorizontalMove ? TranslateTransform.XProperty : TranslateTransform.YProperty, kAnimation );
            }
        }

        private MainViewModel _viewModel;

        private const int TILE_SIZE = 70;

        private Tile[,] m_kTiles;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnLoaded( object sender, RoutedEventArgs e )
        {
            _viewModel = ( MainViewModel ) DataContext;

            _viewModel.CreateBoard += VmOnCreateBoard;
            _viewModel.ShowMoves += VmShowMoves;

            _viewModel.FillBoardCommand.Execute( null );
        }

        private void VmOnCreateBoard( object kSender, CreateBoardEventArgs kEventArgs )
        {
            Board kBoard = kEventArgs.Board;

            m_kGridBoard.Children.Clear();

            m_kGridBoard.Rows = kBoard.RowCount;
            m_kGridBoard.Columns = kBoard.ColumnCount;

            m_kGridBoard.Height = kBoard.RowCount * TILE_SIZE;
            m_kGridBoard.Width = kBoard.ColumnCount * TILE_SIZE;

            m_kTiles = new Tile[ kBoard.RowCount, kBoard.ColumnCount ];

            for ( int i = 0; i < kBoard.RowCount; i++ )
            {
                for ( int j = 0; j < kBoard.ColumnCount; j++ )
                {
                    var kTextBlock = new TextBlock
                    {
                        FontSize = 20,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    var kBorder = new Border
                    {
                        Background = new SolidColorBrush( Colors.WhiteSmoke ),
                        CornerRadius = new CornerRadius( 3 ),
                        Margin = new Thickness( 3 ),
                        Child = kTextBlock
                    };

                    m_kTiles[ i, j ] = new Tile( kBorder );

                    m_kGridBoard.Children.Add( kBorder );
                }
            }

            SetTileValues( kBoard );
        }

        private void VmShowMoves( object sender, EventArgs eventArgs )
        {
            ShowNextMove();
        }

        private void ShowNextMove()
        {
            EightPuzzleState kState = _viewModel.NextMoveState();
            if ( kState == null )
            {
                return;
            }

            Position kBlankTilePosition = kState.Board.BlankTilePosition;

            Debug.Assert( kState.Direction != null, "state.Direction != null" );
            MoveDirection kMoveDirection = kState.Direction.Value.Opposite();

            m_kTiles[ kBlankTilePosition.Row, kBlankTilePosition.Column ].Move( kMoveDirection, () =>
            {
                SetTileValues( kState.Board );
                ShowNextMove();
            } );
        }

        private void SetTileValues( Board board )
        {
            for ( int i = 0; i < board.RowCount; i++ )
            {
                for ( int j = 0; j < board.ColumnCount; j++ )
                {
                    int val = board[ i, j ];

                    var tile = m_kTiles[ i, j ];

                    tile.SetVisibility( val == 0 ? Visibility.Hidden : Visibility.Visible );

                    tile.SetText( val.ToString() );
                }
            }
        }


        private void lstMoves_OnSelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            if ( e.AddedItems.Count > 0 )
            {
                ( ( ListBox ) sender ).ScrollIntoView( e.AddedItems[ 0 ] );
            }
        }
    }
}