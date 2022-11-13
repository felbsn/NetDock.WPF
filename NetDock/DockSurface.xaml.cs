using NetDock.Controls;
using NetDock.Enums;
using NetDock.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace NetDock
{
    /// <summary>
    /// Interaction logic for DockSurface.xaml
    /// </summary>
    public partial class DockSurface : UserControl
    {
        public static HashSet<DockSurface> Surfaces = new HashSet<DockSurface>();

        public int SplitterSize = 3;

        public Brush SplitBrush = Brushes.Transparent;

        public string TabName { get; set; }
        //Storyboard storyboard;

        public DockItem Current { get; private set; }
        DockSurface First { get; set; }
        DockSurface Second { get; set; }
        DockSurface ParentSurface { get; set; }

        List<DockItem> Tabs { get; set; } = new List<DockItem>();
        LayoutDirection Direction { get; set; }


        public DockSurface()
        {
            InitializeComponent();
            InitAnimations();
            DockWindow.WindowMoved += WindowMoving;

            Loaded += (s, e) =>
            {
                Debug.WriteLine("+dock area loaded");
                Surfaces.Add(this);
            };

            Unloaded += (s, e) =>
            {
                Debug.WriteLine("-dock area unloaded");
                Surfaces.Remove(this);
            };
        }



        int zindex = DockWindow.GetZ();
        public Window Window { get; set; }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            FrameworkElement elm = this;
            while (elm.Parent != null)
            {

                if (elm.Parent as FrameworkElement != null)
                    elm = elm.Parent as FrameworkElement;
                else
                {
                    _ = 1;
                }
            }

            if (Window != null)
            {
                Window.Activated -= OnWindowActivated;
                Window = null;
                Debug.WriteLine("removing from window");
            }

            if (elm is Window win)
            {
                Window = win;
                Window.Activated += OnWindowActivated;

                Debug.WriteLine("adding to window");
            }

            Debug.WriteLine("bu nedir " + elm.ToString());
            //tabStatus.Content = $"tablar im {Tabs.Count}";
        }

        public void OnWindowActivated(object? sender, EventArgs args)
        {
            zindex = DockWindow.GetZ();
            tabStatus.Content = $"zindex {zindex}";
        }


        public DockSurface(DockItem item) : this()
        {
            Add(item);
        }

        Storyboard storyboard;
        void InitAnimations()
        {
            var animDuration = 300;
            var ease = new CubicEase();
            //ease.Bounces = 2;
            //ease.Bounciness = 10;
            //ease.Amplitude = 0.7;
            //ease.Oscillations = 0;
            ease.EasingMode = EasingMode.EaseOut;

            ScaleTransform scale = new ScaleTransform(1.0, 1.0);
            rectLeft.RenderTransformOrigin = new Point(0.0, 0.5);
            rectLeft.RenderTransform = scale;

            storyboard = new Storyboard();
            DoubleAnimation growAnimationX = new DoubleAnimation();
            growAnimationX.Duration = TimeSpan.FromMilliseconds(animDuration);
            growAnimationX.From = 0.2;
            growAnimationX.To = 1;
            growAnimationX.EasingFunction = ease;
            storyboard.Children.Add(growAnimationX);

            DoubleAnimation growAnimationY = new DoubleAnimation();
            growAnimationY.Duration = TimeSpan.FromMilliseconds(animDuration);
            growAnimationY.From = 0.2;
            growAnimationY.To = 1;
            growAnimationY.EasingFunction = ease;
            storyboard.Children.Add(growAnimationY);

            Storyboard.SetTargetProperty(growAnimationX, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTarget(growAnimationX, rectLeft);
            Storyboard.SetTargetProperty(growAnimationY, new PropertyPath("RenderTransform.ScaleY"));
            Storyboard.SetTarget(growAnimationY, rectLeft);
        }


        void MakeColumns(DockSurface left, DockSurface right, double ratio = 0.5)
        {
            if (left == null || right == null || left == right)
            {
                throw new Exception("Illegal things happened");
            }

            Direction = LayoutDirection.Columns;

            First = left;
            Second = right;
            Current = null;
            Tabs.Clear();

            left.Parent?.RemoveChild(left);
            right.Parent?.RemoveChild(right);

            left.ParentSurface = this;
            right.ParentSurface = this;

            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();

            var col0 = new ColumnDefinition();
            col0.Width = new GridLength(1, GridUnitType.Star);

            var col1 = new ColumnDefinition();
            col1.Width = new GridLength(SplitterSize, GridUnitType.Pixel); ;

            var col2 = new ColumnDefinition();
            col2.Width = new GridLength(1, GridUnitType.Star);


            grid.ColumnDefinitions.Add(col0);
            grid.ColumnDefinitions.Add(col1);
            grid.ColumnDefinitions.Add(col2);

            var gridSplitter = new GridSplitter();
            gridSplitter.Width = SplitterSize;
            gridSplitter.HorizontalAlignment = HorizontalAlignment.Stretch;
            gridSplitter.Background = SplitBrush;


            Grid.SetColumn(left, 0);
            grid.Children.Add(left);

            Grid.SetColumn(gridSplitter, 1);
            grid.Children.Add(gridSplitter);

            Grid.SetColumn(right, 2);
            grid.Children.Add(right);
        }
        void MakeRows(DockSurface top, DockSurface bottom, double ratio = 0.5)
        {
            if (top == null || bottom == null || top == bottom)
            {
                throw new Exception("Illegal things happened");
            }

            Direction = LayoutDirection.Rows;

            First = top;
            Second = bottom;
            Current = null;
            Tabs.Clear();

            top.Parent?.RemoveChild(top);
            bottom.Parent?.RemoveChild(bottom);

            top.ParentSurface = this;
            bottom.ParentSurface = this;

            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();

            var row0 = new RowDefinition();
            row0.Height = new GridLength(1, GridUnitType.Star);

            var row1 = new RowDefinition();
            row1.Height = new GridLength(SplitterSize, GridUnitType.Pixel); ;

            var row2 = new RowDefinition();
            row2.Height = new GridLength(1, GridUnitType.Star);

            grid.RowDefinitions.Add(row0);
            grid.RowDefinitions.Add(row1);
            grid.RowDefinitions.Add(row2);

            var gridSplitter = new GridSplitter();
            gridSplitter.Height = SplitterSize;
            gridSplitter.HorizontalAlignment = HorizontalAlignment.Stretch;
            gridSplitter.Background = SplitBrush;

            Grid.SetRow(top, 0);
            grid.Children.Add(top);

            Grid.SetRow(gridSplitter, 1);
            grid.Children.Add(gridSplitter);

            Grid.SetRow(bottom, 2);
            grid.Children.Add(bottom);
        }

        public void Remove(DockItem item)
        {
            if (Tabs.Count > 0)
            {
                if (Tabs.Count == 1)
                {
                    grid.Children.Clear();
                    if (ParentSurface != null)
                    {
                        ParentSurface.Remove(item);
                        Current = null;
                    }
                    else
                    {
                        Current = null;
                    }
                }
                else
                {
                    if (Current == item)
                    {
                        var foundIndex = Tabs.IndexOf(item);
                        if (Tabs.Count > foundIndex + 1)
                            Current = Tabs[foundIndex + 1];
                        else
                            Current = Tabs[foundIndex - 1];

                        grid.Children.Clear();
                        grid.Children.Add(Current);
                    }
                }
                Tabs.Remove(item);
                // tab handler
                OnContentChanged();
            }
            else
            {
                DockSurface target, source;
                if (First.Current == item)
                {
                    target = First;
                    source = Second;
                }
                else
                if (Second.Current == item)
                {
                    target = Second;
                    source = First;
                }
                else
                {
                    throw new Exception("Kind of illegal remove, is going on..");
                }

                if (source.Tabs.Count > 0)
                {
                    First = null;
                    Second = null;
                    foreach (var exist in source.Tabs)
                    {
                        Add(exist, DockDirection.Stack);
                    }
                }
                else
                {
                    if (source.Direction == LayoutDirection.Rows)
                    {
                        MakeRows(source.First, source.Second);
                    }
                    else
                    {
                        MakeColumns(source.First, source.Second);
                    }
                }
            }
        }

        private void OnContentChanged()
        {
            //tabStatus.Content = $"tablar im {Tabs.Count}"; 


            if (Tabs.Count == 0)
                tabRowDef.Height = new GridLength(0, GridUnitType.Pixel);

            if (Current == null)
            {
                tab.Visibility = Visibility.Collapsed;
                tabRowDef.Height = new GridLength(0, GridUnitType.Pixel);
            }
            else
            {
                if (Tabs.Count > 0)
                {
                    tabRowDef.Height = new GridLength(32, GridUnitType.Pixel);

                    tabRowDef.Height = GridLength.Auto;
                    tab.Visibility = Visibility.Visible;
                    tab.Children.Clear();
                    foreach (var item in Tabs)
                    {
                        var btn = new Button();
                        Style style = this.FindResource("RoundCornerRightSelected") as Style;
                        btn.Style = style;

                        btn.Width = 100;
                        btn.Content = item.TabName + $"tc:{Tabs.Count}";
                        if (item == Current)
                            btn.Background = Brushes.Blue;

                        tab.Children.Add(btn);

                        btn.Click += (s, e) =>
                        {
                            if (Current != item)
                            {
                                item.BringIntoView();
                            }
                            //this.Remove(item);
                        };
                    }
                }
                else
                {
                    tabRowDef.Height = new GridLength(0, GridUnitType.Pixel);
                }
            }
        }

        public void Add(DockItem item, DockDirection dock = DockDirection.Right)
        {
            if (dock == DockDirection.Stack)
            {
                if (Current == null && First == null && Second == null)
                {
                    Add(item, DockDirection.Right);
                }
                else
                if (Current != null)
                {
                    Tabs.Add(item);
                    Current = item;
                    grid.Children.Clear();
                    grid.Children.Add(item);
                }
                else
                {
                    throw new Exception("Invalid Behaviour, how do i stack that thing?");
                }
            }
            else
            if (Current != null)
            {
                var currentPanel = new DockSurface(Current);
                var itemPanel = new DockSurface(item);

                switch (dock)
                {
                    case DockDirection.Left:
                        MakeColumns(itemPanel, currentPanel);
                        break;
                    case DockDirection.Right:
                        MakeColumns(currentPanel, itemPanel);
                        break;
                    case DockDirection.Top:
                        MakeRows(itemPanel, currentPanel);
                        break;
                    case DockDirection.Bottom:
                        MakeRows(currentPanel, itemPanel);
                        break;
                }
            }
            else
            if (First != null && Second != null)
            {
                if (Direction == LayoutDirection.Columns)
                {
                    switch (dock)
                    {
                        case DockDirection.Left:
                            First.Add(item, DockDirection.Left);
                            break;
                        case DockDirection.Right:
                            Second.Add(item, DockDirection.Right);
                            break;
                        case DockDirection.Top:
                            {
                                var panel = new DockSurface();
                                if (Direction == LayoutDirection.Columns)
                                    panel.MakeColumns(First, Second);
                                else
                                    panel.MakeRows(First, Second);

                                this.MakeRows(new DockSurface(item), panel);
                            }
                            break;
                        case DockDirection.Bottom:
                            {
                                var panel = new DockSurface();
                                if (Direction == LayoutDirection.Columns)
                                    panel.MakeColumns(First, Second);
                                else
                                    panel.MakeRows(First, Second);

                                this.MakeRows(panel, new DockSurface(item));
                            }
                            break;
                    }
                }
                else
                {
                    switch (dock)
                    {
                        case DockDirection.Left:
                            {
                                var panel = new DockSurface();
                                if (Direction == LayoutDirection.Columns)
                                    panel.MakeColumns(First, Second);
                                else
                                    panel.MakeRows(First, Second);

                                this.MakeColumns(new DockSurface(item), panel);
                            }
                            break;
                        case DockDirection.Right:
                            {
                                var panel = new DockSurface();
                                if (Direction == LayoutDirection.Columns)
                                    panel.MakeColumns(First, Second);
                                else
                                    panel.MakeRows(First, Second);

                                this.MakeColumns(panel, new DockSurface(item));
                            }
                            break;
                        case DockDirection.Top:
                            First.Add(item, DockDirection.Top);
                            break;
                        case DockDirection.Bottom:
                            Second.Add(item, DockDirection.Bottom);
                            break;
                    }
                }
            }
            else
            {
                item.Parent?.RemoveChild(item);

                Current = item;

                item.Surface = this;

                grid.ColumnDefinitions.Clear();
                grid.RowDefinitions.Clear();
                grid.Children.Add(item);
                Tabs.Add(item);
            }

            OnContentChanged();

            //FocusManager.SetIsFocusScope(this,true);
            //FocusManager.SetFocusedElement(item, item);
            //item.RaiseEvent(new RoutedEventArgs())
            //
            // item.Focus();
            //this.Focus();
            //item.Focus();
            //Keyboard.Focus(item);
            Task.Run(async () =>
            {
                await Task.Delay(100);
                Dispatcher.Invoke(() =>
                {
                    item.Focus();
                });
            });
        }

        static DockSurface Hovered { get; set; }

        public static async void OnDockWindowMoved(DockWindow win, Point p, bool released)
        {
            var list = Surfaces.Where(s => s.Window != win && s.Window != null && s.Visibility == Visibility.Visible).ToList();
            list.Sort((a, b) => b.zindex - a.zindex);

            foreach (var surface in list)
            {
                var handled = surface.HandleHover(p, released, out var pos);
                if (handled)
                {
                    if (Hovered != surface)
                    {
                        Hovered?.HandleHoverLost();
                        Hovered = surface;
                    }

                    if (released)
                    {
                        var beginW = win.Width;
                        var beginH = win.Height;

                        var beginLeft = win.Left;
                        var beignTop = win.Top;

                       

                        //win.Animate(pos.x, pos.y, pos.width, pos.height);
                        win.runAnim(pos.x, pos.y, pos.width, pos.height).ContinueWith(t => {
                            

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                surface.HandleHoverLost();
                                var child = win.grid.Children[0];
                                win.grid.RemoveChild(child);
                                surface.Add(new DockItem(child) , pos.direction);
                                win.Close();
                            });

                        });

                        //win.animating = true;

                        //win.runAnim(pos.x, pos.y, pos.width, pos.height).ContinueWith(t =>
                        //{
                        //    Application.Current.Dispatcher.Invoke(() =>
                        //    {
                        //        win.animating = false;
                        //        win.ResizeMode = ResizeMode.CanResize;
                        //    });
                        //});

                    }

                    break;
                }
            }

            if (released)
            {
                Hovered = null;
            }
        }
        public class HandlePos
        {
            public DockDirection direction { get; set; }
            public double x { get; set; }
            public double y { get; set; }
            public double width { get; set; }
            public double height { get; set; }
            public double width_offset { get; set; }
            public double height_offset { get; set; }

        }
        public bool HandleHover(Point p, bool released, out HandlePos pos)
        {
            pos = null;
            //if (released )
            //{
            //    lblDebug.Content = "sorry";
            //    //  Debug.WriteLine("reset");
            //    HandleHoverLost();
            //    return false;
            //}

            if (this.IsVisible)//&& Current != null
            {
                var lp = this.PointFromScreen(p);

                if (lp.X > 0 && lp.Y > 0 && lp.X < ActualWidth && lp.Y < ActualHeight)
                {
                    //         System.Diagnostics.Debug.WriteLine("inside");





                    lblDebug.Content = "Content over on me";
                    overlay.Visibility = Visibility.Visible;

                    DockDirection dir;
                    double width_offset = 0;
                    double height_offset = 0;
                    double width = ActualWidth;
                    double height = ActualHeight;

                    if(Current == null)
                    {
                        hideAllExcept(rectAll);
                        dir = DockDirection.Stack;
                    }
                    else
                    if (lp.X < ActualWidth * .3)
                    {
                        if (hideAllExcept(rectLeft))
                        {
                            storyboard.Begin(this);
                        }
                        dir = DockDirection.Left;

                        width = ActualWidth * 0.5;
                        width_offset = 0;
                    }
                    else
                    if (lp.Y < ActualHeight * .3)
                    {
                        hideAllExcept(rectTop);

                        dir = DockDirection.Top;

                        height = ActualHeight * 0.5;
                        height_offset = 0;

                    }
                    else
                    if (lp.X > ActualWidth * .7)
                    {
                        hideAllExcept(rectRight);
                        dir = DockDirection.Right;

                        width = ActualWidth * 0.5;
                        width_offset = ActualWidth * 0.5;
                    }
                    else
                    if (lp.Y > ActualHeight * .7)
                    {
                        hideAllExcept(rectBottom);

                        dir = DockDirection.Bottom;

                        height = ActualHeight * 0.5;
                        height_offset = ActualHeight * 0.5;
                    }
                    else
                    {
                        hideAllExcept(rectAll);
                        dir = DockDirection.Stack;
                    }

                    pos = new HandlePos()
                    {
                        direction = dir,
                        height = height,
                        width = width,
                        width_offset = width_offset,
                        height_offset = height_offset,
                        x = p.X - lp.X + width_offset,
                        y = p.Y - lp.Y + height_offset
                    };

                    Debug.WriteLine($"oluyor dir:{dir}");


                    return true;
                }
                else
                {
                    Debug.WriteLine("olmadi");
                    lblDebug.Content = "sorry";

                    //          System.Diagnostics.Debug.WriteLine("reset");

                    rectAll.Visibility = rectBottom.Visibility = rectTop.Visibility = rectLeft.Visibility = rectRight.Visibility = Visibility.Hidden;
                    overlay.Visibility = Visibility.Collapsed;
                }
            }

            return false;
        }

        public void HandleHoverLost()
        {
            rectAll.Visibility = rectBottom.Visibility = rectTop.Visibility = rectLeft.Visibility = rectRight.Visibility = Visibility.Hidden;
            overlay.Visibility = Visibility.Collapsed;
        }

        public void WindowMoving(object sender, DockWindowMoveEventArgs e)
        {
            var p = e.Point;
            var released = e.Released;

            if (released || e.Handled)
            {
                lblDebug.Content = "sorry";
                //  Debug.WriteLine("reset");

                rectAll.Visibility = rectBottom.Visibility = rectTop.Visibility = rectLeft.Visibility = rectRight.Visibility = Visibility.Hidden;
                overlay.Visibility = Visibility.Collapsed;
                return;
            }

            e.Handled = true;


            if (this.IsVisible)//&& Current != null
            {
                var lp = this.PointFromScreen(p);

                if (lp.X > 0 && lp.Y > 0 && lp.X < ActualWidth && lp.Y < ActualHeight)
                {
                    //         System.Diagnostics.Debug.WriteLine("inside");

                    lblDebug.Content = "Content over on me";
                    overlay.Visibility = Visibility.Visible;

                    if (lp.X < ActualWidth * .3)
                    {
                        if (hideAllExcept(rectLeft))
                        {
                            storyboard.Begin(this);
                        }

                    }
                    else
                    if (lp.Y < ActualHeight * .3)
                    {
                        hideAllExcept(rectTop);
                    }
                    else
                    if (lp.X > ActualWidth * .7)
                    {
                        hideAllExcept(rectRight);
                    }
                    else
                    if (lp.Y > ActualHeight * .7)
                    {
                        hideAllExcept(rectBottom);
                    }
                    else
                    {
                        hideAllExcept(rectAll);
                    }
                }
                else
                {
                    lblDebug.Content = "sorry";

                    //          System.Diagnostics.Debug.WriteLine("reset");

                    rectAll.Visibility = rectBottom.Visibility = rectTop.Visibility = rectLeft.Visibility = rectRight.Visibility = Visibility.Hidden;
                    overlay.Visibility = Visibility.Collapsed;
                }

            }
        }
        bool hideAllExcept(Rectangle except)
        {
            if (except != rectTop)
                rectTop.Visibility = Visibility.Hidden;
            if (except != rectBottom)
                rectBottom.Visibility = Visibility.Hidden;
            if (except != rectRight)
                rectRight.Visibility = Visibility.Hidden;
            if (except != rectLeft)
                rectLeft.Visibility = Visibility.Hidden;
            if (except != rectAll)
                rectAll.Visibility = Visibility.Hidden;

            var isVisible = except.Visibility == Visibility.Visible;
            if (!isVisible)
            {
                except.Visibility = Visibility.Visible;
                return true;
            }
            else
                return false;
        }
    }
}
