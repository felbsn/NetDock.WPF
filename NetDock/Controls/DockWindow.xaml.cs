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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;



public class DockWindowMoveEventArgs : EventArgs
{
    public DockWindowMoveEventArgs(Point point, bool released)
    {
        Point = point;
        Released = released;
    }

    public bool Handled { get; set; } = false;

    public Point Point { get; }

    public bool Released { get; }
}


namespace NetDock.Controls
{
    /// <summary>
    /// Interaction logic for DockWindow.xaml
    /// </summary>
    /// 
    public partial class DockWindow : Window, IDockWindow
    {
        int id = new Random().Next(10, 100);
        int zorder = 0;

        static int zorderx = 1;

        public bool animating = false;

        public static int GetZ()
        {
            return ++zorderx;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            zorder = GetZ();
            Title = $"id:{id}, z:{zorder}";

        }

        Storyboard storyboard;
        DoubleAnimation growAnimationW;
        DoubleAnimation growAnimationH;

        DoubleAnimation growAnimationT;
        DoubleAnimation growAnimationL;

        TaskCompletionSource tsc;
        public Task runAnim(double x, double y,double w, double h)
        {
            tsc = new TaskCompletionSource();

            growAnimationW.From = this.Width;
            growAnimationW.To = w;

            growAnimationH.From = this.Height;
            growAnimationH.To = h;


            growAnimationT.From = this.Top;
            growAnimationT.To = y;

            growAnimationL.From = this.Left;
            growAnimationL.To = x;

        
            storyboard.Begin();

            tsc.Task.ContinueWith((t) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    storyboard.Stop();

                    WindowWidthAnimation = w;
                    WindowHeightAnimation = h;
                });
            });

            return tsc.Task;
        }
        void InitAnimations()
        {
            var animDuration = 300;
            var ease = new CubicEase();
            //ease.Bounces = 2;
            //ease.Bounciness = 10;
            //ease.Amplitude = 0.7;
            //ease.Oscillations = 0;
            ease.EasingMode = EasingMode.EaseOut;

            //ScaleTransform scale = new ScaleTransform(1.0, 1.0);
            //this.RenderTransformOrigin = new Point(0.0, 0.5);
            //rectLeft.RenderTransform = scale;

            storyboard = new Storyboard();
            growAnimationW = new DoubleAnimation();
            growAnimationW.Duration = TimeSpan.FromMilliseconds(animDuration);
            growAnimationW.From = 200;
            growAnimationW.To = 400;
            growAnimationW.EasingFunction = ease;
            storyboard.Children.Add(growAnimationW);
            Storyboard.SetTargetProperty(growAnimationW, new PropertyPath(DockWindow.WindowWidthAnimationProperty));
            Storyboard.SetTarget(growAnimationW, this);

            growAnimationH = new DoubleAnimation();
            growAnimationH.Duration = TimeSpan.FromMilliseconds(animDuration);
            growAnimationH.From = 200;
            growAnimationH.To = 400;
            growAnimationH.EasingFunction = ease;
            storyboard.Children.Add(growAnimationH);
            Storyboard.SetTargetProperty(growAnimationH, new PropertyPath(DockWindow.WindowHeightAnimationProperty));
            Storyboard.SetTarget(growAnimationH, this);

            growAnimationT = new DoubleAnimation();
            growAnimationT.Duration = TimeSpan.FromMilliseconds(animDuration);
            growAnimationT.From = 200;
            growAnimationT.To = 400;
            growAnimationT.EasingFunction = ease;
            storyboard.Children.Add(growAnimationT);
            Storyboard.SetTargetProperty(growAnimationT, new PropertyPath("Top"));
            Storyboard.SetTarget(growAnimationT, this);

            growAnimationL = new DoubleAnimation();
            growAnimationL.Duration = TimeSpan.FromMilliseconds(animDuration);
            growAnimationL.From = 200;
            growAnimationL.To = 400;
            growAnimationL.EasingFunction = ease;
            storyboard.Children.Add(growAnimationL);
            Storyboard.SetTargetProperty(growAnimationL, new PropertyPath("Left"));
            Storyboard.SetTarget(growAnimationL, this);

            storyboard.Completed += Storyboard_Completed;
        }

        private void Storyboard_Completed(object? sender, EventArgs e)
        {
            tsc?.TrySetResult();
            //storyboard.Stop(); 
        }

        public DockWindow(Control content)
        {
            InitializeComponent();
            InitAnimations();
            Loaded += (s, e) =>
            {
                //todo: should i dispose source on Unloaded event
                HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
                source.AddHook((IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) =>
                {
                    switch (msg)
                    {
                        case WM_EXITSIZEMOVE:
                            {
                                // your action
                                //Debug.WriteLine($"WM_EXITSIZEMOVE");
                                //WindowMoved?.Invoke(this, new DockWindowMoveEventArgs(lastLocation, true));

                                //if (IsWindowResized())
                                //    break;
                                //Debug.WriteLine("exit size move...");
                             
                                if (moved)
                                {
                                    moved = false;
                                    var point = System.Windows.Forms.Control.MousePosition;
                                    var p = new Point(point.X, point.Y);
                                    DockSurface.OnDockWindowMoved(this, p, true);
                                }

                           
                            }
                            break;
                    }

                    // Debug.WriteLine($"id:{id}, {msg}");

                    return IntPtr.Zero;
                });
            };

            grid.Children.Add(content);

            zorder = GetZ();
            Title = $"id:{id}, z:{zorder}";



        }

        const int WM_EXITSIZEMOVE = 0x0232;

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {

            return IntPtr.Zero;
        }

        public static bool IsWindowMoving = false;
        public static EventHandler<DockWindowMoveEventArgs> WindowMoved;

        static Point lastLocation;


        double width;
        double height;
        bool moved = false;

        public bool IsWindowResized()
        {
           // Debug.WriteLine($"window resized w:{width}<>{Width}, h:{height}<>{Height}, animating:{animating}");
            if (width != ActualWidth || height != ActualHeight)
            {
                width = ActualWidth;
                height = ActualHeight;
                return true;
            }
            //Debug.WriteLine("NOTE RESIZED");

            return false;
        }

        double left;
        double top;
        public bool IsWindowMoved()
        {
           // Debug.WriteLine($"window moved w:{left}<>{Left}, h:{top}<>{Top}, animating:{animating}");
            if (Math.Abs(left - Left) > 1 || Math.Abs(top - Top) > 2)
            {
                left = Left;
                top = Top;
                return true;
            }

            return false;
        }



        protected override async void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);

            if (animating)
                return;

            if (IsWindowResized())
            {
                return;
            }

            if(!IsWindowMoved())
            {
                return;
            }


            moved = true;

            var point = System.Windows.Forms.Control.MousePosition;
            var p = new Point(point.X, point.Y);

            lastLocation = p;
            //  WindowMoved?.Invoke(this, new DockWindowMoveEventArgs(p,false));

            DockSurface.OnDockWindowMoved(this, p, false);
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int X;
            public int Y;
            public int Width;
            public int Height;
        }

        public enum SpecialWindowHandles
        {
            HWND_TOP = 0,
            HWND_BOTTOM = 1,
            HWND_TOPMOST = -1,
            HWND_NOTOPMOST = -2
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT Rect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public static readonly DependencyProperty WindowHeightAnimationProperty = DependencyProperty.Register(nameof(WindowHeightAnimation), typeof(double),
                                                                                                    typeof(DockWindow), new PropertyMetadata(OnWindowHeightAnimationChanged));

        private static void OnWindowHeightAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;

            if (window != null)
            {
                IntPtr handle = new WindowInteropHelper(window).Handle;
                var rect = new RECT();
                if (GetWindowRect(handle, ref rect))
                {
                    rect.X = (int)window.Left;
                    rect.Y = (int)window.Top;

                    rect.Width = (int)window.ActualWidth;
                    rect.Height = (int)(double)e.NewValue;  // double casting from object to double to int

                    SetWindowPos(handle, new IntPtr((int)SpecialWindowHandles.HWND_TOP), rect.X, rect.Y, rect.Width, rect.Height, (uint)SWP.SHOWWINDOW);
                }
            }
        }

        public double WindowHeightAnimation
        {
            get { return (double)GetValue(WindowHeightAnimationProperty); }
            set { SetValue(WindowHeightAnimationProperty, value); }
        }

        public static readonly DependencyProperty WindowWidthAnimationProperty = DependencyProperty.Register(nameof(WindowWidthAnimation), typeof(double),
                                                                                                    typeof(DockWindow), new PropertyMetadata(OnWindowWidthAnimationChanged));

        private static void OnWindowWidthAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;

            if (window != null)
            {
                IntPtr handle = new WindowInteropHelper(window).Handle;
                var rect = new RECT();
                if (GetWindowRect(handle, ref rect))
                {
                    rect.X = (int)window.Left;
                    rect.Y = (int)window.Top;
                    var width = (int)(double)e.NewValue;
                    rect.Width = width;
                    rect.Height = (int)window.ActualHeight;

                    SetWindowPos(handle, new IntPtr((int)SpecialWindowHandles.HWND_TOP), rect.X, rect.Y, rect.Width, rect.Height, (uint)SWP.SHOWWINDOW);
                }
            }
        }

        public double WindowWidthAnimation
        {
            get { return (double)GetValue(WindowWidthAnimationProperty); }
            set { SetValue(WindowWidthAnimationProperty, value); }
        }

        public void Animate(double x, double y, double w ,double h)
        {
            this.AnimateWindowSize(x,y, w, h);
        }

        /// <summary>
        /// SetWindowPos Flags
        /// </summary>
        public static class SWP
        {
            public static readonly int
            NOSIZE = 0x0001,
            NOMOVE = 0x0002,
            NOZORDER = 0x0004,
            NOREDRAW = 0x0008,
            NOACTIVATE = 0x0010,
            DRAWFRAME = 0x0020,
            FRAMECHANGED = 0x0020,
            SHOWWINDOW = 0x0040,
            HIDEWINDOW = 0x0080,
            NOCOPYBITS = 0x0100,
            NOOWNERZORDER = 0x0200,
            NOREPOSITION = 0x0200,
            NOSENDCHANGING = 0x0400,
            DEFERERASE = 0x2000,
            ASYNCWINDOWPOS = 0x4000;
        }

    }




    public static class WindowUtilties
    {
        public static void AnimateWindowSize(this Window target,double x,double y, double newWidth, double newHeight)
        {
            var duration = TimeSpan.FromMilliseconds(300);
            var sb = new Storyboard { 
                Duration = duration
            };

            var aniTop = new DoubleAnimationUsingKeyFrames();
            aniTop.Duration = duration;
            aniTop.KeyFrames.Add(new EasingDoubleKeyFrame(target.Top, TimeSpan.Zero));
            aniTop.KeyFrames.Add(new EasingDoubleKeyFrame(y, duration));
            Storyboard.SetTarget(aniTop, target);
            Storyboard.SetTargetProperty(aniTop, new PropertyPath(DockWindow.TopProperty));
            sb.Children.Add(aniTop);

            var aniLeft = new DoubleAnimationUsingKeyFrames();
            aniLeft.Duration = duration;
            aniLeft.KeyFrames.Add(new EasingDoubleKeyFrame(target.Left, TimeSpan.Zero));
            aniLeft.KeyFrames.Add(new EasingDoubleKeyFrame(x, duration));
            Storyboard.SetTarget(aniLeft, target);
            Storyboard.SetTargetProperty(aniLeft, new PropertyPath(DockWindow.LeftProperty));
            sb.Children.Add(aniLeft);


            var aniWidth = new DoubleAnimationUsingKeyFrames();
            aniWidth.Duration = duration;
            aniWidth.KeyFrames.Add(new EasingDoubleKeyFrame(target.ActualWidth, TimeSpan.Zero));
            aniWidth.KeyFrames.Add(new EasingDoubleKeyFrame(newWidth, duration));
            Storyboard.SetTarget(aniWidth, target);
            Storyboard.SetTargetProperty(aniWidth, new PropertyPath(DockWindow.WindowWidthAnimationProperty));
            sb.Children.Add(aniWidth);

            var aniHeight = new DoubleAnimationUsingKeyFrames();
            aniHeight.Duration = duration;
            aniHeight.KeyFrames.Add(new EasingDoubleKeyFrame(target.ActualHeight, TimeSpan.Zero));
            aniHeight.KeyFrames.Add(new EasingDoubleKeyFrame(newHeight, duration));
            Storyboard.SetTarget(aniHeight, target);
            Storyboard.SetTargetProperty(aniHeight, new PropertyPath(DockWindow.WindowHeightAnimationProperty));
            sb.Children.Add(aniHeight);

            sb.Begin();
        }
    }
}
