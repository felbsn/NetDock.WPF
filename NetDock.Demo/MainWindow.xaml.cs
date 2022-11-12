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
            //var item = new DockPanel();
            //item.TabName = "Test " + count++;

            var demo = new DemoContent();
            demo.Content = "Test Right" + count++;
            var item = new DockItem(demo);
            demo.Item = item;
            item.TabName = demo.Content;
            dockSurface.Add(item);
           
     
            // dockArea.Panel.Add(demo, DockDirection.Right);


            //dockArea.AddItem(item);
        }

        private void B(object sender, RoutedEventArgs e)
        {
            var demo = new DemoContent();
            demo.Content = "Test Bottom" + count++;
            var item = new DockItem(demo);
            demo.Item = item;
            item.TabName = demo.Content;
            dockSurface.Add(item, DockDirection.Bottom);
        }

        private void OpenWindow(object sender, RoutedEventArgs e)
        {
            var win = new DockWindow();
            win.Show();
        }
    }
}
