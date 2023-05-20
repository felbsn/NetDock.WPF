using NetDock.Interfaces;
using NetDock.Util;
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

namespace NetDock.Controls;

/// <summary>
/// Interaction logic for DockItem.xaml
/// </summary>
public partial class DockItem : UserControl, IDockItem
{
    public DockItem(FrameworkElement element)
    {
        InitializeComponent();
        grid.Children.Add(element);
        DockContent = element;
    }

    public string TabName { get;set; }
    public FrameworkElement DockContent { get; set; }
    public DockSurface Surface { get; set; }

    internal FrameworkElement GetDockItem()
    {
        if (DockContent == null)
            throw new Exception("DockItem should contain a DockContent element");

        if (DockContent.Parent != null)
            DockContent.Parent.RemoveChild(DockContent);

        return DockContent;
    }
}
