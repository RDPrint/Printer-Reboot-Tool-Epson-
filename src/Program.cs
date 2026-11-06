using System;
using System.Windows;

namespace EpsonMassReboot
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            var app = new Application();
            var window = new MainWindow();
            app.Run(window);
        }
    }
}
