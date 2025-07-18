using Infinite_tic_tac_toe.Game;
using Infinite_tic_tac_toe.Game.Players;
using Infinite_tic_tac_toe.Model;
using Infinite_tic_tac_toe.Solvers;
using Infinite_tic_tac_toe.UserInterface.UserControls;
using Infinite_tic_tac_toe.UserInterface.UserControls.Configuration;
using Infinite_tic_tac_toe.UserInterface.ViewModels.ConfigurationViewModels;

namespace Infinite_tic_tac_toe.Services.Factories
{
    /// <summary>
    /// Updated AIPlayerFactory with configuration support
    /// </summary>
    public class AIPlayerFactory : IPlayerFactory
      {
            private readonly GameSolverBase _solver;
            private readonly string _solverName;

            public PlayerDescriptor Descriptor { get; }

            public AIPlayerFactory(GameSolverBase solver, string solverName)
            {
                  _solver = solver;
                  _solverName = solverName;
                  Descriptor = new PlayerDescriptor(
                      $"AI_{solverName}",
                      $"AI Player ({solverName})",
                      $"Computer player using {solverName} algorithm",
                      typeof(AIPlayer),
                      requiresConfiguration: true,
                      configurationType: typeof(AIPlayerConfiguration),
                      configurationViewModelType: typeof(AIPlayerConfigurationViewModel),
                      configurationViewType: typeof(AIPlayerConfigurationUserControl)
                  );
            }

            public IPlayer CreatePlayer(PlayerEnum assignedPlayer, object? configuration = null)
            {
                  AIPlayerConfiguration? config = null;

                  if (configuration is AIPlayerConfiguration aiConfig)
                  {
                        config = aiConfig;
                  }
                  else if (configuration is AIPlayerConfigurationViewModel aiViewModel)
                  {
                        config = aiViewModel.Configuration;
                  }

                  return new AIPlayer(assignedPlayer, _solver, config);
            }
      }
}