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

        public static appConfigs pcurrent;
        public string WinTitle
        {
            get { return (string)GetValue(WinTitleProperty); }
            set { SetValue(WinTitleProperty, value); }
        }

        public static readonly DependencyProperty WinTitleProperty =
            DependencyProperty.Register("WinTitle", typeof(string), typeof(MainWindow), new UIPropertyMetadata($"CPUDoc-{App.version}", WinTitleChanged));
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

            pcurrent = App.AppConfigs[0];

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

            //pcurrent = new appConfigs();
            //appConfigs.Init();

            cbTBAutoStart.IsChecked = pcurrent.ThreadBooster ? true : false;
            cbNumaZero.IsChecked = pcurrent.NumaZero ? true : false;
            cbPSA.IsChecked = pcurrent.PowerSaverActive ? true : false;
            cbSysSetHack.IsChecked = pcurrent.SysSetHack ? true : false;
            cbTraceInfo.IsChecked = App.AppSettings.LogInfo ? true : false;
            cbTraceDebug.IsChecked = App.AppSettings.LogTrace ? true : false;
            cbAUNotifications.IsChecked = App.AppSettings.AUNotifications ? true : false;
            listNumaZeroType.SelectedIndex = pcurrent.NumaZeroType;
            cbPoolingRate.IsChecked = pcurrent.ManualPoolingRate ? true : false;
            listPoolingRate.SelectedIndex = pcurrent.PoolingRate;

            SSHStatus.Text = App.pactive.SysSetHack ? "Enabled" : "Disabled";
            PSAStatus.Text = App.pactive.PowerSaverActive ? "Enabled" : "Disabled";
            N0Status.Text = App.pactive.NumaZero ? "Enabled" : "Disabled";

            var gridLength1 = new GridLength(1.1, GridUnitType.Star);
            var gridLength2 = new GridLength(1, GridUnitType.Star);


            try
            {
                Grid _gridblock = current_cpumask;

                current_cpumask.HorizontalAlignment = HorizontalAlignment.Left;
                Thickness curcpugirmar = new Thickness(4, 4, 4, 4);
                current_cpumask.Margin = curcpugirmar;
                int _col = 0;

                int threads = ProcessorInfo.HardwareCores.Length;

                int _maxrow = threads > 7 ? 7 : threads;
                if (threads == 12 || threads == 24 || threads == 48) _maxrow = 5;
                if (threads == 8) _maxrow = 3;

                int _row = 0;

                Thickness curcpupad = new Thickness(1, 0, 1, 0);
                Thickness curcpumar = new Thickness(1, 2, 1, 2);

                for (int c = 0; c < threads; ++c)
                {
                    int len = 0;
                    int t0 = -1;
                    int t1 = -1;
                    if (c < ProcessorInfo.HardwareCores.Length)
                    {
                        len = ProcessorInfo.HardwareCores[c].LogicalCores.Length;
                        t0 = ProcessorInfo.HardwareCores[c].LogicalCores[0];
                        t1 = ProcessorInfo.HardwareCores[c].LogicalCores[1];
                    }
                    //App.LogDebug($"C{c} T0-{t0} T1-{t1}");
                    Button btnCore = new Button { VerticalAlignment = VerticalAlignment.Center, Content = $"C{c}", Padding = curcpupad, HorizontalAlignment = HorizontalAlignment.Right, Margin = curcpumar };
                    Button btnT0 = new Button { VerticalAlignment = VerticalAlignment.Center, Tag = $"{t0}", Content = $"T0", Padding = curcpupad, HorizontalAlignment = HorizontalAlignment.Right, Margin = curcpumar };
                    Button btnT1 = new Button { VerticalAlignment = VerticalAlignment.Center, Visibility = Visibility.Hidden, Tag = $"{t1}", Content = $"T1", Padding = curcpupad, HorizontalAlignment = HorizontalAlignment.Right, Margin = curcpumar };
                    Button btnSpace = new Button { VerticalAlignment = VerticalAlignment.Center, Visibility = Visibility.Hidden, Content = $"TX", Padding = curcpupad, HorizontalAlignment = HorizontalAlignment.Right, Margin = curcpumar };
                    App.LogDebug($"C{c} {_col} {_row} {threads}");
                    _gridblock.Children.Add(btnCore);
                    Grid.SetColumn(btnCore, _col);
                    Grid.SetRow(btnCore, _row);
                    if (len > 0)
                    {
                        _gridblock.Children.Add(btnT0);
                        Grid.SetColumn(btnT0, _col + 1);
                        Grid.SetRow(btnT0, _row);
                        _gridblock.Children.Add(btnT1);
                        Grid.SetColumn(btnT1, _col + 2);
                        Grid.SetRow(btnT1, _row);
                    }
                    _gridblock.Children.Add(btnSpace);
                    Grid.SetColumn(btnSpace, _col + 3);
                    Grid.SetRow(btnSpace, _row);
                    if (len > 1) btnT1.Visibility = Visibility.Visible;
                    _row++;
                    _col = _row > _maxrow ? _col + 4 : _col;
                    _row = _row > _maxrow ? 0 : _row;
                }

                if (cbTBAutoStart.IsChecked == true)
                {
                    cbNumaZero.IsEnabled = true;
                    cbSysSetHack.IsEnabled = true;
                    cbPSA.IsEnabled = true;
                    listNumaZeroType.IsEnabled = true;
                }
                else
                {
                    cbNumaZero.IsEnabled = false;
                    cbSysSetHack.IsEnabled = false;
                    cbPSA.IsEnabled = false;
                    listNumaZeroType.IsEnabled = false;
                }

                /*
                uint eax = 0;
                uint edx = 0;


                App.LogDebug($"MSR");
                if (App.ReadMsrTx(0xc001029a, ref eax, ref edx, 0))
                    App.LogDebug($"ENERGY={eax:X8} {edx:X8}");
                if (App.ReadMsr(0xc00102b2, ref eax, ref edx))
                    App.LogDebug($"CPPC ENABLE={eax:X8} {edx:X8}");
                */

            }
            catch (Exception ex)
            {
                App.LogDebug($"current cpu exception: {ex}");
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
        private static void WinTitleChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            MainWindow mainWindow = source as MainWindow;
            string newValue = e.NewValue as string;
            mainWindow.Title = newValue;
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

            this.Title = WinTitle;

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
                App.systemInfo.SetSSHStatus(App.pactive.SysSetHack);
                App.systemInfo.SetPSAStatus(App.pactive.PowerSaverActive);
                App.systemInfo.SetN0Status(App.pactive.NumaZero);

                currentcpu_update();

            };
        }

        public void currentcpu_update()
        {
            uint? _lastmask = App.lastSysCpuSetMask;
            for (int i = 0; i < ProcessorInfo.LogicalCoresCount; ++i)
            {
                //int _mask = i << _lastmask;
                uint? _mask = _lastmask;
                IEnumerable<Button> elements = FindVisualChildren<Button>(this).Where(x => x.Content == "T0" || x.Content == "T1");
                foreach (Button btn in elements)
                {
                    if (btn.Tag.ToString() == $"{i}")
                    {
                        if ((((1 << i) & _mask) != 0) || _mask == null || _mask == 0 || !App.pactive.ThreadBooster )
                        {
                            btn.Foreground = Brushes.White;
                            btn.Background = Brushes.Green;
                        }
                        else if (!(((1 << i) & _mask) != 0) && (App.n0disabledT0.Contains(i) || App.n0disabledT1.Contains(i)))
                        {
                            btn.Foreground = Brushes.DarkGray;
                            btn.Background = Brushes.Black;
                        }
                        else
                        {
                            btn.Foreground = Brushes.LightGray;
                            btn.Background = Brushes.DarkRed;
                        }
                    }
                }
            }
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
                pcurrent.ThreadBooster = true;
                cbNumaZero.IsEnabled = true;
                cbSysSetHack.IsEnabled = true;
                cbPSA.IsEnabled = true;
                listNumaZeroType.IsEnabled = true;
            }
            else
            {
                pcurrent.ThreadBooster = false;
                cbNumaZero.IsEnabled = false;
                cbSysSetHack.IsEnabled = false;
                cbPSA.IsEnabled = false;
                listNumaZeroType.IsEnabled = false;
            }
        }
        private void TraceDebugCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb.IsChecked == true)
            {
                App.AppSettings.LogTrace = true;
            }
            else
            {
                App.AppSettings.LogTrace = false;
            }
            App.TraceLogging(App.AppSettings.LogTrace);
            ProtBufSettings.WriteSettings();
        }
        private void AUNotificationsCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb.IsChecked == true)
            {
                App.AppSettings.AUNotifications = true;
            }
            else
            {
                App.AppSettings.AUNotifications = false;
            }
            App.AUNotifications(App.AppSettings.AUNotifications);
            ProtBufSettings.WriteSettings();
        }
        private void TraceInfoCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb.IsChecked == true)
            {
                App.AppSettings.LogInfo = true;
            }
            else
            {
                App.AppSettings.LogInfo = false;
            }
            App.InfoLogging(App.AppSettings.LogInfo);
            ProtBufSettings.WriteSettings();
        }
        private void SysSetHack_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb.IsChecked == true)
            {
                pcurrent.SysSetHack = true;
            }
            else
            {
                pcurrent.SysSetHack = false;
            }
        }
        private void PSA_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb.IsChecked == true)
            {
                pcurrent.PowerSaverActive = true;
            }
            else
            {
                pcurrent.PowerSaverActive = false;
            }
        }
        private void NumaZero_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb.IsChecked == true)
            {
                pcurrent.NumaZero = true;
            }
            else
            {
                pcurrent.NumaZero = false;
            }
        }
        private void PoolingRate_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb.IsChecked == true)
            {
                pcurrent.ManualPoolingRate = true;
            }
            else
            {
                pcurrent.ManualPoolingRate = false;
            }
        }
        private void NumaZeroType_Select(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            //App.LogDebug($"NumaZeroType {pcurrent.NumaZeroType} {pcurrent.NumaZeroType.GetType()}");
            if (pcurrent != null) pcurrent.NumaZeroType = cb.SelectedIndex;
        }
        private void PoolingRate_Select(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            //App.LogDebug($"PoolingRate {pcurrent.PoolingRate} {pcurrent.PoolingRate.GetType()}");
            if (pcurrent != null) pcurrent.PoolingRate = cb.SelectedIndex;
        }
        private void SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            //App.LogInfo($"NumaZeroType Index={cb.SelectedIndex} {App.pactive.NumaZeroType} P0={App.AppConfigs[0].NumaZeroType}");
            //App.LogInfo($"NumaZeroType {App.pactive.NumaZeroType} P0={App.AppConfigs[0].NumaZeroType}");
            //App.AppConfigs[pcurrent.id] = pcurrent;
            App.AppConfigs[0] = pcurrent;
            App.SetActiveConfig(pcurrent.id);
            ProtBufSettings.WriteSettings();
            //App.LogDebug($"Wr Config[{App.pactive.id}] TBA={App.pactive.ThreadBooster} SSH={App.AppConfigs[0].SysSetHack}:{pcurrent.SysSetHack}:{App.pactive.SysSetHack} PSA={App.pactive.PowerSaverActive} N0={App.pactive.NumaZero}:{App.pactive.NumaZeroType}");
            ProtBufSettings.ReadSettings();
            //App.LogDebug($"Rd Config[{App.pactive.id}] TBA={App.AppConfigs[App.pactive.id].ThreadBooster} SSH={App.AppConfigs[App.pactive.id].SysSetHack}:{App.pactive.SysSetHack} PSA={App.AppConfigs[App.pactive.id].PowerSaverActive} N0={App.pactive.NumaZero}:{App.AppConfigs[App.pactive.id].NumaZero}:{App.AppConfigs[App.pactive.id].NumaZeroType}");
        }
        private void ResetSettings_Click(object sender, RoutedEventArgs e)
        {
            //App.LogInfo($"NumaZeroType Index={cb.SelectedIndex} {App.pactive.NumaZeroType} P0={App.AppConfigs[0].NumaZeroType}");
            //App.LogInfo($"NumaZeroType {App.pactive.NumaZeroType} P0={App.AppConfigs[0].NumaZeroType}");
            //App.AppConfigs[pcurrent.id] = pcurrent;
            ProtBufSettings.ResetSettings();
            ProtBufSettings.ReadSettings();
            App.SetActiveConfig(0);
            ProtBufSettings.WriteSettings();
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

                td.Actions.Add(new ExecAction(System.IO.Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), null, AppDomain.CurrentDomain.BaseDirectory));

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

        private void AdonisWindow_Activated(object sender, EventArgs e)
        {
            App.systemInfo.RefreshLabels();
        }
    }
}
