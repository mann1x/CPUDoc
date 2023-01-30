using AutoUpdaterDotNET;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
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
using System.Windows.Threading;
using Gma.System.MouseKeyHook;
using net.r_eg.Conari;
using net.r_eg.Conari.Accessors;
using net.r_eg.Conari.Types;
using OSVersionExtension;
using CPUDoc.Windows;
using System.Management;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;
using System.Security;
using net.r_eg.Conari.Core;
using System.ServiceProcess;
using Microsoft.Extensions.Hosting.WindowsServices;
using static LibreHardwareMonitor.Interop.AdvApi32;
using Newtonsoft.Json.Linq;
using PowerManagerAPI;

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

        public static Logger logger = LogManager.GetCurrentClassLogger();
        public static bool logger_b = false;

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

        public static ulong TimerEffectiveus;
        public static uint TimerResolution = 1;
        public static bool TimerResolution_b = false;

        public static int InterlockHWM = 0;
        public static int InterlockTB = 0;
        public static int InterlockSys = 0;

        public static CancellationTokenSource hwmcts = new CancellationTokenSource();
        public static CancellationTokenSource tbcts = new CancellationTokenSource();
        public static CancellationTokenSource syscts = new CancellationTokenSource();

        public static ManualResetEventSlim mreshwm = new ManualResetEventSlim(false);
        public static ManualResetEventSlim mrestb = new ManualResetEventSlim(false);
        public static ManualResetEventSlim mressys = new ManualResetEventSlim(false);

        public static System.Timers.Timer hwmtimer = new System.Timers.Timer();
        public static System.Timers.Timer tbtimer = new System.Timers.Timer();
        public static System.Timers.Timer systimer = new System.Timers.Timer();

        public static Thread thrMonitor;
        public static int thridMonitor;
        public static Thread thrThreadBooster;
        public static int thridThreadBooster;
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
        public static Guid PPGuidV1 = new Guid("FBB9C3D1-AF6E-46CF-B02B-C411928D1BE1");
        public static Guid PPGuid = new Guid("FBB9C3D1-AF6E-46CF-B02B-C411928D2CE0");
        public static string PPGuidV2B11 = "FBB9C3D1-AF6E-46CF-B02B-C411928D2CE1";
        public static string PPNameV2B11 = "CPUDocDynamicW11_v2_Balanced.pow";
        public static string PPGuidV2U11 = "FBB9C3D1-AF6E-46CF-B02B-C411928D2CE2";
        public static string PPNameV2U11 = "CPUDocDynamicW11_v2_Ultimate.pow";
        public static string PPGuidV2B10 = "FBB9C3D1-AF6E-46CF-B02B-C411928D2CE3";
        public static string PPNameV2B10 = "CPUDocDynamicW10_v2_Balanced.pow";
        public static string PPGuidV2U10 = "FBB9C3D1-AF6E-46CF-B02B-C411928D2CE4";
        public static string PPNameV2U10 = "CPUDocDynamicW10_v2_Ultimate.pow";

        public static string boot_ppname;
        public static Guid? boot_ppguid = null;

        public static bool psact_b = false;
        public static bool psact_plan = false;
        public static bool psact_light_b = false;
        public static bool psact_deep_b = false;
        public static int PPImportErrCnt = 0;
        public static bool PPImportErrStatus = false;
        public static bool numazero_b = false;

        public static int PPDutyCycling = 1;
        public static int PPUSBSSuspend = 0;
        public static int PPUSBSSuspendTimeout = 2000;

        public static bool IsHibernateAllowed = false;
        public static bool IsSuspendAllowed = false;

        public static MovingAverage cpuTotalLoad;
        public static MovingAverage cpuTotalLoadLong;

        public static MovingAverage TBPoolingAverage;

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

        private static IDictionary<uint, string> _map;
        private static ManagementEventWatcher _processStartedWatcher;
        private static ManagementEventWatcher _processStoppedwatcher;

        // © Rafael Rivera
        // License: MIT

        [ComImport]
        [Guid("F53321FA-34F8-4B7F-B9A3-361877CB94CF")]
        public class QuietHoursSettings { }

        [Guid("6BFF4732-81EC-4FFB-AE67-B6C1BC29631F")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IQuietHoursSettings
        {
            string UserSelectedProfile
            {
                [return: MarshalAs(UnmanagedType.LPWStr)]
                get;
                [param: MarshalAs(UnmanagedType.LPWStr)]
                set;
            }

            IQuietHoursProfile GetProfile([MarshalAs(UnmanagedType.LPWStr)] string profile);

            void GetAllProfileData(); /* Incomplete definition */

            [return: MarshalAs(UnmanagedType.LPWStr)]
            string GetDisplayNameForProfile([MarshalAs(UnmanagedType.LPWStr)] string profile);

            IntPtr QuietMomentsManager { get; } /* Incomplete definition */

            string OffProfileId { [return: MarshalAs(UnmanagedType.LPWStr)] get; }

            string ActiveQuietMomentProfile
            {
                [return: MarshalAs(UnmanagedType.LPWStr)]
                get;
                [param: MarshalAs(UnmanagedType.LPWStr)]
                set;
            }

            string ActiveProfile { [return: MarshalAs(UnmanagedType.LPWStr)] get; }

            IntPtr QuietHoursPinnedContactManager { get; } /* Incomplete definition */

            void SetQuietMomentsShowSummaryEnabled([MarshalAs(UnmanagedType.Bool)] bool isEnabled);

            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetQuietMomentsShowSummaryEnabled();

            IntPtr GetAlwaysAllowedApps(out uint count);

            void StartProcessing();

            void StopProcessing();
        }

        [Guid("e813fe81-62b6-417d-b951-9d2e08486ac1")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IQuietHoursProfile
        {
            [return: MarshalAs(UnmanagedType.LPWStr)]
            string GetDisplayName();

            [return: MarshalAs(UnmanagedType.LPWStr)]
            string GetProfileId();

            IntPtr GetSetting(int setting); /* Incomplete definition */

            IntPtr SetSetting(int setting, int value); /* Incomplete definition */

            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetIsCustomizable();

            void GetAllowedContacts(); /* Incomplete definition */

            void AddAllowedContact(); /* Incomplete definition */

            void RemoveAllowedContact(); /* Incomplete definition */

            IntPtr GetAllowedApps([Out] out uint numAllowedApps);

            void AddAllowedApp(); /* Incomplete definition */

            void RemoveAllowedApp(); /* Incomplete definition */

            void GetDescription(); /* Incomplete definition */

            void GetCustomizeLinkText(); /* Incomplete definition */

            void GetRestrictiveLevel(); /* Incomplete definition */
        }

        [Flags]
        public enum SERVICE_CONTROL : uint
        {
            STOP = 0x00000001,
            PAUSE = 0x00000002,
            CONTINUE = 0x00000003,
            INTERROGATE = 0x00000004,
            SHUTDOWN = 0x00000005,
            PARAMCHANGE = 0x00000006,
            NETBINDADD = 0x00000007,
            NETBINDREMOVE = 0x00000008,
            NETBINDENABLE = 0x00000009,
            NETBINDDISABLE = 0x0000000A,
            DEVICEEVENT = 0x0000000B,
            HARDWAREPROFILECHANGE = 0x0000000C,
            POWEREVENT = 0x0000000D,
            SESSIONCHANGE = 0x0000000E
        }

        public enum SERVICE_STATE : uint
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007
        }

        [Flags]
        public enum SERVICE_ACCEPT : uint
        {
            STOP = 0x00000001,
            PAUSE_CONTINUE = 0x00000002,
            SHUTDOWN = 0x00000004,
            PARAMCHANGE = 0x00000008,
            NETBINDCHANGE = 0x00000010,
            HARDWAREPROFILECHANGE = 0x00000020,
            POWEREVENT = 0x00000040,
            SESSIONCHANGE = 0x00000080,
        }

        [Flags]
        public enum SERVICE_ACCESS : uint
        {
            STANDARD_RIGHTS_REQUIRED = 0xF0000,
            SERVICE_QUERY_CONFIG = 0x00001,
            SERVICE_CHANGE_CONFIG = 0x00002,
            SERVICE_QUERY_STATUS = 0x00004,
            SERVICE_ENUMERATE_DEPENDENTS = 0x00008,
            SERVICE_START = 0x00010,
            SERVICE_STOP = 0x00020,
            SERVICE_PAUSE_CONTINUE = 0x00040,
            SERVICE_INTERROGATE = 0x00080,
            SERVICE_USER_DEFINED_CONTROL = 0x00100,
            SERVICE_ALL_ACCESS = 0xF003F
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SERVICE_STATUS
        {
            public long serviceType;
            public SERVICE_STATE currentState;
            public long controlsAccepted;
            public long win32ExitCode;
            public long serviceSpecificExitCode;
            public long checkPoint;
            public long waitHint;
        };

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "OpenSCManagerW", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr OpenSCManager(string MachineName, string DatabaseName, SERVICE_ACCESS DesiredAccess);

        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, EntryPoint = "OpenServiceA", SetLastError = true)]
        private static extern IntPtr OpenService(IntPtr hSCManager, string ServiceName, SERVICE_ACCESS DesiredAccess);

        [DllImport("advapi32.dll", EntryPoint = "CloseServiceHandle", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseServiceHandle(IntPtr hSCObject);

        [DllImport("advapi32.dll", EntryPoint = "QueryServiceStatus", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool QueryServiceStatus(IntPtr hService, ref SERVICE_STATUS ServiceStatus);

        [DllImport("advapi32.dll", EntryPoint = "DeleteService", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteService(IntPtr hService);

        [DllImport("advapi32.dll", EntryPoint = "ControlService", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ControlService(IntPtr hService, SERVICE_CONTROL Control, ref SERVICE_STATUS ServiceStatus);
        internal struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }
        
        [DllImport("powrprof.dll")]
        public static extern bool IsPwrHibernateAllowed();

        [DllImport("powrprof.dll")]
        public static extern bool IsPwrSuspendAllowed();

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
        [DllImport("ntdll.dll")]
        private static extern uint RtlAdjustPrivilege
        (
            int Privilege,
            bool bEnablePrivilege,
            bool IsThreadPrivilege,
            out bool PreviousValue
        );

        [DllImport(@"cputools.dll")]
        private static extern int SetSystemCpuSet(uint bitMask);

        [DllImport(@"cputools.dll")]
        public static extern void wsleep(uint us);

        [DllImport(@"cputools.dll")]
        private static extern int ResetSystemCpuSetProc(int pid, uint bitMask);

        [DllImport(@"cputools.dll")]
        public static extern ulong GetTimerResolution();

        [DllImport(@"cputools.dll")]
        public static extern ulong GetSysTimerResolution();

        [DllImport(@"cputools.dll")]
        public static extern double testTimerOverall(ulong duration_us, int timePeriod);

        [DllImport(@"cputools.dll")]
        public static extern double testTimerPerf(ulong duration_us, int iterations, int timePeriod, bool waitable);

        public enum MEMORY_PRIORITY_INFORMATION : int
        {
            MEMORY_PRIORITY_VERY_LOW = 1,
            MEMORY_PRIORITY_LOW = 2,
            MEMORY_PRIORITY_MEDIUM = 3,
            MEMORY_PRIORITY_BELOW_NORMAL = 4,
            MEMORY_PRIORITY_NORMAL = 5
        }
        public struct _MEMORY_PRIORITY_INFORMATION
        {
            public MEMORY_PRIORITY_INFORMATION memPrio { get; set; }
        }

        [DllImport(@"cputools.dll")]
        public static extern int setMemPrio(int pid, _MEMORY_PRIORITY_INFORMATION memPrio);

        public struct PROCESS_POWER_THROTTLING_STATE
        {
            public ulong Version { get; set; }
            public ulong ControlMask { get; set; }
            public ulong StateMask { get; set; }
        }

        [DllImport(@"kernel32.dll")]
        public static extern uint SetProcessInformation(IntPtr hWnd, int pclass, PROCESS_POWER_THROTTLING_STATE pinfo, int size);

        [DllImport(@"cputools.dll")]
        public static extern int setPowerThrottlingExecSpeed(int pid, bool enable);

        [DllImport(@"cputools.dll")]
        public static extern int setPowerThrottlingIgnoreTimer(int pid, bool enable);

        [DllImport(@"cputools.dll")]
        public static extern int resetPowerThrottling(int pid);

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
        public static bool IsInVisualStudio => LicenseManager.UsageMode == LicenseUsageMode.Designtime || Debugger.IsAttached == true || StringComparer.OrdinalIgnoreCase.Equals("devenv", Process.GetCurrentProcess().ProcessName);

        [DllImport("ntdll.dll")]
        private static extern uint NtQueryWnfStateData(IntPtr pStateName, IntPtr pTypeId, IntPtr pExplicitScope, out uint nChangeStamp, out IntPtr pBuffer, ref uint nBufferSize);


        [StructLayout(LayoutKind.Sequential)]

        public struct WNF_TYPE_ID
        {
            public Guid TypeId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WNF_STATE_NAME
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public uint[] Data;

            public WNF_STATE_NAME(uint Data1, uint Data2) : this()
            {
                uint[] newData = new uint[2];
                newData[0] = Data1;
                newData[1] = Data2;
                Data = newData;
            }
        }

        public enum FocusAssistResult
        {
            NOT_SUPPORTED = -2,
            FAILED = -1,
            OFF = 0,
            PRIORITY_ONLY = 1,
            ALARMS_ONLY = 2
        };

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

        [DllImport("shell32.dll")]
        static extern int SHQueryUserNotificationState(
        out QUERY_USER_NOTIFICATION_STATE pquns);

        public enum QUERY_USER_NOTIFICATION_STATE
        {
            QUNS_NOT_PRESENT = 1,
            QUNS_BUSY = 2,
            QUNS_RUNNING_D3D_FULL_SCREEN = 3,
            QUNS_PRESENTATION_MODE = 4,
            QUNS_ACCEPTS_NOTIFICATIONS = 5,
            QUNS_QUIET_TIME = 6,
            QUNS_FAILED = 7
        };

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int GetDeviceCaps(IntPtr hDC, int nIndex);

        public enum DeviceCap
        {
            VERTRES = 10,
            DESKTOPVERTRES = 117
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TIMECAPS
        {
            public uint wPeriodMin { get; set; }
            public uint wPeriodMax { get; set; }
        }

        public const uint TIMERR_BASE = 96;
        public const uint TIMERR_NOERROR = 0;                /* no error */
        public const uint TIMERR_NOCANDO = TIMERR_BASE + 1;  /* request not completed */
        public const uint TIMERR_STRUCT = TIMERR_BASE + 33;  /* time struct size */

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport("winmm.dll", EntryPoint = "timeGetDevCaps")]
        public static extern uint timeGetDevCaps(out TIMECAPS ptc, uint size);

        /// <summary>TimeBeginPeriod(). See the Windows API documentation for details.</summary>

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]

        public static extern uint TimeBeginPeriod(uint uMilliseconds);

        /// <summary>TimeEndPeriod(). See the Windows API documentation for details.</summary>

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]

        public static extern uint TimeEndPeriod(uint uMilliseconds);

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_POWER_INFORMATION
        {
            public uint MaxIdlenessAllowed;
            public uint Idleness;
            public uint TimeRemaining;
            public string CoolingMode;
        }

        [DllImport("PowrProf.dll", EntryPoint = "CallNtPowerInformation")]
        public static extern uint CallPowerInformationSPI(int Level, IntPtr inBuf, IntPtr szin, [Out] SYSTEM_POWER_INFORMATION outBuf, int szout);
        
        [DllImport("PowrProf.dll", EntryPoint = "CallNtPowerInformation")]
        private static extern uint CallPowerCapabilities(int Level, IntPtr inBuf, int szin, IntPtr outBuf, int szout);

        public static double GetWindowsScreenScalingFactor(bool percentage = true)
        {
            //Create Graphics object from the current windows handle
            Graphics GraphicsObject = Graphics.FromHwnd(IntPtr.Zero);
            //Get Handle to the device context associated with this Graphics object
            IntPtr DeviceContextHandle = GraphicsObject.GetHdc();
            //Call GetDeviceCaps with the Handle to retrieve the Screen Height
            int LogicalScreenHeight = GetDeviceCaps(DeviceContextHandle, (int)DeviceCap.VERTRES);
            int PhysicalScreenHeight = GetDeviceCaps(DeviceContextHandle, (int)DeviceCap.DESKTOPVERTRES);
            //Divide the Screen Heights to get the scaling factor and round it to two decimals
            double ScreenScalingFactor = Math.Round(PhysicalScreenHeight / (double)LogicalScreenHeight, 2);
            //If requested as percentage - convert it
            if (percentage)
            {
                ScreenScalingFactor *= 100.0;
            }
            //Release the Handle and Dispose of the GraphicsObject object
            GraphicsObject.ReleaseHdc(DeviceContextHandle);
            GraphicsObject.Dispose();
            //Return the Scaling Factor
            return ScreenScalingFactor;
        }
        const int ENUM_CURRENT_SETTINGS = -1;
        [DllImport("user32.dll")]
        static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public System.Windows.Forms.ScreenOrientation dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }
        public static bool IsForegroundWindowFullScreen(bool primaryonly)
        {
            IntPtr handle = GetForegroundWindow();
            if (handle == IntPtr.Zero) return false;

            W32RECT wRect;

            System.Windows.Forms.Screen scr = System.Windows.Forms.Screen.FromHandle(handle);
            int scrX = GetSystemMetrics(SM_CXSCREEN),
                scrY = GetSystemMetrics(SM_CYSCREEN);
            string _device = scr.DeviceName.Trim();

            foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
            {
                var dm = new DEVMODE();
                dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
                EnumDisplaySettings(screen.DeviceName, ENUM_CURRENT_SETTINGS, ref dm);

                //App.LogDebug($"dm0={screen.DeviceName} dm1={_device}");

                if (screen.DeviceName.Trim() == _device)
                {
                    if (!primaryonly || (primaryonly && scr.Primary))
                        {
                        if (!GetWindowRect(handle, out wRect)) return false;
                        //App.LogDebug($"scrX={(wRect.Right - wRect.Left)} scrY={(wRect.Bottom - wRect.Top)}");

                        scrX = dm.dmPelsWidth;
                        scrY = dm.dmPelsHeight;

                        return scrX == (wRect.Right - wRect.Left) && scrY == (wRect.Bottom - wRect.Top);
                    }
                }
                //App.LogDebug($"Device: {screen.DeviceName} Primary={screen.Primary}");
                //App.LogDebug($"Real Resolution: {dm.dmPelsWidth}x{dm.dmPelsHeight}");
                //App.LogDebug($"Virtual Resolution: {screen.Bounds.Width}x{screen.Bounds.Height}");
            }
            return false;

            //if (primaryonly)

            //double sf = GetWindowsScreenScalingFactor(false);

            /*
            if (!scr.Primary && !primaryonly)
            {
                var dm = new DEVMODE();
                dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
                EnumDisplaySettings(scr.DeviceName, ENUM_CURRENT_SETTINGS, ref dm);
                scrX = dm.dmPelsWidth;
                scrY = dm.dmPelsHeight;
            }

            App.LogDebug($"Screen={scr.DeviceName} ScreenPrimary={scr.Primary} scrX={scrX} scrX={scrY}");
            
            foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
            {
                var dm = new DEVMODE();
                dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
                EnumDisplaySettings(screen.DeviceName, ENUM_CURRENT_SETTINGS, ref dm);

                App.LogDebug($"Device: {screen.DeviceName} Primary={screen.Primary}");
                App.LogDebug($"Real Resolution: {dm.dmPelsWidth}x{dm.dmPelsHeight}");
                App.LogDebug($"Virtual Resolution: {screen.Bounds.Width}x{screen.Bounds.Height}");
            }
            

            W32RECT wRect;
            if (!GetWindowRect(handle, out wRect)) return false;
            //App.LogDebug($"scrX={(wRect.Right - wRect.Left)} scrY={(wRect.Bottom - wRect.Top)}");

            return scrX == (wRect.Right - wRect.Left) && scrY == (wRect.Bottom - wRect.Top);
            */
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
                Mode = BindingMode.OneWay;
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
        public static int ProcSetCpuSet(int pid, uint BitMask = 0)
        {
            int _ret = 0;
            using (var lam = new ConariL(@"cputools.dll"))
            {
                _ret = lam.DLR.ResetSystemCpuSetProc<int>(pid, BitMask);
            }
            /*
            _ret = ProcSetCpuSet(pid, BitMask);
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
        public static FocusAssistResult GetFocusAssistState()
        {
            //  Focus Assist:   Windows 10 build >= 17083
            WNF_STATE_NAME WNF_SHEL_QUIETHOURS_ACTIVE_PROFILE_CHANGED = new WNF_STATE_NAME(0xA3BF1C75, 0xD83063E);
            uint nBufferSize = (uint)Marshal.SizeOf(typeof(IntPtr));
            IntPtr pStateName = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WNF_STATE_NAME)));
            Marshal.StructureToPtr(WNF_SHEL_QUIETHOURS_ACTIVE_PROFILE_CHANGED, pStateName, false);
            bool success = NtQueryWnfStateData(pStateName, IntPtr.Zero, IntPtr.Zero, out uint nChangeStamp, out IntPtr pBuffer, ref nBufferSize) == 0;
            Marshal.FreeHGlobal(pStateName);
            if (success)
            {
                return (FocusAssistResult)pBuffer;
            }
            return FocusAssistResult.FAILED;
        }
        public static bool IsAvailableUserNotification()
        {
            //  Quiet Hours:   Windows 10 build < 17083
            QUERY_USER_NOTIFICATION_STATE state;
            bool success = SHQueryUserNotificationState(out state) == 0;
            return success;
        }
        public static QUERY_USER_NOTIFICATION_STATE GetUserNotificationState()
        {
            //  Quiet Hours:   Windows 10 build < 17083
            QUERY_USER_NOTIFICATION_STATE state;
            bool success = SHQueryUserNotificationState(out state) == 0;
            if (success)
            {
                return (QUERY_USER_NOTIFICATION_STATE)state;
            }
            return QUERY_USER_NOTIFICATION_STATE.QUNS_FAILED;
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

                //TIER 2 E-Core if Hybrid, First Thread
                for (int j = 0; j < CPPC.Length; ++j)
                {
                    tcore = CPPC[j];
                    if (systemInfo.IntelHybrid && systemInfo.Ecores.Contains(tcore) && ProcessorInfo.HardwareCores[tcore].LogicalCores.Length == 1)
                    {
                        _runt0.Add(ProcessorInfo.HardwareCores[tcore].LogicalCores[0]);
                        /*
                        if (!systemInfo.bECoresLast)
                        {
                            _runt0.Add(ProcessorInfo.HardwareCores[tcore].LogicalCores[0]);
                        }
                        if (systemInfo.bECoresLast)
                        {
                        }
                        else
                        {
                            _runt0.Add(ProcessorInfo.HardwareCores[tcore].LogicalCores[0]);
                        }
                        */
                    }
                }

                //TIER 3 No E-Core if Hybrid, Second Thread
                for (int j = CPPC.Length - 1; j >= 0; --j)
                {
                    tcore = CPPC[j];
                    //App.LogDebug($"t2 tcore {tcore} isecore={systemInfo.Ecores.Contains(tcore)} tcoreloglen={ProcessorInfo.HardwareCores[tcore].LogicalCores.Length}");
                    if ((!systemInfo.IntelHybrid || (systemInfo.IntelHybrid && !systemInfo.Ecores.Contains(tcore))) && ProcessorInfo.HardwareCores[tcore].LogicalCores.Length > 1)
                    {
                        _runt1.Add(ProcessorInfo.HardwareCores[tcore].LogicalCores[1]);
                        //App.LogDebug($"t2 add {tcore} logical={ProcessorInfo.HardwareCores[tcore].LogicalCores[1]}");
                        /*
                        if (ProcessorInfo.HardwareCores[tcore].LogicalCores[1] == 1 && App.CoreZeroT1T0)
                        {
                            _runt0.Add(ProcessorInfo.HardwareCores[tcore].LogicalCores[1]);

                        }
                        else
                        {
                            _runt1.Add(ProcessorInfo.HardwareCores[tcore].LogicalCores[1]);
                        }
                        */
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
        private static void StartTestTimerOverall(int period)
        {
            ulong duration_us;
            double _overall1;
            double _overall10;
            double _overall100;
            double _overall2000;
            string _periodlabel = "default";
            if (period != 0)
                _periodlabel = $"{period} ms";

            duration_us = 2000;
            _overall2000 = testTimerOverall(duration_us, period);
            duration_us = 100;
            _overall100 = testTimerOverall(duration_us, period);
            duration_us = 10;
            _overall10 = testTimerOverall(duration_us, period);
            duration_us = 1;
            _overall1 = testTimerOverall(duration_us, period);
            LogInfo($"Timer testing at {_periodlabel} resolution, average requested/effective duration in us: [1/{String.Format("{0:0.#}", _overall1)}] [10/{String.Format("{0:0.#}", _overall10)}] [100/{String.Format("{0:0.#}", _overall100)}] [2000/{String.Format("{0:0.#}", _overall2000)}]");
        }
        private static void StartTestTimerPerf(int period, int iterations)
        {
            ulong duration_us = 500;
            double _perf;
            double _perfwaitable;
            string _periodlabel = "default";

            if (period != 0)
                _periodlabel = $"{period} ms";

            _perf = testTimerPerf(duration_us, iterations, period, false);
            _perfwaitable = testTimerPerf(duration_us, iterations, period, true);

            LogInfo($"Timer testing at {_periodlabel} resolution, sleeps of {((int)(duration_us) * iterations)/1000} ms took: non-waitable={String.Format("{0:0.00}", _perf * 1000)} ms waitable={String.Format("{0:0.00}", _perfwaitable * 1000)} ms");
        }

        private static void InitTimers()
        {
            try
            {
                //uint _period;

                int swresolution = (int)(1E9 / Stopwatch.Frequency);
                
                TimerEffectiveus = App.GetTimerResolution();
                LogInfo($"Timer Resolution: {String.Format("{0:0.0#}", TimerEffectiveus / 1e4)} ms - System: {String.Format("{0:0.0#}", App.GetSysTimerResolution() / 1e4)} ms - .NET StopWatch Resolution: {swresolution} us");

                TIMECAPS timecaps = new TIMECAPS();
                uint _retcaps = timeGetDevCaps(out timecaps, (uint)Marshal.SizeOf(timecaps));
                if (_retcaps != 0)
                {
                    LogInfo($"timeGetDevCaps failed: {_retcaps}");
                }
                else
                {
                    LogInfo($"Timer Resolution Caps min: {String.Format("{0:0.0#}", timecaps.wPeriodMin)} ms - max: {String.Format("{0:0.0#}", timecaps.wPeriodMax)} ms");
                }

                SetThrottleExecSpeed(0, false);
                SetIgnoreTimer(0, false);
                SetThrottleExecSpeed(Process.GetCurrentProcess().Id, false);
                SetIgnoreTimer(Process.GetCurrentProcess().Id, false);
                
                StartTestTimerOverall(0);
                StartTestTimerPerf(0, 10);
                StartTestTimerPerf(0, 100);

                bool prev;
                int _priv = (int)RtlAdjustPrivilege(20, true, false, out prev);
                if (_priv != 0) LogInfo($"SetPrivilege failed: {_priv}");

                /*
                _period = TimerResolution;
                NewTimePeriod(_period);

                if (TimerResolution != (int)TimerEffectiveus / 1e4)
                {
                    StartTestTimerOverall((int)_period);
                    StartTestTimerPerf((int)_period, 10);
                    StartTestTimerPerf((int)_period, 100);
                }
                else
                {
                    LogInfo($"Default Timer Resolution is same as desired, skipping test...");
                }
                TimerEffectiveus = App.GetTimerResolution();
                LogInfo($"Timer Resolution: {String.Format("{0:0.0#}", TimerEffectiveus / 1e4)} ms - System: {String.Format("{0:0.0#}", App.GetSysTimerResolution() / 1e4)} ms");
                */

            }
            catch (Exception ex)
            {
                Trace.WriteLine($"InitTimers Exception: {ex}");
                LogExError($"InitTimers exception: {ex.Message}", ex);
            }

        }

        public static void ResetThrottling(int _pid)
        {
            try
            {
                int _reset = resetPowerThrottling(_pid);
                if (_reset != 0) LogInfo($"Power Throttling reset failed [PID={_pid}]: Error {_reset}");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ResetThrottling Exception [PID={_pid}]: {ex}");
                LogExError($"ResetThrottling exception [PID={_pid}]: {ex.Message}", ex);
            }
        }

        public static void SetThrottleExecSpeed(int _pid, bool enable)
        {
            try
            {
                int _execspeed = setPowerThrottlingExecSpeed(_pid, enable);
                if (_execspeed != 0) LogInfo($"Power Throttling failed to set Execution Speed {enable} [PID={_pid}]: Error {_execspeed}");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"SetThrottleExecSpeed Exception [PID={_pid}]: {ex}");
                LogExError($"SetThrottleExecSpeed exception [PID={_pid}]: {ex.Message}", ex);
            }
        }
        public static void SetIgnoreTimer(int _pid, bool enable)
        {
            try
            {
                int _ignoretimer = setPowerThrottlingIgnoreTimer(_pid, enable);
                if (_ignoretimer != 0) LogInfo($"Power Throttling failed to set Ignore Timer {enable} [PID={_pid}]: Error {_ignoretimer}");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"SetThrottleExecSpeed Exception [PID={_pid}]: {ex}");
                LogExError($"SetThrottleExecSpeed exception [PID={_pid}]: {ex.Message}", ex);
            }
        }

        public static void NewTimePeriod(uint period)
        {
            try
            {
                uint _retperiod;
                if (!TimerResolution_b)
                {
                    _retperiod = TimeEndPeriod(TimerResolution);
                    if (_retperiod != 0) LogInfo($"TimeEndPeriod failed: Error {_retperiod}");
                }
                _retperiod = TimeBeginPeriod(period);
                if (_retperiod != 0) LogInfo($"TimeBeginPeriod failed: Error {_retperiod}");
                TimerResolution_b = true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"NewTimePeriod Exception: {ex}");
                LogExError($"NewTimePeriod exception: {ex.Message}", ex);
            }
        }

        public static void ClearTimePeriod()
        {
            try
            {
                if (!TimerResolution_b)
                {
                    uint _retperiod;
                    _retperiod = TimeEndPeriod(TimerResolution);
                    if (_retperiod != 0) LogInfo($"TimeEndPeriod failed: Error {_retperiod}");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ClearTimePeriod Exception: {ex}");
                LogExError($"ClearTimePeriod exception: {ex.Message}", ex);
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
                logger_b = true;

            }
            catch (NLogConfigurationException ex)
            {
                Trace.WriteLine($"InitNLog Exception: {ex}");
                LogExError($"InitNLog exception: {ex.Message}", ex);
            }
            catch
            {
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
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string lib);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr module, string proc);
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                instanceMutex = new Mutex(true, mutexName, out bMutex);

                if (!bMutex)
                {
                    InteropMethods.PostMessage((IntPtr)InteropMethods.HWND_BROADCAST, InteropMethods.WM_SHOWME,
                        IntPtr.Zero, IntPtr.Zero);
                    //MessageBox.Show("CPUDoc is already running, cannot start it again.");
                    Current.Shutdown();
                    Environment.Exit(0);
                }

                GC.KeepAlive(instanceMutex);
                SplashWindow.Start();
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format("CPUDoc: an exception occurred when checking the run mutex: {0}", ex.Message);
                MessageBox.Show(errorMessage);
            }

            try
            {

                // handle unhandled better
                Dispatcher.UnhandledException += OnDispatcherUnhandledException;

                Version _version = Assembly.GetExecutingAssembly().GetName().Version;
                version = string.Format("v{0}.{1}.{2}", _version.Major, _version.Minor, _version.Build);
                _versionInfo = string.Format("{0}.{1}.{2}", _version.Major, _version.Minor, _version.Build);
                
                Directory.CreateDirectory(@".\Logs");

                if (Process.GetProcessesByName("vgc").Length <= 0 && File.Exists("inpoutx64.dlh")) File.Move("inpoutx64.dlh", "inpoutx64.dll");
                if (Process.GetProcessesByName("vgc").Length > 0 && File.Exists("inpoutx64.dll")) File.Move("inpoutx64.dll", "inpoutx64.dlh");
                //if (IsModuleLoaded("CPUDoc.sys")) UnloadModule("CPUDoc.sys");

                SplashWindow.Loading(10);

                CleanUpOldFiles();

                Trace.AutoFlush = true;

                InitNLog();

                LogInfo($"CPUDoc Version {_versionInfo}");

                InitTimers();

                SplashWindow.Loading(15);

                Ring0.Open();

                SplashWindow.Loading(20);

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

                SplashWindow.Loading(30);

                base.OnStartup(e);

                SplashWindow.Loading(35);

                if (OSVersion.GetOSVersion().Version.Major >= 10) App.Win10 = true;
                if (OSVersion.GetOSVersion().Version.Major >= 10 && OSVersion.GetOSVersion().Version.Build >= 22000) App.Win11 = true;

                SplashWindow.Loading(30);

                cpuTotalLoad = new MovingAverage(4);
                cpuTotalLoadLong = new MovingAverage(16);
                TBPoolingAverage = new MovingAverage(64);

                SplashWindow.Loading(40);

                bool cpuloadperfcount = ProcessorInfo.CpuLoadInit();
                LogInfo($"CPULoad using Performance Counters: {cpuloadperfcount}");

                SplashWindow.Loading(50);

                systemInfo = new();

                systemInfo.AppVersion = version;

                SplashWindow.Loading(60);

                SettingsInit();

                SplashWindow.Loading(65);

                PSAInit();

                SplashWindow.Loading(70);

                InitColors();

                TBStart();
                HWMStart();

                SplashWindow.Loading(75);

                SetActiveConfig(0);

                Processes.Init();

                SplashWindow.Loading(80);

                TraceLogging((AppSettings.LogTrace ?? false));
                InfoLogging((AppSettings.LogInfo ?? false));

                SetLastT0Affinity();

                SplashWindow.Loading(90);

                //Binding bindToggleSSH = new Binding(App.systemInfo.ToggleSSH);
                //bindToggleSSH.Mode = BindingMode.OneWay;
                //bindToggleSSH.Source = App.systemInfo;
                //BindingOperations.SetBinding(, ProgressBar.ValueProperty, bindLive);

                AutoUpdater.ReportErrors = false;
                AutoUpdater.InstalledVersion = new Version(App._versionInfo);
                AutoUpdater.DownloadPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                AutoUpdater.RunUpdateAsAdmin = false;
                AutoUpdater.Synchronous = false;
                AutoUpdater.ParseUpdateInfoEvent += AutoUpdaterOnParseUpdateInfoEvent;
               
                autimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                AUNotifications(App.AppSettings.AUNotifications ?? false);
                autimer.Tick += (sender, args) =>
                {
                    autimer.Interval = TimeSpan.FromSeconds(3600);
                    AutoUpdater.Start(AutoUpdaterUrl);
                };

                UAStamp = DateTime.Now;


                SubscribeGlobal();

                SplashWindow.Loading(95);

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
                using (var eventHookFactory = new EventHook.EventHookFactory())
                {
                    applicationWatcher = eventHookFactory.GetApplicationWatcher();
                    applicationWatcher.Start();
                    applicationWatcher.OnApplicationWindowChange += CheckApplication;
                    //keyboardWatcher = eventHookFactory.GetKeyboardWatcher();
                    //keyboardWatcher.Start();
                    //keyboardWatcher.OnKeyInput += CheckHotKey;
                }
                */
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

                SplashWindow.Loading(100);

                SplashWindow.Stop();

                tbIcon = (TaskbarIcon)FindResource("tbIcon");

                tbIcon.ToolTip = $"CPUDoc {App.version}";
                tbIcon.Icon = new Icon(Application.GetResourceStream(new Uri("pack://application:,,,/images/cpudoc.ico")).Stream);

                //ShowBalloon($"CPUDoc {App.version}", "Now is running in the system tray");
                ShowFancyBalloon($"CPUDoc {App.version}", "Now running minimized in the system tray, click here to open the App");

                try
                {
                    _map = new ConcurrentDictionary<uint, string>();

                    _processStartedWatcher = new ManagementEventWatcher("SELECT ProcessID, ProcessName FROM Win32_ProcessStartTrace");
                    _processStartedWatcher.EventArrived += CheckStartApplication;
                    _processStartedWatcher.Start();

                    _processStoppedwatcher = new ManagementEventWatcher("SELECT ProcessID FROM Win32_ProcessStopTrace");
                    _processStoppedwatcher.EventArrived += CheckStopApplication;
                    _processStoppedwatcher.Start();
                }
                catch (ManagementException ex)
                {
                    LogExError("Could not start listening to WMI events: ", ex);
                }

            }
            catch (Exception ex)
            {
                if (logger_b) LogExError($"OnStartup exception: {ex.Message}", ex);
            }

            //TEST
            
            //throw new InvalidOperationException();
        }

        void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Application_Cleanup();
            string errorMessage = string.Format("CPUDoc: an unhandled exception occurred: {0}", e.Exception.Message);
            //PSAPlanDisable();
            MessageBox.Show(errorMessage);
            if (logger_b)
            {
                LogInfo(errorMessage);
                Trace.Flush();
                Trace.Close();
                LogManager.Shutdown();
            }
            e.Handled = MainWindow?.IsLoaded ?? false;
            ClearTimePeriod();
            Current.Shutdown();
            Environment.Exit(-1);
        }

        public void ShowFancyBalloon(string title, string text)
        {
            FancyBalloon balloon = new FancyBalloon();
            balloon.BalloonText = text;
            balloon.BalloonTitle = title;
            tbIcon.ShowCustomBalloon(balloon, PopupAnimation.Slide, 4000);
        }
        public void ShowBalloon(string title, string text, BalloonIcon icon = BalloonIcon.Info)
        {         
            tbIcon.ShowBalloonTip(title, text, icon);
        }
        public void HideBalloon()
        {
            tbIcon.HideBalloonTip();
        }
        public static void AUNotifications(bool enable)
        {
            if (enable)
            {
                autimer.Start();
            }
            else
            {
                autimer.Stop();
            }
        }

        public static void PowerSavingActivation(bool enable)
        {
            if (enable)
            {
                //RESTORE POWER SAVING
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
            }
            else
            {
                // NO POWER SAVING
                SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_AWAYMODE_REQUIRED);
            }
        }

        public void PSAInit()
        {

            try
            {
                boot_ppname = powerManager.GetActivePlanFriendlyName();
                boot_ppguid = powerManager.GetActiveGuid();

                if (Win11)
                {
                    ThreadBooster.hepfg = 5;
                    ThreadBooster.hepbg = 2;
                }
                else
                {
                    if (!systemInfo.Zen3)
                    {
                        ThreadBooster.hepfg = 2;
                        ThreadBooster.hepbg = 4;
                    }
                    else
                    {
                        ThreadBooster.hepfg = 5;
                        ThreadBooster.hepbg = 5;
                    }
                }
                ThreadBooster.hepfgdeep = 4;
                ThreadBooster.hepbgdeep = 4;

                App.LogDebug($"Power Plan at boot: {boot_ppname}");
            }
            catch (Exception ex)
            {
                App.LogExError("PSAInit Exception:", ex);
            }
        }
        public static void ImportPowerPlan(string planName)
        {
            try
            {
                var cmd = new Process { StartInfo = { FileName = "powercfg" } };
                using (cmd)
                {
                    var inputPath = System.IO.Path.Combine(Environment.CurrentDirectory, planName);

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
        public static void PSAPlanDisable()
        {
            try
            {
                bool fallback = false;
                if (boot_ppguid != null)
                {
                    if (boot_ppguid.ToString().ToUpper().Contains("FBB9C3D1-AF6E-46CF-B02B-C41192"))
                    {
                        fallback = PSAPlanFallback();
                        App.LogDebug($"PSA Disabled Power Plan fall back to: {boot_ppname}");
                    }
                    else
                    {
                        fallback = powerManager.SetActiveGuid((Guid)boot_ppguid);
                        App.LogDebug($"PSA Disabled Power Plan set back to: {boot_ppname}");
                    }
                }
                if (fallback) psact_plan = false;
            }
            catch (Exception ex)
            {
                App.LogExError("PSAPlanDisable Exception:", ex);
            }
        }
        public static bool PSAPlanFallback()
        {
            try
            {
                bool _done = false;
                _done = PSAPlanFallbackDo("Balanced");
                if (!_done) PSAPlanFallbackDo("High Performance");
                if (!_done) PSAPlanFallbackDo("Ultimate Performance");
                if (!_done) PSAPlanFallbackDo("Power saver");
                if (_done)
                {
                    App.LogDebug($"PSA Fall Back Power Plan done, active is: {powerManager.GetActivePlanFriendlyName()}");
                    return true;
                }
                else
                {
                    App.LogDebug($"PSA Fall Back Power Plan failed, active is: {powerManager.GetActivePlanFriendlyName()}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                App.LogExError("PSAPlanFallback Exception:", ex);
                return false;
            }
        }
        public static bool PSAPlanFallbackDo(string planname)
        {
            try
            {
                bool _done = false;
                var plans = powerManager.GetPlans();
                foreach (var plan in plans)
                {
                    if (plan.name == planname)
                    {
                        powerManager.SetActiveGuid(plan.guid);
                        _done = true;
                    }
                }
                return _done;
            }
            catch (Exception ex)
            {
                App.LogExError("PSAPlanFallbackDo Exception:", ex);
                return false;
            }
        }
        public static void PSAEnable()
        {
            try
            {
                bool isactive = false;
                if ((App.pactive.PowerSaverActive ?? false) && !PPImportErrStatus)
                {
                    if (systemInfo.CPUName.Contains("Intel"))
                    {
                        PPDutyCycling = 0;
                        PPUSBSSuspend = 0;
                    }
                    else
                    {
                        PPUSBSSuspend = 1;
                        PPDutyCycling = 1;
                    }

                    int _personality = 1;
                    int _activepersonality = (App.pactive.Personality ?? 0);
                    int _activepowertweak = (App.pactive.PowerTweak ?? 1);

                    string planName = "";

                    if (_activepersonality == 0)
                    {
                        if (_activepowertweak == 0) _personality = 1;
                        if ((Win11 && _activepowertweak != 0) || _activepowertweak == 2) _personality = 2;
                    }
                    else
                    {
                        _personality = _activepersonality;
                    }

                    if (_personality == 2)
                    {
                        PPGuid = Win11 ? new Guid(PPGuidV2U11) : new Guid(PPGuidV2U10);
                        planName = Win11 ? PPNameV2U11 : PPNameV2U10;
                    }
                    else
                    {
                        PPGuid = Win11 ? new Guid(PPGuidV2B11) : new Guid(PPGuidV2B10);
                        planName = Win11 ? PPNameV2B11 : PPNameV2B10;
                    }

                    if (_activepowertweak == 0)
                    {
                        App.pactive.PSALightSleepSeconds = 15;
                        App.pactive.PSADeepSleepSeconds = 60;
                        App.pactive.PSALightSleepThreshold = 12;
                        App.pactive.PSADeepSleepThreshold = 6;
                        ThreadBooster.ProcPerfBoostEco = 95;
                        if (App.systemInfo.Ecores.Count > 0 && App.systemInfo.Pcores.Count > 0 && App.systemInfo.IntelHybrid)
                        {
                            ThreadBooster.coreparking = 10;
                            int _pcoresmin = (100 / (App.systemInfo.Pcores.Count)) + 1;
                            _pcoresmin = _pcoresmin <= 1 ? 1 : _pcoresmin >= 100 ? 100 : _pcoresmin;
                            ThreadBooster.coreparking_ec1 = _pcoresmin;
                            ThreadBooster.coreparking_light = 0;
                            ThreadBooster.coreparking_light_ec1 = 0;
                        }
                    }
                    else if (_activepowertweak == 2)
                    {
                        App.pactive.PSALightSleepSeconds = 90;
                        App.pactive.PSADeepSleepSeconds = 240;
                        App.pactive.PSALightSleepThreshold = 8;
                        App.pactive.PSADeepSleepThreshold = 4;
                        ThreadBooster.ProcPerfBoostEco = 100;
                        if (App.systemInfo.Ecores.Count > 0)
                        {
                            if (App.systemInfo.Ecores.Count > 0 && App.systemInfo.Pcores.Count > 0 && App.systemInfo.IntelHybrid)
                            {
                                ThreadBooster.coreparking = 10;
                                int _pcoresmin = (100 / (App.systemInfo.Pcores.Count)) + 1;
                                _pcoresmin = _pcoresmin <= 1 ? 1 : _pcoresmin >= 100 ? 100 : _pcoresmin;
                                ThreadBooster.coreparking_ec1 = _pcoresmin;
                                int _ecoresmin = ((2 * 100) / (App.systemInfo.Ecores.Count)) + 2;
                                _ecoresmin = _ecoresmin <= 1 ? 1 : _ecoresmin >= 100 ? 100 : _ecoresmin;
                                ThreadBooster.coreparking_ec1 = _pcoresmin;
                                _ecoresmin = ((1 * 100) / (App.systemInfo.Ecores.Count)) + 1;
                                _ecoresmin = _ecoresmin <= 1 ? 1 : _ecoresmin >= 100 ? 100 : _ecoresmin;
                                ThreadBooster.coreparking_light = _ecoresmin;
                                ThreadBooster.coreparking_light_ec1 = _pcoresmin;
                            }
                        }
                    }
                    else
                    {
                        App.pactive.PSALightSleepSeconds = 30;
                        App.pactive.PSADeepSleepSeconds = 90;
                        App.pactive.PSALightSleepThreshold = 14;
                        App.pactive.PSADeepSleepThreshold = 8;
                        ThreadBooster.ProcPerfBoostEco = 100;
                        if (App.systemInfo.Ecores.Count > 0 && App.systemInfo.Pcores.Count > 0 && App.systemInfo.IntelHybrid)
                        {
                            ThreadBooster.coreparking = 10;
                            int _pcoresmin = (100 / (App.systemInfo.Pcores.Count)) + 1;
                            _pcoresmin = _pcoresmin <= 1 ? 1 : _pcoresmin >= 100 ? 100 : _pcoresmin;
                            ThreadBooster.coreparking_ec1 = _pcoresmin;
                            ThreadBooster.coreparking_light = 10;
                            ThreadBooster.coreparking_light_ec1 = 0;
                        }
                    }

                    if (powerManager.GetActiveGuid() != PPGuid)
                    {
                        if (!powerManager.PlanExists(PPGuid)) ImportPowerPlan(planName);

                        if (powerManager.PlanExists(PPGuid)) isactive = powerManager.SetActiveGuid(PPGuid);
                        if (isactive)
                        {
                            psact_b = true;
                            psact_plan = true;
                            App.LogInfo($"CPUDoc Dynamic Power Plan enabled: {powerManager.GetActivePlanFriendlyName()}");
                        }
                        else
                        {
                            PPImportErrCnt++;
                            if (PPImportErrCnt > 25)
                            {
                                PPImportErrCnt = 0;
                                PPImportErrStatus = true;
                            }
                        }
                    }
                    if (psact_b && psact_plan) 
                    {
                        uint _value;
                        //USB selective suspend setting
                        _value = (uint)App.PPUSBSSuspend;
                        App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.USB_SUBGROUP, new Guid("48e6b7a6-50f5-4782-a5d4-53bb8f07e226"), PowerManagerAPI.PowerMode.AC, _value);

                        //Hub Selective Suspend Timeout
                        _value = (uint)App.PPUSBSSuspendTimeout;
                        App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.USB_SUBGROUP, new Guid("0853a681-27c8-4100-a2fd-82013e970683"), PowerManagerAPI.PowerMode.AC, _value);

                        App.powerManager.SetActiveGuid(App.PPGuid);

                        if (pactive.MonitorIdle == null || pactive.SleepIdle == null || pactive.HyberIdle == null)
                        {
                            pactive.MonitorIdle = PPSecToIdx((int)PowerManager.GetPlanSetting(PPGuid, SettingSubgroup.VIDEO_SUBGROUP, Setting.VIDEOIDLE, PowerMode.AC));
                            pactive.SleepIdle = PPSecToIdx((int)PowerManager.GetPlanSetting(PPGuid, SettingSubgroup.SLEEP_SUBGROUP, Setting.STANDBYIDLE, PowerMode.AC));
                            pactive.HyberIdle = PPSecToIdx((int)PowerManager.GetPlanSetting(PPGuid, SettingSubgroup.SLEEP_SUBGROUP, Setting.HIBERNATEIDLE, PowerMode.AC));
                        }

                        App.powerManager.SetDynamic(SettingSubgroup.VIDEO_SUBGROUP, new Guid("3c0bc021-c8a8-4e07-a973-6b14cbcb2b7e"), PowerManagerAPI.PowerMode.AC | PowerManagerAPI.PowerMode.DC, (uint)PPIdxToSec((pactive.MonitorIdle ?? 0)));
                        App.powerManager.SetDynamic(SettingSubgroup.SLEEP_SUBGROUP, new Guid("29f6c1db-86da-48c5-9fdb-f2b67b1f44da"), PowerManagerAPI.PowerMode.AC | PowerManagerAPI.PowerMode.DC, (uint)PPIdxToSec((pactive.SleepIdle ?? 0)));
                        App.powerManager.SetDynamic(SettingSubgroup.SLEEP_SUBGROUP, new Guid("9d7815a6-7ee4-497e-8888-515a05f02364"), PowerManagerAPI.PowerMode.AC | PowerManagerAPI.PowerMode.DC, (uint)PPIdxToSec((pactive.HyberIdle ?? 0)));

                    }
                }
                else
                {
                    PSAPlanDisable();
                }
            }
            catch (Exception ex)
            {
                psact_plan = false;
                psact_b = false;
                App.LogExError("PowerSaverActive Init Exception:", ex);
            }
        }
        public static int PPSecToIdx(int value)
        {
            return value <= 0 ? 0
                        : value <= 60 ? 1
                        : value <= 120 ? 2
                        : value <= 180 ? 3
                        : value <= 300 ? 4
                        : value <= 600 ? 5
                        : value <= 900 ? 6
                        : value <= 1200 ? 7
                        : value <= 1500 ? 8
                        : value <= 1800 ? 9
                        : value <= 2700 ? 10
                        : value <= 3600 ? 10
                        : value <= 10800 ? 12
                        : value <= 14400 ? 13
                        : value <= 18000 ? 14
                        : 14;
        }
        public static int PPIdxToSec(int value)
        {
            return value <= 0 ? 0
                        : value <= 1 ? 60
                        : value <= 2 ? 120
                        : value <= 3 ? 180
                        : value <= 4 ? 300
                        : value <= 5 ? 600
                        : value <= 6 ? 900
                        : value <= 7 ? 1200
                        : value <= 8 ? 1500
                        : value <= 9 ? 1800
                        : value <= 10 ? 2700
                        : value <= 11 ? 3600
                        : value <= 12 ? 10800
                        : value <= 13 ? 14400
                        : value <= 14 ? 18000
                        : 18000;
        }
        private void CheckApplication(object sender, EventHook.ApplicationEventArgs e)
        {
            //App.LogDebug($"{(int)e.ApplicationData.HWnd} {e.ApplicationData.AppPath} Application window of '{e.ApplicationData.AppName}' with the title '{e.ApplicationData.AppTitle}' was {e.Event}");

            uint processId = 0;
            GetWindowThreadProcessId(e.ApplicationData.HWnd, out processId);

            if (e.Event == EventHook.ApplicationEvents.Launched) Processes.AppLaunched(System.IO.Path.GetFileNameWithoutExtension(e.ApplicationData.AppPath), (int)processId);

            if (e.Event == EventHook.ApplicationEvents.Closed) Processes.AppClosed(System.IO.Path.GetFileNameWithoutExtension(e.ApplicationData.AppPath), (int)processId);

            //App.LogDebug($"[PID={processId}] AppBin={System.IO.Path.GetFileNameWithoutExtension(e.ApplicationData.AppPath)} Application window of '{e.ApplicationData.AppName}' with the title '{e.ApplicationData.AppTitle}' was {e.Event}");

        }

        private void CheckStartApplication(object sender, EventArrivedEventArgs e)
        {
            var processName = (string)e.NewEvent["ProcessName"];
            var processId = (uint)e.NewEvent["ProcessId"];

            if (string.IsNullOrEmpty(processName))
                return;

            _map[processId] = System.IO.Path.GetFileNameWithoutExtension(processName);

            Processes.AppLaunched(System.IO.Path.GetFileNameWithoutExtension(processName), (int)processId);

            //App.LogDebug($"Starting [PID={(int)processId}] AppBin={System.IO.Path.GetFileNameWithoutExtension(processName)}");

        }
        private void CheckStopApplication(object sender, EventArrivedEventArgs e)
        {
            var processId = (uint)e.NewEvent["ProcessId"];
            string processName = GetProcessName(processId);
            
            Processes.AppClosed(processName, (int)processId);

            _map.Remove(processId);

            //App.LogDebug($"Exiting [PID={(int)processId}] AppBin={processName}");
        }

        public static string GetProcessName(uint processId)
        {
            if (_map == null)
                return "";

            if (_map.TryGetValue(processId, out string processName))
            {
                return processName;
            }
            return "";
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
                App.pactive.ThreadBooster = true;
                tbtimer.Enabled = true;
                ThreadBooster.ForceCustomBitMask(false);
            }
            else
            {
                App.pactive.ThreadBooster = false;
                tbtimer.Enabled = false;
                SysCpuSetMask = ThreadBooster.defFullBitMask;
                lastSysCpuSetMask = ThreadBooster.defFullBitMask;
                Thread.Sleep(500);
                SetSysCpuSet(ThreadBooster.defFullBitMask);
                PSAPlanDisable();
            }

            //App.LogDebug($"ThreadBooster Started PID={thridMonitor} ALIVE={thrMonitor.IsAlive}");

        }

        public static void SettingsInit()
        {
            try
            {
                systemInfo.CPPCActiveOrder = systemInfo.CPPCOrder;
                systemInfo.CPPCActiveLabel = systemInfo.CPPCLabel;

                systemInfo.bECores = CPUDoc.Properties.Settings.Default.ECores;
                systemInfo.bECoresLast = false;

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

                _runlogicals = null;
                _runcores = null;

                if (!ProtBufSettings.ReadSettings()) ProtBufSettings.ReadSettings();

                App.LogDebug($"Profile 0 ThreadBooster Active={AppConfigs[0].ThreadBooster}");

                pactive = AppConfigs[0];

                App.LogDebug($"Active Profile ThreadBooster Active={pactive.ThreadBooster}");

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

        /*
        public static void RunUI()
        {

            //App.LogDebug("RUN TUIB");
            uitimer.Interval = 1000;
            uitimer.Elapsed += new ElapsedEventHandler(UpdateUI_ElapsedEventHandler);
            if (uitimer.Enabled)
            {
                //App.LogDebug("START UI");
                uitimer.Start();
            }

        }
        */
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

        private SERVICE_STATUS DoQueryServiceStatus(IntPtr intPtrServ)
        {
            SERVICE_STATUS serviceStatus = new SERVICE_STATUS();

            QueryServiceStatus(intPtrServ, ref serviceStatus);

            return serviceStatus;
        }
        private int Stop_WinRing0()
        {
            int _exitcode = 0;
            try
            {
                IntPtr intPtrSCM = OpenSCManager(null, null, SERVICE_ACCESS.SERVICE_ALL_ACCESS);
                if (intPtrSCM == IntPtr.Zero) return -1;

                IntPtr intPtrServ = OpenService(intPtrSCM, "R0CPUDoc", SERVICE_ACCESS.SERVICE_ALL_ACCESS);
                if (intPtrServ == IntPtr.Zero) return -2;

                SERVICE_STATUS stat = new SERVICE_STATUS();
                bool res = ControlService(intPtrServ, SERVICE_CONTROL.STOP, ref stat);

                if (!res)
                {
                    for (int i = 0; i < 8; ++i)
                    {
                        SERVICE_STATUS serviceStatus = DoQueryServiceStatus(intPtrServ);
                        if (serviceStatus.currentState == SERVICE_STATE.SERVICE_STOPPED)
                        {
                            res = true;
                            i = 9;
                        }
                        Thread.Sleep(250);
                    }
                    if (!res) App.LogDebug("Could not Stop WinRing0 service");

                }
                
                res = DeleteService(intPtrServ);

                if (!res) App.LogDebug("Could not delete WinRing0 service");

                CloseServiceHandle(intPtrServ);
                CloseServiceHandle(intPtrSCM);

            }
            catch (Exception ex)
            {
                App.LogDebug($"Exception Stop_WinRing0: {ex}");
            }

            return _exitcode;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Application_Cleanup();
            Current.Shutdown();
            Environment.Exit(0);
        }
        private void Application_Cleanup()
        {
            try
            {
                //keyboardWatcher.Stop();
                //mouseWatcher.Stop();
                //clipboardWatcher.Stop();
                //applicationWatcher.Stop();
                //printWatcher.Stop();
                //Unsubscribe();
                Unsubscribe();
                eventHookFactory.Dispose();

                if (_processStartedWatcher != null)
                {
                    _processStartedWatcher.Stop();
                    _processStartedWatcher.Dispose();
                    _processStartedWatcher = null;
                }

                if (_processStoppedwatcher != null)
                {
                    _processStoppedwatcher.Stop();
                    _processStoppedwatcher.Dispose();
                    _processStoppedwatcher = null;
                }

                HWMonitor.Close();
                ThreadBooster.Close();

                hwmcts?.Dispose();
                tbcts?.Dispose();
                syscts?.Dispose();
                tbIcon.Dispose();

                if (ThreadBooster.zencontrol_b) ThreadBooster.ZenControlPBO("hpx");

                Ring0.Close();
            }
            catch (Exception ex)
            {
                if (logger_b) LogExWarn($"Application_Cleanup exception: {ex.Message}", ex);
            }
            try
            {
                //RESTORE POWER SAVING
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
            }
            catch (Exception ex)
            {
                if (logger_b) LogExWarn($"Application_Cleanup exception: {ex.Message}", ex);
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
                if (logger_b) LogExWarn($"Application_Cleanup exception: {ex.Message}", ex);
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
                if (logger_b) LogExWarn($"Application_Cleanup exception: {ex.Message}", ex);
            }
            try
            {
                SetSystemCpuSet(0);
                if (IsModuleLoaded("CPUDoc.sys")) UnloadModule("CPUDoc.sys");
                if (IsModuleLoaded("CPUTools.dll")) UnloadModule("CPUTools.dll");
                int _stopR0 = Stop_WinRing0();
                if (_stopR0 != 0) LogDebug($"WinRing0 could not stop: {_stopR0}");
            }
            catch (Exception ex)
            {
                if (logger_b) LogExWarn($"Application_Cleanup exception: {ex.Message}", ex);
            }
            if (logger_b) LogInfo($"CPUDoc Version {_versionInfo} closed");
            Trace.Flush();
            Trace.Close();
            LogManager.Shutdown();
            ClearTimePeriod();
        }

        private bool IsModuleLoaded(string ModuleName)
        {
            bool loaded = false;
            Process _process = Process.GetCurrentProcess();
            ProcessModuleCollection myProcessModuleCollection;

            try
            {
                ProcessModule myProcessModule = null;
                myProcessModuleCollection = _process.Modules;

                for (int j = 0; j < myProcessModuleCollection.Count; j++)
                {
                    myProcessModule = myProcessModuleCollection[j];

                    if (myProcessModule.ModuleName.Contains(ModuleName))
                    {
                        loaded = true;
                        break;
                    }
                }
            }
            catch { loaded = false; }
            return loaded;
        }

        private void InitColors()
        {
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
        public static int ExecuteCmd(string exe, string arguments, string path)
        {
            try
            {
                var cmd = new Process { StartInfo = { FileName = exe, WorkingDirectory = path } };
                using (cmd)
                {
                    cmd.StartInfo.CreateNoWindow = true;
                    cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    cmd.StartInfo.Arguments = arguments;
                    cmd.Start();

                    cmd.WaitForExit();

                    return cmd.ExitCode;
                }
            }
            catch (Exception ex)
            {
                LogExError($"ExecuteCmd exception: {ex.Message}", ex);
                return -1;
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

            if (pactive.ThreadBooster ?? false)
            {
                tbtimer.Enabled = true;
                if (!(pactive.ManualPoolingRate ?? false)) SetPoolingRate();
                if (pactive.ManualPoolingRate ?? false) SetPoolingRate((int)(pactive.PoolingRate ?? 250));
            }
            else
            {
                tbtimer.Enabled = false;
                systimer.Enabled = false;
                hwmtimer.Enabled = false;
            }

            if (!(pactive.SysSetHack ?? false) && !(pactive.NumaZero ?? false))
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

            try
            {
                if ((pactive.NumaZero ?? false))
                {
                    int _found = 0;

                    if (pactive.NumaZeroType == 0)
                    {
                        for (int i = 0; i < ProcessorInfo.LogicalCoresCount; ++i)
                        {
                            List<int> _cores = ProcessorInfo.LogicalsNumaZero();
                            if (_cores.Count() > 0)
                            {
                                for (int c = 0; c < _cores.Count; ++c)
                                {
                                    if (_cores[c] == i)
                                    {
                                        if (logicalsT1.Contains(i)) n0enabledT1.Add(i);
                                        if (logicalsT0.Contains(i)) n0enabledT0.Add(i);
                                        App.LogDebug($"N0 ENABLED CoresN0={i} T0={logicalsT0.Contains(i)} T1={logicalsT1.Contains(i)}");
                                    }
                                }
                            }
                            _cores.Clear();
                            _cores = ProcessorInfo.LogicalsEfficient();
                            if (_cores.Count() > 0)
                            {
                                for (int c = 0; c < _cores.Count(); ++c)
                                {
                                    if (_cores[c] == i)
                                    {
                                        _found++;
                                        if (logicalsT1.Contains(i)) n0disabledT1.Add(i);
                                        if (logicalsT0.Contains(i)) n0disabledT0.Add(i);
                                        App.LogDebug($"N0 DISABLED CoresEfficient={i} T0={logicalsT0.Contains(i)} T1={logicalsT1.Contains(i)}");
                                    }
                                }
                            }
                            _cores.Clear();
                            _cores = ProcessorInfo.LogicalsCache();
                            if (_cores.Count() > 0)
                            {
                                for (int c = 0; c < _cores.Count(); ++c)
                                {
                                    if (_cores[c] == i)
                                    {
                                        _found++;
                                        if (logicalsT1.Contains(i)) n0disabledT1.Add(i);
                                        if (logicalsT0.Contains(i)) n0disabledT0.Add(i);
                                        App.LogDebug($"N0 DISABLED CoresCache={i} T0={logicalsT0.Contains(i)} T1={logicalsT1.Contains(i)}");
                                    }
                                }
                            }
                            _cores.Clear();
                            _cores = ProcessorInfo.LogicalsIndex();
                            if (_cores.Count() > 0)
                            {
                                for (int c = 0; c < _cores.Count(); ++c)
                                {
                                    if (_cores[c] == i)
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
                    App.LogDebug($"N0={pactive.NumaZero} N0Type={pactive.NumaZeroType} CORES={ProcessorInfo.LogicalCoresCount - _found}/{ProcessorInfo.LogicalCoresCount}[{_found}] HT={systemInfo.HyperThreading}");
                    App.LogDebug($"N0_ENABLED={n0enabledT0.Count+n0enabledT1.Count} N0_DISABLED={n0disabledT0.Count + n0disabledT1.Count} E-CORES={App.systemInfo.Ecores.Count}");

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

                if (n0disabledT0.Count() > 0 || n0disabledT1.Count() > 0 && (pactive.NumaZero ?? false)) numazero_b = true;

                if (systemInfo.IntelHybrid)
                {
                    if (numazero_b)
                    {
                        powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("7f2f5cfa-f10c-4823-b5e1-e93ae85f46b5"), PowerManagerAPI.PowerMode.AC | PowerManagerAPI.PowerMode.DC, 3);
                        powerManager.SetActiveGuid(powerManager.GetActiveGuid());
                    }
                    else
                    {
                        powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("7f2f5cfa-f10c-4823-b5e1-e93ae85f46b5"), PowerManagerAPI.PowerMode.AC | PowerManagerAPI.PowerMode.DC, 4);
                        powerManager.SetActiveGuid(powerManager.GetActiveGuid());
                    }
                }
            }
            catch (Exception ex)
            {
                App.LogExError($"NumaZero Init Exception:", ex);
            }
            App.systemInfo.SetSSHStatus(pactive.SysSetHack ?? false);
            App.systemInfo.SetPSAStatus(pactive.PowerSaverActive ?? false);
            App.systemInfo.SetN0Status(pactive.NumaZero ?? false);

            App.LogDebug($"N0={pactive.NumaZero} N0Active={numazero_b} HT={systemInfo.HyperThreading}");

            ThreadBooster.bInit = false;

        }

        public static bool ReadMsr(uint msr, ref uint eax, ref uint edx)
        {
            return Ring0.Rdmsr(msr, out eax, out edx);
        }

        public static bool ReadMsrTx(uint msr, ref uint eax, ref uint edx, uint index)
        {
            GroupAffinity affinity = GroupAffinity.Single(0, (int)index);

            return Ring0.RdmsrTx(msr, out eax, out edx, affinity);
        }

        public static bool WriteMsr(uint msr, uint eax, uint edx)
        {
            bool res = true;

            for (var i = 0; i < ProcessorInfo.LogicalCoresCount; i++)
            {
                res = Ring0.WrmsrTx(msr, eax, edx, GroupAffinity.Single(0, i));
            }

            return res;
        }
        public static bool WriteMsrTx(uint msr, uint eax, uint edx, uint index)
        {
            bool res = true;
            res = Ring0.WrmsrTx(msr, eax, edx, GroupAffinity.Single(0, (int)index));

            return res;
        }

        public static void SetPoolingRate(int rate = -1)
        {
            TBPoolingAverage = new MovingAverage(64);
            switch (rate)
            {
                case 0: //Very Slow
                    ThreadBooster.PoolingInterval = 750;
                    HWMonitor.MonitoringPooling = 500;
                    HWMonitor.MonitoringPoolingFast = 500;
                    cpuTotalLoad = new MovingAverage(3);
                    cpuTotalLoadLong = new MovingAverage(12);
                    return;
                case 1:
                    ThreadBooster.PoolingInterval = 500;
                    HWMonitor.MonitoringPooling = 250;
                    HWMonitor.MonitoringPoolingFast = 250;
                    cpuTotalLoad = new MovingAverage(4);
                    cpuTotalLoadLong = new MovingAverage(16);
                    return;
                case 2:
                    ThreadBooster.PoolingInterval = 350;
                    HWMonitor.MonitoringPooling = 250;
                    HWMonitor.MonitoringPoolingFast = 250;
                    cpuTotalLoad = new MovingAverage(4);
                    cpuTotalLoadLong = new MovingAverage(16);
                    return;
                case 4:
                    ThreadBooster.PoolingInterval = 200;
                    HWMonitor.MonitoringPooling = 200;
                    HWMonitor.MonitoringPoolingFast = 200;
                    cpuTotalLoad = new MovingAverage(6);
                    cpuTotalLoadLong = new MovingAverage(20);
                    return;
                case 5:
                    ThreadBooster.PoolingInterval = 150;
                    HWMonitor.MonitoringPooling = 150;
                    HWMonitor.MonitoringPoolingFast = 150;
                    cpuTotalLoad = new MovingAverage(8);
                    cpuTotalLoadLong = new MovingAverage(32);
                    return;
                case 6:
                    ThreadBooster.PoolingInterval = 100;
                    HWMonitor.MonitoringPooling = 100;
                    HWMonitor.MonitoringPoolingFast = 100;
                    cpuTotalLoad = new MovingAverage(10);
                    cpuTotalLoadLong = new MovingAverage(40);
                    return;
                case 7: //Crazy Fast
                    ThreadBooster.PoolingInterval = 50;
                    HWMonitor.MonitoringPooling = 50;
                    HWMonitor.MonitoringPoolingFast = 50;
                    cpuTotalLoad = new MovingAverage(16);
                    cpuTotalLoadLong = new MovingAverage(64);
                    return;
                default: 
                case 3: //Default
                    ThreadBooster.PoolingInterval = 250;
                    HWMonitor.MonitoringPooling = 250;
                    HWMonitor.MonitoringPoolingFast = 250;
                    cpuTotalLoad = new MovingAverage(4);
                    cpuTotalLoadLong = new MovingAverage(16);
                    return;
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
                
                if (powerManager.PlanExists(PPGuidV1)) powerManager.DeletePlan(PPGuidV1);

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
