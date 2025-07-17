using Infinite_tic_tac_toe.UserInterface.ViewModels;
using System.Windows.Controls;

namespace Infinite_tic_tac_toe.UserInterface.UserControls
{
      /// <summary>
      /// Interaction logic for GameUserControl.xaml
      /// </summary>
      public partial class GameUserControl : UserControl
      {
            GameViewModel _vm;

            public GameUserControl()
            {
                  _vm = new GameViewModel();
                  this.DataContext = _vm;
                  InitializeComponent();
            }
      }
}
