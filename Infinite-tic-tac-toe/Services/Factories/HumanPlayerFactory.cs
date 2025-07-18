using Infinite_tic_tac_toe.Game;
using Infinite_tic_tac_toe.Game.Players;
using Infinite_tic_tac_toe.Model;

namespace Infinite_tic_tac_toe.Services.Factories
{
      /// <summary>
      /// A factory that constructs human players
      /// </summary>
      public class HumanPlayerFactory : IPlayerFactory
      {
            public PlayerDescriptor Descriptor { get; } = new PlayerDescriptor(
                "Human",
                "Human Player",
                "Local human player using mouse/keyboard input",
                typeof(HumanPlayer)
            );

            public IPlayer CreatePlayer(PlayerEnum assignedPlayer, object? configuration = null)
            {
                  return new HumanPlayer(assignedPlayer);
            }
      }
}