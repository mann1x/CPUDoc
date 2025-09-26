using AdonisUI.Extensions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using Windows.Devices.Display.Core;

namespace CPUDoc.Windows
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow
    {
        //internal static readonly AppSettings appSettings = (Application.Current as App)?.settings;
        //internal static readonly Updater updater = (Application.Current as App)?.updater;
        //public static SplashWindow splash = new SplashWindow();
      
        // To refresh the UI immediately
        private delegate void RefreshDelegate();
        private static void doRefresh(DependencyObject obj)
        {
            if (App.splash.IsVisible)
                obj.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input,
                    (RefreshDelegate)delegate { }); return; ;
        }

        public SplashWindow()
        {
            InitializeComponent();
        }

        public void Start()
        {
            Binding bindSplash = new Binding($"SplashContent");
            bindSplash.Mode = BindingMode.OneWay;
            bindSplash.Source = App.splashInfo;
            BindingOperations.SetBinding(status, ProgressBarExtension.ContentProperty, bindSplash);

            Binding bindSplash2 = new Binding($"SplashProgress");
            bindSplash2.Mode = BindingMode.OneWay;
            bindSplash2.Source = App.splashInfo;
            BindingOperations.SetBinding(status, ProgressBar.ValueProperty, bindSplash2);

            Closing += (sender, args) =>
                    AdonisWindow_Closing(sender, args);
            Show();
        }
        public void Refresh()
        {
            doRefresh(status);
        }
        public void Stop()
        {
            Dispatcher.ShutdownFinished += (sender, args) =>
                    Shutdown_Finished(sender, args);
            status.Dispatcher.InvokeShutdown();
            Dispatcher.InvokeShutdown();
        }
        private void Shutdown_Finished(object sender, System.EventArgs e)
        {
            Close();
        }

        private void AdonisWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Window swindow = sender as SplashWindow;
            e.Cancel = true;
            BindingOperations.ClearAllBindings(status);
            var anim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(2.5));
            anim.Completed += (s, _) => {
                doRefresh(status);
                while (status.Value < 100)
                {
                    App.wsleep(500);
                }
                App.wsleep(20000);
                swindow.Hide(); Stop(); };
            swindow.BeginAnimation(UIElement.OpacityProperty, anim);
        }
    }
}