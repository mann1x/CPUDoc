using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.Intrinsics.X86;
using System.Web;
using System.Reflection;
using AutoUpdaterDotNET;
using Newtonsoft.Json;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32.TaskScheduler;

namespace CPUDoc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    using WindowSettings = Properties.Settings;
    public partial class MainWindow
    {
        static bool InitUI = true;
        static bool WinLoaded = false;
        static DispatcherTimer uitimer;
        static bool AutoStartTask;

        public string WinTitle
        {
            get { return (string)GetValue(WinTitleProperty); }
            set { SetValue(WinTitleProperty, value); }
        }

        public static readonly DependencyProperty WinTitleProperty =
            DependencyProperty.Register("WinTitle", typeof(string), typeof(MainWindow), new UIPropertyMetadata($"CPUDoc-{App.version}"));
        public MainWindow()
        {
            InitializeComponent();
        }
        private void TextBox_KeyEnterUpdate(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tBox = (TextBox)sender;
                DependencyProperty prop = TextBox.TextProperty;

                BindingExpression binding = BindingOperations.GetBindingExpression(tBox, prop);
                if (binding != null) { binding.UpdateSource(); }
            }
        }
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            App.LogDebug($"SourceInit Window Initialized {WindowSettings.Default.Initialized}");
            App.systemInfo.WinMaxSize = System.Windows.SystemParameters.WorkArea.Height;

            if (WindowSettings.Default.Initialized)
            {
                App.LogDebug($"Restoring Window Position {WindowSettings.Default.Top} {WindowSettings.Default.Left} {WindowSettings.Default.Height} {WindowSettings.Default.Width} {WindowSettings.Default.Maximized}");
                App.LogDebug($"Restoring Window WorkArea {SystemParameters.WorkArea.Top} {SystemParameters.WorkArea.Left} {SystemParameters.WorkArea.Height} {SystemParameters.WorkArea.Width}");

                WindowState = WindowState.Normal;
                Top = WindowSettings.Default.Top < SystemParameters.WorkArea.Top ? SystemParameters.WorkArea.Top : WindowSettings.Default.Top;
                Left = WindowSettings.Default.Left < SystemParameters.WorkArea.Left ? SystemParameters.WorkArea.Left : WindowSettings.Default.Left;
                Height = WindowSettings.Default.Height > SystemParameters.WorkArea.Height ? SystemParameters.WorkArea.Height : WindowSettings.Default.Height;
                Width = WindowSettings.Default.Width > SystemParameters.WorkArea.Width ? SystemParameters.WorkArea.Width : WindowSettings.Default.Width;
                if (WindowSettings.Default.Maximized)
                {
                    WindowState = WindowState.Maximized;
                }
            }
            else
            {
                double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
                double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
                double windowWidth = this.Width;
                double windowHeight = this.Height;
                this.Left = (screenWidth / 2) - (windowWidth / 2);
                this.Top = (screenHeight / 2) - (windowHeight / 2);
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                App.LogDebug($"SizeChanged Set Center and Save");
                WindowSettings.Default.Initialized = true;
                SaveWinPos();
            }

            SizeToContent = SizeToContent.WidthAndHeight;
            SetValue(MinWidthProperty, ActualWidth);
            SetValue(MinHeightProperty, ActualHeight);
            ClearValue(SizeToContentProperty);

            if (App.tbtimer.Enabled) BtnThreadBoostLabel.Text = "Stop";

            if (WindowSettings.Default.ThreadBooster == true)
            {
                cbTBAutoStart.IsChecked = true;
            }
            else
            {
                cbTBAutoStart.IsChecked = false;
            }
            AutoStartTask = CheckStartTask();

            if (AutoStartTask) BtnAutoStartTaskLabel.Text = "Delete AutoStart Task";

            App.uitimer.Enabled = true;

            WinLoaded = true;
        }

        private void Window_SizeChanged(object sender, EventArgs e)
        {
            //App.LogDebug($"SizeChanged Window Initialized {WindowSettings.Default.Initialized}");
            if (WindowSettings.Default.Initialized && WinLoaded)
            {
                SaveWinPos();
            }
            UpdateLayout();
        }
        private void SaveWinPos()
        {
            if (this.IsActive)
            {
                if (WindowState == WindowState.Maximized)
                {
                    WindowSettings.Default.Top = RestoreBounds.Top;
                    WindowSettings.Default.Left = RestoreBounds.Left;
                    WindowSettings.Default.Height = RestoreBounds.Height;
                    WindowSettings.Default.Width = RestoreBounds.Width;
                    WindowSettings.Default.Maximized = true;
                }
                else
                {
                    WindowSettings.Default.Top = Top;
                    WindowSettings.Default.Left = Left;
                    WindowSettings.Default.Height = Height;
                    WindowSettings.Default.Width = Width;
                    WindowSettings.Default.Maximized = false;
                }
                WindowSettings.Default.Save();
                App.LogDebug($"Saving Window Position {WindowSettings.Default.Top} {WindowSettings.Default.Left} {WindowSettings.Default.Height} {WindowSettings.Default.Width} {WindowSettings.Default.Maximized}");
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsActive && WindowSettings.Default.Initialized)
            {
                SaveWinPos();
                App.LogDebug($"Saved Window Position Closing {WindowSettings.Default.Top} {WindowSettings.Default.Left} {WindowSettings.Default.Height} {WindowSettings.Default.Width} {WindowSettings.Default.Maximized}");
            }
            //App.uitimer.Enabled = false;
            uitimer.Stop();
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
        public static IEnumerable<T> FindVisualParent<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(depObj);
                if (parent != null && parent is T)
                {
                    yield return (T)parent;
                }

                foreach (T parentOfparent in FindVisualParent<T>(parent))
                {
                    yield return parentOfparent;
                }
            }
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitMainUI();

            string LicensePath = @".\LICENSE";
            if (File.Exists(LicensePath))
            {
                boxLicense.Text = File.ReadAllText(LicensePath);
            }

            UpdateLayout();

            Activate();
            Focus();

            App.systemInfo.SetThreadBoosterStatus("N/A");

            uitimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            uitimer.Start();
            uitimer.Tick += (sender, args) =>
            {
                if (App.tbtimer.Enabled)
                {
                    App.systemInfo.SetThreadBoosterStatus("Running");

                }
                else
                {
                    App.systemInfo.SetThreadBoosterStatus("Stopped");
                }

            };
        }

        private void EcoresMode(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.IsChecked == true)
            {
                if ((string)rb.Tag == "Disabled")
                {
                    App.systemInfo.bECores = false;
                    WindowSettings.Default.ECores = false;
                }
                else
                {
                    App.systemInfo.bECores = true;
                    WindowSettings.Default.ECores = true;
                }
                WindowSettings.Default.Save();
                App.LogDebug($"RB CHECKED {rb.Name} {rb.Tag} SETTINGS BtnECores {WindowSettings.Default.ECores}");
            }

        }
        /*
        private void CheckThreads(object sender, RoutedEventArgs e)
        {
            ToggleButton cb = sender as ToggleButton;

            string Tag = (string)cb.Tag;

            if (cb.IsChecked == true)
            {
                if (!WindowSettings.Default.Threads.Contains(Tag)) WindowSettings.Default.Threads.Add(Tag);
                App.LogDebug($"CB CHECKED {cb.Tag}");
                if (!InitUI)
                {
                    RadioCustom.IsChecked = true;
                    RadioSTMT.IsChecked = false;
                    App.systemInfo.STMT = false;
                    WindowSettings.Default.BtnSTMT = false;
                }
            }
            else
            {
                if (WindowSettings.Default.Threads.Contains(Tag)) WindowSettings.Default.Threads.Remove(Tag);
                App.LogDebug($"CB UNCHECKED {cb.Tag}");
                RadioCustom.IsChecked = true;
                RadioSTMT.IsChecked = false;
                App.systemInfo.STMT = false;
                WindowSettings.Default.BtnSTMT = false;
            }
            WindowSettings.Default.Save();

        }
        private void CheckCustomCPPC(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            App.LogDebug($"CUSTOMCPPC {WindowSettings.Default.CustomCPPC}");

            if (cb.IsChecked == true)
            {
                WindowSettings.Default.cbCustomCPPC = true;
                App.LogDebug($"CB CHECKED {cb.Tag}");
                App.systemInfo.CPPCActiveOrder = App.systemInfo.CPPCCustomOrder;
                App.systemInfo.CPPCActiveLabel = App.GetCustomLabel();
                CPPCActiveLabel.Text = App.GetCustomLabel();
                App.LogDebug($"LABEL {App.GetCustomLabel()} {App.systemInfo.CPPCActiveLabel}");
            }
            else
            {
                WindowSettings.Default.cbCustomCPPC = false;
                App.LogDebug($"CB UNCHECKED {cb.Tag}");
                App.systemInfo.CPPCActiveOrder = App.systemInfo.CPPCOrder;
                App.systemInfo.CPPCActiveLabel = App.systemInfo.CPPCLabel;
                CPPCActiveLabel.Text = App.systemInfo.CPPCLabel;
                App.LogDebug($"LABEL {App.systemInfo.CPPCLabel} {App.systemInfo.CPPCActiveLabel}");
            }
            WindowSettings.Default.Save();

        }
        */
        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
               ? Application.Current.Windows.OfType<T>().Any()
               : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }
        
        /*
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();
        }
        */

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = e.Uri.ToString(),
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                App.LogDebug($"Hyperlink_RequestNavigate Exception ({e.Uri}): {ex}");
            }
        }

        /*
        private void CustomCPPC_Save(object sender, RoutedEventArgs e)
        {

            int i = 0;
            foreach (ListBoxItem _core in CustomCPPC.Items)
            {
                App.systemInfo.CPPCCustomOrder[i] = Convert.ToInt32(_core.Tag);
                i++;
                App.LogDebug($"1 = {_core.Tag}");
            }

            WindowSettings.Default.CustomCPPC = App.GetCustomLabel();
            WindowSettings.Default.Save();

            if (cbCustomCPPC.IsChecked == true)
            {
                App.systemInfo.CPPCActiveOrder = App.systemInfo.CPPCCustomOrder;
                App.systemInfo.CPPCActiveLabel = App.GetCustomLabel();
                CPPCActiveLabel.Text = App.GetCustomLabel();
                App.LogDebug($"LABEL {App.GetCustomLabel()} {App.systemInfo.CPPCActiveLabel}");
            }

            App.LogDebug($"CUSTOMCPPC {WindowSettings.Default.CustomCPPC}");
            App.LogDebug($"CUSTOMLABEL {App.GetCustomLabel()}");
        }
        */
       
        private void ButtonCheckUpdate(object sender, RoutedEventArgs e)
        {
            AutoUpdater.Start(App.AutoUpdaterUrl);
        }
        private void ButtonReset(object sender, RoutedEventArgs e)
        {

            WindowSettings.Default.Reset();

            App.SettingsInit();

            InitMainUI();

        }
        private void InitMainUI()
        {

            InitUI = true;

            InitUI = false;
        }

        /*
        private void RestoreCustomCPPC()
        {
            try
            {
                Thickness coreborder = new Thickness(1, 1, 1, 1);
                Thickness coremargin = new Thickness(3, 3, 3, 3);

                foreach (string thr in WindowSettings.Default.Threads)
                {
                    App.LogDebug($" RESTORING CB {thr}");
                    IEnumerable<CheckBox> elements = FindVisualChildren<CheckBox>(this).Where(x => x.Tag != null && x.Tag.ToString() == thr.ToString());
                    foreach (CheckBox cb in elements)
                    {
                        cb.IsChecked = true;
                    }
                }

                CustomCPPC.Items.Clear();

                foreach (int _core in App.systemInfo.CPPCCustomOrder)
                {
                    string _strcore = _core.ToString();
                    CustomCPPC.Items.Add(new ListBoxItem { Tag = _strcore, Content = _strcore, BorderBrush = Brushes.Lavender, BorderThickness = coreborder, Margin = coremargin });
                }

                if (WindowSettings.Default.cbCustomCPPC)
                {
                    cbCustomCPPC.IsChecked = true;
                    App.systemInfo.CPPCActiveOrder = App.systemInfo.CPPCCustomOrder;
                    App.systemInfo.CPPCActiveLabel = App.GetCustomLabel();
                }
                else
                {
                    cbCustomCPPC.IsChecked = false;
                    App.systemInfo.CPPCActiveOrder = App.systemInfo.CPPCOrder;
                    App.systemInfo.CPPCActiveLabel = App.systemInfo.CPPCLabel;
                }

            }
            catch (Exception ex)
            {
                App.LogDebug($"RestorCustomCPPC exception: {ex}");
            }
        }

        */

        public static void CenterWindowTopScreen(Window _this)
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double windowWidth = _this.Width;
            _this.Left = (screenWidth / 2) - (windowWidth / 2);
            _this.Top = SystemParameters.WorkArea.Top;
        }
        private void btnRefreshInfo_Click(object sender, RoutedEventArgs e)
        {
            if (!object.ReferenceEquals(null, App.systemInfo.Zen))
            {
                App.systemInfo.ZenRefreshStatic(true);
                App.systemInfo.ZenRefreshCO();
            }
            App.systemInfo.RefreshLabels();
        }
        private void TabItemEnter(object sender, RoutedEventArgs e)
        {
            SizeToContent = SizeToContent.WidthAndHeight;
            SetValue(MinWidthProperty, ActualWidth);
            SetValue(MinHeightProperty, ActualHeight);
            ClearValue(SizeToContentProperty);
        }
        private void ButtonLogsFolder(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", @".\Logs");
        }

        private void BtnThreadBoost_Click(object sender, RoutedEventArgs e)
        {
            if (BtnThreadBoostLabel.Text == "Start")
            {
                BtnThreadBoost.IsEnabled = false;
                App.TbSetStart();
                BtnThreadBoostLabel.Text = "Stop";
                BtnThreadBoost.IsEnabled = true;
            }
            else
            {
                BtnThreadBoost.IsEnabled = false;
                App.TbSetStart(false);
                BtnThreadBoostLabel.Text = "Start";
                BtnThreadBoost.IsEnabled = true;
            }
        }

        private void TBAutoStartCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb.IsChecked == true)
            {
                CPUDoc.Properties.Settings.Default.ThreadBooster = true;
            }
            else
            {
                CPUDoc.Properties.Settings.Default.ThreadBooster = false;
            }
            CPUDoc.Properties.Settings.Default.Save();
        }
        private void BtnAutoStartTask_Click(object sender, RoutedEventArgs e)
        {
            if (AutoStartTask)
            {
                DeleteStartTask();
                BtnAutoStartTaskLabel.Text = "Create AutoStart Task";
                AutoStartTask = false;
            }
            else
            {
                CreateStartTask();
                BtnAutoStartTaskLabel.Text = "Delete AutoStart Task";
                AutoStartTask = true;
            }
        }
        private void CreateStartTask()
        {
            if (!CheckStartTask())
            {
                TaskDefinition td = TaskService.Instance.NewTask();
                td.RegistrationInfo.Description = "CPUDoc AutoStart";
                td.Principal.LogonType = TaskLogonType.InteractiveToken;
                td.Principal.RunLevel = TaskRunLevel.Highest;

                LogonTrigger lTrigger = (LogonTrigger)td.Triggers.Add(new LogonTrigger());

                td.Actions.Add(new ExecAction(System.IO.Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), AppDomain.CurrentDomain.BaseDirectory, null));

                // Register the task in the root folder
                const string taskName = "CPUDoc AutoStart";
                TaskService.Instance.RootFolder.RegisterTaskDefinition(taskName, td);
            }

        }
        private void DeleteStartTask()
        {
            const string taskName = "CPUDoc AutoStart";
            if (CheckStartTask())
            {
                TaskDefinition td = TaskService.Instance.FindTask(taskName).Definition;
                if (td == null) return;
                TaskFolder tf = TaskService.Instance.RootFolder;
                tf.DeleteTask(taskName);
            }
        }
        private bool CheckStartTask()
        {
            try
            {
                const string taskName = "CPUDoc AutoStart";
                using (TaskService ts = new TaskService())
                {
                    Microsoft.Win32.TaskScheduler.Task task = ts.GetTask(taskName);
                    return task != null;
                }
            }
            catch (Exception ex)
            {
                App.LogInfo($"CheckStartTask Exception: {ex}");
                return false;
            }
        }
    }
}
