using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NetDock.Demo
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var app = new Application();

            app.Startup += (s, e) =>
            {
                new MainWindow().Show();
            };
            app.Run();
        }
    }
}
