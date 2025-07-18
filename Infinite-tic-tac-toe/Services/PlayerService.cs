using Infinite_tic_tac_toe.Game;
using Infinite_tic_tac_toe.Model;
using Infinite_tic_tac_toe.Solvers;
using System.Collections.ObjectModel;

namespace Infinite_tic_tac_toe.Services
{
      /// <summary>
      /// Describes a concrete player type
      /// </summary>
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

      /// <summary>
      /// An interface that all factories creating player instances should implement
      /// </summary>
      public interface IPlayerFactory
      {
            IPlayer CreatePlayer(PlayerEnum assignedPlayer, object? configuration = null);
            PlayerDescriptor Descriptor { get; }
      }

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

      /// <summary>
      /// A factory that constructs AI players
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
                      typeof(AIPlayer)
                  );
            }

            public IPlayer CreatePlayer(PlayerEnum assignedPlayer, object? configuration = null)
            {
                  return new AIPlayer(assignedPlayer, _solver);
            }
      }

      /// <summary>
      /// An interface that any service serving player types should implement
      /// </summary>
      public interface IPlayerService
      {
            ReadOnlyCollection<PlayerDescriptor> GetAvailablePlayerTypes();
            IPlayer CreatePlayer(string playerTypeName, PlayerEnum assignedPlayer, object? configuration = null);
            PlayerDescriptor? GetPlayerDescriptor(string playerTypeName);
            object? GetConfigurationView(string playerTypeName);
      }

      /// <summary>
      /// A service that server players wherever they may be needed
      /// </summary>
      public class PlayerService : IPlayerService
      {
            #region Private members

            private readonly Dictionary<string, IPlayerFactory> _playerFactories;
            private readonly ReadOnlyCollection<PlayerDescriptor> _availablePlayerTypes;

            #endregion

            #region Constructor

            public PlayerService()
            {
                  _playerFactories = new Dictionary<string, IPlayerFactory>();
                  RegisterDefaultPlayers();
                  _availablePlayerTypes = new ReadOnlyCollection<PlayerDescriptor>(
                      _playerFactories.Values.Select(f => f.Descriptor).ToList()
                  );
            }

            #endregion

            #region Public Methods

            /// <summary>
            /// Adds a new player factory to this service
            /// </summary>
            /// <param name="factory">The factory to add</param>
            public void RegisterPlayerFactory(IPlayerFactory factory)
            {
                  _playerFactories[factory.Descriptor.Name] = factory;
            }

            /// <summary>
            /// Provides a menu of all the players we are serving
            /// </summary>
            /// <returns></returns>
            public ReadOnlyCollection<PlayerDescriptor> GetAvailablePlayerTypes()
            {
                  return _availablePlayerTypes;
            }

            /// <summary>
            /// Creates a player with the specified type and assigned piece, with optional configuration
            /// </summary>
            /// <param name="playerTypeName">The name of the type we are constructing</param>
            /// <param name="assignedPiece">The peice this player is assigned to</param>
            /// <param name="configuration"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentException"></exception>
            public IPlayer CreatePlayer(string playerTypeName, PlayerEnum assignedPiece, object? configuration = null)
            {
                  if (!_playerFactories.TryGetValue(playerTypeName, out var factory))
                  {
                        throw new ArgumentException($"Unknown player type: {playerTypeName}");
                  }

                  return factory.CreatePlayer(assignedPiece, configuration);
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

            #endregion

            #region Private Methods

            private void RegisterDefaultPlayers()
            {
                  RegisterPlayerFactory(new HumanPlayerFactory());
                  RegisterPlayerFactory(new AIPlayerFactory(new SimpleMiniMaxGameSolver(), "MiniMax"));
            }

            #endregion
      }
}