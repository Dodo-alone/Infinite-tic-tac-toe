using Infinite_tic_tac_toe.Model;
using Infinite_tic_tac_toe.Solvers;

namespace Infinite_tic_tac_toe.Game
{
      public enum PlayerType
      {
            Local,
            Remote
      }

      public interface IPlayer
      {
            public string Name { get; }
            public PlayerEnum AssignedPlayer { get; }
            public PlayerType PlayerType { get; }

            public Task<GameBoard?> GetNextMove(GameBoard board);
      }

      public class AIPlayer : IPlayer
      {
            public string Name => _solver.GetType().Name;
            public PlayerEnum AssignedPlayer { get; }
            public PlayerType PlayerType => PlayerType.Remote;

            private readonly GameSolverBase _solver;

            public AIPlayer(PlayerEnum assignedPlayer, GameSolverBase solver)
            {
                  AssignedPlayer = assignedPlayer;
                  _solver = solver;
            }

            public Task<GameBoard?> GetNextMove(GameBoard board)
            {
                  return Task.Run(() => _solver.GetBestMove(board, AssignedPlayer).nextBoard);
            }
      }

      public class HumanPlayer : IPlayer
      {
            private TaskCompletionSource<GameBoard?>? _moveTcs;

            public string Name => "Human";
            public PlayerEnum AssignedPlayer { get; }
            public PlayerType PlayerType => PlayerType.Local;

            public HumanPlayer(PlayerEnum assignedPlayer)
            {
                  AssignedPlayer = assignedPlayer;
            }

            public void SubmitMove(GameBoard nextBoard)
            {
                  _moveTcs?.TrySetResult(nextBoard);
            }

            public Task<GameBoard?> GetNextMove(GameBoard board)
            {
                  _moveTcs = new TaskCompletionSource<GameBoard?>();
                  return _moveTcs.Task;
            }
      }
}
