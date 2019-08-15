using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MSsqlTool.CommandBehaviors
{
    public class MouseLeftButtonDown
    {
        public static readonly System.Windows.DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command",
                typeof(ICommand),
                typeof(MouseLeftButtonDown),
                new UIPropertyMetadata(CommandChanged));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter",
                typeof(object),
                typeof(MouseLeftButtonDown),
                new UIPropertyMetadata(null));

        public static void SetCommand(DependencyObject target, ICommand value)
        {
            target.SetValue(CommandProperty, value);
        }

        public static void SetCommandParameter(DependencyObject target, object value)
        {
            target.SetValue(CommandParameterProperty, value);
        }
        public static object GetCommandParameter(DependencyObject target)
        {
            return target.GetValue(CommandParameterProperty);
        }

        private static void CommandChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            Grid grid = target as Grid;
            if (grid != null)
            {
                if ((e.NewValue != null) && (e.OldValue == null))
                {
                    grid.MouseLeftButtonDown += OnMouseLeftButtonDown;
                }
                else if ((e.NewValue == null) && (e.OldValue != null))
                {
                    grid.MouseLeftButtonDown -= OnMouseLeftButtonDown;
                }
            }
        }

        private static void OnMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            Grid grid = sender as Grid;
            ICommand command = (ICommand)grid.GetValue(CommandProperty);
            object commandParameter = grid.GetValue(CommandParameterProperty);
            command.Execute(commandParameter);
            Console.WriteLine($"CommandParameter is {commandParameter}");
        }
    }
}
