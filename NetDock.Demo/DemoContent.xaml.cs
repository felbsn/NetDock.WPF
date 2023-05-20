using NetDock.Controls;
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
    /// Interaction logic for DemoContent.xaml
    /// </summary>
    public partial class DemoContent : UserControl
    {
        public DockItem Item { get; set; }
        public DemoContent()
        {
            InitializeComponent();
            RandomizeColor(root);
        }

        public DemoContent(string content)
        {
            InitializeComponent();
            RandomizeColor(root);
            Content = content;
        }

        public string Content { get => label.Content.ToString(); set => label.Content = value; }

        public void RandomizeColor(Panel panel)
        {
            var rng = new Random();

            var ang = rng.NextDouble() * 90;

            var bytes = new byte[3];
            rng.NextBytes(bytes);
            var c0 = Color.FromRgb(bytes[0], bytes[1], bytes[2]);

            rng.NextBytes(bytes);
            var c1 = Color.FromRgb(bytes[0], bytes[1], bytes[2]);

            var brush = new LinearGradientBrush(c0, c1, ang);


            panel.Background = brush;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Item.Surface?.Remove(Item);
        }
    }
}
