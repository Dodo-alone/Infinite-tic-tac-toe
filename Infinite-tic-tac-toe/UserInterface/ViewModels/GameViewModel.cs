using Infinite_tic_tac_toe.Game;
using Infinite_tic_tac_toe.Model;
using Infinite_tic_tac_toe.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Infinite_tic_tac_toe.UserInterface.ViewModels
{
      public class GameViewModel : ViewModelBase
      {
            private readonly IPlayerService _playerService;
            private GameCoordinator? _gameCoordinator;
            private IPlayer? _player1;
            private IPlayer? _player2;
            private HumanPlayer? _currentHumanPlayer;

            private bool _isGameRunning;
            private string _statusMessage = "Ready to start";
            private (int x, int y)? _selectedPosition = null;

            public ObservableCollection<object?> BoardPositions { get; } = new ObservableCollection<object?>(new object?[9]);

            public ICommand CellClickCommand { get; }
            public ICommand StartGameCommand { get; }
            public ICommand ResetGameCommand { get; }

            // Available player types from the service
            public ReadOnlyCollection<PlayerDescriptor> AvailablePlayerTypes { get; }

            private PlayerDescriptor? _selectedPlayer1Type;
            public PlayerDescriptor? SelectedPlayer1Type
            {
                  get => _selectedPlayer1Type;
                  set
                  {
                        if (_selectedPlayer1Type != value)
                        {
                              _selectedPlayer1Type = value;
                              UpdatePlayer1();
                              OnPropertyChanged();
                              OnPropertyChanged(nameof(SelectedPlayer1SettingsView));
                        }
                  }
            }

            private PlayerDescriptor? _selectedPlayer2Type;
            public PlayerDescriptor? SelectedPlayer2Type
            {
                  get => _selectedPlayer2Type;
                  set
                  {
                        if (_selectedPlayer2Type != value)
                        {
                              _selectedPlayer2Type = value;
                              UpdatePlayer2();
                              OnPropertyChanged();
                              OnPropertyChanged(nameof(SelectedPlayer2SettingsView));
                        }
                  }
            }

            public object? SelectedPlayer1SettingsView =>
                _selectedPlayer1Type?.RequiresConfiguration == true ?
                _playerService.GetConfigurationView(_selectedPlayer1Type.Name) : null;

            public object? SelectedPlayer2SettingsView =>
                _selectedPlayer2Type?.RequiresConfiguration == true ?
                _playerService.GetConfigurationView(_selectedPlayer2Type.Name) : null;

            public IPlayer? Player1
            {
                  get => _player1;
                  private set { _player1 = value; OnPropertyChanged(); }
            }

            public IPlayer? Player2
            {
                  get => _player2;
                  private set { _player2 = value; OnPropertyChanged(); }
            }

            public string StatusMessage
            {
                  get => _statusMessage;
                  private set { _statusMessage = value; OnPropertyChanged(); }
            }

            public bool IsCellClickEnabled => _isGameRunning && _currentHumanPlayer?.PlayerType == PlayerType.Local;

            public GameViewModel() : this(new PlayerService())
            {
            }

            public GameViewModel(IPlayerService playerService)
            {
                  _playerService = playerService;
                  AvailablePlayerTypes = _playerService.GetAvailablePlayerTypes();

                  CellClickCommand = new RelayCommand(OnCellClicked, _ => IsCellClickEnabled);
                  StartGameCommand = new RelayCommand(async _ => await StartGame(), _ => !_isGameRunning);
                  ResetGameCommand = new RelayCommand(_ => ResetGame(), _ => _isGameRunning);

                  // Set default selections
                  SelectedPlayer1Type = AvailablePlayerTypes.FirstOrDefault(p => p.Name == "Human");
                  SelectedPlayer2Type = AvailablePlayerTypes.FirstOrDefault(p => p.Name.StartsWith("AI_"));
            }

            private void UpdatePlayer1()
            {
                  Player1 = _selectedPlayer1Type != null ?
                      _playerService.CreatePlayer(_selectedPlayer1Type.Name, PlayerEnum.Cross) : null;
            }

            private void UpdatePlayer2()
            {
                  Player2 = _selectedPlayer2Type != null ?
                      _playerService.CreatePlayer(_selectedPlayer2Type.Name, PlayerEnum.Naught) : null;
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
                              BoardPositions[i] = board.GetPosition(i % 3, i / 3);
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
      }
}