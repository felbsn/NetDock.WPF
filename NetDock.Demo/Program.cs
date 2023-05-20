using NetDock.Demo;
using System.Threading;
using System.Windows;

Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

 
 var app = new Application();

 app.Startup += (s, e) =>
 {
     new MainWindow().Show();
 };
 app.Run();
 