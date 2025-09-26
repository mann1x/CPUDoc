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
using System.Globalization;
using System.Text.RegularExpressions;
using net.r_eg.Conari.Core;
using Windows.ApplicationModel.Store;
using net.r_eg.Conari;
using Config.Net;

namespace CPUDoc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    using WindowSettings = Properties.Settings;
    public partial class MainWindow
    {
        //static bool InitUI = true;
        static bool WinLoaded = false;
        static DispatcherTimer uitimer;
        static bool AutoStartTask;
        private ulong? _lastmask_current = ulong.MaxValue;
        private int PoolingTick = 0;

        public static IAppConfig pcurrent = new ConfigurationBuilder<IAppConfig>()
                    .UseInMemoryDictionary()
                    .Build();
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

        private static void pcurrentLoad(int id)
        {
            try
            {
                App.AppConfigs[id].CopyPropertiesTo(pcurrent);
            }
            catch (Exception ex)
            {
                App.LogDebug($"pcurrentLoad exception: {ex}");
            }
        }
        private static void pcurrentSave(int id)
        {
            try
            {
                pcurrent.CopyPropertiesTo(App.AppConfigs[id]);
            }
            catch (Exception ex)
            {
                App.LogDebug($"pcurrentSave exception: {ex}");
            }
        }
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            try
            {
                App.LogDebug($"SourceInit Window Initialized {WindowSettings.Default.Initialized}");
                App.systemInfo.WinMaxSize = System.Windows.SystemParameters.WorkArea.Height;
                
                //pcurrent = new ConfigurationBuilder<IAppConfig>()
                //    .UseInMemoryDictionary()
                //    .Build();

                App.LogDebug($"SourceInit Window Initialized 2");

                pcurrentLoad(0);

                App.LogDebug($"SourceInit Window Initialized 3");

                if (WindowSettings.Default.Initialized)
                {
                    //App.LogDebug($"Restoring Window Position {WindowSettings.Default.Top} {WindowSettings.Default.Left} {WindowSettings.Default.Height} {WindowSettings.Default.Width} {WindowSettings.Default.Maximized}");
                    //App.LogDebug($"Restoring Window WorkArea {SystemParameters.WorkArea.Top} {SystemParameters.WorkArea.Left} {SystemParameters.WorkArea.Height} {SystemParameters.WorkArea.Width}");

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
//                    App.LogDebug($"SizeChanged Set Center and Save");
                    WindowSettings.Default.Initialized = true;
                    this.UpdateLayout();
                    SaveWinPos();
                }

                InitWindowUI();

                //pcurrent = new appConfigs();
                //appConfigs.Init();

                Create_CpuDisplay();

                currentcpu_update();

                /*
                uint eax = 0;
                uint edx = 0;


                App.LogDebug($"MSR");
                if (App.ReadMsrTx(0xc001029a, ref eax, ref edx, 0))
                    App.LogDebug($"ENERGY={eax:X8} {edx:X8}");
                if (App.ReadMsr(0xc00102b2, ref eax, ref edx))
                    App.LogDebug($"CPPC ENABLE={eax:X8} {edx:X8}");
                */

                this.UpdateLayout();
                SaveWinPos();

                AutoStartTask = CheckStartTask();

                /*
                App.AppSettings.PropertyChanged += (sender, e) =>
                {
                    App.LogDebug($"Changed {e.PropertyName}");
                };
                */

            }
            catch (Exception ex)
            {
                App.LogDebug($"Window_SourceInitialized exception: {ex}");
            }
            finally
            {
                if (AutoStartTask) BtnAutoStartTaskLabel.Text = "Delete AutoStart Task";

                SizeToContent = SizeToContent.WidthAndHeight;

                SetValue(MinWidthProperty, ActualWidth);
                SetValue(MinHeightProperty, ActualHeight);

                WinLoaded = true;
            }

        }
        private void InitWindowUI()
        {
            App.systemInfo.SetSleepsAllowed();

            cbTBAutoStart.IsChecked = pcurrent.ThreadBooster ? true : false;
            cbNumaZero.IsChecked = pcurrent.NumaZero ? true : false;
            cbPSA.IsChecked = pcurrent.PowerSaverActive ? true : false;
            cbZC.IsChecked = pcurrent.ZenControl ? true : false;
            cbSysSetHack.IsChecked = pcurrent.SysSetHack ? true : false;
            cbTraceLog.IsChecked = App.AppSettings.LogTrace ? true : false;
            cbInpoutdll.IsChecked = App.AppSettings.inpoutdlldisable ? true : false;
            cbAUNotifications.IsChecked = App.AppSettings.AUNotifications ? true : false;
            listNumaZeroType.SelectedIndex = (int)pcurrent.NumaZeroType;
            cbPoolingRate.IsChecked = pcurrent.ManualPoolingRate ? true : false;
            listPoolingRate.SelectedIndex = (int)pcurrent.PoolingRate;

            PowerTweak_LowPo.IsChecked = (pcurrent.PowerTweak) == 0 ? true : false;
            PowerTweak_AutoPo.IsChecked = (pcurrent.PowerTweak) == 1 ? true : false;
            PowerTweak_HighPo.IsChecked = (pcurrent.PowerTweak) == 2 ? true : false;
            App.LogDebug($"Personality? {pcurrent.Personality}");
            Persona_Auto.IsChecked = (pcurrent.Personality) == 0;
            Persona_Bala.IsChecked = (pcurrent.Personality) == 1;
            Persona_Ulti.IsChecked = (pcurrent.Personality) == 2;
            AMBias_Auto.IsChecked = pcurrent.ActiveModeBias == -1 ? true : false;
            AMBias_Economizer.IsChecked = pcurrent.ActiveModeBias == 0 ? true : false;
            AMBias_Standard.IsChecked = pcurrent.ActiveModeBias == 1 ? true : false;
            AMBias_Booster.IsChecked = pcurrent.ActiveModeBias == 2 ? true : false;
            GMBias_Auto.IsChecked = pcurrent.GameModeBias == -1 ? true : false;
            GMBias_Economizer.IsChecked = pcurrent.GameModeBias == 0 ? true : false;
            GMBias_Standard.IsChecked = pcurrent.GameModeBias == 1 ? true : false;
            GMBias_Booster.IsChecked = pcurrent.GameModeBias == 2 ? true : false;

            listSleepIdle.SelectedIndex = (pcurrent.SleepIdle);
            listMonitorIdle.SelectedIndex = pcurrent.MonitorIdle;
            listHyberIdle.SelectedIndex = (pcurrent.HyberIdle);
            App.LogDebug($"MonitorIdle? {listMonitorIdle.SelectedIndex}={(int)pcurrent.MonitorIdle}");
            cbWakeTimers.IsChecked = pcurrent.WakeTimers ? true : false;
            cbPSALightSleep.IsChecked = pcurrent.PSALightSleep ? true : false;
            cbPSADeepSleep.IsChecked = pcurrent.PSADeepSleep ? true : false;
            cbPSAAudioBlocksLightSleep.IsChecked = pcurrent.PSAAudioBlocksLightSleep ? true : false;
            cbPSAAudioBlocksDeepSleep.IsChecked = pcurrent.PSAAudioBlocksDeepSleep ? true : false;
            cbZCCPUTemp.IsChecked = pcurrent.ZenControlCPUTempTrayIcon ? true : false;

            GMDetect.IsChecked = pcurrent.GameMode ? true : false;
            GMFocusAssist.IsChecked = pcurrent.FocusAssist ? true : false;
            GMUserNotification.IsChecked = pcurrent.UserNotification ? true : false;
            GMFGSecondary.IsChecked = pcurrent.SecondaryMonitor ? true : false;
            cbPLPerfMode.IsChecked = pcurrent.PLPerfMode ? true : false;

            SSHStatus.Text = App.pactive.SysSetHack ? "Enabled" : "Disabled";
            PSAStatus.Text = App.pactive.PowerSaverActive ? "Enabled" : "Disabled";
            N0Status.Text = App.pactive.NumaZero ? "Enabled" : "Disabled";

            cbPPT.IsChecked = pcurrent.ZenControlPPTAuto ? true : false;
            cbTDC.IsChecked = pcurrent.ZenControlTDCAuto ? true : false;
            cbEDC.IsChecked = pcurrent.ZenControlEDCAuto ? true : false;

            PPThpx.IsEnabled = !cbPPT.IsChecked == true;
            TDChpx.IsEnabled = !cbTDC.IsChecked == true;
            EDChpx.IsEnabled = !cbEDC.IsChecked == true;

            if (App.systemInfo.ZenStates)
            {
                if (App.systemInfo.ZenMaxPPT > 0) cbPPT.Content = App.systemInfo.ZenMaxPPT > 0 ? $"Auto PPT (Max: {App.systemInfo.ZenMaxPPT})" : $"Auto PPT";
                if (App.systemInfo.ZenMaxTDC > 0) cbTDC.Content = App.systemInfo.ZenMaxTDC > 0 ? $"Auto TDC (Max: {App.systemInfo.ZenMaxTDC})" : $"Auto TDC";
                if (App.systemInfo.ZenMaxEDC > 0) cbEDC.Content = App.systemInfo.ZenMaxEDC > 0 ? $"Auto EDC (Max: {App.systemInfo.ZenMaxEDC})" : $"Auto EDC";
            }

            if (pcurrent.ZenControlPPThpx.ToString() == "" || pcurrent.ZenControlPPThpx < 1) pcurrent.ZenControlPPThpx = App.systemInfo.ZenMaxPPT > 0 ? App.systemInfo.ZenMaxPPT : App.systemInfo.ZenPPT > 0 ? App.systemInfo.ZenPPT : 142;
            if (pcurrent.ZenControlTDChpx.ToString() == "" || pcurrent.ZenControlTDChpx < 1) pcurrent.ZenControlTDChpx = App.systemInfo.ZenMaxTDC > 0 ? App.systemInfo.ZenMaxTDC : App.systemInfo.ZenTDC > 0 ? App.systemInfo.ZenTDC : 90;
            if (pcurrent.ZenControlEDChpx.ToString() == "" || pcurrent.ZenControlEDChpx < 1) pcurrent.ZenControlEDChpx = App.systemInfo.ZenMaxEDC > 0 ? App.systemInfo.ZenMaxEDC : App.systemInfo.ZenEDC > 0 ? App.systemInfo.ZenEDC : 95;

            PPThpx.Text = pcurrent.ZenControlPPThpx.ToString();
            TDChpx.Text = pcurrent.ZenControlTDChpx.ToString();
            EDChpx.Text = pcurrent.ZenControlEDChpx.ToString();

            if (App.systemInfo.ZenCOb)
            {
                COCounts.Visibility = Visibility.Visible;
                COCountsLabel.Visibility = Visibility.Visible;
            }

            if (App.thrThreadBoosterRunning) BtnThreadBoostLabel.Text = "Stop";

            Config_Init();

        }

        private void Config_Init()
        {
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
        }

        private void Create_CpuDisplay()
        {
            try
            {
                var gridLength1 = new GridLength(1.1, GridUnitType.Star);
                var gridLength2 = new GridLength(1, GridUnitType.Star);

                Grid _gridblock = current_cpumask;

                current_cpumask.HorizontalAlignment = HorizontalAlignment.Left;
                Thickness curcpugirmar = new Thickness(4, 4, 4, 4);
                current_cpumask.Margin = curcpugirmar;
                int _col = 0;

                int threads = ProcessorInfo.HardwareCpuSets.Length;

                int _maxrow = threads > 7 ? 7 : threads;
                if (threads == 12 || threads == 20 || threads == 24 || threads == 48) _maxrow = 5;
                if (threads == 8) _maxrow = 3;

                int _row = 0;

                Thickness curcpupad = new Thickness(1, 0, 1, 0);
                Thickness curcpumar = new Thickness(1, 2, 1, 2);

                int len = 0;
                int t0 = -1;
                int t1 = -1;

                int pcore = 0;

                for (int c = 0; c < threads ; ++c)
                {
                    len = 0;
                    if (t1 != ProcessorInfo.HardwareCpuSets[c].LogicalProcessorIndex || (c == threads && len == 1))
                    {
                        t1 = -1;
                        if (c < ProcessorInfo.HardwareCpuSets.Length)
                        {
                            t0 = ProcessorInfo.HardwareCpuSets[c].LogicalProcessorIndex;
                            len = 1;
                            if (c < threads - 1)
                            {
                                if (ProcessorInfo.HardwareCpuSets[c].CoreIndex == ProcessorInfo.HardwareCpuSets[c + 1].CoreIndex)
                                {
                                    len = 2;
                                    t1 = ProcessorInfo.HardwareCpuSets[c + 1].LogicalProcessorIndex;
                                }
                            }
                        }
                        //App.LogDebug($"C{c} T0-{t0} T1-{t1}");
                        Button btnCore = new Button { Width = 32, VerticalAlignment = VerticalAlignment.Center, Content = $"C{pcore}", Padding = curcpupad, HorizontalAlignment = HorizontalAlignment.Right, Margin = curcpumar };
                        Button btnT0 = new Button { Width = 16, VerticalAlignment = VerticalAlignment.Center, Tag = $"{t0}", Content = $"T0", Padding = curcpupad, HorizontalAlignment = HorizontalAlignment.Right, Margin = curcpumar };
                        ProgressBar loadT0 = new ProgressBar { Name = "tload", Maximum = 100, VerticalAlignment = VerticalAlignment.Center, Tag = $"{t0}", Padding = curcpupad, HorizontalAlignment = HorizontalAlignment.Right, Margin = curcpumar };
                        Button btnT1 = new Button { Width = 16, VerticalAlignment = VerticalAlignment.Center, Visibility = Visibility.Hidden, Tag = $"{t1}", Content = $"T1", Padding = curcpupad, HorizontalAlignment = HorizontalAlignment.Right, Margin = curcpumar };
                        ProgressBar loadT1 = new ProgressBar { Name = "tload", Maximum = 100, VerticalAlignment = VerticalAlignment.Center, Visibility = Visibility.Hidden, Tag = $"{t1}", Padding = curcpupad, HorizontalAlignment = HorizontalAlignment.Right, Margin = curcpumar };

                        loadT0.Width = 16;
                        loadT1.Width = 16;
                        loadT0.Height = 14;
                        loadT1.Height = 14;
                        loadT0.MaxWidth = 16;
                        loadT1.MaxWidth = 16;
                        loadT0.IsIndeterminate = false;
                        loadT1.IsIndeterminate = false;

                        Button btnSpace = new Button { VerticalAlignment = VerticalAlignment.Center, Visibility = Visibility.Hidden, Content = $"TX", Padding = curcpupad, HorizontalAlignment = HorizontalAlignment.Right, Margin = curcpumar };
                        App.LogDebug($"C{pcore} {t0} {t1} {_col} {_row} {threads}");
                        _gridblock.Children.Add(btnCore);
                        Grid.SetColumn(btnCore, _col);
                        Grid.SetRow(btnCore, _row);
                        if (len > 0)
                        {
                            _gridblock.Children.Add(btnT0);
                            Grid.SetColumn(btnT0, _col + 1);
                            Grid.SetRow(btnT0, _row);
                            _gridblock.Children.Add(loadT0);
                            Grid.SetColumn(loadT0, _col + 2);
                            Grid.SetRow(loadT0, _row);
                            _gridblock.Children.Add(btnT1);
                            Grid.SetColumn(btnT1, _col + 3);
                            Grid.SetRow(btnT1, _row);
                            _gridblock.Children.Add(loadT1);
                            Grid.SetColumn(loadT1, _col + 4);
                            Grid.SetRow(loadT1, _row);

                            Binding bindLive = new Binding($"CpuTload[{t0}]");
                            bindLive.Mode = BindingMode.OneWay;
                            bindLive.Source = App.systemInfo;
                            BindingOperations.SetBinding(loadT0, ProgressBar.ValueProperty, bindLive);

                        }

                        _gridblock.Children.Add(btnSpace);
                        Grid.SetColumn(btnSpace, _col + 5);
                        Grid.SetRow(btnSpace, _row);

                        if (t1 != -1)
                        {
                            btnT1.Visibility = Visibility.Visible;
                            loadT1.Visibility = Visibility.Visible;

                            Binding bindLive = new Binding($"CpuTload[{t1}]");
                            bindLive.Mode = BindingMode.OneWay;
                            bindLive.Source = App.systemInfo;
                            BindingOperations.SetBinding(loadT1, ProgressBar.ValueProperty, bindLive);
                        }

                        pcore++;
                        _row++;
                        _col = _row > _maxrow ? _col + 6 : _col;
                        _row = _row > _maxrow ? 0 : _row;
                    }

                }

                _row = 0;
                _col++;

                TextBox cpuLoadLabel = new TextBox { Name = "cpuLoadLabel", Text = "Load: ", VerticalAlignment = VerticalAlignment.Center, Padding = curcpupad, HorizontalAlignment = HorizontalAlignment.Right, Margin = curcpumar };
                _gridblock.Children.Add(cpuLoadLabel);
                Grid.SetColumn(cpuLoadLabel, _col + 1);
                Grid.SetRow(cpuLoadLabel, _row);

                TextBox cpuLoad = new TextBox { Name = "cpuLoad", Text = "", VerticalAlignment = VerticalAlignment.Center, Padding = curcpupad, HorizontalAlignment = HorizontalAlignment.Left, Margin = curcpumar };
                _gridblock.Children.Add(cpuLoad);
                Grid.SetColumn(cpuLoad, _col + 2);
                Grid.SetRow(cpuLoad, _row);

                Binding bindLiveLoad = new Binding($"LiveCpuLoad");
                bindLiveLoad.Mode = BindingMode.OneWay;
                bindLiveLoad.Source = App.systemInfo;
                BindingOperations.SetBinding(cpuLoad, TextBox.TextProperty, bindLiveLoad);

                _row++;

                if (App.systemInfo.ZenStates && !App.ZenBlockRefresh)
                {

                    if (App.systemInfo.Zen.cpuTemp != null)
                    {
                        TextBox cpuTempLabel = new TextBox { Name = "cpuTempLabel", Text = "CPU: ", VerticalAlignment = VerticalAlignment.Center, Padding = curcpupad, HorizontalAlignment = HorizontalAlignment.Right, Margin = curcpumar };
                        _gridblock.Children.Add(cpuTempLabel);
                        Grid.SetColumn(cpuTempLabel, _col + 1);
                        Grid.SetRow(cpuTempLabel, _row);

                        TextBox cpuTemp = new TextBox { Name = "cpuTemp", Text = "", VerticalAlignment = VerticalAlignment.Center, Padding = curcpupad, HorizontalAlignment = HorizontalAlignment.Left, Margin = curcpumar };
                        _gridblock.Children.Add(cpuTemp);
                        Grid.SetColumn(cpuTemp, _col + 2);
                        Grid.SetRow(cpuTemp, _row);

                        Binding bindLive = new Binding($"ZenCpuTemp");
                        bindLive.Mode = BindingMode.OneWay;
                        bindLive.Source = App.systemInfo;
                        BindingOperations.SetBinding(cpuTemp, TextBox.TextProperty, bindLive);

                        _row++;
                    }

                    if (App.systemInfo.Zen.ccd1Temp != null)
                    {
                        TextBox ccd1TempLabel = new TextBox { Name = "ccd1TempLabel", Text = "CCD1: ", VerticalAlignment = VerticalAlignment.Center, Padding = curcpupad, HorizontalAlignment = HorizontalAlignment.Right, Margin = curcpumar };
                        _gridblock.Children.Add(ccd1TempLabel);
                        Grid.SetColumn(ccd1TempLabel, _col + 1);
                        Grid.SetRow(ccd1TempLabel, _row);

                        TextBox ccd1Temp = new TextBox { Name = "ccd1Temp", Text = "", VerticalAlignment = VerticalAlignment.Center, Padding = curcpupad, HorizontalAlignment = HorizontalAlignment.Left, Margin = curcpumar };
                        _gridblock.Children.Add(ccd1Temp);
                        Grid.SetColumn(ccd1Temp, _col + 2);
                        Grid.SetRow(ccd1Temp, _row);

                        Binding bindLive = new Binding($"ZenCcd1Temp");
                        bindLive.Mode = BindingMode.OneWay;
                        bindLive.Source = App.systemInfo;
                        BindingOperations.SetBinding(ccd1Temp, TextBox.TextProperty, bindLive);

                        _row++;
                    }

                    if (App.systemInfo.Zen.ccd2Temp != null)
                    {
                        TextBox ccd2TempLabel = new TextBox { Name = "ccd2TempLabel", Text = "CCD2: ", VerticalAlignment = VerticalAlignment.Center, Padding = curcpupad, HorizontalAlignment = HorizontalAlignment.Right, Margin = curcpumar };
                        _gridblock.Children.Add(ccd2TempLabel);
                        Grid.SetColumn(ccd2TempLabel, _col + 1);
                        Grid.SetRow(ccd2TempLabel, _row);
                        
                        TextBox ccd2Temp = new TextBox { Name = "ccd2Temp", Text = "", VerticalAlignment = VerticalAlignment.Center, Padding = curcpupad, HorizontalAlignment = HorizontalAlignment.Left, Margin = curcpumar };
                        _gridblock.Children.Add(ccd2Temp);
                        Grid.SetColumn(ccd2Temp, _col + 2);
                        Grid.SetRow(ccd2Temp, _row);

                        Binding bindLive = new Binding($"ZenCcd2Temp");
                        bindLive.Mode = BindingMode.OneWay;
                        bindLive.Source = App.systemInfo;
                        BindingOperations.SetBinding(ccd2Temp, TextBox.TextProperty, bindLive);

                        _row++;
                    }

                    if (App.systemInfo.Zen.cpuVcore != null)
                    {
                        TextBox cpuVcoreLabel = new TextBox { Name = "cpuVcoreLabel", Text = "vCore: ", VerticalAlignment = VerticalAlignment.Center, Padding = curcpupad, HorizontalAlignment = HorizontalAlignment.Right, Margin = curcpumar };
                        _gridblock.Children.Add(cpuVcoreLabel);
                        Grid.SetColumn(cpuVcoreLabel, _col + 1);
                        Grid.SetRow(cpuVcoreLabel, _row);

                        TextBox cpuVcore = new TextBox { Name = "cpuVcore", Text = "", VerticalAlignment = VerticalAlignment.Center, Padding = curcpupad, HorizontalAlignment = HorizontalAlignment.Left, Margin = curcpumar };
                        _gridblock.Children.Add(cpuVcore);
                        Grid.SetColumn(cpuVcore, _col + 2);
                        Grid.SetRow(cpuVcore, _row);

                        Binding bindLive = new Binding($"ZenCpuVcore");
                        bindLive.Mode = BindingMode.OneWay;
                        bindLive.Source = App.systemInfo;
                        BindingOperations.SetBinding(cpuVcore, TextBox.TextProperty, bindLive);

                        _row++;
                    }

                    if (App.systemInfo.Zen.cpuVsoc != null)
                    {
                        TextBox cpuVsocLabel = new TextBox { Name = "cpuVsocLabel", Text = "vSOC: ", VerticalAlignment = VerticalAlignment.Center, Padding = curcpupad, HorizontalAlignment = HorizontalAlignment.Right, Margin = curcpumar };
                        _gridblock.Children.Add(cpuVsocLabel);
                        Grid.SetColumn(cpuVsocLabel, _col + 1);
                        Grid.SetRow(cpuVsocLabel, _row);

                        TextBox cpuVsoc = new TextBox { Name = "cpuVsoc", Text = "", VerticalAlignment = VerticalAlignment.Center, Padding = curcpupad, HorizontalAlignment = HorizontalAlignment.Left, Margin = curcpumar };
                        _gridblock.Children.Add(cpuVsoc);
                        Grid.SetColumn(cpuVsoc, _col + 2);
                        Grid.SetRow(cpuVsoc, _row);

                        Binding bindLive = new Binding($"ZenCpuVsoc");
                        bindLive.Mode = BindingMode.OneWay;
                        bindLive.Source = App.systemInfo;
                        BindingOperations.SetBinding(cpuVsoc, TextBox.TextProperty, bindLive);

                        _row++;
                    }
                }

            }
            catch (Exception ex)
            {
                App.LogExError($"Create_CpuDisplay exception:", ex);
            }
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
                //App.LogDebug($"Saving Window Position {WindowSettings.Default.Top} {WindowSettings.Default.Left} {WindowSettings.Default.Height} {WindowSettings.Default.Width} {WindowSettings.Default.Maximized}");
            }
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsActive && WindowSettings.Default.Initialized)
            {
                SaveWinPos();
                //App.LogDebug($"Saved Window Position Closing {WindowSettings.Default.Top} {WindowSettings.Default.Left} {WindowSettings.Default.Height} {WindowSettings.Default.Width} {WindowSettings.Default.Maximized}");
            }

            uitimer.IsEnabled = false;
            uitimer.Stop();

            GC.Collect();

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

            App.MainWindowOpen = true;

            InitMainUI();

            string LicensePath = @".\LICENSE";
            if (File.Exists(LicensePath))
            {
                boxLicense.Text = File.ReadAllText(LicensePath);
            }

            UpdateLayout();

            this.Title = WinTitle;

            App.systemInfo.SetThreadBoosterStatus("N/A");

            //InitUI = true;

            uitimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            uitimer.Start();
            uitimer.Tick += (sender, args) =>
            {
                if (App.thrThreadBoosterRunning)
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
                App.systemInfo.SetSleepsAllowed();

                if (PoolingTick == 3) App.systemInfo.RefreshLabels();

                //App.RefreshThreadsStatus();

                PoolingTick++;
                PoolingTick = PoolingTick > 3 ? 0 : PoolingTick;

                if (App.systemInfo.ZenCOb) App.systemInfo.ZenRefreshCO();
                if (App.systemInfo.ZenStates) App.systemInfo.ZenRefreshStatic(false);
                if (WinLoaded) currentcpu_update();

            };

            Activate();
            Focus();

        }

        public void currentcpu_update()
        {
            try 
            {
                ulong? _lastmask = App.lastSysCpuSetMask;
                //App.LogDebug($"UI Mask _lastmask:{_lastmask:X8} _lastmask_current:{_lastmask_current:X8}");
                if (_lastmask != _lastmask_current || _lastmask_current == ulong.MaxValue)
                {
                    for (int i = 0; i < ProcessorInfo.LogicalCoresCount; ++i)
                    {
                        //int _mask = i << _lastmask;
                        ulong? _mask = _lastmask;
#pragma warning disable CS0252 // Possible unintended reference comparison; left hand side needs cast
                        IEnumerable<Button> elements = FindVisualChildren<Button>(this).Where(x => x.Content == "T0" || x.Content== "T1");
#pragma warning restore CS0252 // Possible unintended reference comparison; left hand side needs cast
                        foreach (Button btn in elements)
                        {
                            if (btn.Tag.ToString() == $"{i}")
                            {
                                if ((((ulong)(1 << i) & _mask) != 0) || _mask == null || _mask == 0 || !((bool)App.pactive.ThreadBooster))
                                {
                                    btn.Foreground = Brushes.White;
                                    btn.Background = Brushes.Green;
                                }
                                else if (!(((ulong)(1 << i) & _mask) != 0) && (App.n0disabledT0.Contains(i) || App.n0disabledT1.Contains(i)))
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
                    _lastmask_current = _lastmask;
                }
            }
            catch { }            
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

        private void InitMainUI()
        {
            //InitUI = false;
        }

        /*
        private void ButtonReset(object sender, RoutedEventArgs e)
        {

            WindowSettings.Default.Reset();

            App.SettingsInit();

            InitMainUI();

        }

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
                App.TBStart();
                BtnThreadBoostLabel.Text = "Stop";
                BtnThreadBoost.IsEnabled = true;
            }
            else
            {
                BtnThreadBoost.IsEnabled = false;
                App.TBStart(1);
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
            App.LogInfo($"Set AU notifications logging: {App.AppSettings.AUNotifications}");
            App.AUNotifications(App.AppSettings.AUNotifications);
            //SettingsManager.WriteSettings();
        }
        private void TraceInfoCheckBox_Click(object sender, RoutedEventArgs e)
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

            App.LogInfo($"Set diagnostic logging: {App.AppSettings.LogTrace}");
            App.TraceLogging(App.AppSettings.LogTrace);
        }
        private void InpoutCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb.IsChecked == true)
            {
                App.AppSettings.inpoutdlldisable = true;
            }
            else
            {
                App.AppSettings.inpoutdlldisable = false;
            }

            App.LogInfo($"Set inpoutx64.dll disable: {App.AppSettings.inpoutdlldisable}");
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
        private void ZC_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb.IsChecked == true)
            {
                pcurrent.ZenControl = true;
            }
            else
            {
                pcurrent.ZenControl = false;
            }
        }
        private void ZCPPT_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb.IsChecked == true)
            {
                pcurrent.ZenControlPPTAuto = true;
                PPThpx.IsEnabled = false;
            }
            else
            {
                pcurrent.ZenControlPPTAuto = false;
                PPThpx.IsEnabled = true;
            }
        }
        private void ZCTDC_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb.IsChecked == true)
            {
                pcurrent.ZenControlTDCAuto = true;
                TDChpx.IsEnabled = false;
            }
            else
            {
                pcurrent.ZenControlTDCAuto = false;
                TDChpx.IsEnabled = true;
            }
        }
        private void ZCEDC_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb.IsChecked == true)
            {
                pcurrent.ZenControlEDCAuto = true;
                EDChpx.IsEnabled = false;
            }
            else
            {
                pcurrent.ZenControlEDCAuto = false;
                EDChpx.IsEnabled = true;
            }
        }
        private void pbolimit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            if (regex.IsMatch(e.Text))
            {
                MessageBox.Show("Only positive numbers allowed for PBO Limits!");
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

        private void HyberIdle_Select(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            //App.LogDebug($"PoolingRate {pcurrent.PoolingRate} {pcurrent.PoolingRate.GetType()}");
            if (pcurrent != null) pcurrent.HyberIdle = cb.SelectedIndex;
        }
        private void SleepIdle_Select(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            //App.LogDebug($"PoolingRate {pcurrent.PoolingRate} {pcurrent.PoolingRate.GetType()}");
            if (pcurrent != null) pcurrent.SleepIdle = cb.SelectedIndex;
        }
        private void MonitorIdle_Select(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            //App.LogDebug($"PoolingRate {pcurrent.PoolingRate} {pcurrent.PoolingRate.GetType()}");
            if (pcurrent != null) pcurrent.MonitorIdle = cb.SelectedIndex;
        }
        private bool isNumericText(string strValue)
        {
            int intVal = 0;
            return int.TryParse(strValue, out intVal);
        }
        private bool ValidateLimit(string _itemdesc, TextBox _textbox)
        {
            bool _valid = true;
            if (!isNumericText(_textbox.Text)) _valid = false;
            if (_valid) {
                if (Int32.Parse(_textbox.Text) < 1) _valid = false;
            }
            if (!_valid) MessageBox.Show($"Only positive numbers allowed for {_itemdesc}!");
            return true;
        }

        private void SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //App.LogInfo($"NumaZeroType Index={cb.SelectedIndex} {App.pactive.NumaZeroType} P0={App.AppConfigs[0].NumaZeroType}");
                //App.LogInfo($"NumaZeroType {App.pactive.NumaZeroType} P0={App.AppConfigs[0].NumaZeroType}");
                //App.AppConfigs[pcurrent.id] = pcurrent;
                bool validated = true;

                
                if (cbZC.IsChecked == true && cbPPT.IsChecked == false) if(!ValidateLimit("PPT Default", PPThpx)) validated = false;
                if (cbZC.IsChecked == true && cbTDC.IsChecked == false) if (!ValidateLimit("TDC Default", TDChpx)) validated = false;
                if (cbZC.IsChecked == true && cbEDC.IsChecked == false) if (!ValidateLimit("EDC Default", EDChpx)) validated = false;

                pcurrent.ZenControlPPThpx = Convert.ToInt32(PPThpx.Text.ToString().Trim());
                pcurrent.ZenControlTDChpx = Convert.ToInt32(TDChpx.Text.ToString().Trim());
                pcurrent.ZenControlEDChpx = Convert.ToInt32(EDChpx.Text.ToString().Trim());

                if (validated)
                {
                    pcurrent.ZenControlPPTAuto = cbPPT.IsChecked == true ? true : false;
                    pcurrent.ZenControlTDCAuto = cbTDC.IsChecked == true ? true : false;
                    pcurrent.ZenControlEDCAuto = cbEDC.IsChecked == true ? true : false;

                    pcurrent.ZenControlPPTAuto = cbPPT.IsChecked == true ? true : false;
                    pcurrent.ZenControlTDCAuto = cbTDC.IsChecked == true ? true : false;
                    pcurrent.ZenControlEDCAuto = cbEDC.IsChecked == true ? true : false;

                    pcurrent.ThreadBooster = cbTBAutoStart.IsChecked == true ? true : false;
                    pcurrent.NumaZero = cbNumaZero.IsChecked == true ? true : false;
                    pcurrent.PowerSaverActive = cbPSA.IsChecked == true ? true : false;
                    pcurrent.ZenControl = cbZC.IsChecked == true ? true : false;
                    pcurrent.SysSetHack = cbSysSetHack.IsChecked == true ? true : false;
                    pcurrent.NumaZeroType = listNumaZeroType.SelectedIndex;
                    pcurrent.ManualPoolingRate = cbPoolingRate.IsChecked == true ? true : false;
                    pcurrent.PoolingRate = listPoolingRate.SelectedIndex;
                    pcurrent.SleepIdle = listSleepIdle.SelectedIndex;
                    pcurrent.MonitorIdle = listMonitorIdle.SelectedIndex;
                    pcurrent.HyberIdle = listHyberIdle.SelectedIndex;
                    pcurrent.WakeTimers = cbWakeTimers.IsChecked == true ? true : false;
                    pcurrent.PSALightSleep = cbPSALightSleep.IsChecked == true ? true : false;
                    pcurrent.PSADeepSleep = cbPSADeepSleep.IsChecked == true ? true : false;
                    pcurrent.PSAAudioBlocksLightSleep = cbPSAAudioBlocksLightSleep.IsChecked == true ? true : false;
                    pcurrent.PSAAudioBlocksDeepSleep = cbPSAAudioBlocksDeepSleep.IsChecked == true ? true : false;
                    pcurrent.ZenControlCPUTempTrayIcon = cbZCCPUTemp.IsChecked == true ? true : false;
                    pcurrent.PLPerfMode = cbPLPerfMode.IsChecked == true ? true : false;

                    if (PowerTweak_LowPo.IsChecked == true) pcurrent.PowerTweak = 0;
                    if (PowerTweak_AutoPo.IsChecked == true) pcurrent.PowerTweak = 1;
                    if (PowerTweak_HighPo.IsChecked == true)  pcurrent.PowerTweak = 2;

                    if (Persona_Auto.IsChecked == true) pcurrent.Personality = 0;
                    if (Persona_Bala.IsChecked == true) pcurrent.Personality = 1;
                    if (Persona_Ulti.IsChecked == true) pcurrent.Personality = 2;

                    if (AMBias_Auto.IsChecked == true) pcurrent.ActiveModeBias = -1;
                    if (AMBias_Economizer.IsChecked == true) pcurrent.ActiveModeBias = 0;
                    if (AMBias_Standard.IsChecked == true) pcurrent.ActiveModeBias = 1;
                    if (AMBias_Booster.IsChecked == true) pcurrent.ActiveModeBias = 2;

                    if (GMBias_Auto.IsChecked == true) pcurrent.GameModeBias = -1;
                    if (GMBias_Economizer.IsChecked == true) pcurrent.GameModeBias = 0;
                    if (GMBias_Standard.IsChecked == true) pcurrent.GameModeBias = 1;
                    if (GMBias_Booster.IsChecked == true) pcurrent.GameModeBias = 2;

                    pcurrent.GameMode = GMDetect.IsChecked == true ? true : false;
                    pcurrent.FocusAssist = GMFocusAssist.IsChecked == true ? true : false;
                    pcurrent.UserNotification = GMUserNotification.IsChecked == true ? true : false;
                    pcurrent.SecondaryMonitor = GMFGSecondary.IsChecked == true ? true : false;

                    pcurrentSave(pcurrent.id);

                    //var dump = ObjectDumper.Dump(pcurrent);
                    //App.LogDebug($"Dump pcurrent.id={pcurrent.id}\n{dump}");

                    //dump = ObjectDumper.Dump(App.AppConfigs[pcurrent.id]);
                    //App.LogDebug($"Dump AppConfigs={pcurrent.id}\n{dump}");

                    App.SetActiveConfig(pcurrent.id);

                    //dump = ObjectDumper.Dump(App.pactive);
                    //App.LogDebug($"Dump pactive.id={App.pactive.id}\n{dump}");
                    
                    if (App.pactive.ThreadBooster)
                    {
                        BtnThreadBoost.IsEnabled = false;
                        BtnThreadBoostLabel.Text = "Stop";
                        BtnThreadBoost.IsEnabled = true;
                    }
                    else
                    {
                        BtnThreadBoost.IsEnabled = false;
                        BtnThreadBoostLabel.Text = "Start";
                        BtnThreadBoost.IsEnabled = true;
                    }

                    _lastmask_current = ulong.MaxValue;
                    ThreadBooster.bInit = false;

                    InitWindowUI();
                    //App.LogDebug($"Rd Config[{App.pactive.id}] TBA={App.AppConfigs[App.pactive.id].ThreadBooster} SSH={App.AppConfigs[App.pactive.id].SysSetHack}:{App.pactive.SysSetHack} PSA={App.AppConfigs[App.pactive.id].PowerSaverActive} N0={App.pactive.NumaZero}:{App.AppConfigs[App.pactive.id].NumaZero}:{App.AppConfigs[App.pactive.id].NumaZeroType}");
                }
            }
            catch (Exception ex)
            {
                App.LogExInfo("SaveConfig_Click exception:", ex);
            }
        }
        private void ResetSettings_Click(object sender, RoutedEventArgs e)
        {
            //App.LogInfo($"NumaZeroType Index={cb.SelectedIndex} {App.pactive.NumaZeroType} P0={App.AppConfigs[0].NumaZeroType}");
            //App.LogInfo($"NumaZeroType {App.pactive.NumaZeroType} P0={App.AppConfigs[0].NumaZeroType}");
            //App.AppConfigs[pcurrent.id] = pcurrent;
            /*
            SettingsManager.ResetSettings();
            if (!SettingsManager.ReadSettings()) SettingsManager.ReadSettings();
            App.SetActiveConfig(0);
            if (!SettingsManager.WriteSettings()) SettingsManager.WriteSettings();
            ThreadBooster.bInit = false;
            InitWindowUI();
            */
            App.resetSettings = true;
            App.QuitApplication();
        }
        private void PopulateSettings_Click(object sender, RoutedEventArgs e)
        {
            //App.LogInfo($"NumaZeroType Index={cb.SelectedIndex} {App.pactive.NumaZeroType} P0={App.AppConfigs[0].NumaZeroType}");
            //App.LogInfo($"NumaZeroType {App.pactive.NumaZeroType} P0={App.AppConfigs[0].NumaZeroType}");
            //App.AppConfigs[pcurrent.id] = pcurrent;
            /*
            SettingsManager.ResetSettings();
            if (!SettingsManager.ReadSettings()) SettingsManager.ReadSettings();
            App.SetActiveConfig(0);
            if (!SettingsManager.WriteSettings()) SettingsManager.WriteSettings();
            ThreadBooster.bInit = false;
            InitWindowUI();
            */
            Application.Current.Dispatcher.Invoke(new System.Action(() => { App.PopulateSettings(); }));
            
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

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                uitimer.IsEnabled = false;
            } 
            else
            {
                uitimer.IsEnabled = true;
            }
        }

        private void TabItemEnter(object sender, MouseEventArgs e)
        {
            ClearValue(MinWidthProperty);
            ClearValue(MinHeightProperty);
            UpdateLayout();
            SizeToContent = SizeToContent.WidthAndHeight;


            SetValue(MinWidthProperty, ActualWidth);
            SetValue(MinHeightProperty, ActualHeight);
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            App.MainWindowOpen= false;
        }
    }
    public class NumericValidationRule : ValidationRule
    {
        public Type ValidationType { get; set; }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string strValue = Convert.ToString(value);

            if (string.IsNullOrEmpty(strValue))
                return new ValidationResult(false, $"Value cannot be converted to string.");
            bool canConvert = false;
            switch (ValidationType.Name)
            {

                case "Boolean":
                    bool boolVal = false;
                    canConvert = bool.TryParse(strValue, out boolVal);
                    return canConvert ? new ValidationResult(true, null) : new ValidationResult(false, $"Input should be type of boolean");
                case "Int32":
                    int intVal = 0;
                    canConvert = int.TryParse(strValue, out intVal);
                    return canConvert ? new ValidationResult(true, null) : new ValidationResult(false, $"Input should be type of Int32");
                case "Double":
                    double doubleVal = 0;
                    canConvert = double.TryParse(strValue, out doubleVal);
                    return canConvert ? new ValidationResult(true, null) : new ValidationResult(false, $"Input should be type of Double");
                case "Int64":
                    long longVal = 0;
                    canConvert = long.TryParse(strValue, out longVal);
                    return canConvert ? new ValidationResult(true, null) : new ValidationResult(false, $"Input should be type of Int64");
                default:
                    throw new InvalidCastException($"{ValidationType.Name} is not supported");
            }
        }
    }
}
