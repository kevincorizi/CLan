using System.Windows.Input;

namespace CLanWPFTest.Extensions
{
    public static class CLanCommands
    {
        public static readonly RoutedUICommand Exit = new RoutedUICommand(
            "Exit",
            "Exit",
            typeof(CLanCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.F4, ModifierKeys.Alt)
            }
        );

        public static readonly RoutedUICommand PrivateMode = new RoutedUICommand(
            "Attiva modalità privata",
            "PrivateMode",
            typeof(CLanCommands)
        );

    }
}
