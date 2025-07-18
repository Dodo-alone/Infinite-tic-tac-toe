using Infinite_tic_tac_toe.Game;
using Infinite_tic_tac_toe.Game.Players;
using Infinite_tic_tac_toe.Model;
using Infinite_tic_tac_toe.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Infinite_tic_tac_toe.UserInterface.ViewModels
{
      public class GameViewModel : ViewModelBase
      {
            #region private members

            private readonly IPlayerService _playerService;
            private GameCoordinator? _gameCoordinator;
            private IPlayer? _player1;
            private IPlayer? _player2;
            private HumanPlayer? _currentHumanPlayer;

            private bool _isGameRunning;
            private string _statusMessage = "Ready to start";
            private (int x, int y)? _selectedPosition = null;

            private PlayerDescriptor? _selectedPlayer1Type;
            private PlayerDescriptor? _selectedPlayer2Type;

            #endregion

            #region Public properties

            public ObservableCollection<object?> BoardPositions { get; } = new ObservableCollection<object?>(new object?[9]);

            public ICommand CellClickCommand { get; }
            public ICommand StartGameCommand { get; }
            public ICommand ResetGameCommand { get; }

            // Available player types from the service
            public ReadOnlyCollection<PlayerDescriptor> AvailablePlayerTypes { get; }

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
                              OnPropertyChanged(nameof(SelectedPlayer1SettingsViewModel));
                        }
                  }
            }

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
                              OnPropertyChanged(nameof(SelectedPlayer2SettingsViewModel));
                        }
                  }
            }

            public object? SelectedPlayer1SettingsView =>
                _selectedPlayer1Type?.RequiresConfiguration == true ?
                _playerService.GetConfigurationView(_selectedPlayer1Type.Name) : null;

            public object? SelectedPlayer2SettingsView =>
                _selectedPlayer2Type?.RequiresConfiguration == true ?
                _playerService.GetConfigurationView(_selectedPlayer2Type.Name) : null;

            public object? SelectedPlayer1SettingsViewModel =>
                _selectedPlayer1Type?.RequiresConfiguration == true ?
                _playerService.GetConfigurationViewModel(_selectedPlayer1Type.Name) : null;

            public object? SelectedPlayer2SettingsViewModel =>
                _selectedPlayer2Type?.RequiresConfiguration == true ?
                _playerService.GetConfigurationViewModel(_selectedPlayer2Type.Name) : null;

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

            public bool IsStartGameEnabled => !_isGameRunning && ArePlayersValid;

            private bool ArePlayersValid
            {
                  get
                  {
                        if (_selectedPlayer1Type == null || _selectedPlayer2Type == null)
                              return false;

                        // Check if configurations are valid for players that require them
                        if (_selectedPlayer1Type.RequiresConfiguration)
                        {
                              var config1 = _playerService.GetConfigurationViewModel(_selectedPlayer1Type.Name);
                              if (config1 is IConfigurationViewModel<IPlayerConfiguration> configVM1 && !configVM1.IsValid)
                                    return false;
                        }

                        if (_selectedPlayer2Type.RequiresConfiguration)
                        {
                              var config2 = _playerService.GetConfigurationViewModel(_selectedPlayer2Type.Name);
                              if (config2 is IConfigurationViewModel<IPlayerConfiguration> configVM2 && !configVM2.IsValid)
                                    return false;
                        }

                        return true;
                  }
            }

            #endregion

            #region Constructors

            public GameViewModel() : this(new PlayerService()) { }

            public GameViewModel(IPlayerService playerService)
            {
                  _playerService = playerService;
                  AvailablePlayerTypes = _playerService.GetAvailablePlayerTypes();

                  CellClickCommand = new RelayCommand(OnCellClicked, _ => IsCellClickEnabled);
                  StartGameCommand = new RelayCommand(async _ => await StartGame(), _ => IsStartGameEnabled);
                  ResetGameCommand = new RelayCommand(_ => ResetGame(), _ => _isGameRunning);

                  // Set default selections
                  SelectedPlayer1Type = AvailablePlayerTypes.FirstOrDefault(p => p.Name == "Human");
                  SelectedPlayer2Type = AvailablePlayerTypes.FirstOrDefault(p => p.Name.StartsWith("AI_"));

                  // Subscribe to configuration changes to update command availability
                  SubscribeToConfigurationChanges();
            }

            #endregion

            #region Private Methods

            private void SubscribeToConfigurationChanges()
            {
                  // Subscribe to property changes on configuration view models
                  foreach (var playerType in AvailablePlayerTypes.Where(p => p.RequiresConfiguration))
                  {
                        var configViewModel = _playerService.GetConfigurationViewModel(playerType.Name);
                        if (configViewModel is INotifyPropertyChanged notifyConfig)
                        {
                              notifyConfig.PropertyChanged += (s, e) =>
                              {
                                    if (e.PropertyName == nameof(IConfigurationViewModel<IPlayerConfiguration>.IsValid))
                                    {
                                          OnPropertyChanged(nameof(IsStartGameEnabled));
                                          CommandManager.InvalidateRequerySuggested();
                                    }
                              };
                        }
                  }
            }

            private void UpdatePlayer1()
            {
                  if (_selectedPlayer1Type != null)
                  {
                        var config = _playerService.GetConfigurationViewModel(_selectedPlayer1Type.Name);
                        Player1 = _playerService.CreatePlayer(_selectedPlayer1Type.Name, PlayerEnum.Cross, config);
                  }
                  else
                  {
                        Player1 = null;
                  }

                  OnPropertyChanged(nameof(IsStartGameEnabled));
            }

            private void UpdatePlayer2()
            {
                  if (_selectedPlayer2Type != null)
                  {
                        var config = _playerService.GetConfigurationViewModel(_selectedPlayer2Type.Name);
                        Player2 = _playerService.CreatePlayer(_selectedPlayer2Type.Name, PlayerEnum.Naught, config);
                  }
                  else
                  {
                        Player2 = null;
                  }

                  OnPropertyChanged(nameof(IsStartGameEnabled));
            }

            private async Task StartGame()
            {
                  if (Player1 == null || Player2 == null)
                  {
                        MessageBox.Show("Please select both players.");
                        return;
                  }

                  if (!ArePlayersValid)
                  {
                        MessageBox.Show("Please fix configuration errors before starting the game.");
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
                  OnPropertyChanged(nameof(IsStartGameEnabled));
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
                  var playerSymbol = _currentHumanPlayer.AssignedPiece == PlayerEnum.Cross ? PositionEnum.Cross : PositionEnum.Naught;
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

            #endregion
      }
}