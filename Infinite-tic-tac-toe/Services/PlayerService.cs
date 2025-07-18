using Infinite_tic_tac_toe.Game;
using Infinite_tic_tac_toe.Model;
using Infinite_tic_tac_toe.Services.Factories;
using Infinite_tic_tac_toe.Solvers;
using System.Collections.ObjectModel;
using System.Windows;

namespace Infinite_tic_tac_toe.Services
{
      /// <summary>
      /// Updated PlayerDescriptor with generic configuration support
      /// </summary>
      public class PlayerDescriptor
      {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public Type PlayerType { get; set; }
            public bool RequiresConfiguration { get; set; }
            public Type? ConfigurationType { get; set; }
            public Type? ConfigurationViewModelType { get; set; }
            public Type? ConfigurationViewType { get; set; }
            public Func<object>? CreateConfigurationViewModel { get; set; }
            public Func<object>? CreateConfigurationView { get; set; }

            public PlayerDescriptor(string name, string displayName, string description, Type playerType,
                bool requiresConfiguration = false,
                Type? configurationType = null,
                Type? configurationViewModelType = null,
                Type? configurationViewType = null)
            {
                  Name = name;
                  DisplayName = displayName;
                  Description = description;
                  PlayerType = playerType;
                  RequiresConfiguration = requiresConfiguration;
                  ConfigurationType = configurationType;
                  ConfigurationViewModelType = configurationViewModelType;
                  ConfigurationViewType = configurationViewType;

                  // Set up factory methods if types are provided
                  if (configurationViewModelType != null)
                  {
                        CreateConfigurationViewModel = () => Activator.CreateInstance(configurationViewModelType)!;
                  }

                  if (configurationViewType != null)
                  {
                        CreateConfigurationView = () => Activator.CreateInstance(configurationViewType)!;
                  }
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
      /// Updated PlayerService with configuration support
      /// </summary>
      public class PlayerService : IPlayerService
      {
            #region Private members

            private readonly Dictionary<string, IPlayerFactory> _playerFactories;
            private readonly Dictionary<string, object> _configurationViewModels;
            private readonly ReadOnlyCollection<PlayerDescriptor> _availablePlayerTypes;

            #endregion

            #region Constructor

            public PlayerService()
            {
                  _playerFactories = new Dictionary<string, IPlayerFactory>();
                  _configurationViewModels = new Dictionary<string, object>();
                  RegisterDefaultPlayers();
                  _availablePlayerTypes = new ReadOnlyCollection<PlayerDescriptor>(
                      _playerFactories.Values.Select(f => f.Descriptor).ToList()
                  );
            }

            #endregion

            #region Public Methods

            public void RegisterPlayerFactory(IPlayerFactory factory)
            {
                  _playerFactories[factory.Descriptor.Name] = factory;

                  // Create and cache configuration view model if needed
                  if (factory.Descriptor.RequiresConfiguration && factory.Descriptor.CreateConfigurationViewModel != null)
                  {
                        _configurationViewModels[factory.Descriptor.Name] = factory.Descriptor.CreateConfigurationViewModel();
                  }
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

            public object? GetConfigurationViewModel(string playerTypeName)
            {
                  _configurationViewModels.TryGetValue(playerTypeName, out var viewModel);
                  return viewModel;
            }

            public object? GetConfigurationView(string playerTypeName)
            {
                  var descriptor = GetPlayerDescriptor(playerTypeName);
                  if (descriptor?.CreateConfigurationView != null)
                  {
                        var view = descriptor.CreateConfigurationView();

                        // Bind the view's DataContext to the configuration view model
                        if (view is FrameworkElement element)
                        {
                              element.DataContext = GetConfigurationViewModel(playerTypeName);
                        }

                        return view;
                  }

                  return null;
            }

            public T? GetConfiguration<T>(string playerTypeName) where T : class, IPlayerConfiguration
            {
                  if (_configurationViewModels.TryGetValue(playerTypeName, out var viewModel))
                  {
                        if (viewModel is IConfigurationViewModel<T> configViewModel)
                        {
                              return configViewModel.Configuration;
                        }
                  }

                  return null;
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

      /// <summary>
      /// Updated interface for player service
      /// </summary>
      public interface IPlayerService
      {
            ReadOnlyCollection<PlayerDescriptor> GetAvailablePlayerTypes();
            IPlayer CreatePlayer(string playerTypeName, PlayerEnum assignedPlayer, object? configuration = null);
            PlayerDescriptor? GetPlayerDescriptor(string playerTypeName);
            object? GetConfigurationView(string playerTypeName);
            object? GetConfigurationViewModel(string playerTypeName);
            T? GetConfiguration<T>(string playerTypeName) where T : class, IPlayerConfiguration;
      }
}