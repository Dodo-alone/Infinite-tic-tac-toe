// Configuration base interface
// AI Player Configuration
using Infinite_tic_tac_toe.Game.Players;
using Infinite_tic_tac_toe.Services;

namespace Infinite_tic_tac_toe.UserInterface.ViewModels.ConfigurationViewModels
{
      /// <summary>
      /// View model for AI player configuration
      /// </summary>
      public class AIPlayerConfigurationViewModel : ConfigurationViewModelBase<AIPlayerConfiguration>
      {
            public AIPlayerConfigurationViewModel() : base(new AIPlayerConfiguration()) { }

            public int MoveDelayMs
            {
                  get => Configuration.MoveDelayMs;
                  set
                  {
                        if (Configuration.MoveDelayMs != value)
                        {
                              Configuration.MoveDelayMs = value;
                              OnPropertyChanged();
                              OnConfigurationChanged();
                        }
                  }
            }
      }
}