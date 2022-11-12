using NetDock.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;





namespace NetDock.Controls
{
    /// <summary>
    /// Interaction logic for DockWindow.xaml
    /// </summary>
    public partial class DockWindow : Window, IDockWindow
    {
        public DockWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                //todo: should i dispose source on Unloaded event
                HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
                source.AddHook(new HwndSourceHook(WndProc));
            };
        }

        const int WM_NCLBUTTONDOWN = 0x00A1;
        const int WM_NCLBUTTONUP = 0x00A2;
        const int WM_EXITSIZEMOVE = 0x0232;

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_EXITSIZEMOVE:
                    {
                        // your action
                        Debug.WriteLine($"WM_EXITSIZEMOVE");
                        WindowMoved?.Invoke(lastLocation, false);
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        public static bool IsWindowMoving = false;
        public static event Action<Point, bool> WindowMoved;


        public DateTime last_time;


        static Point lastLocation;
 
        protected override async void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            var point = System.Windows.Forms.Control.MousePosition;
            var p = new Point(point.X, point.Y);
             
            WindowMoved?.Invoke(p, true);
            lastLocation = p;
        }
    }
}
 