using Infinite_tic_tac_toe.Game;
using Infinite_tic_tac_toe.Model;
using Infinite_tic_tac_toe.Solvers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Infinite_tic_tac_toe.UserInterface.ViewModels
{
      public class GameViewModel : ViewModelBase
      {
            public event PropertyChangedEventHandler? PropertyChanged;

            private GameCoordinator? _gameCoordinator;
            private IPlayer? _player1;
            private IPlayer? _player2;
            private HumanPlayer? _currentHumanPlayer;

            private bool _isGameRunning;
            private string _statusMessage = "Ready to start";
            private (int x, int y)? _selectedPosition = null;

            public object? SelectedPlayer1SettingsView => null;
            public object? SelectedPlayer2SettingsView => null;

            public ObservableCollection<object?> BoardPositions { get; } = new ObservableCollection<object?>(new object?[9]);

            public ICommand CellClickCommand { get; }
            public ICommand StartGameCommand { get; }
            public ICommand ResetGameCommand { get; }

            public enum PlayerTypeOption
            {
                  Human,
                  Ai
            }

            public ObservableCollection<PlayerTypeOption> PlayerOptions { get; } =
                new ObservableCollection<PlayerTypeOption>(Enum.GetValues(typeof(PlayerTypeOption)).Cast<PlayerTypeOption>());

            private PlayerTypeOption _selectedPlayer1Type = PlayerTypeOption.Human;
            public PlayerTypeOption SelectedPlayer1Type
            {
                  get => _selectedPlayer1Type;
                  set
                  {
                        if (_selectedPlayer1Type != value)
                        {
                              _selectedPlayer1Type = value;
                              Player1 = CreatePlayer(value, PlayerEnum.Cross);
                              OnPropertyChanged();
                              OnPropertyChanged(nameof(SelectedPlayer1SettingsView));
                        }
                  }
            }

            private PlayerTypeOption _selectedPlayer2Type = PlayerTypeOption.Ai;
            public PlayerTypeOption SelectedPlayer2Type
            {
                  get => _selectedPlayer2Type;
                  set
                  {
                        if (_selectedPlayer2Type != value)
                        {
                              _selectedPlayer2Type = value;
                              Player2 = CreatePlayer(value, PlayerEnum.Naught);
                              OnPropertyChanged();
                              OnPropertyChanged(nameof(SelectedPlayer2SettingsView));
                        }
                  }
            }


            public IPlayer? Player1
            {
                  get => _player1;
                  set { _player1 = value; OnPropertyChanged(); }
            }

            public IPlayer? Player2
            {
                  get => _player2;
                  set { _player2 = value; OnPropertyChanged(); }
            }

            public string StatusMessage
            {
                  get => _statusMessage;
                  private set { _statusMessage = value; OnPropertyChanged(); }
            }

            public bool IsCellClickEnabled => _isGameRunning && _currentHumanPlayer?.PlayerType == PlayerType.Local;

            public GameViewModel()
            {
                  CellClickCommand = new RelayCommand(OnCellClicked, _ => IsCellClickEnabled);
                  StartGameCommand = new RelayCommand(async _ => await StartGame(), _ => !_isGameRunning);
                  ResetGameCommand = new RelayCommand(_ => ResetGame(), _ => _isGameRunning);

                  Player1 = new HumanPlayer(PlayerEnum.Cross);
                  Player2 = new AIPlayer(PlayerEnum.Naught, new SimpleMiniMaxGameSolver());
            }

            private async Task StartGame()
            {
                  if (Player1 == null || Player2 == null)
                  {
                        MessageBox.Show("Please select both players.");
                        return;
                  }

                  _gameCoordinator = new GameCoordinator(Player1, Player2);
                  _gameCoordinator.BoardUpdated += OnBoardUpdated;
                  _gameCoordinator.PlayerTurnStarted += OnPlayerTurnStarted;
                  _gameCoordinator.GameOver += OnGameOver;

                  _isGameRunning = true;
                  StatusMessage = "Game in progress...";
                  UpdateInputAvailability();

                  await _gameCoordinator.StartGameAsync();
            }

            private void ResetGame()
            {
                  _gameCoordinator?.CancelGame();
                  _isGameRunning = false;
                  StatusMessage = "Game reset.";
                  _selectedPosition = null;

                  for (int i = 0; i < 9; i++)
                        BoardPositions[i] = null;

                  _currentHumanPlayer = null;
                  UpdateInputAvailability();
            }

            private void OnBoardUpdated(GameBoard board)
            {
                  Application.Current.Dispatcher.Invoke(() =>
                  {
                        for (int i = 0; i < 9; i++)
                              BoardPositions[i] = board.GetPosition(i%3, i/3);
                  });
            }

            private void OnPlayerTurnStarted(IPlayer player)
            {
                  _currentHumanPlayer = player as HumanPlayer;
                  _selectedPosition = null;
                  UpdateInputAvailability();
            }

            private void OnGameOver(IPlayer winner, string result)
            {
                  Application.Current.Dispatcher.Invoke(() =>
                  {
                        _isGameRunning = false;
                        StatusMessage = result;
                        UpdateInputAvailability();
                  });
            }

            private void UpdateInputAvailability()
            {
                  OnPropertyChanged(nameof(IsCellClickEnabled));
                  CommandManager.InvalidateRequerySuggested();
            }

            private void OnCellClicked(object? parameter)
            {
                  if (_currentHumanPlayer == null || parameter is not string indexStr || !int.TryParse(indexStr, out int index))
                        return;

                  if (_gameCoordinator == null)
                        return;

                  var (x, y) = GameBoard.FromIndex(index);
                  var currentBoard = _gameCoordinator.CurrentBoard;
                  var playerSymbol = _currentHumanPlayer.AssignedPlayer == PlayerEnum.Cross ? PositionEnum.Cross : PositionEnum.Naught;
                  var playerPositions = currentBoard.GetPlayerPositions(playerSymbol);

                  if (playerPositions.Count < 3)
                  {
                        // Placement phase
                        if (currentBoard.GetPosition(x, y) != PositionEnum.Empty)
                              return;

                        var newBoard = currentBoard.CloneWithChange(x, y, playerSymbol);
                        _currentHumanPlayer.SubmitMove(newBoard);
                  }
                  else
                  {
                        // Movement phase
                        if (_selectedPosition == null)
                        {
                              if (currentBoard.GetPosition(x, y) == playerSymbol)
                                    _selectedPosition = (x, y); // select piece to move
                        }
                        else
                        {
                              if (_selectedPosition.Value == (x, y))
                              {
                                    _selectedPosition = null; // deselect
                                    return;
                              }

                              if (currentBoard.GetPosition(x, y) != PositionEnum.Empty)
                              {
                                    _selectedPosition = null;
                                    return;
                              }

                              var (fromX, fromY) = _selectedPosition.Value;

                              var cleared = currentBoard.CloneWithChange(fromX, fromY, PositionEnum.Empty);
                              var moved = cleared.CloneWithChange(x, y, playerSymbol);

                              _selectedPosition = null;
                              _currentHumanPlayer.SubmitMove(moved);
                        }
                  }
            }

            private IPlayer CreatePlayer(PlayerTypeOption type, PlayerEnum assigned)
            {
                  return type switch
                  {
                        PlayerTypeOption.Human => new HumanPlayer(assigned),
                        PlayerTypeOption.Ai => new AIPlayer(assigned, new SimpleMiniMaxGameSolver()),
                        _ => throw new InvalidOperationException("Unsupported player type")
                  };
            }

      }
}
