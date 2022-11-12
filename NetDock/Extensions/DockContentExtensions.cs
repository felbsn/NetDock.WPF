using NetDock.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetDock.Extensions
{
    public static class DockContentExtensions
    {
        public static void Close(this IDockItem dockContent)
        {
            //dockContent.Surface.Remove()
        }
    }
}
