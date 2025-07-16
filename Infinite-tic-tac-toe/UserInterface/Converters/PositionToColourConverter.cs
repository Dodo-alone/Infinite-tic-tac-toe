using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Infinite_tic_tac_toe.Model;

namespace Infinite_tic_tac_toe.UserInterface.Converters
{
      public class PositionToColorConverter : IValueConverter
      {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                  if (value is PositionEnum position)
                  {
                        switch (position)
                        {
                              case PositionEnum.Cross:
                              // Red gradient for X
                              return new LinearGradientBrush
                              {
                                    StartPoint = new Point(0, 0),
                                    EndPoint = new Point(1, 1),
                                    GradientStops = new GradientStopCollection
                                    {
                                          new GradientStop(Color.FromRgb(255, 99, 132), 0.0),
                                          new GradientStop(Color.FromRgb(255, 159, 64), 1.0)
                                    }
                              };

                              case PositionEnum.Naught:
                              // Blue gradient for O
                              return new LinearGradientBrush
                              {
                                    StartPoint = new Point(0, 0),
                                    EndPoint = new Point(1, 1),
                                    GradientStops = new GradientStopCollection
                                    {
                                          new GradientStop(Color.FromRgb(54, 162, 235), 0.0),
                                          new GradientStop(Color.FromRgb(153, 102, 255), 1.0)
                                    }
                              };

                              case PositionEnum.Empty:
                              default:
                              // Default white gradient for empty cells
                              return new LinearGradientBrush
                              {
                                    StartPoint = new Point(0, 0),
                                    EndPoint = new Point(0, 1),
                                    GradientStops = new GradientStopCollection
                                    {
                                          new GradientStop(Colors.White, 0.0),
                                          new GradientStop(Color.FromRgb(248, 249, 250), 1.0)
                                    }
                              };
                        }
                  }

                  // Fallback to default gradient
                  return new LinearGradientBrush
                  {
                        StartPoint = new Point(0, 0),
                        EndPoint = new Point(0, 1),
                        GradientStops = new GradientStopCollection
                        {
                              new GradientStop(Colors.White, 0.0),
                              new GradientStop(Color.FromRgb(248, 249, 250), 1.0)
                        }
                  };
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                  throw new NotImplementedException();
            }
      }
}