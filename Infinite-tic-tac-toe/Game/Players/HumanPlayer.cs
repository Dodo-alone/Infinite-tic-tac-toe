using Infinite_tic_tac_toe.Model;

namespace Infinite_tic_tac_toe.Game.Players
{
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

            public void FinishGame(GameBoard board)
            {
                  return;
            }
      }
}
