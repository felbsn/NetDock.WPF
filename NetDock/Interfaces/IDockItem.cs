using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NetDock.Interfaces
{
    public interface IDockItem
    {
        DockSurface Surface { get; set; }
        public string TabName { get; set; }
    }
}
