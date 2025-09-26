using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using H.NotifyIcon;

namespace CPUDoc;

/// <summary>
/// Provides bindable properties and commands for the NotifyIcon. In this sample, the
/// view model is assigned to the NotifyIcon in XAML. Alternatively, the startup routing
/// in App.xaml.cs could have created this view model, and assigned it to the NotifyIcon.
/// </summary>
public partial class NotifyIconViewModel : ObservableObject
{

    /// <summary>
    /// Shows a window, if none is already open.
    /// </summary>
    [RelayCommand()]
    public void ShowWindow()
    {
        Application.Current.MainWindow ??= new MainWindow();
        /*
        if (!IsWindowOpen<MainWindow>())
        {
            Application.Current.MainWindow = new MainWindow();

            Application.Current.MainWindow.DataContext = new
            {
                settings = CPUDoc.Properties.Settings.Default,
                App.systemInfo,
            };

        }
        if (Application.Current.MainWindow.WindowState != WindowState.Normal) Application.Current.MainWindow.WindowState = WindowState.Normal;
        Application.Current.MainWindow.Show();
        Application.Current.MainWindow.Focus();
        Application.Current.MainWindow.Activate();
        */
    }
    /// <summary>
    /// Toggle SysSetHack
    /// </summary>
    [RelayCommand]
    public void ToggleSSH()
    {
        App.pactive.SysSetHack = (App.pactive.SysSetHack) ? false : true;
        App.SetActiveConfig();
        App.LogDebug($"SysSetHack Toggle: {App.pactive.SysSetHack}");
    }
    /// <summary>
    /// Toggle PSA
    /// </summary>
    [RelayCommand]
    public void TogglePSA()
    {
        App.pactive.PowerSaverActive = (App.pactive.PowerSaverActive) ? false : true;
        App.SetActiveConfig();
        App.LogDebug($"PSA Toggle: {App.pactive.PowerSaverActive}");
    }
    /// <summary>
    /// Toggle NumaZero
    /// </summary>
    [RelayCommand]
    public void ToggleNumaZero()
    {
        App.pactive.NumaZero = (App.pactive.NumaZero) ? false : true;
        App.SetActiveConfig();
        App.LogDebug($"N0 Toggle: {App.pactive.NumaZero}");
    }
    /// <summary>
    /// Shuts down the application.
    /// </summary>
    [RelayCommand]
    public void ExitApplication()
    {
        Application.Current.Shutdown();
    }

    public static bool IsWindowOpen<T>(string name = "") where T : Window
    {
        return string.IsNullOrEmpty(name)
           ? Application.Current.Windows.OfType<T>().Any()
           : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
    }

    public string Version
    {
        get
        {
            Version _version = Assembly.GetExecutingAssembly().GetName().Version;
            return string.Format("CPUDoc v{0}.{1}.{2}", _version.Major, _version.Minor, _version.Build);
        }
    }
}