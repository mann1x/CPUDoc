using AutoUpdaterDotNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows;
using System.Windows.Data;
using System.Threading;
using System.Timers;
using System.Windows.Media;
using ZenStates.Core;
using System.Drawing;
using System.ComponentModel;
using NLog;
using NLog.Targets.Wrappers;
using NLog.Targets;
using System.Text;
using NLog.Config;
using System.Threading.Tasks;
using Hardcodet.Wpf.TaskbarNotification;

namespace CPUDoc
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 

    public partial class App : Application
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        private static LoggingRule logtraceRule;

        private static LoggingRule logfileRule;

        internal const string mutexName = "Local\\CPUDoc";
        internal static Mutex instanceMutex;
        internal bool bMutex;

        public static int InterlockBench = 0;
        public static int InterlockHWM = 0;

        public static CancellationTokenSource hwmcts = new CancellationTokenSource();
        public static CancellationTokenSource benchcts = new CancellationTokenSource();

        public static ManualResetEventSlim mresbench = new ManualResetEventSlim(false);
        public static ManualResetEventSlim mreshwm = new ManualResetEventSlim(false);

        public static System.Timers.Timer hwmtimer = new System.Timers.Timer();

        public static Thread thrMonitor;
        public static int thridMonitor;
        public static Thread thrBench;
        public static int thridBench;

        public static Process BenchProc = new Process();
        public static int RunningProcess = -1;

        public static bool TaskRunning;
        public static bool MultiRunning;

        public static SystemInfo systemInfo;
        public static string version;
        public static string ss_filename;
        public static string _versionInfo;

        public static string ZenPTSubject = "";
        public static string ZenPTBody = "";

        public static DateTime TSRunStart = DateTime.Now;
        public static bool benchrunning = false;
        public static bool benchclosed = true;
        public static double scoreMinWidth = 0;
        public static List<int> TieredLogicals = new();
        public static int BenchIterations = 1;
        public static string BenchScoreUnit;
        public static int IterationPretime = 20;
        public static int IterationRuntime = 100;
        public static int IterationPostime = 5;
        public static DateTime IterationPretimeTS = DateTime.MinValue;
        public static DateTime IterationRuntimeTS = DateTime.MinValue;
        public static DateTime IterationPostimeTS = DateTime.MinValue;

        public static List<HWSensorItem> hwsensors;

        public static SolidColorBrush boxbrush1 = new SolidColorBrush();
        public static SolidColorBrush boxbrush2 = new SolidColorBrush();
        public static SolidColorBrush scorebrush = new SolidColorBrush();
        public static SolidColorBrush thrbgbrush = new SolidColorBrush();
        public static SolidColorBrush thrbrush1 = new SolidColorBrush();
        public static SolidColorBrush thrbrush2 = new SolidColorBrush();
        public static SolidColorBrush maxbrush = new SolidColorBrush();
        public static SolidColorBrush tempbrush = new SolidColorBrush();
        public static SolidColorBrush voltbrush = new SolidColorBrush();
        public static SolidColorBrush clockbrush1 = new SolidColorBrush();
        public static SolidColorBrush clockbrush2 = new SolidColorBrush();
        public static SolidColorBrush powerbrush = new SolidColorBrush();
        public static SolidColorBrush additionbrush = new SolidColorBrush();
        public static SolidColorBrush detailsbrush = new SolidColorBrush();
        public static SolidColorBrush blackbrush = new SolidColorBrush();
        public static SolidColorBrush whitebrush = new SolidColorBrush();
        public static Thickness thickness;

        public static TaskbarIcon tbIcon;

        [DllImport("kernel32", SetLastError = true)]
        static extern bool FreeLibrary(IntPtr hModule);
        public static void UnloadModule(string moduleName)
        {
            foreach (ProcessModule mod in Process.GetCurrentProcess().Modules)
            {
                if (mod.ModuleName == moduleName)
                {
                    FreeLibrary(mod.BaseAddress);
                }
            }
        }
        // Sleep Control
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }
        protected static bool IsInVisualStudio => LicenseManager.UsageMode == LicenseUsageMode.Designtime || Debugger.IsAttached == true || StringComparer.OrdinalIgnoreCase.Equals("devenv", Process.GetCurrentProcess().ProcessName);
        public class SettingBindingExtension : Binding
        {
            public SettingBindingExtension()
            {
                Initialize();
            }

            public SettingBindingExtension(string path)
                : base(path)
            {
                Initialize();
            }

            private void Initialize()
            {
                Source = CPUDoc.Properties.Settings.Default;
                Mode = BindingMode.TwoWay;
            }
        }

        public static int GetLastThread(int _base = 1)
        {
            try
            {
                int _thr = ProcessorInfo.LastThreadID();
                if (_base == 0) _thr--;
                return _thr;
            }
            catch (Exception ex)
            {
                LogExError($"GetLastThread exception: {ex.Message}", ex);
                return 1;
            }
        }

        public static (List<int>, List<int>) GetTieredLists()
        {
            List<int> _runlogicals = new();
            List<int> _runcores = new();

            try
            {
                int tcore = 0;
                int[] CPPC = App.systemInfo.CPPCActiveOrder;

                //TIER 1 - No E-Core if Hybrid, First Thread
                for (int j = 0; j < CPPC.Length; ++j)
                {
                    tcore = CPPC[j];
                    //App.LogDebug($"t1 tcore {tcore} isecore={systemInfo.Ecores.Contains(tcore)} tcoreloglen={ProcessorInfo.HardwareCores[tcore].LogicalCores.Length}");
                    if (!systemInfo.IntelHybrid || (systemInfo.IntelHybrid && !systemInfo.Ecores.Contains(tcore)))
                    {
                        //App.LogDebug($"t1 add {tcore} logical={ProcessorInfo.HardwareCores[tcore].LogicalCores[0]}");
                        _runcores.Add(tcore);
                        _runlogicals.Add(ProcessorInfo.HardwareCores[tcore].LogicalCores[0]);
                    }
                }

                //TIER 2 No E-Core if Hybrid, Second Thread
                for (int j = 0; j < CPPC.Length; ++j)
                {
                    tcore = CPPC[j];
                    //App.LogDebug($"t2 tcore {tcore} isecore={systemInfo.Ecores.Contains(tcore)} tcoreloglen={ProcessorInfo.HardwareCores[tcore].LogicalCores.Length}");
                    if ((!systemInfo.IntelHybrid || (systemInfo.IntelHybrid && !systemInfo.Ecores.Contains(tcore))) && ProcessorInfo.HardwareCores[tcore].LogicalCores.Length > 1)
                    {
                        //App.LogDebug($"t2 add {tcore} logical={ProcessorInfo.HardwareCores[tcore].LogicalCores[1]}");
                        _runcores.Add(tcore);
                        _runlogicals.Add(ProcessorInfo.HardwareCores[tcore].LogicalCores[1]);
                    }
                }

                //TIER 3 E-Core if Hybrid, First Thread
                for (int j = 0; j < CPPC.Length; ++j)
                {
                    tcore = CPPC[j];
                    if (systemInfo.IntelHybrid && systemInfo.Ecores.Contains(tcore))
                    {
                        _runcores.Add(tcore);
                        _runlogicals.Add(ProcessorInfo.HardwareCores[tcore].LogicalCores[0]);
                    }
                }

                //TIER 4 E-Core if Hybrid, Second Thread
                for (int j = 0; j < CPPC.Length; ++j)
                {
                    tcore = CPPC[j];
                    if (systemInfo.IntelHybrid && systemInfo.Ecores.Contains(tcore) && ProcessorInfo.HardwareCores[tcore].LogicalCores.Length > 1)
                    {
                        _runcores.Add(tcore);
                        _runlogicals.Add(ProcessorInfo.HardwareCores[tcore].LogicalCores[1]);
                    }
                }

                LogInfo($"Tiered RunCores: {String.Join(", ", _runcores.ToArray())}");
                LogInfo($"Tiered RunLogicals: {String.Join(", ", _runlogicals.ToArray())}");
            }
            catch (Exception ex)
            {
                LogExError($"GetTieredLists exception: {ex.Message}", ex);
            }
            return (_runlogicals, _runcores);
        }

        public static string GetCustomLabel()
        {
            try
            {
                string CPPCLabel = "";
                for (int i = 0; i < systemInfo.CPPCCustomOrder.Length; i++)
                {
                    CPPCLabel += String.Format("{0} ", systemInfo.CPPCCustomOrder[i]);
                    if (i != systemInfo.CPPCCustomOrder.Length - 1) CPPCLabel += ", ";

                }
                return CPPCLabel;
            }
            catch (Exception ex)
            {
                LogExWarn($"GetCustomLabel exception: {ex.Message}", ex);
                return "";
            }
        }
        private static void InitNLog()
        {
            try
            {
                if (IsInVisualStudio)
                {
                    // set internal log level
                    NLog.Common.InternalLogger.LogLevel = LogLevel.Debug;
                    // enable internal logging to the console
                    NLog.Common.InternalLogger.LogToTrace = false;
                    NLog.Common.InternalLogger.LogFile = ".\\Logs\\nlog-internal.txt";
                }
                var logconfig = new LoggingConfiguration();

                NLog.Layouts.Layout loglayout = "${longdate}|${level:uppercase=true}|${threadid}|${message:exceptionSeparator=\r\n:withexception=true}";
                NLog.Layouts.Layout debbugerlayout = "${level:uppercase=true}|${threadid}|${message:exceptionSeparator=\r\n:withexception=true}";

                FileTarget logfile = new FileTarget();
                logfile.FileName = "${processdir}\\Logs\\LogFile.txt";
                logfile.KeepFileOpen = true;
                logfile.Layout = loglayout;
                logfile.Encoding = Encoding.UTF8;
                logfile.ArchiveNumbering = ArchiveNumberingMode.Date;
                logfile.ArchiveEvery = FileArchivePeriod.Day;
                logfile.MaxArchiveDays = 7;
                logfile.ReplaceFileContentsOnEachWrite = false;

                AsyncTargetWrapper logfilewrap = new AsyncTargetWrapper();
                logfilewrap.Name = "logfile";
                logfilewrap.WrappedTarget = logfile;
                logfilewrap.QueueLimit = 5000;
                logfilewrap.OverflowAction = AsyncTargetWrapperOverflowAction.Grow;

                FileTarget logtrace = new FileTarget();
                logtrace.FileName = "${processdir}\\Logs\\LogTrace.txt";
                logtrace.KeepFileOpen = true;
                logtrace.Layout = loglayout;
                logtrace.Encoding = Encoding.UTF8;
                logtrace.ArchiveNumbering = ArchiveNumberingMode.Date;
                logtrace.ArchiveEvery = FileArchivePeriod.Day;
                logtrace.MaxArchiveDays = 7;
                logtrace.ReplaceFileContentsOnEachWrite = false;

                AsyncTargetWrapper logtracewrap = new AsyncTargetWrapper();
                logtracewrap.Name = "logtrace";
                logtracewrap.WrappedTarget = logtrace;
                logtracewrap.QueueLimit = 5000;
                logtracewrap.OverflowAction = AsyncTargetWrapperOverflowAction.Grow;

                logtraceRule = new LoggingRule("*", LogLevel.Trace, LogLevel.Fatal, logtracewrap);
                logfileRule = new LoggingRule("*", LogLevel.Info, LogLevel.Fatal, logfilewrap);

                logconfig.LoggingRules.Add(logfileRule);

                if (IsInVisualStudio)
                {
                    DebuggerTarget logdebugger = new DebuggerTarget();
                    logdebugger.Layout = debbugerlayout;

                    AsyncTargetWrapper logdebuggerwrap = new AsyncTargetWrapper();
                    logdebuggerwrap.Name = "debugger";
                    logdebuggerwrap.WrappedTarget = logdebugger;
                    logdebuggerwrap.QueueLimit = 5000;
                    logdebuggerwrap.OverflowAction = AsyncTargetWrapperOverflowAction.Grow;

                    var logdebuggerRule = new LoggingRule("*", LogLevel.Debug, LogLevel.Fatal, logdebuggerwrap);
                    logconfig.LoggingRules.Add(logdebuggerRule);

                    var consolelogconfig = new LoggingConfiguration();
                    consolelogconfig.LoggingRules.Add(logdebuggerRule);

                    logconfig.LoggingRules.Add(logtraceRule);

                }
                LogManager.Configuration = logconfig;
                //LogManager.ReconfigExistingLoggers();
                logger = LogManager.GetCurrentClassLogger();

            }
            catch (NLogConfigurationException ex)
            {
                Trace.WriteLine($"InitNLog Exception: {ex}");
                LogExError($"InitNLog exception: {ex.Message}", ex);
            }

        }
        public void TraceLogging(bool enable)
        {
            if (enable && !LogManager.Configuration.LoggingRules.Contains(logtraceRule))
            {
                LogManager.Configuration.LoggingRules.Add(logtraceRule);
                LogManager.ReconfigExistingLoggers();
            }
            else if (!enable && LogManager.Configuration.LoggingRules.Contains(logtraceRule))
            {
                LogManager.Configuration.LoggingRules.Remove(logtraceRule);
                LogManager.ReconfigExistingLoggers();
            }
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                Directory.CreateDirectory(@".\Logs");

                instanceMutex = new Mutex(true, mutexName, out bMutex);

                if (!bMutex)
                {
                    InteropMethods.PostMessage((IntPtr)InteropMethods.HWND_BROADCAST, InteropMethods.WM_SHOWME,
                        IntPtr.Zero, IntPtr.Zero);
                    Current.Shutdown();
                    Environment.Exit(0);
                }

                GC.KeepAlive(instanceMutex);

                CleanUpOldFiles();

                Trace.AutoFlush = true;

                InitNLog();

                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);

                if (principal.IsInRole(WindowsBuiltInRole.Administrator) == false && principal.IsInRole(WindowsBuiltInRole.User) == true)
                {
                    ProcessStartInfo objProcessInfo = new ProcessStartInfo();
                    objProcessInfo.UseShellExecute = true;
                    objProcessInfo.FileName = System.AppContext.BaseDirectory + Assembly.GetExecutingAssembly().GetName().Name + ".exe";
                    objProcessInfo.Verb = "runas";
                    try
                    {
                        Process proc = Process.Start(objProcessInfo);
                        Application.Current.Shutdown();
                    }
                    catch (Exception ex)
                    {
                        LogExError($"OnStartup exception: {ex.Message}", ex);
                    }
                }

                base.OnStartup(e);

                SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_SYSTEM_REQUIRED);

                Version _version = Assembly.GetExecutingAssembly().GetName().Version;
                version = string.Format("v{0}.{1}.{2}", _version.Major, _version.Minor, _version.Build);
                _versionInfo = string.Format("{0}.{1}.{2}", _version.Major, _version.Minor, _version.Build);

                LogInfo($"CPUDoc Version {_versionInfo}");

                systemInfo = new();

                systemInfo.AppVersion = version;


                SettingsInit();

                InitColors();

                HWMStart();

                //App.LogDebug($"OnStartup App End");

                tbIcon = (TaskbarIcon)FindResource("tbIcon");

                tbIcon.ToolTip = $"CPUDoc {App.version}";
                tbIcon.Icon = new Icon(Application.GetResourceStream(new Uri("pack://application:,,,/images/cpudoc.ico")).Stream);

            }
            catch (Exception ex)
            {
                LogExError($"OnStartup exception: {ex.Message}", ex);
            }
        }
        public static void HWMStart()
        {
            thrMonitor = new Thread(RunHWM);
            thridMonitor = thrMonitor.ManagedThreadId;
            thrMonitor.Priority = ThreadPriority.Highest;

            hwmtimer.Enabled = true;

            thrMonitor.Start();
            hwmtimer.Start();

            //App.LogDebug($"HWMonitor Started PID={thridMonitor} ALIVE={thrMonitor.IsAlive}");

        }

        public static int RunTask()
        {
            if (MultiRunning)
            {
                return 2;
            }
            if (TaskRunning || InterlockBench != 0)
            {
                return 1;
            }
            return 0;
            //thrBench = new Thread(BenchRun.RunBench);
            //thridBench = thrBench.ManagedThreadId;

        }
        public static void SettingsInit()
        {
            try
            {
                systemInfo.CPPCActiveOrder = systemInfo.CPPCOrder;
                systemInfo.CPPCActiveLabel = systemInfo.CPPCLabel;

                systemInfo.bECores = CPUDoc.Properties.Settings.Default.ECores;

                List<int> _runlogicals = new();
                List<int> _runcores = new();

                (_runlogicals, _runcores) = GetTieredLists();

                SetLastTieredThreadAffinity(_runlogicals);

                _runlogicals = null;
                _runcores = null;

            }
            catch (Exception ex)
            {
                LogExError($"SettingsInit exception: {ex.Message}", ex);
            }
        }
        private static void Monitor_ElapsedEventHandler(object sender, ElapsedEventArgs e)
        {
            //App.LogDebug("HWM ELAPSED");
            int sync = Interlocked.CompareExchange(ref InterlockHWM, 1, 0);
            if (sync == 0)
            {
                //HWMonitor.OnHWM(sender, e);
                InterlockHWM = 0;
                //App.LogDebug("HWM ELAPSED ON");
            }
        }
        public static void RunHWM()
        {

            //App.LogDebug("RUN HWM");
            hwmtimer.Interval = HWMonitor.MonitoringPooling;
            hwmtimer.Elapsed += new ElapsedEventHandler(Monitor_ElapsedEventHandler);
            if (hwmtimer.Enabled)
            {
                //App.LogDebug("START HWM");
                hwmtimer.Start();
            }

        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                HWMonitor.Close();
                hwmcts?.Dispose();
                benchcts?.Dispose();
                tbIcon.Dispose();
            }
            catch (Exception ex)
            {
                LogExWarn($"Application_Exit exception: {ex.Message}", ex);
            }
            try
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);

            }
            catch (Exception ex)
            {
                LogExWarn($"Application_Exit exception: {ex.Message}", ex);
            }
            try
            {
                if (!object.ReferenceEquals(null, systemInfo.Zen))
                {
                    systemInfo.Zen?.Dispose();
                    systemInfo.Zen = null;
                }

            }
            catch (Exception ex)
            {
                LogExWarn($"Application_Exit exception: {ex.Message}", ex);
            }
            try
            {
                if (!object.ReferenceEquals(null, HWMonitor.computer))
                {
                    HWMonitor.computer = null;
                }
            }
            catch (Exception ex)
            {
                LogExWarn($"Application_Exit exception: {ex.Message}", ex);
            }
            try
            {
                UnloadModule("CPUDoc.sys");
            }
            catch (Exception ex)
            {
                LogExWarn($"Application_Exit exception: {ex.Message}", ex);
            }
            LogInfo($"CPUDoc Version {_versionInfo} closed");
            Trace.Flush();
            Trace.Close();
            LogManager.Shutdown();
            Current.Shutdown();
            Environment.Exit(0);
        }

        private void InitColors()
        {
            /*
			//boxbrush1 = (SolidColorBrush)new BrushConverter().ConvertFrom("#648585");
			//boxbrush2 = (SolidColorBrush)new BrushConverter().ConvertFrom("#648585");
			boxbrush1 = (SolidColorBrush)new BrushConverter().ConvertFrom("#B6CECE");
			boxbrush2 = (SolidColorBrush)new BrushConverter().ConvertFrom("#B6CECE");
			thrbrush1 = (SolidColorBrush)new BrushConverter().ConvertFrom("#CEEDE2");
			thrbrush2 = (SolidColorBrush)new BrushConverter().ConvertFrom("#C3E0D6");
			//maxbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#6D006 750E17");
			maxbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#A10008");
			tempbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#005D70");
			voltbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#8300A3");
			clockbrush1 = (SolidColorBrush)new BrushConverter().ConvertFrom("#140D4F");
			//clockbrush2 = (SolidColorBrush)new BrushConverter().ConvertFrom("#115C6B");
			clockbrush2 = (SolidColorBrush)new BrushConverter().ConvertFrom("#A31746"); 
			powerbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#D95D04");
			//powerbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#D16700");

			additionbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#431571");
			blackbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#0A0A0A");
			whitebrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#F4F4F6");
			*/
            //DOPO

            // BOX BACKGROUND ODD
            boxbrush1 = (SolidColorBrush)new BrushConverter().ConvertFrom("#B6CECE");
            // BOX BACKGROUND EVEN
            boxbrush2 = (SolidColorBrush)new BrushConverter().ConvertFrom("#B6CECE");
            // BOX BACKGROUND THREADS
            thrbgbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#2F4F4F");
            // FONT SCORE RESULT
            scorebrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#007300");
            // FONT NUM THREADS
            thrbrush1 = (SolidColorBrush)new BrushConverter().ConvertFrom("#CEEDE2");
            // FONT t THREADS
            thrbrush2 = (SolidColorBrush)new BrushConverter().ConvertFrom("#C3E0D6");
            // FONT ALL Max VALUES
            maxbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#A10008");
            // FONT CPU TEMP
            tempbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#115C6B");
            // FONT VOLTAGES
            voltbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#C51B54");
            // FONT AVERAGE CLOCK
            clockbrush1 = (SolidColorBrush)new BrushConverter().ConvertFrom("#251AED");
            // FONT MAX CLOCK
            clockbrush2 = (SolidColorBrush)new BrushConverter().ConvertFrom("#8300A3");
            // FONT POWER 
            powerbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#750E17");
            // FONT ADDITIONAL BOX (CCDS)
            additionbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#431571");
            // FONT EXPANDER DETAILS
            detailsbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#F2DFC2");
            // FONT ALL BLACK (N/A, STARTED, FINISHED, LOAD, SCORE UNITS)
            blackbrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#2A2B34");
            // FONT ALL WHITE (BOX SCORE BG)
            whitebrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#F4F5F6");

            thickness = new Thickness(4, 3, 4, 3);

        }
        public static void SetLastThreadAffinity(int thread = -1)
        {
            try
            {
                using (Process thisprocess = Process.GetCurrentProcess())
                {
                    if (thread == -1) thread = App.GetLastThread(0);
                    if (thisprocess.ProcessorAffinity != (IntPtr)(1L << App.GetLastThread(0)))
                        thisprocess.ProcessorAffinity = (IntPtr)(1L << App.GetLastThread(0));
                }
            }
            catch (Exception ex)
            {
                LogExWarn($"SetLastThreadAffinity exception: {ex.Message}", ex);
            }
        }
        public static void SetLastTieredThreadAffinity(List<int> tieredlist)
        {
            try
            {
                LogDebug($"Setting Process affinity, tiered list: {tieredlist.Any()}");
                if (tieredlist.Any())
                {
                    using (Process thisprocess = Process.GetCurrentProcess())
                    {
                        LogDebug($"Setting Process affinity from: {thisprocess.ProcessorAffinity} to #{tieredlist.Last()}: {(IntPtr)(1L << tieredlist.Last())}");
                        if (thisprocess.ProcessorAffinity != (IntPtr)(1L << tieredlist.Last()))
                            thisprocess.ProcessorAffinity = (IntPtr)(1L << tieredlist.Last());
                        LogDebug($"New Process affinity: {thisprocess.ProcessorAffinity}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogExError($"SetLastTieredThreadAffinity exception: {ex.Message}", ex);
            }
        }
        public static void CleanUpOldFiles()
        {
            try
            {

                Directory.GetFiles(@".\Logs", "*.txt")
                    .Select(f => new FileInfo(f))
                    .Where(f => f.LastWriteTime < DateTime.Now.AddDays(-30))
                    .ToList()
                    .ForEach(f => f.Delete());

                Directory.GetFiles(@".\", "*.tmp")
                    .Select(f => new FileInfo(f))
                    .Where(f => f.LastWriteTime < DateTime.Now.AddDays(-1))
                    .ToList()
                    .ForEach(f => f.Delete());

                Directory.GetFiles(@".\", "CPUDoc-v*.zip")
                    .Select(f => new FileInfo(f))
                    .Where(f => f.LastWriteTime < DateTime.Now.AddDays(-1))
                    .ToList()
                    .ForEach(f => f.Delete());

            }
            catch (Exception ex)
            {
                LogExWarn($"CleanUpOldFiles exception: {ex.Message}", ex);
            }
        }
        public static void LogInfo(string msg)
        {
            logger.Info(msg);
        }
        public static void LogError(string msg)
        {
            logger.Error(msg);
        }
        public static void LogWarn(string msg)
        {
            logger.Warn(msg);
        }
        public static void LogDebug(string msg)
        {
            logger.Debug(msg);
        }
        public static void LogExInfo(string msg, Exception ex)
        {
            logger.Info(ex, msg);
        }
        public static void LogExError(string msg, Exception ex)
        {
            logger.Error(ex, msg);
        }
        public static void LogExWarn(string msg, Exception ex)
        {
            logger.Warn(ex, msg);
        }
    }
}
