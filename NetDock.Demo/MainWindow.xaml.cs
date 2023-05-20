using NetDock.Controls;
using NetDock.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NetDock.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        int count = 0;

        private void A(object sender, RoutedEventArgs e)
        {
            var name = "Right:" + count++;

            var demo = new DemoContent()
            {
                Content = name
            };

            var item = new DockItem(demo)
            {
                TabName = name
            };

            demo.Item = item;

            dockSurface.Add(item);
        }

        private void B(object sender, RoutedEventArgs e)
        {
            var name = "Bottom:" + count++;

            var demo = new DemoContent()
            {
                Content = name
            };

            var item = new DockItem(demo)
            {
                TabName = name
            };

            demo.Item = item;

            dockSurface.Add(item , DockDirection.Bottom);
        }

        private void OpenWindow(object sender, RoutedEventArgs e)
        {
            var b = new SolidColorBrush();
                b.Color = Color.FromRgb((byte)Random.Shared.Next(255), (byte)Random.Shared.Next(255), (byte)Random.Shared.Next(255));

            var id = new Random().Next(10, 100);
            var label = new Label() { Content = "Content for " + id, Background = b };
            label.FontSize = 32;
            label.HorizontalContentAlignment = HorizontalAlignment.Center;
            label.VerticalContentAlignment = VerticalAlignment.Center;
            

            var di = new DockItem(label) { TabName = "Title for " + id };
            var win = new DockWindow(di);

            label.MouseDoubleClick += (s, e) =>
            {
                di.Surface?.Remove(di);
            };

            win.Show();
        }
    }
}
