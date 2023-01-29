using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace CPUDoc.Windows
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow
    {
        //internal static readonly AppSettings appSettings = (Application.Current as App)?.settings;
        //internal static readonly Updater updater = (Application.Current as App)?.updater;
        public static readonly SplashWindow splash = new SplashWindow();

        // To refresh the UI immediately
        private delegate void RefreshDelegate();
        private static void Refresh(DependencyObject obj)
        {
            obj.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render,
                (RefreshDelegate) delegate { });
        }

        public SplashWindow()
        {
            InitializeComponent();
        }

        public static void Start()
        {
            splash.Show();
        }

        public static void Stop() => splash.Close();

        public static void Loading(int progress)
        {
            splash.status.Value = progress;
            Refresh(splash.status);
        }

        private void AdonisWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Window window = sender as Window;
            e.Cancel = true;
            var anim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(1.5));
            anim.Completed += (s, _) => { window.Hide(); window.Close(); };
            window.BeginAnimation(UIElement.OpacityProperty, anim);
        }
    }
}