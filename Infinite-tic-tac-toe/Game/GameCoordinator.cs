using Infinite_tic_tac_toe.Model;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infinite_tic_tac_toe.Game
{
      public class GameCoordinator
      {
            public IPlayer PlayerCross { get; }
            public IPlayer PlayerNaught { get; }

            public GameBoard CurrentBoard { get; private set; }
            public PlayerEnum CurrentTurn { get; private set; } = PlayerEnum.Cross;

            public bool IsGameOver { get; private set; }

            public event Action<GameBoard>? BoardUpdated;
            public event Action<IPlayer>? PlayerTurnStarted;
            public event Action<IPlayer, string>? GameOver;

            private bool _isRunning = false;
            private CancellationTokenSource? _cts;

            public GameCoordinator(IPlayer playerX, IPlayer playerO)
            {
                  PlayerCross = playerX;
                  PlayerNaught = playerO;
                  CurrentBoard = new GameBoard(); // Empty board
            }

            public async Task StartGameAsync()
            {
                  if (_isRunning)
                        return;

                  _isRunning = true;
                  IsGameOver = false;
                  _cts = new CancellationTokenSource();

                  while (!IsGameOver && !_cts.Token.IsCancellationRequested)
                  {
                        var currentPlayer = GetCurrentPlayer();
                        PlayerTurnStarted?.Invoke(currentPlayer);

                        GameBoard? proposedBoard = null;
                        try
                        {
                              proposedBoard = await currentPlayer.GetNextMove(CurrentBoard);
                        }
                        catch (Exception ex)
                        {
                              GameOver?.Invoke(currentPlayer, $"Error during player move: {ex.Message}");
                              break;
                        }

                        if (proposedBoard == null)
                        {
                              GameOver?.Invoke(currentPlayer, "No move was submitted.");
                              break;
                        }

                        // Validate the move
                        if (!IsValidMove(CurrentBoard, proposedBoard, currentPlayer.AssignedPlayer))
                        {
                              GameOver?.Invoke(currentPlayer, "Invalid move submitted.");
                              break;
                        }

                        CurrentBoard = proposedBoard;
                        BoardUpdated?.Invoke(CurrentBoard);

                        var winner = CurrentBoard.CheckWinner();
                        if (winner != PositionEnum.Empty)
                        {
                              IsGameOver = true;
                              var winningPlayer = winner == PositionEnum.Cross ? PlayerCross :
                                                  winner == PositionEnum.Naught ? PlayerNaught : null;

                              var result = $"Player {winningPlayer?.Name} ({winner}) wins!";
                              GameOver?.Invoke(winningPlayer!, result);
                              break;
                        }

                        SwitchTurn();
                  }

                  _isRunning = false;
            }

            public void CancelGame()
            {
                  _cts?.Cancel();
                  _isRunning = false;
            }

            private IPlayer GetCurrentPlayer()
            {
                  return CurrentTurn == PlayerEnum.Cross ? PlayerCross : PlayerNaught;
            }

            private void SwitchTurn()
            {
                  CurrentTurn = CurrentTurn == PlayerEnum.Cross ? PlayerEnum.Naught : PlayerEnum.Cross;
            }

            /// <summary>
            /// Ensures move follows the 3-piece max rule, and either adds a piece or moves one (but not both).
            /// </summary>
            private bool IsValidMove(GameBoard current, GameBoard next, PlayerEnum player)
            {
                  var playerSymbol = player == PlayerEnum.Cross ? PositionEnum.Cross : PositionEnum.Naught;

                  var currentPositions = current.GetPlayerPositions(playerSymbol);
                  var nextPositions = next.GetPlayerPositions(playerSymbol);

                  if (next.Equals(current))
                        return false; // Null move

                  if (nextPositions.Count > 3)
                        return false; // Too many pieces

                  if (currentPositions.Count < 3)
                  {
                        // Still placing
                        if (nextPositions.Count != currentPositions.Count + 1)
                              return false;

                        // Ensure only one new piece added
                        return current.GetEmptyPositions()
                            .Any(index => next.GetPosition(index.x, index.y) == playerSymbol);
                  }
                  else
                  {
                        // Must move one existing piece
                        if (nextPositions.Count != 3)
                              return false;

                        var movedFrom = currentPositions.Except(nextPositions).ToList();
                        var movedTo = nextPositions.Except(currentPositions).ToList();

                        if (movedFrom.Count != 1 || movedTo.Count != 1)
                              return false;

                        // Prevent moving to same square (null move)
                        if (movedFrom[0] == movedTo[0])
                              return false;

                        // Ensure all other positions are the same
                        return true;
                  }
            }
      }
}
