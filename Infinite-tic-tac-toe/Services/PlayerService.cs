using Infinite_tic_tac_toe.Game;
using Infinite_tic_tac_toe.Model;
using Infinite_tic_tac_toe.Solvers;
using System.Collections.ObjectModel;

namespace Infinite_tic_tac_toe.Services
{
      public class PlayerDescriptor
      {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public Type PlayerType { get; set; }
            public bool RequiresConfiguration { get; set; }
            public Func<object?>? GetConfigurationView { get; set; }

            public PlayerDescriptor(string name, string displayName, string description, Type playerType, bool requiresConfiguration = false, Func<object?>? getConfigurationView = null)
            {
                  Name = name;
                  DisplayName = displayName;
                  Description = description;
                  PlayerType = playerType;
                  RequiresConfiguration = requiresConfiguration;
                  GetConfigurationView = getConfigurationView;
            }
      }

      public interface IPlayerFactory
      {
            IPlayer CreatePlayer(PlayerEnum assignedPlayer, object? configuration = null);
            PlayerDescriptor Descriptor { get; }
      }

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
                      typeof(AIPlayer)
                  );
            }

            public IPlayer CreatePlayer(PlayerEnum assignedPlayer, object? configuration = null)
            {
                  return new AIPlayer(assignedPlayer, _solver);
            }
      }

      public interface IPlayerService
      {
            ReadOnlyCollection<PlayerDescriptor> GetAvailablePlayerTypes();
            IPlayer CreatePlayer(string playerTypeName, PlayerEnum assignedPlayer, object? configuration = null);
            PlayerDescriptor? GetPlayerDescriptor(string playerTypeName);
            object? GetConfigurationView(string playerTypeName);
      }

      public class PlayerService : IPlayerService
      {
            private readonly Dictionary<string, IPlayerFactory> _playerFactories;
            private readonly ReadOnlyCollection<PlayerDescriptor> _availablePlayerTypes;

            public PlayerService()
            {
                  _playerFactories = new Dictionary<string, IPlayerFactory>();
                  RegisterDefaultPlayers();
                  _availablePlayerTypes = new ReadOnlyCollection<PlayerDescriptor>(
                      _playerFactories.Values.Select(f => f.Descriptor).ToList()
                  );
            }

            private void RegisterDefaultPlayers()
            {
                  // Register human player
                  RegisterPlayerFactory(new HumanPlayerFactory());

                  // Register AI players with different solvers
                  RegisterPlayerFactory(new AIPlayerFactory(new SimpleMiniMaxGameSolver(), "MiniMax"));

                  // Easy to add more AI players with different solvers:
                  // RegisterPlayerFactory(new AIPlayerFactory(new AlphaBetaSolver(), "AlphaBeta"));
                  // RegisterPlayerFactory(new AIPlayerFactory(new MCTSSolver(), "MCTS"));
            }

            public void RegisterPlayerFactory(IPlayerFactory factory)
            {
                  _playerFactories[factory.Descriptor.Name] = factory;
            }

            public ReadOnlyCollection<PlayerDescriptor> GetAvailablePlayerTypes()
            {
                  return _availablePlayerTypes;
            }

            public IPlayer CreatePlayer(string playerTypeName, PlayerEnum assignedPlayer, object? configuration = null)
            {
                  if (!_playerFactories.TryGetValue(playerTypeName, out var factory))
                  {
                        throw new ArgumentException($"Unknown player type: {playerTypeName}");
                  }

                  return factory.CreatePlayer(assignedPlayer, configuration);
            }

            public PlayerDescriptor? GetPlayerDescriptor(string playerTypeName)
            {
                  _playerFactories.TryGetValue(playerTypeName, out var factory);
                  return factory?.Descriptor;
            }

            public object? GetConfigurationView(string playerTypeName)
            {
                  var descriptor = GetPlayerDescriptor(playerTypeName);
                  return descriptor?.GetConfigurationView?.Invoke();
            }
      }
}