﻿using EightPuzzleSolver.EightPuzzle;
using EightPuzzleSolverApp.Model;
using EightPuzzleSolverApp.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media.Imaging;

namespace EightPuzzleSolverApp.ViewModel
{
    public enum EWorkState
    {
        IDLE,
        SEARCHING,
        SHOWING_MOVES,
        STOPPING,
        MANUAL_PLAYING
    }

    public sealed class MainViewModel : ViewModelBase, IDisposable
    {
        public IDialogService DialogService => _dialogService;

        private const string ALL_FILES_FILTER = "All files(*.*)|*.*";
        private const string TEXT_FILES_FILTER = "Text files (*.txt)|*.txt|" + ALL_FILES_FILTER;
        private const string IMAGE_FILES_FILTER = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*jpg|" + ALL_FILES_FILTER;
        private readonly IPuzzleSolverService _puzzleSolverService;
        private readonly IDialogService _dialogService;

        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        public MainViewModel( IPuzzleSolverService puzzleSolverService, IDialogService dialogService )
        {
            _puzzleSolverService = puzzleSolverService;
            _dialogService = dialogService;

            if ( IsInDesignMode )
            {
                SearchResult = new SolutionSearchResult( true, new List<EightPuzzleState>()
                {
                    new EightPuzzleState(null),
                    new EightPuzzleState(null, null, MoveDirection.Left),
                    new EightPuzzleState(null, null, MoveDirection.Bottom),
                    new EightPuzzleState(null, null, MoveDirection.Right),
                }, TimeSpan.FromMilliseconds( 3678 ) );
                CurrentMoveNumber = 1;
            }
        }

        public delegate void CreateBoardHandler( object sender, CreateBoardEventArgs e );

        /// <summary>
        /// Occurs when a (new) board needs to be displayed
        /// </summary>
        public event CreateBoardHandler CreateBoard;

        public delegate void ShowMovesHandler( object sender, EventArgs e );

        /// <summary>
        /// Occurs when the solution moves need to be displayed
        /// </summary>
        public event ShowMovesHandler ShowMoves;

        public delegate void SetBoardImageHandler( object kSender, SetBoardImageEventArgs kEventArgs );

        /// <summary>
        /// Occurs when the board image is set
        /// </summary>
        public event SetBoardImageHandler SetBoardImage;

        private EWorkState _state = EWorkState.IDLE;
        public EWorkState State
        {
            get
            {
                return _state;
            }
            set
            {
                if ( _state == value )
                {
                    return;
                }

                _state = value;
                RaisePropertyChanged();

                SolveOrStopCommand.RaiseCanExecuteChanged();
                GenerateBoardCommand.RaiseCanExecuteChanged();
                LoadBoardCommand.RaiseCanExecuteChanged();
                SaveBoardCommand.RaiseCanExecuteChanged();
                FillBoardCommand.RaiseCanExecuteChanged();
                EnterOrExitManualPlayCommand.RaiseCanExecuteChanged();
                LoadImageCommand.RaiseCanExecuteChanged();
                UnloadImageCommand.RaiseCanExecuteChanged();
            }
        }

        private int _rowCount = 3;
        public int RowCount
        {
            get
            {
                return _rowCount;
            }
            set
            {
                if ( _rowCount == value )
                {
                    return;
                }

                _rowCount = value;
                RaisePropertyChanged();
            }
        }

        private int _columnCount = 3;
        public int ColumnCount
        {
            get
            {
                return _columnCount;
            }
            set
            {
                if ( _columnCount == value )
                {
                    return;
                }

                _columnCount = value;
                RaisePropertyChanged();
            }
        }

        private string m_strBoardInputText = "8 6 7\r\n2 5 4\r\n3 0 1";
        public string BoardInputText
        {
            get
            {
                return m_strBoardInputText;
            }
            set
            {
                if ( m_strBoardInputText == value )
                {
                    return;
                }

                m_strBoardInputText = value;
                RaisePropertyChanged();
            }
        }

        private BitmapImage m_kBoardImage = null;
        public BitmapImage BoardImage
        {
            get
            {
                return m_kBoardImage;
            }
            set
            {
                if ( m_kBoardImage == value )
                {
                    return;
                }

                m_kBoardImage = value;
                RaisePropertyChanged();
            }
        }

        private Board m_kCurrentBoard;
        public Board CurrentBoard
        {
            get
            {
                return m_kCurrentBoard;
            }
            set
            {
                if ( m_kCurrentBoard == value )
                {
                    return;
                }

                m_kCurrentBoard = value;
                RaisePropertyChanged();
            }
        }

        private SolutionSearchResult _searchResult;
        public SolutionSearchResult SearchResult
        {
            get
            {
                return _searchResult;
            }
            set
            {
                if ( _searchResult == value )
                {
                    return;
                }

                _searchResult = value;
                RaisePropertyChanged();
            }
        }

        private int _currentMoveNumber = -1;

        [SuppressMessage( "ReSharper", "ExplicitCallerInfoArgument" )]
        public int CurrentMoveNumber
        {
            get
            {
                return _currentMoveNumber;
            }
            set
            {
                if ( _currentMoveNumber == value )
                {
                    return;
                }

                _currentMoveNumber = value;
                RaisePropertyChanged();

                RaisePropertyChanged( nameof( CurrentMoveIndex ) );
            }
        }

        public int CurrentMoveIndex => CurrentMoveNumber - 1;

        public static IList<EAlgorithm> Algorithms
        {
            get;
        } = new[]
        {
            EAlgorithm.A_STAR,
            EAlgorithm.RECURSIVE_BEST_FIRST_SEARCH,
            EAlgorithm.ITERATIVE_DEEPENING_SEARCH,
            EAlgorithm.BREADTH_FIRST_SEARCH
        };

        private EAlgorithm _selectedAlgorithm = Algorithms.First();
        public EAlgorithm SelectedAlgorithm
        {
            get
            {
                return _selectedAlgorithm;
            }
            set
            {
                if ( _selectedAlgorithm == value )
                {
                    return;
                }

                _selectedAlgorithm = value;
                RaisePropertyChanged();
            }
        }

        public static IList<EHeuristicFunction> HeuristicFunctions { get; } = ( EHeuristicFunction[] ) Enum.GetValues( typeof( EHeuristicFunction ) );

        private EHeuristicFunction _selectedHeuristicFunction = HeuristicFunctions[ 1 ];
        public EHeuristicFunction SelectedHeuristicFunction
        {
            get
            {
                return _selectedHeuristicFunction;
            }
            set
            {
                if ( _selectedHeuristicFunction == value )
                {
                    return;
                }

                _selectedHeuristicFunction = value;
                RaisePropertyChanged();
            }
        }

        private RelayCommand _generateBoardCommand;
        public RelayCommand GenerateBoardCommand
        {
            get
            {
                return _generateBoardCommand
                       ?? ( _generateBoardCommand = new RelayCommand(
                           GenerateBoard,
                           () => State == EWorkState.IDLE ) );
            }
        }

        private RelayCommand _solveOrStopCommand;
        public RelayCommand SolveOrStopCommand
        {
            get
            {
                return _solveOrStopCommand
                       ?? ( _solveOrStopCommand = new RelayCommand(
                           () =>
                           {
                               if ( State == EWorkState.IDLE )
                               {
                                   Solve();
                               }
                               else
                               {
                                   Stop();
                               }
                           },
                           () => State == EWorkState.IDLE || State == EWorkState.SEARCHING || State == EWorkState.SHOWING_MOVES ) );
            }
        }

        private RelayCommand m_kLoadBoardCommand;
        public RelayCommand LoadBoardCommand
        {
            get
            {
                return m_kLoadBoardCommand
                       ?? ( m_kLoadBoardCommand = new RelayCommand(
                           LoadBoard,
                           () => State == EWorkState.IDLE ) );
            }
        }

        private RelayCommand m_kSaveBoardCommand;
        public RelayCommand SaveBoardCommand
        {
            get
            {
                return m_kSaveBoardCommand
                       ?? ( m_kSaveBoardCommand = new RelayCommand(
                           SaveBoard,
                           () => State == EWorkState.IDLE ) );
            }
        }

        private RelayCommand _fillBoardCommand;
        public RelayCommand FillBoardCommand
        {
            get
            {
                return _fillBoardCommand
                       ?? ( _fillBoardCommand = new RelayCommand(
                           FillBoard,
                           () => State == EWorkState.IDLE ) );
            }
        }

        private RelayCommand m_kEnterOrExitManualPlayCommand;
        public RelayCommand EnterOrExitManualPlayCommand
        {
            get
            {
                return m_kEnterOrExitManualPlayCommand
                       ?? ( m_kEnterOrExitManualPlayCommand = new RelayCommand(
                           () =>
                           {
                               if ( State == EWorkState.IDLE )
                               {
                                   EnterManualPlay();
                               }
                               else
                               {
                                   ExitManualPlay();
                               }
                           },
                           () => State == EWorkState.IDLE || State == EWorkState.MANUAL_PLAYING ) );
            }
        }

        private RelayCommand m_kLoadImageCommand;
        public RelayCommand LoadImageCommand
        {
            get
            {
                return m_kLoadImageCommand
                       ?? ( m_kLoadImageCommand = new RelayCommand(
                           LoadImage,
                           () => State == EWorkState.IDLE ) );
            }
        }

        private RelayCommand m_kUnloadImageCommand;
        public RelayCommand UnloadImageCommand
        {
            get
            {
                return m_kUnloadImageCommand
                       ?? ( m_kUnloadImageCommand = new RelayCommand(
                           UnloadImage,
                           () => State == EWorkState.IDLE ) );
            }
        }

        public EightPuzzleState NextMoveState()
        {
            if ( CurrentMoveNumber + 1 > SearchResult.MoveCount || _cancellationToken.IsCancellationRequested )
            {
                State = EWorkState.IDLE;

                return null;
            }

            CurrentMoveNumber++;

            return SearchResult.Solution[ CurrentMoveNumber ];
        }

        private void GenerateBoard()
        {
            var board = Board.GenerateSolvableBoard( RowCount, ColumnCount );

            BoardInputText = BoardToText( board );

            FillBoard();
        }

        private void LoadBoard()
        {
            OpenFileDialog kDialog = new OpenFileDialog();
            kDialog.Filter = TEXT_FILES_FILTER;
            if ( kDialog.ShowDialog() == true )
            {
                BoardInputText = File.ReadAllText( kDialog.FileName );
                FillBoard();
            }
        }

        private void SaveBoard()
        {
            SaveFileDialog kDialog = new SaveFileDialog();
            kDialog.Filter = TEXT_FILES_FILTER;
            if ( kDialog.ShowDialog() == true )
            {
                File.WriteAllText( kDialog.FileName, BoardInputText );
            }
        }

        private void FillBoard()
        {
            Board board;

            try
            {
                board = TextToBoard( BoardInputText );
            }
            catch ( Exception ex )
            {
                _dialogService.ShowError( "Incorrect input: " + ex.Message );
                return;
            }

            if ( !board.IsCorrect() )
            {
                _dialogService.ShowError( "Board is not correct." );
                return;
            }

            BoardInputText = BoardToText( board );

            CurrentBoard = board;

            OnCreateBoard( new CreateBoardEventArgs( board ) );
        }

        private async void Solve()
        {
            OnCreateBoard( new CreateBoardEventArgs( CurrentBoard ) );

            if ( !CurrentBoard.IsSolvable() )
            {
                if ( !_dialogService.ShowConfirmation( "Looks like this board is not solvable. Are you sure you want to continue?" ) )
                    return;
            }

            CreateCancellationToken();

            SearchResult = null;

            State = EWorkState.SEARCHING;

            try
            {
                SearchResult = await _puzzleSolverService.SolveAsync( CurrentBoard, SelectedAlgorithm, SelectedHeuristicFunction, _cancellationToken );

                if ( !SearchResult.Success )
                {
                    _dialogService.ShowError( "Solution was not found." );
                }
                else
                {
                    StartShowingMoves();

                    return;
                }
            }
            catch ( OperationCanceledException )
            {
            }
            catch ( Exception ex )
            {
                _dialogService.ShowError( ex.Message );
            }

            State = EWorkState.IDLE;
        }

        private void StartShowingMoves()
        {
            OnCreateBoard( new CreateBoardEventArgs( CurrentBoard ) );

            CreateCancellationToken();

            State = EWorkState.SHOWING_MOVES;

            CurrentMoveNumber = 0;

            OnShowMoves();
        }

        private void EnterManualPlay()
        {
            OnCreateBoard( new CreateBoardEventArgs( CurrentBoard ) );
            State = EWorkState.MANUAL_PLAYING;
        }

        private void ExitManualPlay()
        {
            State = EWorkState.IDLE;
        }

        private void LoadImage()
        {
            OpenFileDialog kDialog = new OpenFileDialog();
            kDialog.Filter = IMAGE_FILES_FILTER;
            if ( kDialog.ShowDialog() == true )
            {
                BoardImage = CreateBitmapImageFromImageName( kDialog.FileName );
                OnSetBoardImage( new SetBoardImageEventArgs( BoardImage ) );
            }
        }

        private void OnSetBoardImage( SetBoardImageEventArgs kEventArgs )
        {
            SetBoardImage?.Invoke( this, kEventArgs );
        }

        private BitmapImage CreateBitmapImageFromImageName( string strImageName )
        {
            BitmapImage kBitmap = new BitmapImage();
            kBitmap.BeginInit();
            kBitmap.UriSource = new Uri( strImageName );
            kBitmap.EndInit();
            return kBitmap;
        }

        private void UnloadImage()
        {
            BoardImage = null;
            OnSetBoardImage( new SetBoardImageEventArgs( BoardImage ) );
        }

        private void Stop()
        {
            State = EWorkState.STOPPING;

            _cancellationTokenSource.Cancel();
        }

        private void CreateCancellationToken()
        {
            _cancellationTokenSource?.Dispose();

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
        }

        private static string BoardToText( Board board )
        {
            var sb = new StringBuilder();

            for ( int i = 0; i < board.RowCount; i++ )
            {
                for ( int j = 0; j < board.ColumnCount; j++ )
                {
                    sb.Append( board[ i, j ] + " " );
                }
                if ( i < board.RowCount - 1 )
                    sb.AppendLine();
            }

            return sb.ToString();
        }

        private static Board TextToBoard( string str )
        {
            var lines = str.Split( new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries );

            List<List<byte>> rows;
            try
            {
                rows = lines.Select( l => l.Split( new[] { " " }, StringSplitOptions.RemoveEmptyEntries )
                                         .Select( byte.Parse ).ToList() )
                                    .ToList();
            }
            catch ( FormatException )
            {
                throw new Exception( "must contains only numbers and spaces/newlines." );
            }

            int rowCount = rows.Count;
            int columnCount = rows.First().Count;

            if ( rows.Any( r => r.Count != columnCount ) )
                throw new Exception( "all rows must have the same size" );

            var data = new byte[ rowCount, columnCount ];

            for ( int i = 0; i < rowCount; i++ )
            {
                for ( int j = 0; j < columnCount; j++ )
                {
                    data[ i, j ] = rows[ i ][ j ];
                }
            }

            return new Board( data );
        }

        private void OnCreateBoard( CreateBoardEventArgs args )
        {
            CreateBoard?.Invoke( this, args );
        }

        private void OnShowMoves()
        {
            ShowMoves?.Invoke( this, EventArgs.Empty );
        }

        public void Dispose()
        {
            if ( _cancellationTokenSource != null )
            {
                _cancellationTokenSource.Dispose();
            }
        }

        public EStatus MoveTile( Position kTilePosition, out MoveDirection kBlankTileMoveDirection )
        {
            Position kBlankTilePosition = CurrentBoard.BlankTilePosition;
            kBlankTileMoveDirection = GetBlankTileMoveDirection( kBlankTilePosition, kTilePosition );
            if ( kBlankTileMoveDirection.Equals( MoveDirection.None ) )
            {
                return EStatus.FAILURE;
            }

            CurrentBoard = CurrentBoard.Move( kBlankTileMoveDirection );
            BoardInputText = BoardToText( CurrentBoard );
            return EStatus.SUCCESS;
        }

        private MoveDirection GetBlankTileMoveDirection( Position kBlankTilePosition, Position kTilePosition )
        {
            foreach ( MoveDirection kDirection in MoveDirection.AllDirections )
            {
                Position kNewBlankTilePosition = kBlankTilePosition.Move( kDirection );
                if ( kNewBlankTilePosition == kTilePosition )
                {
                    return kDirection;
                }
            }
            return MoveDirection.None;
        }
    }
}