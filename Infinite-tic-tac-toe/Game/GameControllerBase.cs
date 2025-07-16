using Infinite_tic_tac_toe.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinite_tic_tac_toe.Game
{
      public abstract class GameControllerBase
      {
            protected GameBoard _board;
            protected PlayerEnum _currentPlayer;
            protected bool _gameOver;
            protected CancellationTokenSource _cancellationTokenSource;

            public event EventHandler<GameStateChangedEventArgs> GameStateChanged;
            public event EventHandler<PlayerTurnEventArgs> PlayerTurnChanged;
            public event EventHandler<GameOverEventArgs> GameOver;

            public GameBoard Board => _board;
            public PlayerEnum CurrentPlayer => _currentPlayer;
            public bool IsGameOver => _gameOver;

            public virtual void StartNewGame()
            {
                  _board = new GameBoard();
                  _currentPlayer = PlayerEnum.Cross; // Crosses always go first
                  _gameOver = false;

                  OnGameStateChanged();
                  OnPlayerTurnChanged();
            }

            public virtual void StopGame()
            {
                  _cancellationTokenSource?.Cancel();
            }

            protected virtual void OnGameStateChanged()
            {
                  GameStateChanged?.Invoke(this, new GameStateChangedEventArgs(_board.GetBoardArray()));
            }

            protected virtual void OnPlayerTurnChanged()
            {
                  PlayerTurnChanged?.Invoke(this, new PlayerTurnEventArgs(_currentPlayer));
            }

            protected virtual void OnGameOver(PositionEnum winner)
            {
                  _gameOver = true;
                  GameOver?.Invoke(this, new GameOverEventArgs(winner));
            }

            protected virtual void SwitchPlayer()
            {
                  _currentPlayer = _currentPlayer == PlayerEnum.Cross ? PlayerEnum.Naught : PlayerEnum.Cross;
                  OnPlayerTurnChanged();
            }

            protected virtual bool CheckWinner()
            {
                  var winner = _board.CheckWinner();
                  if (winner != PositionEnum.Empty)
                  {
                        OnGameOver(winner);
                        return true;
                  }
                  return false;
            }

            protected virtual int CountPieces(PositionEnum player)
            {
                  var positions = _board.GetBoardArray();
                  int count = 0;
                  foreach (var pos in positions)
                  {
                        if (pos == player) count++;
                  }
                  return count;
            }
      }

      public class GameStateChangedEventArgs : EventArgs
      {
            public PositionEnum[] BoardPositions { get; }
            public GameStateChangedEventArgs(PositionEnum[] positions) => BoardPositions = positions;
      }

      public class PlayerTurnEventArgs : EventArgs
      {
            public PlayerEnum CurrentPlayer { get; }
            public PlayerTurnEventArgs(PlayerEnum player) => CurrentPlayer = player;
      }

      public class GameOverEventArgs : EventArgs
      {
            public PositionEnum Winner { get; }
            public GameOverEventArgs(PositionEnum winner) => Winner = winner;
      }
}
