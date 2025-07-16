using Infinite_tic_tac_toe.Model;
using System.Globalization;
using System.Windows.Data;

namespace Infinite_tic_tac_toe.UserInterface.Converters
{
      public class PositionToSymbolConverter : IValueConverter
      {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                  if (value is PositionEnum position)
                  {
                        switch (position)
                        {
                              case PositionEnum.Cross:
                                    return "X";
                              case PositionEnum.Naught:
                                    return "O";
                              case PositionEnum.Empty:
                              default:
                                    return "";
                        }
                  }
                  return "";
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                  throw new NotImplementedException();
            }
      }
}
