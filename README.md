# NetDock WIP 
a modern docking windows approach for WPF

 [![NetDock Demo](https://i.ytimg.com/vi/RXwcplshrpo/maxresdefault.jpg)](https://www.youtube.com/watch?v=RXwcplshrpo)
 (click to image to watch library in [action](https://www.youtube.com/watch?v=RXwcplshrpo) or action before the second action)
 
 ## Usage in XAML
 
 add namespace as shown
 ```xaml
  xmlns:netdock="clr-namespace:NetDock;assembly=NetDock"
 ```
 
 create surface for docking
 ```xaml
  <netdock:DockSurface x:Name="dockSurface"></netdock:DockSurface>
 ```
 
you can add your dock contents programmatically
```csharp
  var item = new DockItem(yourCustomWpfUserControl)
  {
      TabName = "tab & window title"
  };
  dockSurface.Add(item , DockDirection.Bottom);
```

for windows, use dock item with DockWindow
```csharp
DockItem item = GetMyDockItem();
var win = new DockWindow(item);
// win.Show() or some other action etc
```
