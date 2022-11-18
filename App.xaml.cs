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
using System.Windows.Threading;
using Gma.System.MouseKeyHook;
using net.r_eg.Conari;
using net.r_eg.Conari.Accessors;
using net.r_eg.Conari.Types;
using OSVersionExtension;

namespace CPUDoc
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 

    public partial class App : Application
    {
        public static readonly string AutoUpdaterUrl = "https://raw.githubusercontent.com/mann1x/CPUDoc/master/CPUDoc/AutoUpdaterCPUDoc.json";

        //public static readonly string AutoUpdaterUrl = "https://raw.githubusercontent.com/mann1x/CPUDoc/master/CPUDoc/AutoUpdaterCPUDocTest.json";
        //public static readonly string AutoUpdaterUrl = "https://raw.githubusercontent.com/mann1x/BenchMaestro/master/BenchMaestro/AutoUpdaterBenchMaestroTest.json";

        public static Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly EventHook.EventHookFactory eventHookFactory = new EventHook.EventHookFactory();
        private static EventHook.ApplicationWatcher applicationWatcher;
        private static EventHook.KeyboardWatcher keyboardWatcher;
        //private static EventHook.MouseWatcher mouseWatcher;

        //private static string codebase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
        //private static string basepath = new Uri(System.IO.Path.GetDirectoryName(codebase)).LocalPath;

        private IKeyboardMouseEvents m_Events;

        public static DateTime UAStamp;

        private static LoggingRule logtraceRule;

        private static LoggingRule logfileRule;

        internal const string mutexName = "Local\\CPUDoc";
        internal static Mutex instanceMutex;
        internal bool bMutex;

        public static int InterlockHWM = 0;
        public static int InterlockTB = 0;
        public static int InterlockUI = 0;
        public static int InterlockSys = 0;

        public static CancellationTokenSource hwmcts = new CancellationTokenSource();
        public static CancellationTokenSource tbcts = new CancellationTokenSource();
        public static CancellationTokenSource syscts = new CancellationTokenSource();

        public static ManualResetEventSlim mreshwm = new ManualResetEventSlim(false);
        public static ManualResetEventSlim mrestb = new ManualResetEventSlim(false);
        public static ManualResetEventSlim mressys = new ManualResetEventSlim(false);

        public static System.Timers.Timer hwmtimer = new System.Timers.Timer();
        public static System.Timers.Timer tbtimer = new System.Timers.Timer();
        public static System.Timers.Timer uitimer = new System.Timers.Timer();
        public static System.Timers.Timer systimer = new System.Timers.Timer();

        public static Thread thrMonitor;
        public static int thridMonitor;
        public static Thread thrThreadBooster;
        public static int thridThreadBooster;
        public static Thread thrUpdateUI;
        public static int thridUpdateUI;
        public static Thread thrSys;
        public static int thridSys;

        public static SystemInfo systemInfo;
        public static string version;
        public static string ss_filename;
        public static string _versionInfo;

        public static string ZenPTSubject = "";
        public static string ZenPTBody = "";

        public static bool Win10 = false;
        public static bool Win11 = false;

        public static uint? SysCpuSetMask = 0x0;
        public static uint? lastSysCpuSetMask = null;

        public static int? PSABiasCurrent = null;
        public static int? lastPSABiasCurrent = null;

        public static bool CoreBestT1T0 = false;
        public static bool CoreZeroT1T0 = false;

        public static int SysMaskPooling = 25;

        public static IPowerPlanManager powerManager = PowerManagerProvider.CreatePowerManager();
        public static Guid PPGuid = new Guid("FBB9C3D1-AF6E-46CF-B02B-C411928D1BE1");

        public static string boot_ppname;
        public static Guid boot_ppguid;

        public static bool psact_plan = false;
        public static bool psact_light_b = false;
        public static bool psact_deep_b = false;

        public static bool numazero_b = false;

        public static MovingAverage cpuTotalLoad;
        public static MovingAverage cpuTotalLoadLong;

        public static bool reapplyProfile = false;

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

        public static List<int> logicalsT0;
        public static List<int> logicalsT1;

        public static List<int> n0enabledT0;
        public static List<int> n0enabledT1;

        public static List<int> n0disabledT0;
        public static List<int> n0disabledT1;

        public static appSettings AppSettings;
        public static appConfigs pactive;
        public static List<appConfigs> AppConfigs = new List<appConfigs>();
        public static appProfiles AppProfiles;
        
        public static DispatcherTimer autimer;

        internal struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport(@"User32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport(@"User32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport(@"kernel32.dll")]
        private static extern uint GetLastError();
        public static uint GetIdleTime()
        {
            LASTINPUTINFO lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            GetLastInputInfo(ref lastInPut);

            return ((uint)Environment.TickCount - lastInPut.dwTime);
        }
        public static long GetLastInputTime()
        {
            LASTINPUTINFO lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            if (!GetLastInputInfo(ref lastInPut))
            {
                throw new Exception(GetLastError().ToString());
            }

            return lastInPut.dwTime;
        }

        [DllImport(@"cputools.dll")]
        private static extern int SetSystemCpuSet(uint bitMask);

        [DllImport(@"cputools.dll")]
        private static extern int ResetSystemCpuSetProc(int pid, uint bitMask);

        [DllImport(@"kernel32.dll", SetLastError = true)]
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
        [DllImport(@"kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
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


        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int smIndex);

        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out W32RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct W32RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        public static bool IsForegroundWwindowFullScreen()
        {
            int scrX = GetSystemMetrics(SM_CXSCREEN),
                scrY = GetSystemMetrics(SM_CYSCREEN);

            IntPtr handle = GetForegroundWindow();
            if (handle == IntPtr.Zero) return false;

            W32RECT wRect;
            if (!GetWindowRect(handle, out wRect)) return false;

            return scrX == (wRect.Right - wRect.Left) && scrY == (wRect.Bottom - wRect.Top);
        }
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

        public static int SetSysCpuSet(uint? BitMask = 0)
        {
            uint _BitMask = 0x0;
            int _ret = 0;
            if (BitMask != null) _BitMask = (uint)BitMask;
            using (var lam = new ConariX(@"cputools.dll"))
            {
                _ret = lam.DLR.SetSystemCpuSet<int>(_BitMask);
            }
            /*
            _ret = SetSystemCpuSet(_BitMask);
            */
            return _ret;
        }
        public static int ResetSysCpuSetProc(int pid, uint BitMask = 0)
        {
            int _ret = 0;
            using (var lam = new ConariL(@"cputools.dll"))
            {
                _ret = lam.DLR.ResetSystemCpuSetProc<int>(pid, BitMask);
            }
            /*
            _ret = ResetSystemCpuSetProc(pid, BitMask);
            */
            return _ret;
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

        public static int GetLastThreadT0()
        {
            try
            {
                return logicalsT0.Last();

            }
            catch (Exception ex)
            {
                LogExError($"GetLastThreadT0 exception: {ex.Message}", ex);
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

        public static (List<int>, List<int>) GetTieredListsThreads()
        {
            List<int> _runt0 = new();
            List<int> _runt1 = new();

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
                        _runt0.Add(ProcessorInfo.HardwareCores[tcore].LogicalCores[0]);
                    }
                }

                //TIER 2 No E-Core if Hybrid, Second Thread
                for (int j = CPPC.Length - 1; j >= 0; --j)
                {
                    tcore = CPPC[j];
                    //App.LogDebug($"t2 tcore {tcore} isecore={systemInfo.Ecores.Contains(tcore)} tcoreloglen={ProcessorInfo.HardwareCores[tcore].LogicalCores.Length}");
                    if ((!systemInfo.IntelHybrid || (systemInfo.IntelHybrid && !systemInfo.Ecores.Contains(tcore))) && ProcessorInfo.HardwareCores[tcore].LogicalCores.Length > 1)
                    {
                        //App.LogDebug($"t2 add {tcore} logical={ProcessorInfo.HardwareCores[tcore].LogicalCores[1]}");
                        if (ProcessorInfo.HardwareCores[tcore].LogicalCores[1] == 1 && App.CoreZeroT1T0)
                        {
                            _runt0.Add(ProcessorInfo.HardwareCores[tcore].LogicalCores[1]);

                        }
                        else
                        {
                            _runt1.Add(ProcessorInfo.HardwareCores[tcore].LogicalCores[1]);
                        }
                    }
                }

                //TIER 3 E-Core if Hybrid, First Thread
                for (int j = 0; j < CPPC.Length; ++j)
                {
                    tcore = CPPC[j];
                    if (systemInfo.IntelHybrid && systemInfo.Ecores.Contains(tcore))
                    {
                        if (systemInfo.bECoresLast)
                        {
                            _runt1.Add(ProcessorInfo.HardwareCores[tcore].LogicalCores[0]);
                        }
                        else
                        {
                            _runt0.Add(ProcessorInfo.HardwareCores[tcore].LogicalCores[0]);
                        }
                    }
                }

                //TIER 4 E-Core if Hybrid, Second Thread
                for (int j = 0; j < CPPC.Length; ++j)
                {
                    tcore = CPPC[j];
                    if (systemInfo.IntelHybrid && systemInfo.Ecores.Contains(tcore) && ProcessorInfo.HardwareCores[tcore].LogicalCores.Length > 1)
                    {
                        if (systemInfo.bECoresLast)
                        {
                            _runt1.Add(ProcessorInfo.HardwareCores[tcore].LogicalCores[1]);
                        }
                        else
                        {
                            _runt0.Add(ProcessorInfo.HardwareCores[tcore].LogicalCores[1]);
                        }
                    }
                }

                LogInfo($"E-Cores Last: {systemInfo.bECoresLast}");
                LogInfo($"Tiered T0: {String.Join(", ", _runt0.ToArray())}");
                LogInfo($"Tiered T1: {String.Join(", ", _runt1.ToArray())}");
            }
            catch (Exception ex)
            {
                LogExError($"GetTieredListsThreads exception: {ex.Message}", ex);
            }
            return (_runt0, _runt1);
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
        public static void TraceLogging(bool enable)
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
        public static void InfoLogging(bool enable)
        {
            if (enable && !LogManager.Configuration.LoggingRules.Contains(logfileRule))
            {
                LogManager.Configuration.LoggingRules.Add(logfileRule);
                LogManager.ReconfigExistingLoggers();
            }
            else if (!enable && LogManager.Configuration.LoggingRules.Contains(logfileRule))
            {
                LogManager.Configuration.LoggingRules.Remove(logfileRule);
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

                // NO POWER SAVING
                //SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_SYSTEM_REQUIRED);

                Version _version = Assembly.GetExecutingAssembly().GetName().Version;
                version = string.Format("v{0}.{1}.{2}", _version.Major, _version.Minor, _version.Build);
                _versionInfo = string.Format("{0}.{1}.{2}", _version.Major, _version.Minor, _version.Build);

                if (OSVersion.GetOSVersion().Version.Major >= 10) App.Win10 = true;
                if (OSVersion.GetOSVersion().Version.Major >= 10 && OSVersion.GetOSVersion().Version.Build >= 22000) App.Win11 = true;

                LogInfo($"CPUDoc Version {_versionInfo}");

                cpuTotalLoad = new MovingAverage(4);
                cpuTotalLoadLong = new MovingAverage(16);

                bool cpuloadperfcount = ProcessorInfo.CpuLoadInit();
                LogInfo($"CPULoad using Performance Counters: {cpuloadperfcount}");

                systemInfo = new();

                systemInfo.AppVersion = version;

                SettingsInit();

                PSAInit();

                InitColors();

                TBStart();
                HWMStart();
                //UIStart();

                SetActiveConfig(0);

                TraceLogging(AppSettings.LogTrace);
                InfoLogging(AppSettings.LogInfo);


                tbIcon = (TaskbarIcon)FindResource("tbIcon");

                tbIcon.ToolTip = $"CPUDoc {App.version}";
                tbIcon.Icon = new Icon(Application.GetResourceStream(new Uri("pack://application:,,,/images/cpudoc.ico")).Stream);

                SetLastT0Affinity();

                AutoUpdater.ReportErrors = false;
                AutoUpdater.InstalledVersion = new Version(App._versionInfo);
                AutoUpdater.DownloadPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                AutoUpdater.RunUpdateAsAdmin = false;
                AutoUpdater.Synchronous = false;
                AutoUpdater.ParseUpdateInfoEvent += AutoUpdaterOnParseUpdateInfoEvent;

                autimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                AUNotifications(App.AppSettings.AUNotifications);
                autimer.Tick += (sender, args) =>
                {
                    autimer.Interval = TimeSpan.FromSeconds(3600);
                    AutoUpdater.Start(AutoUpdaterUrl);
                };

                UAStamp = DateTime.Now;

                Processes.Init();

                SubscribeGlobal();

                /*

                powerManager = PowerManagerProvider.CreatePowerManager();
                plans = powerManager.GetPlans();

                keyboardWatcher = eventHookFactory.GetKeyboardWatcher();
                keyboardWatcher.Start();
                keyboardWatcher.OnKeyInput += UAListener.EHookKeyPress;
                keyboardWatcher.OnKeyInput += EHookKeyPress;

                mouseWatcher = eventHookFactory.GetMouseWatcher();
                mouseWatcher.Start();
                mouseWatcher.OnMouseInput += EHookMouseDown;
                mouseWatcher.OnMouseInput += UAListener.EHookMouseDown;
              
                using (var eventHookFactory = new EventHookFactory())
                {
                    var keyboardWatcher = eventHookFactory.GetKeyboardWatcher();
                    keyboardWatcher.Start();
                    keyboardWatcher.OnKeyInput += (s, e) =>
                    {
                        Console.WriteLine("Key {0} event of key {1}", e.KeyData.EventType, e.KeyData.Keyname);
                        textBox1.AppendText(string.Format("Key {0} event of key {1}", e.KeyData.EventType, e.KeyData.Keyname));
                        SetEvent(evtUserActivity);
                    };

                    var mouseWatcher = eventHookFactory.GetMouseWatcher();
                    mouseWatcher.Start();
                    mouseWatcher.OnMouseInput += (s, e) =>
                    {
                        //Console.WriteLine("Mouse event {0} at point {1},{2}", e.Message.ToString(), e.Point.x, e.Point.y);
                        //textBox1.AppendText(string.Format("Mouse event {0} at point {1},{2}", e.Message.ToString(), e.Point.x, e.Point.y));
                        SetEvent(evtUserActivity);
                    };

                }
                */
                using (var eventHookFactory = new EventHook.EventHookFactory())
                {
                    applicationWatcher = eventHookFactory.GetApplicationWatcher();
                    applicationWatcher.Start();
                    applicationWatcher.OnApplicationWindowChange += CheckApplication;
                    //keyboardWatcher = eventHookFactory.GetKeyboardWatcher();
                    //keyboardWatcher.Start();
                    //keyboardWatcher.OnKeyInput += CheckHotKey;
                }
                /*
                applicationWatcher = eventHookFactory.GetApplicationWatcher();
                applicationWatcher.Start();
                applicationWatcher.OnApplicationWindowChange += CheckApplication;
                /*
                applicationWatcher.OnApplicationWindowChange += (s, e) =>
                {
                    Debug.WriteLine($"Application window of '{e.ApplicationData.AppName}' with the title '{e.ApplicationData.AppTitle}' was {e.Event}");
                };
                    */

                //App.LogDebug($"OnStartup App End");
            }
            catch (Exception ex)
            {
                LogExError($"OnStartup exception: {ex.Message}", ex);
            }
        }
        public static void AUNotifications(bool enable)
        {
            if (enable) autimer.Start();
            if (!enable) autimer.Stop();
        }
        public void PSAInit()
        {
            boot_ppname = powerManager.GetActivePlanFriendlyName();
            boot_ppguid = powerManager.GetActiveGuid();
            App.LogDebug($"Power Plan at boot: {boot_ppname}");
        }
        public static void PSAPlanDisable()
        {
            if (psact_plan)
            {
                bool isactive = powerManager.SetActiveGuid(boot_ppguid);
                psact_plan = false;
                if (isactive)
                {
                    App.LogDebug($"PSA Disabled Power Plan set back to: {boot_ppname}");
                }
                else
                {
                    App.LogDebug($"PSA Disabled Power Plan failed to set back to original, active is: {powerManager.GetActivePlanFriendlyName()}");
                }
            }
        }
        public static void PSAEnable()
        {
            try
            {
                if (App.pactive.PowerSaverActive)
                {
                    if (powerManager.GetActiveGuid() != PPGuid)
                    {
                        if (!powerManager.PlanExists(PPGuid)) ImportPowerPlan();
                        bool isactive = powerManager.SetActiveGuid(PPGuid);
                        if (isactive)
                        {
                            psact_plan = true;
                            App.LogInfo($"CPUDoc Dynamic Power Plan enabled: {powerManager.GetActivePlanFriendlyName()}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.LogExError("PowerSaverActive Init Exception:", ex);
            }
        }
        private void CheckApplication(object sender, EventHook.ApplicationEventArgs e)
        {
            //App.LogDebug($"{(int)e.ApplicationData.HWnd} {e.ApplicationData.AppPath} Application window of '{e.ApplicationData.AppName}' with the title '{e.ApplicationData.AppTitle}' was {e.Event}");

            uint processId = 0;
            GetWindowThreadProcessId(e.ApplicationData.HWnd, out processId);

            if (e.Event == EventHook.ApplicationEvents.Launched) Processes.AppLaunched(Path.GetFileNameWithoutExtension(e.ApplicationData.AppPath), (int)processId);

            if (e.Event == EventHook.ApplicationEvents.Closed) Processes.AppClosed(Path.GetFileNameWithoutExtension(e.ApplicationData.AppPath), (int)processId);

            App.LogDebug($"[PID={processId}] AppBin={Path.GetFileNameWithoutExtension(e.ApplicationData.AppPath)} Application window of '{e.ApplicationData.AppName}' with the title '{e.ApplicationData.AppTitle}' was {e.Event}");

        }

        private void CheckHotKey(object sender, EventHook.KeyInputEventArgs e)
        {
            App.LogDebug($"Key {e.KeyData.EventType} event of key {e.KeyData.Keyname}");

        }
        private void AutoUpdaterOnParseUpdateInfoEvent(ParseUpdateInfoEventArgs args)
        {
            dynamic json = JsonConvert.DeserializeObject(args.RemoteData);
            args.UpdateInfo = new UpdateInfoEventArgs
            {
                CurrentVersion = json.version,
                ChangelogURL = json.changelog,
                DownloadURL = json.url,
                Mandatory = new Mandatory
                {
                    Value = json.mandatory.value,
                    UpdateMode = json.mandatory.mode,
                    MinimumVersion = json.mandatory.minVersion
                },
                CheckSum = new CheckSum
                {
                    Value = json.checksum.value,
                    HashingAlgorithm = json.checksum.hashingAlgorithm
                }
            };
            App.systemInfo.SetLastVersionOnServer($"{args.UpdateInfo.CurrentVersion} @ {DateTime.Now:dddd, dd MMMM yyyy HH:mm:ss}");

        }

        public static void HWMStart()
        {
            thrMonitor = new Thread(RunHWM);
            thridMonitor = thrMonitor.ManagedThreadId;
            thrMonitor.Priority = ThreadPriority.AboveNormal;

            hwmtimer.Enabled = false;

            thrMonitor.Start();
            hwmtimer.Start();

            //App.LogDebug($"HWMonitor Started PID={thridMonitor} ALIVE={thrMonitor.IsAlive}");

        }
        public static void TBStart()
        {
            thrThreadBooster = new Thread(RunTB);
            thridThreadBooster = thrThreadBooster.ManagedThreadId;
            thrThreadBooster.Priority = ThreadPriority.AboveNormal;

            thrSys = new Thread(RunSysMask);
            thridSys = thrSys.ManagedThreadId;
            thrSys.Priority = ThreadPriority.AboveNormal;

            tbtimer.Enabled = false;
            systimer.Enabled = false;

            thrThreadBooster.Start();
            thrSys.Start();
            //tbtimer.Start();

            //App.LogDebug($"ThreadBooster Started PID={thridMonitor} ALIVE={thrMonitor.IsAlive}");

        }

        public static void TbSetStart(bool enable = true)
        {

            if (enable)
            {
                tbtimer.Enabled = true;
            }
            else
            {
                tbtimer.Enabled = false;
                Thread.Sleep(500);
                SetSysCpuSet();
            }

            //App.LogDebug($"ThreadBooster Started PID={thridMonitor} ALIVE={thrMonitor.IsAlive}");

        }

        public static void UIStart()
        {
            thrUpdateUI = new Thread(RunUI);
            thridUpdateUI = thrUpdateUI.ManagedThreadId;

            uitimer.Enabled = false;

            thrUpdateUI.Start();
            uitimer.Start();

            //App.LogDebug($"ThreadBooster Started PID={thridMonitor} ALIVE={thrMonitor.IsAlive}");

        }

        public static void SettingsInit()
        {
            try
            {
                systemInfo.CPPCActiveOrder = systemInfo.CPPCOrder;
                systemInfo.CPPCActiveLabel = systemInfo.CPPCLabel;

                systemInfo.bECores = CPUDoc.Properties.Settings.Default.ECores;
                systemInfo.bECoresLast = CPUDoc.Properties.Settings.Default.ECoresLast;

                List<int> _runlogicals = new();
                List<int> _runcores = new();

                (_runlogicals, _runcores) = GetTieredLists();

                logicalsT0 = new();
                logicalsT1 = new();

                n0enabledT0 = new();
                n0enabledT1 = new();

                n0disabledT0 = new();
                n0disabledT1 = new();

                (logicalsT0, logicalsT1) = GetTieredListsThreads();

                //SetLastTieredThreadAffinity(logicalsT0);

                _runlogicals = null;
                _runcores = null;

                ProtBufSettings.ReadSettings();

                App.LogDebug($"p0 tba {AppConfigs[0].ThreadBooster}");

                pactive = AppConfigs[0];

                App.LogDebug($"pact tba {pactive.ThreadBooster}");

                //systemInfo.TBAutoStart = CPUDoc.Properties.Settings.Default.ThreadBooster;

            }
            catch (Exception ex)
            {
                LogExError($"SettingsInit exception: {ex.Message}", ex);
            }
        }

        public static void AddConfig(int _id, bool enabled = false, string description = "") {
            AppConfigs.Add(new appConfigs()
            {
                id = _id,
                Enabled = enabled,
                Description = description,
            });
        }
        private static void Monitor_ElapsedEventHandler(object sender, ElapsedEventArgs e)
        {
            //App.LogDebug("HWM ELAPSED");
            int sync = Interlocked.CompareExchange(ref InterlockHWM, 1, 0);
            if (sync == 0)
            {
                HWMonitor.OnHWM(sender, e);
                InterlockHWM = 0;
                //App.LogDebug("HWM ELAPSED ON");
            }
        }
        private static void ThreadBooster_ElapsedEventHandler(object sender, ElapsedEventArgs e)
        {
            //App.LogDebug("TB ELAPSED");
            int sync = Interlocked.CompareExchange(ref InterlockTB, 1, 0);
            if (sync == 0)
            {
                ThreadBooster.OnThreadBooster(sender, e);
                InterlockTB = 0;
                //App.LogDebug("TB ELAPSED ON");
            }
        }
        private static void UpdateUI_ElapsedEventHandler(object sender, ElapsedEventArgs e)
        {
            //App.LogDebug("UI ELAPSED");
            int sync = Interlocked.CompareExchange(ref InterlockUI, 1, 0);
            if (sync == 0)
            {
                OnUpdateUI(sender, e);
                InterlockUI = 0;
                //App.LogDebug("UI ELAPSED ON");
            }
        }

        private static void SysMask_ElapsedEventHandler(object sender, ElapsedEventArgs e)
        {
            //App.LogDebug("SysMask ELAPSED");
            int sync = Interlocked.CompareExchange(ref InterlockSys, 1, 0);
            if (sync == 0)
            {
                ThreadBooster.OnSysCpuSet(sender, e);
                InterlockSys = 0;
                //App.LogDebug("SysMask ELAPSED ON");
            }
        }

        private static void OnUpdateUI(object sender, ElapsedEventArgs e)
        {
            if (thrThreadBooster.ThreadState == System.Threading.ThreadState.Running || thrThreadBooster.ThreadState == System.Threading.ThreadState.Background)
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

            App.systemInfo.RefreshLabels();
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
        public static void RunTB()
        {

            //App.LogDebug("RUN TB");
            tbtimer.Interval = ThreadBooster.PoolingInterval;
            tbtimer.Elapsed += new ElapsedEventHandler(ThreadBooster_ElapsedEventHandler);
            if (tbtimer.Enabled)
            {
                //App.LogDebug("START TB");
                tbtimer.Start();
            }

        }
        public static void RunUI()
        {

            App.LogDebug("RUN TUIB");
            uitimer.Interval = 1000;
            uitimer.Elapsed += new ElapsedEventHandler(UpdateUI_ElapsedEventHandler);
            if (uitimer.Enabled)
            {
                App.LogDebug("START UI");
                uitimer.Start();
            }

        }
        public static void RunSysMask()
        {

            //App.LogDebug("RUN SysMask");
            systimer.Interval = SysMaskPooling;
            systimer.Elapsed += new ElapsedEventHandler(SysMask_ElapsedEventHandler);
            if (systimer.Enabled)
            {
                //App.LogDebug("START SysMask");
                systimer.Start();
            }

        }
        private void SubscribeGlobal()
        {
            Unsubscribe();
            Subscribe(Hook.GlobalEvents());
        }
        private void OnKeyPressDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            //system.windows.forms instead input
            //UAStamp = DateTime.Now;
            //int error = Marshal.GetLastWin32Error();
            //if (error > 0) Log(string.Format("Key  \t\t {0}\n", e.KeyCode));
            //App.LogInfo($"KeyDown  \t\t {e.KeyCode}");
        }

        private void OnKeyPressUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            //system.windows.forms instead input
            //UAStamp = DateTime.Now;
            //int error = Marshal.GetLastWin32Error();
            //if (error > 0) Log(string.Format("Key  \t\t {0}\n", e.KeyCode));
            //App.LogInfo($"KeyUp  \t\t {e.KeyCode}");
        }
        private void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            UAStamp = DateTime.Now;
            //int error = Marshal.GetLastWin32Error();
            //if (error > 0) Log(string.Format("Mouse \t\t {0}\n", e.Button));
            //if (error > 0) Log(string.Format("Error \t\t {0}\n", error.ToString()));
            //Debug.WriteLine("Key  \t\t {e.Button}");
        }
        private void Subscribe(IKeyboardMouseEvents events)
        {

            m_Events = events;

            m_Events.KeyDown += OnKeyPressDown;
            m_Events.KeyUp += OnKeyPressUp;
            /*
            m_Events.MouseUp += OnMouseMove;
            m_Events.MouseDown += OnMouseMove;
            m_Events.MouseClick += OnMouseMove;
            m_Events.MouseDoubleClick += OnMouseMove;
            m_Events.MouseWheel += OnMouseMove;

            m_Events.MouseMove += OnMouseMove;
            */
        }
        private void Unsubscribe()
        {
            if (m_Events == null) return;
            m_Events.KeyDown -= OnKeyPressDown;
            m_Events.KeyUp -= OnKeyPressUp;

            /*
            m_Events.MouseUp -= OnMouseMove;
            m_Events.MouseClick -= OnMouseMove;
            m_Events.MouseDoubleClick -= OnMouseMove;

            m_Events.MouseMove -= OnMouseMove;
            */

            m_Events.Dispose();
            m_Events = null;
        }


        private void Application_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                //keyboardWatcher.Stop();
                //mouseWatcher.Stop();
                //clipboardWatcher.Stop();
                applicationWatcher.Stop();
                //printWatcher.Stop();
                //Unsubscribe();
                Unsubscribe();
                eventHookFactory.Dispose();

                HWMonitor.Close();
                ThreadBooster.Close();
                hwmcts?.Dispose();
                tbcts?.Dispose();
                syscts?.Dispose();
                tbIcon.Dispose();

            }
            catch (Exception ex)
            {
                LogExWarn($"Application_Exit exception: {ex.Message}", ex);
            }
            try
            {
                //RESTORE POWER SAVING
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
                SetSystemCpuSet(0);
                UnloadModule("CPUDoc.sys");
                UnloadModule("CPUTools.dll");
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
        public static void Set0T1Affinity()
        {
            try
            {
                using (Process thisprocess = Process.GetCurrentProcess())
                {
                    if (thisprocess.ProcessorAffinity != (IntPtr)(1L << 1))
                        thisprocess.ProcessorAffinity = (IntPtr)(1L << 1);
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
                }
            }
            catch (Exception ex)
            {
                LogExWarn($"Set0T1Affinity exception: {ex.Message}", ex);
            }
        }
        public static void SetLastT0Affinity()
        {
            try
            {
                int thread = App.GetLastThreadT0();
                using (Process thisprocess = Process.GetCurrentProcess())
                {
                    if (thisprocess.ProcessorAffinity != (IntPtr)(1L << thread))
                        thisprocess.ProcessorAffinity = (IntPtr)(1L << thread);
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
                }
            }
            catch (Exception ex)
            {
                LogExWarn($"SetLastT0Affinity exception: {ex.Message}", ex);
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
        public static void ImportPowerPlan()
        {
            try
            {
                var cmd = new Process { StartInfo = { FileName = "powercfg" } };
                using (cmd)
                {
                    string planName = "CPUDocDynamicW10_v1.pow";
                    if (App.Win11)
                        planName = "CPUDocDynamicW11_v1.pow";

                    var inputPath = Path.Combine(Environment.CurrentDirectory, planName);

                    //This hides the resulting popup window
                    cmd.StartInfo.CreateNoWindow = true;
                    cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    //Import the new power plan
                    cmd.StartInfo.Arguments = $"-import \"{inputPath}\" {PPGuid}";
                    cmd.Start();

                    cmd.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                LogExError($"ImportPowerPlan exception: {ex.Message}", ex);
            }

        }
        public static void ExecuteCmd(string exe, string arguments, string path)
        {
            var cmd = new Process { StartInfo = { FileName = exe, WorkingDirectory = path } };
            using (cmd)
            {
                cmd.StartInfo.CreateNoWindow = true;
                cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                cmd.StartInfo.Arguments = arguments;
                cmd.Start();

                cmd.WaitForExit();
            }
        }

        public static void SetActiveConfig(int id = -1)
        {
            if (id >= 0) 
                pactive = AppConfigs[id];

            if (Win10)
            {
                systimer.Enabled = true;
                hwmtimer.Enabled = true;
            }
            else
            {
                systimer.Enabled = false;
                hwmtimer.Enabled = false;
                pactive.NumaZero = false;
                pactive.SysSetHack = false;
            }

            if (pactive.ThreadBooster)
            {
                tbtimer.Enabled = true;
            }
            else
            {
                tbtimer.Enabled = false;
                systimer.Enabled = false;
                hwmtimer.Enabled = false;
            }

            if (!pactive.SysSetHack && !pactive.NumaZero)
            {
                lastSysCpuSetMask = 0;
                SetSysCpuSet(0);
                systimer.Enabled = false;
                systimer.Stop();
            }
            else if (systimer.Enabled)
            {
                systimer.Start();
            }

            numazero_b = false;

            n0enabledT0 = new List<int>();
            n0enabledT1 = new List<int>();
            n0disabledT0 = new List<int>();
            n0disabledT1 = new List<int>();

            if (pactive.NumaZero)
            {
                int _found = 0;

                if (pactive.NumaZeroType == 0)
                {
                    for (int i = 0; i < ProcessorInfo.LogicalCoresCount; ++i)
                    {
                        int[][] _cores = ProcessorInfo.CoresNumaZero();
                        for (int c = 0; c < _cores.Count(); ++c)
                        {
                            if (_cores[c] != null)
                            {
                                if (_cores[c][0] == i)
                                {
                                    if (logicalsT1.Contains(i)) n0enabledT1.Add(i);
                                    if (logicalsT0.Contains(i)) n0enabledT0.Add(i);
                                }
                            }
                        }
                        _cores = ProcessorInfo.CoresEfficient();
                        for (int c = 0; c < _cores.Count(); ++c)
                        {
                            if (_cores[c] != null)
                            {
                                if (_cores[c][0] == i)
                                {
                                    _found++;
                                    if (logicalsT1.Contains(i)) n0disabledT1.Add(i);
                                    if (logicalsT0.Contains(i)) n0disabledT0.Add(i);
                                    App.LogDebug($"N0 DISABLED CoresEfficient={i} T0={logicalsT0.Contains(i)} T1={logicalsT1.Contains(i)}");
                                }
                            }
                        }
                        _cores = ProcessorInfo.CoresCache();
                        for (int c = 0; c < _cores.Count(); ++c)
                        {
                            if (_cores[c] != null)
                            {
                                if (_cores[c][0] == i)
                                {
                                    _found++;
                                    if (logicalsT1.Contains(i)) n0disabledT1.Add(i);
                                    if (logicalsT0.Contains(i)) n0disabledT0.Add(i);
                                    App.LogDebug($"N0 DISABLED CoresCache={i} T0={logicalsT0.Contains(i)} T1={logicalsT1.Contains(i)}");
                                }
                            }
                        }
                        _cores = ProcessorInfo.CoresIndex();
                        for (int c = 0; c < _cores.Count(); ++c)
                        {
                            if (_cores[c] != null)
                            {
                                if (_cores[c][0] == i)
                                {
                                    _found++;
                                    if (logicalsT1.Contains(i)) n0disabledT1.Add(i);
                                    if (logicalsT0.Contains(i)) n0disabledT0.Add(i);
                                    App.LogDebug($"N0 DISABLED CoresIndex={i} T0={logicalsT0.Contains(i)} T1={logicalsT1.Contains(i)}");
                                }
                            }
                        }
                    }
                }

                if (_found > 0) numazero_b = true;
                App.LogDebug($"N0={pactive.NumaZero} N0Type={pactive.NumaZeroType} CORES={ProcessorInfo.PhysicalCoresCount-_found}/{ProcessorInfo.PhysicalCoresCount}[{_found}] HT={systemInfo.HyperThreading}");

                if (pactive.NumaZeroType > 0 || _found < 1) 
                {
                    int _forced = pactive.NumaZeroType == 1 ? 8 : pactive.NumaZeroType == 2 ? 6 : pactive.NumaZeroType == 3 ? 4 : pactive.NumaZeroType == 4 ? 2 : 0;
                    int _auto = ProcessorInfo.PhysicalCoresCount > 12 ? 8 : ProcessorInfo.PhysicalCoresCount == 12 ? 6 : ProcessorInfo.PhysicalCoresCount - 2;                    
                    _forced = pactive.NumaZeroType == 0 ? _auto : _forced;
                    if (_forced > 0)
                    {
                        numazero_b = true;
                        if (systemInfo.HyperThreading) _forced = _forced * 2;
                        App.LogDebug($"N0={pactive.NumaZero} N0Type={pactive.NumaZeroType} FORCED={_forced} HT={systemInfo.HyperThreading}");
                        for (int i = 0; i < ProcessorInfo.LogicalCoresCount; ++i)
                        {
                            if (i > _forced - 1)
                            {
                                if (logicalsT1.Contains(i))
                                {
                                    n0disabledT1.Add(i);
                                    App.LogDebug($"N0 Disabled T1={i}");
                                }
                                else if (logicalsT0.Contains(i))
                                {
                                    n0disabledT0.Add(i);
                                    App.LogDebug($"N0 Disabled T0={i}");
                                }
                            }
                            else
                            {
                                if (logicalsT1.Contains(i))
                                {
                                    n0enabledT1.Add(i);
                                    App.LogDebug($"N0 Enabled T1={i}");
                                }
                                else if (logicalsT0.Contains(i))
                                {
                                    n0enabledT0.Add(i);
                                    App.LogDebug($"N0 Enabled T0={i}");
                                }
                            }
                        }
                    }
                }
            }

            if (n0disabledT0.Count() > 0 || n0disabledT1.Count() > 0 && pactive.NumaZero) numazero_b = true;

            App.systemInfo.SetSSHStatus(pactive.SysSetHack);
            App.systemInfo.SetPSAStatus(pactive.PowerSaverActive);
            App.systemInfo.SetN0Status(pactive.NumaZero);

            App.LogDebug($"N0={pactive.NumaZero} N0Active={numazero_b} HT={systemInfo.HyperThreading}");

            ThreadBooster.bInit = false;

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
