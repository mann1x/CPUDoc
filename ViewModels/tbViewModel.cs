using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace CPUDoc
{
    /// <summary>
    /// Provides bindable properties and commands for the NotifyIcon. In this sample, the
    /// view model is assigned to the NotifyIcon in XAML. Alternatively, the startup routing
    /// in App.xaml.cs could have created this view model, and assigned it to the NotifyIcon.
    /// </summary>
    public class tbViewModel
    {
        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
               ? Application.Current.Windows.OfType<T>().Any()
               : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }

        /// <summary>
        /// Shows a window, if none is already open.
        /// </summary>
        public ICommand ShowWindowCommand => new DelegateCommand
        {
            //CanExecuteFunc = () => Application.Current.MainWindow == null,
            CanExecuteFunc = () =>
            {
                return true;
            },
            CommandAction = () =>
            {
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
            }
        };

        /// <summary>
        /// Hides the main window. This command is only enabled if a window is open.
        /// </summary>
        public ICommand HideWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () =>
                    {
                        if (Application.Current.MainWindow != null)
                        {
                            if (Application.Current.MainWindow.IsActive) return true;
                        }
                        return false;
                    },
                    CommandAction = () => Application.Current.MainWindow.Close()
                };
            }
        }

        /// <summary>
        /// Toggle NumaZero
        /// </summary>
        public ICommand ToggleNumaZero
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () =>
                    {
                        return true;
                    },
                    CommandAction = () =>
                    {
                        App.pactive.NumaZero = (App.pactive.NumaZero) ? false : true;
                        App.SetActiveConfig();
                        App.LogDebug($"N0 Toggle: {App.pactive.NumaZero}");
                    }
                    
                };
            }
        }

        /// <summary>
        /// Toggle PSA
        /// </summary>
        public ICommand TogglePSA
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () =>
                    {
                        return true;
                    },
                    CommandAction = () =>
                    {
                        App.pactive.PowerSaverActive = (App.pactive.PowerSaverActive) ? false : true;
                        App.SetActiveConfig();
                        App.LogDebug($"PSA Toggle: {App.pactive.PowerSaverActive}");
                    }

                };
            }
        }

        /// <summary>
        /// Toggle SysSetHack
        /// </summary>
        public ICommand ToggleSSH
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () =>
                    {
                        return true;
                    },
                    CommandAction = () =>
                    {
                        App.pactive.SysSetHack = (App.pactive.SysSetHack) ? false : true;
                        App.SetActiveConfig();
                        App.LogDebug($"SysSetHack Toggle: {App.pactive.SysSetHack}");
                    }

                };
            }
        }

        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get
            {
                return new DelegateCommand { CommandAction = () => Application.Current.Shutdown() };
            }
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
}