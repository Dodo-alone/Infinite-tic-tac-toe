using Infinite_tic_tac_toe.Model;
using Infinite_tic_tac_toe.Services;
using Infinite_tic_tac_toe.Solvers;

namespace Infinite_tic_tac_toe.Game.Players
{
      /// <summary>
      /// Updated AIPlayer with configuration support
      /// </summary>
      public class AIPlayer : IPlayer
      {
            public string Name => $"{_solver.GetType().Name})";
            public PlayerEnum AssignedPiece { get; }
            public PlayerType PlayerType => PlayerType.Remote;

            private readonly GameSolverBase _solver;
            private readonly AIPlayerConfiguration _configuration;

            /// <summary>
            /// Constructs an instance of this player
            /// </summary>
            /// <param name="assignedPiece">The piece we are assigned to</param>
            /// <param name="solver">An instance of GameSolverBase to use for this player</param>
            /// <param name="configuration">Optional configuration for the AI player</param>
            public AIPlayer(PlayerEnum assignedPiece, GameSolverBase solver, AIPlayerConfiguration? configuration = null)
            {
                  AssignedPiece = assignedPiece;
                  _solver = solver;
                  _configuration = configuration ?? new AIPlayerConfiguration();
            }

            public async Task<GameBoard?> GetNextMove(GameBoard board)
            {

                  // Apply move delay if configured
                  if (_configuration.MoveDelayMs > 0)
                  {
                        await Task.Delay(_configuration.MoveDelayMs);
                  }

                  // Get the best move using the solver
                  var result = await Task.Run(() => _solver.GetBestMove(board, AssignedPiece));

                  return result.nextBoard;
            }

            /// <summary>
            /// Gets the current configuration
            /// </summary>
            public AIPlayerConfiguration Configuration => _configuration;
      }

      /// <summary>
      /// Configuration settings for AI players
      /// </summary>
      public class AIPlayerConfiguration : IPlayerConfiguration
      {
            private int _moveDelayMs = 1000;

            /// <summary>
            /// Delay between AI moves in milliseconds
            /// </summary>
            public int MoveDelayMs
            {
                  get => _moveDelayMs;
                  set => _moveDelayMs = Math.Max(0, Math.Min(10000, value)); // Clamp between 0-10 seconds
            }

            public List<string> Validate()
            {
                  var errors = new List<string>();

                  if (MoveDelayMs < 0)
                        errors.Add("Move delay cannot be negative");

                  if (MoveDelayMs > 10000)
                        errors.Add("Move delay cannot exceed 10 seconds");

                  return errors;
            }
      }
}