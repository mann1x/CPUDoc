using System;
using System.Drawing;
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
        /// <summary>
        /// Shows a window, if none is already open.
        /// </summary>
        public ICommand ShowWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => Application.Current.MainWindow == null,
                    CommandAction = () =>
                    {
                        Application.Current.MainWindow = new MainWindow();

                        Application.Current.MainWindow.DataContext = new
                        {
                            settings = CPUDoc.Properties.Settings.Default,
                            App.systemInfo
                        };

                        Application.Current.MainWindow.Show();
                        Application.Current.MainWindow.Focus();
                    }
                };
            }
        }

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
                    CommandAction = () => Application.Current.MainWindow.Close(),
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