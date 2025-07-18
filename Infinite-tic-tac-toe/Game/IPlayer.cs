using Infinite_tic_tac_toe.Model;
using Infinite_tic_tac_toe.Solvers;

namespace Infinite_tic_tac_toe.Game
{
      /// <summary>
      /// A local player needs to use the UI to play the game,
      /// A remote player has moves not sourced from the local UI
      /// </summary>
      public enum PlayerType
      {
            Local,
            Remote
      }

      /// <summary>
      /// An Interface representing an object that is able to play the game
      /// </summary>
      public interface IPlayer
      {
            /// <summary>
            /// The name of this player
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// The assigned piece for this player
            /// </summary>
            public PlayerEnum AssignedPiece { get; }

            /// <summary>
            /// if Local we need to enable the ui, if remote we do not
            /// </summary>
            public PlayerType PlayerType { get; }

            /// <summary>
            /// An async method that will at some point return the next valid board position that
            /// this player wants to select as their move
            /// </summary>
            /// <param name="board">The board we are moving from</param>
            /// <returns>The board after our move completes</returns>
            public Task<GameBoard?> GetNextMove(GameBoard board);
      }

      public class AIPlayer : IPlayer
      {
            public string Name => _solver.GetType().Name;
            public PlayerEnum AssignedPiece { get; }
            public PlayerType PlayerType => PlayerType.Remote;

            private readonly GameSolverBase _solver;

            /// <summary>
            /// Constructs an instance of this player
            /// </summary>
            /// <param name="assignedPiece">The piece we are assigned to</param>
            /// <param name="solver">an instance of <code>GameSolverBase</code> To use for this player</param>
            public AIPlayer(PlayerEnum assignedPiece, GameSolverBase solver)
            {
                  AssignedPiece = assignedPiece;
                  _solver = solver;
            }

            public Task<GameBoard?> GetNextMove(GameBoard board)
            {
                  return Task.Run(() => _solver.GetBestMove(board, AssignedPiece).nextBoard);
            }
      }

      public class HumanPlayer : IPlayer
      {
            private TaskCompletionSource<GameBoard?>? _moveTcs;

            public string Name => "Human";
            public PlayerEnum AssignedPiece { get; }
            public PlayerType PlayerType => PlayerType.Local;

            /// <summary>
            /// Constructs an instance of this player
            /// </summary>
            /// <param name="assignedPiece">The piece we are assigned to</param>
            public HumanPlayer(PlayerEnum assignedPiece)
            {
                  AssignedPiece = assignedPiece;
            }

            /// <summary>
            /// called to submit a move for this human player
            /// </summary>
            /// <param name="nextBoard">The board after the move has completed</param>
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
