using Infinite_tic_tac_toe.Model;

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
}
