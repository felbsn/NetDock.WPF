using NetDock.Controls;
using NetDock.Enums;
using NetDock.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
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
using System.Windows.Media.Media3D;
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

        public Brush SplitBrush = new SolidColorBrush(new System.Windows.Media.Color() { R = 0, G = 0, B = 0, A = 12 });

        public string TabName { get; set; }

        DockItem Item { get; set; }
        DockSurface First { get; set; }
        DockSurface Second { get; set; }
        List<DockItem> Stack { get; set; } = new List<DockItem>();
        DockSurface ParentSurface { get; set; }
        LayoutDirection Direction { get; set; }


        public DockSurface()
        {
            InitializeComponent();
            InitAnimations();
            DockWindow.WindowMoved += WindowMoving;

            Loaded += (s, e) =>
            {
                //Debug.WriteLine("+dock area loaded");
                Surfaces.Add(this);
            };

            Unloaded += (s, e) =>
            {
                //Debug.WriteLine("-dock area unloaded");
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
                //Debug.WriteLine("removing from window");
            }

            if (elm is Window win)
            {
                Window = win;
                Window.Activated += OnWindowActivated;

                //Debug.WriteLine("adding to window");
            }
            else
            if (elm is System.Windows.Documents.AdornerDecorator decorator)
            {
                // WinForms integration...
                //Debug.WriteLine("pretend a window");
                OnWindowActivated(this, EventArgs.Empty);

                _ = elm;
            }
        }

        public void OnWindowActivated(object? sender, EventArgs args)
        {
            zindex = DockWindow.GetZ();
            //tabStatus.Content = $"zindex {zindex} surfaces:${Surfaces.Count}";
        }

        public DockSurface(DockItem item) : this()
        {
            item.Surface = this;
            Add(item);
        }

        public DockSurface(DockItem item, List<DockItem> stack) : this()
        {
            Item = item;
            Stack = stack;
            grid.Children.Add(item.GetDockItem());
        }

        Storyboard rectLeftAnimation;
        Storyboard rectRightAnimation;
        Storyboard rectTopAnimation;
        Storyboard rectBottomAnimation;
        Storyboard rectAllAnimation;
        void InitAnimations()
        {
            var animDuration = 300;
            var ease = new CubicEase();
            //ease.Bounces = 2;
            //ease.Bounciness = 10;
            //ease.Amplitude = 0.7;
            //ease.Oscillations = 0;
            ease.EasingMode = EasingMode.EaseOut;

            //InitRectLeftAnimation(animDuration, ease);

            rectLeftAnimation = InitGrowAnimation(rectLeft, new Point(0.0, 0.5), animDuration, ease);
            rectRightAnimation = InitGrowAnimation(rectRight, new Point(1.0, 0.5), animDuration, ease);

            rectTopAnimation = InitGrowAnimation(rectTop, new Point(0.5, 0.0), animDuration, ease);
            rectBottomAnimation = InitGrowAnimation(rectBottom, new Point(0.5, 1.0), animDuration, ease);

            rectAllAnimation = InitGrowAnimation(rectAll, new Point(0.5, 0.5), animDuration, ease);
        }

        private void InitRectLeftAnimation(int animDuration, CubicEase ease)
        {
            ScaleTransform scale = new ScaleTransform(1.0, 1.0);
            rectLeft.RenderTransformOrigin = new Point(0.0, 0.5);
            rectLeft.RenderTransform = scale;

            rectLeftAnimation = new Storyboard();
            DoubleAnimation growAnimationX = new DoubleAnimation();
            growAnimationX.Duration = TimeSpan.FromMilliseconds(animDuration);
            growAnimationX.From = 0.2;
            growAnimationX.To = 1;
            growAnimationX.EasingFunction = ease;
            rectLeftAnimation.Children.Add(growAnimationX);

            DoubleAnimation growAnimationY = new DoubleAnimation();
            growAnimationY.Duration = TimeSpan.FromMilliseconds(animDuration);
            growAnimationY.From = 0.2;
            growAnimationY.To = 1;
            growAnimationY.EasingFunction = ease;
            rectLeftAnimation.Children.Add(growAnimationY);

            Storyboard.SetTargetProperty(growAnimationX, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTarget(growAnimationX, rectLeft);
            Storyboard.SetTargetProperty(growAnimationY, new PropertyPath("RenderTransform.ScaleY"));
            Storyboard.SetTarget(growAnimationY, rectLeft);
        }

        private Storyboard InitGrowAnimation(UIElement target, Point origin, int animDuration, CubicEase ease)
        {
            ScaleTransform scale = new ScaleTransform(1.0, 1.0);
            target.RenderTransformOrigin = origin;
            target.RenderTransform = scale;

            var anim = new Storyboard();
            DoubleAnimation growAnimationX = new DoubleAnimation();
            growAnimationX.Duration = TimeSpan.FromMilliseconds(animDuration);
            growAnimationX.From = 0.2;
            growAnimationX.To = 1;
            growAnimationX.EasingFunction = ease;
            anim.Children.Add(growAnimationX);

            DoubleAnimation growAnimationY = new DoubleAnimation();
            growAnimationY.Duration = TimeSpan.FromMilliseconds(animDuration);
            growAnimationY.From = 0.2;
            growAnimationY.To = 1;
            growAnimationY.EasingFunction = ease;
            anim.Children.Add(growAnimationY);

            Storyboard.SetTargetProperty(growAnimationX, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTarget(growAnimationX, target);
            Storyboard.SetTargetProperty(growAnimationY, new PropertyPath("RenderTransform.ScaleY"));
            Storyboard.SetTarget(growAnimationY, target);
            return anim;
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
            Item = null;
            Stack.Clear();

            left.Parent?.RemoveChild(left);
            right.Parent?.RemoveChild(right);

            left.Stack.ForEach(i => i.Surface = left);
            right.Stack.ForEach(i => i.Surface = right);

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
            Item = null;
            Stack.Clear();

            top.Parent?.RemoveChild(top);
            bottom.Parent?.RemoveChild(bottom);

            top.Stack.ForEach(i => i.Surface = top);
            bottom.Stack.ForEach(i => i.Surface = bottom);

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

        void ResetLayout()
        {
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
        }

        public void Remove(DockSurface surface)
        {
            ResetLayout();

            DockSurface remain = null;
            if (First == surface)
            {
                First = null;
                remain = Second;
            }
            if (Second == surface)
            {
                Second = null;
                remain = First;
            }

            if (remain != null)
            {
                if (remain.Item != null)
                {
                    Item = remain.Item;
                    Stack = remain.Stack.ToList();

                    grid.Children.Clear();
                    grid.Children.Add(remain.Item.GetDockItem());
                    First = Second = null;
                }
                else
                {
                    if(remain.First != null && remain.Second != null)
                    {
                        if (remain.Direction == LayoutDirection.Rows)
                        {
                            MakeRows(remain.First, remain.Second);
                        }
                        else
                        {
                            MakeColumns(remain.First, remain.Second);
                        }
                    }
                }
            }else
            {
                Item = null;
                grid.Children.Clear();
                Stack.Clear();
                ParentSurface?.Remove(this);
            }
            OnContentChanged();
        }

        public bool Remove(DockItem item)
        {
            if (Stack.Remove(item))
            {
                if (Stack.Count == 0)
                {
                    Item = null;
                    grid.Children.Clear();
                    this.ParentSurface?.Remove(this);
                }
                else
                {
                    if(Item == item)
                    {
                        var next = Stack.First();
                        Item = next;
                        grid.Children.Clear();
                        grid.Children.Add(next.GetDockItem());
                    }
                }

                OnContentChanged();
                item.Surface = null;

                return true;
            }else
            {
                return false;
            }
        }

        private void OnContentChanged()
        {
            if (Item == null)
            {
                tab.Visibility = Visibility.Collapsed;
                tabRowDef.Height = new GridLength(0, GridUnitType.Pixel);
            }
            else
            {
                if (Stack.Count > 0)
                {
                    tabRowDef.Height = new GridLength(32, GridUnitType.Pixel);
                    tabRowDef.Height = GridLength.Auto;
                    tab.Visibility = Visibility.Visible;
                    tab.Children.Clear();
                    foreach (var item in Stack)
                    {
                        var btn = new Button();
                        var style = this.FindResource("TabItem") as Style;
                        var selected_style = this.FindResource("TabItemSelected") as Style;

                        btn.Style = Item == item ? selected_style : style;
                        btn.MinWidth = 100;
                        btn.Content = item.TabName;

                        tab.Children.Add(btn);
                        btn.Click += (s, e) =>
                        {
                            foreach (Button button in tab.Children)
                                button.Style = button == btn ? selected_style : button.Style = style;

                            var control = item.GetDockItem();
                            grid.Children.Clear();
                            grid.Children.Add(control);
                            Item = item;
                        };

                        var down = false;
                        var downTabName = item.TabName;
                        Point begin;
                        btn.PreviewMouseDown += (s, e) =>
                        {
                            if(e.LeftButton == MouseButtonState.Pressed)
                            {
                                var pos = System.Windows.Forms.Control.MousePosition;
                                begin = btn.PointFromScreen(new Point(pos.X, pos.Y));
                                down = true;
                            }else if (e.MiddleButton == MouseButtonState.Pressed)
                            {
                                Remove(item);
                            }
                            //Debug.WriteLine($"DOWNAD {downTabName} -> {btn.Content} ");
                        };
                        btn.MouseDoubleClick += (s, e) =>
                        {
                            InTransition = true;

                            Remove(item);

                            var win = new DockWindow(item);
                            var point = System.Windows.Forms.Control.MousePosition;
                            win.Left = point.X - begin.X;
                            win.Top = point.Y - begin.Y;
                            win.Show();
                            InTransition = false;
                            win.Activate();
                        };

                        this.PreviewMouseUp += (s, e) =>
                        {
                            down = false;
                            //Debug.WriteLine($"UPPAD {downTabName} -> {btn.Content}");
                        };
                        btn.PreviewMouseMove += (s, e) =>
                        {
                            if (down)
                            {
                                var buttons = System.Windows.Forms.Control.MouseButtons;
                                var point = System.Windows.Forms.Control.MousePosition;
                                var lp = btn.PointFromScreen(new Point(point.X, point.Y));
                                if (lp.X > 0 && lp.Y > 0 && lp.X < btn.ActualWidth && lp.Y < btn.ActualHeight)
                                {
                                    //Debug.WriteLine($"Moving {point.X}, {point.Y} ");
                                }
                                else
                                {
                                    Remove(item);
        
                                    var win = new DockWindow(item);
                                    win.Show();
                                    win.Left = point.X - begin.X;
                                    win.Top = point.Y - begin.Y;
                                    //Debug.WriteLine($"OUTSIDE {point.X}, {point.Y} {downTabName} -> {btn.Content.ToString()}");
                                    down = false;
                                    if (buttons.HasFlag(System.Windows.Forms.MouseButtons.Left))
                                    {
                                        win.Activate();
                                        win.DragMove();
                                    }
                                }
                            }
                            else
                            {
                                //Debug.WriteLine($"NOD DOWN  ");
                            }
                        };

                    }
                }
                else
                {
                    tab.Children.Clear();
                    tabRowDef.Height = new GridLength(0, GridUnitType.Pixel);
                }
            }
        }

        public void Add(DockItem newItem, DockDirection dock = DockDirection.Right)
        {
            if (dock == DockDirection.Stack)
            {
                if (Item == null && First == null && Second == null)
                {
                    Add(newItem, DockDirection.Right);
                }
                else
                if (Item != null)
                {
                    Stack.Add(newItem);
                    Item = newItem;
                    grid.Children.Clear();
                    grid.Children.Add(newItem.GetDockItem());
                }
                else
                {
                    throw new Exception("Invalid Behaviour, how do i stack that thing?");
                }
            }
            else
            if (Item != null)
            {
                var currentPanel = new DockSurface();
                currentPanel.Stack = Stack.ToList();
                currentPanel.Item = Item;
                currentPanel.grid.Children.Clear();
                currentPanel.grid.Children.Add(Item.GetDockItem());
                var itemPanel = new DockSurface(newItem);


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
                currentPanel.OnContentChanged();
            }
            else
            if (First != null && Second != null)
            {
                if (Direction == LayoutDirection.Columns)
                {
                    switch (dock)
                    {
                        case DockDirection.Left:
                            First.Add(newItem, DockDirection.Left);
                            break;
                        case DockDirection.Right:
                            Second.Add(newItem, DockDirection.Right);
                            break;
                        case DockDirection.Top:
                            {
                                var panel = new DockSurface();
                                panel.ParentSurface = this;
                                if (Direction == LayoutDirection.Columns)
                                    panel.MakeColumns(First, Second);
                                else
                                    panel.MakeRows(First, Second);

                                this.MakeRows(new DockSurface(newItem), panel);
                            }
                            break;
                        case DockDirection.Bottom:
                            {
                                var panel = new DockSurface();
                                panel.ParentSurface = this;
                                if (Direction == LayoutDirection.Columns)
                                    panel.MakeColumns(First, Second);
                                else
                                    panel.MakeRows(First, Second);

                                this.MakeRows(panel, new DockSurface(newItem));
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
                                panel.ParentSurface = this;
                                if (Direction == LayoutDirection.Columns)
                                    panel.MakeColumns(First, Second);
                                else
                                    panel.MakeRows(First, Second);

                                this.MakeColumns(new DockSurface(newItem), panel);
                            }
                            break;
                        case DockDirection.Right:
                            {
                                var panel = new DockSurface();
                                panel.ParentSurface = this;
                                if (Direction == LayoutDirection.Columns)
                                    panel.MakeColumns(First, Second);
                                else
                                    panel.MakeRows(First, Second);

                                this.MakeColumns(panel, new DockSurface(newItem));
                            }
                            break;
                        case DockDirection.Top:
                            First.Add(newItem, DockDirection.Top);
                            break;
                        case DockDirection.Bottom:
                            Second.Add(newItem, DockDirection.Bottom);
                            break;
                    }
                }
            }
            else
            {
                newItem.Parent?.RemoveChild(newItem);

                newItem.Surface = this;

                grid.ColumnDefinitions.Clear();
                grid.RowDefinitions.Clear();
                Item = newItem;

                var dockItem = newItem.GetDockItem();
                grid.Children.Add(dockItem);

                Stack.Add(newItem);
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
            //Task.Run(async () =>
            //{
            //    await Task.Delay(100);
            //    Dispatcher.Invoke(() =>
            //    {
            //        newItem.Focus();
            //    });
            //});
        }

        static DockSurface Hovered { get; set; }
        public static bool InTransition { get; private set; }

        public static void OnDockWindowMoved(DockWindow win, Point p, bool released)
        {
            if (InTransition)
                return;

            //var list = Surfaces.Where(s => s.Window != win && s.Window != null && s.Visibility == Visibility.Visible).ToList();
            var list = Surfaces.Where(s => s.Window != win && s.Visibility == Visibility.Visible).ToList();
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

                        DockSurface.InTransition = true;
                        //win.Animate(pos.x, pos.y, pos.width, pos.height);
                        _ = win.BeginTransition(pos.x, pos.y, pos.width, pos.height).ContinueWith(t =>
                        {
                            //todo:fix the crash if no current app
                            win.Dispatcher.Invoke(() =>
                            {
                                InTransition = false;
                                surface.HandleHoverLost();
                                surface.Add(win.Item, pos.direction);
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
            if (this.Item == null && First != null && Second != null)
            {
                pos = null;
                return false;
            }
            pos = null;

            if (this.IsVisible)//&& Current != null
            {
                var lp = this.PointFromScreen(p);

                if (lp.X > 0 && lp.Y > 0 && lp.X < ActualWidth && lp.Y < ActualHeight)
                {
                    //System.Diagnostics.Debug.WriteLine("inside");
                    //lblDebug.Content = "Content over on me";
                    overlay.Visibility = Visibility.Visible;

                    DockDirection dir;
                    double width_offset = 0;
                    double height_offset = 0;
                    double width = ActualWidth;
                    double height = ActualHeight;

                    if (Item == null)
                    {
                        if (hideAllExcept(rectAll))
                        {
                            rectAllAnimation.Begin(this);
                        }
                        dir = DockDirection.Stack;
                    }
                    else
                    if (lp.X < ActualWidth * .3)
                    {
                        if (hideAllExcept(rectLeft))
                        {
                            rectLeftAnimation.Begin(this);
                        }
                        dir = DockDirection.Left;

                        width = ActualWidth * 0.5;
                        width_offset = 0;
                    }
                    else
                    if (lp.Y < ActualHeight * .3)
                    {
                        if (hideAllExcept(rectTop))
                        {
                            rectTopAnimation.Begin(this);
                        }

                        dir = DockDirection.Top;

                        height = ActualHeight * 0.5;
                        height_offset = 0;

                    }
                    else
                    if (lp.X > ActualWidth * .7)
                    {
                        if (hideAllExcept(rectRight))
                        {
                            rectRightAnimation.Begin(this);
                        }
                        dir = DockDirection.Right;

                        width = ActualWidth * 0.5;
                        width_offset = ActualWidth * 0.5;
                    }
                    else
                    if (lp.Y > ActualHeight * .7)
                    {
                        if (hideAllExcept(rectBottom))
                        {
                            rectBottomAnimation.Begin(this);
                        }

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

                    //Debug.WriteLine($"oluyor dir:{dir}");


                    return true;
                }
                else
                {
                    //Debug.WriteLine("olmadi");
                    //lblDebug.Content = "sorry";
                    //System.Diagnostics.Debug.WriteLine("reset");

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
                //lblDebug.Content = "sorry";
                // Debug.WriteLine("reset");

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
                    //System.Diagnostics.Debug.WriteLine("inside");
                    //lblDebug.Content = "Content over on me";
                    overlay.Visibility = Visibility.Visible;

                    if (lp.X < ActualWidth * .3)
                    {
                        if (hideAllExcept(rectLeft))
                        {
                            rectLeftAnimation.Begin(this);
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
                    //lblDebug.Content = "sorry";
                    //System.Diagnostics.Debug.WriteLine("reset");

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
