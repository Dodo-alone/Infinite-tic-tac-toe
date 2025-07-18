// Configuration base interface
using Infinite_tic_tac_toe.UserInterface.ViewModels;
using System.Collections.ObjectModel;

namespace Infinite_tic_tac_toe.Services
{
      /// <summary>
      /// Base interface for all player configurations
      /// </summary>
      public interface IPlayerConfiguration
      {
            /// <summary>
            /// Validates the configuration and returns any error messages
            /// </summary>
            /// <returns>List of validation errors, empty if valid</returns>
            List<string> Validate();
      }

      /// <summary>
      /// Generic interface for configuration view models
      /// </summary>
      /// <typeparam name="T">The configuration type</typeparam>
      public interface IConfigurationViewModel<T> where T : IPlayerConfiguration
      {
            /// <summary>
            /// The configuration instance being edited
            /// </summary>
            T Configuration { get; }

            /// <summary>
            /// Whether the current configuration is valid
            /// </summary>
            bool IsValid { get; }

            /// <summary>
            /// Validation errors for the current configuration
            /// </summary>
            ObservableCollection<string> ValidationErrors { get; }
      }

      /// <summary>
      /// Base class for configuration view models
      /// </summary>
      /// <typeparam name="T">The configuration type</typeparam>
      public abstract class ConfigurationViewModelBase<T> : ViewModelBase, IConfigurationViewModel<T>
          where T : IPlayerConfiguration
      {
            protected T _configuration;
            private ObservableCollection<string> _validationErrors = new();

            public T Configuration
            {
                  get => _configuration;
                  protected set
                  {
                        _configuration = value;
                        OnPropertyChanged();
                        ValidateConfiguration();
                  }
            }

            public bool IsValid => !ValidationErrors.Any();

            public ObservableCollection<string> ValidationErrors
            {
                  get => _validationErrors;
                  private set
                  {
                        _validationErrors = value;
                        OnPropertyChanged();
                        OnPropertyChanged(nameof(IsValid));
                  }
            }

            protected ConfigurationViewModelBase(T configuration)
            {
                  _configuration = configuration;
                  ValidateConfiguration();
            }

            protected virtual void ValidateConfiguration()
            {
                  var errors = Configuration.Validate();
                  ValidationErrors.Clear();
                  foreach (var error in errors)
                        ValidationErrors.Add(error);
            }

            protected void OnConfigurationChanged()
            {
                  ValidateConfiguration();
            }
      }
}