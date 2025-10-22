using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using LibreHardwareMonitor.Hardware;
using System.Timers;
using System.Diagnostics;
using System.IO;
using ZenStates.Core;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using Windows.UI.Notifications;
using static CPUDoc.App;
using net.r_eg.Conari.Types;
using System.Runtime.InteropServices;
using NLog;
using System.Diagnostics.Metrics;
using AutoUpdaterDotNET;
using System.Windows.Documents;
using Vanara.PInvoke;
using ManagedBass;
using ManagedBass.Wasapi;
using System.Management;
using static Vanara.PInvoke.Ole32;
using static Vanara.PInvoke.Gdi32;
using static Vanara.PInvoke.Ole32.PROPERTYKEY.System;
using System.Windows.Threading;
using static net.r_eg.Conari.Log.Message;
using static Vanara.PInvoke.PowrProf;


namespace CPUDoc
{
    public class ThreadBooster
    {
        public static CancellationToken tbtoken = new CancellationToken();
        public static CancellationToken systoken = new CancellationToken();
        public static int PoolingTick = 0;
        public static int PoolingInterval = 250;
        public static int PoolingIntervalSlow = 1000;
        public static int PoolingIntervalDefault = 250;
        public static int PoolingIntervalSlowDefault = 1000;
        public static int SysMaskPooling = 100;
        public static ulong defBitMask = 0;
        public static ulong newBitMask = 0;
        public static ulong CustomSysMask = 0; 
        public static ulong defFullBitMask = 0;
        public static int prevNeedcores = 0;
        public static int prevMorecores = 0;
        public static int basecores = 0;
        public static int addcores = 0;
        public static int needcores = 0;
        public static int usedcores = 0;
        public static int morecores = 0;
        public static int setcores = 0;
        public static int newsetcores = 0;
        public static int deltalesscores = 0;
        public static int coreratio = 0;
        public static int forceCustomSysMaskDuration = 90;
        public static double TotalEnabledLoadNorm = 0;
        public static bool forceCustomSysMask = false;
        public static int InterlockBitMaskUpdate = 0;
        public static DateTime forceCustomSysMaskStamp = DateTime.MinValue;
        public static DateTime prevIncreaseStamp = DateTime.MinValue;
        public static DateTime prevFullcoresStamp = DateTime.MinValue;
        public static DateTime prevActivityStamp = DateTime.MinValue;
        public static TimeSpan _deltaStamp;
        public static TimeSpan _deltaActivityStamp;
        public static DateTime tbpoolingStamp = DateTime.MinValue;
        public static TimeSpan _deltaForceCustomBitMask;
        public static TimeSpan _deltaPooling;
        public static TimeSpan _deltaUA;
        public static TimeSpan _deltaHPX;
        public static TimeSpan _deltaBAL;
        public static DateTime _stampHPX = DateTime.MinValue;
        public static DateTime _stampBAL = DateTime.MinValue;

        public static TimeSpan _deltaLSL;
        public static TimeSpan _deltaDSL;
        public static DateTime _stampLSL = DateTime.MinValue;
        public static DateTime _stampDSL = DateTime.MinValue;

        public static bool bAudioCheck = false;
        public static TimeSpan _deltaAPC;
        public static DateTime _stampAPC = DateTime.MinValue;

        public static bool bInit = false;
        public static int IncreaseHysteresis = 6;
        public static bool SetHysteresis = true;
        public static int FullLoadHystSecs = 10;
        public static double HighTotalLoadThreshold = 90;
        public static double HighTotalLoadLowThreshold = 80;
        public static double HighTotalLoadFactor = 0.90;
        //public static double TotalEnabledLoad = 0;
        //public static double TotalEnabledLoadNorm = 0;
        public static double _cpuTotalLoad;
        public static int LoadZeroThresholdCount = 3;
        public static int LoadMediumThresholdCount = 3;
        public static int LoadHighThresholdCount = 2;
        public static int ClearForceThreshold = 5;
        public static int ClearForceLoadThreshold = 10;
        public static int ProcPerfBoostEco = 100;
        public static bool PLEvtPerfMode = false;
        public static string CurrentOverlay = "Better";

        // Look at PSAEnable() for details.
        public static int hepfg = 5;
        public static int hepbg = 5;
        public static int hepfgdeep = 5;
        public static int hepbgdeep = 5;
        public static int hetPolicy = 4;
        public static int autonomousmode = 1;
        public static int proceppp = 0;

        public static int coreparking_min = 100;
        public static int coreparking_min_ec1 = 100;
        public static int coreparking_min_light = 100;
        public static int coreparking_min_light_ec1 = 100;

        public static int coreparking_concurrency = 100;

        public static int cpuBoostModeSleep = 0;
        public static int cpuBoostModeEco = 3;
        public static int cpuBoostModeStd = 2;
        public static int cpuBoostModeBoost = 2;

        public static bool ActiveMode = true;
        public static bool GameMode = false;
        public static bool FocusAssist = false;
        public static bool UserNotification = false;
        public static bool FGFullScreen = false;
        public static bool FGFullScreenPrimary = false;
        public static int GModeMinBias;
        public static int AModeMinBias;
        public static QUERY_USER_NOTIFICATION_STATE unstate;

        public static string ZenControlPBO_lastmode = "";
        public static List<ZenControlMode> zenControlModes = new List<ZenControlMode>();
        public static bool zencontrol_b = false;
        public static bool zencontrol_b_ppt = false;
        public static bool zencontrol_b_tdc = false;
        public static bool zencontrol_b_edc = false;
        public static bool zencontrol_reapply = false;
        public static bool zencontrol_activated = false;
        public static int zencontrol_mode = 0;
        public static int Zen_lastPPT = 0;
        public static int Zen_lastTDC = 0;
        public static int Zen_lastEDC = 0;
        
        public static bool debugtb_steps = false;

        public static IntPtr evtPL;
        

        public static void BuildDefaultMask(string whoami = "Unknown")
        {
            try
            {
                defBitMask = 0;
                basecores = 0;
                addcores = 0;

                if (App.activeconfig_b)
                {

                    /*
                    List<int> _t0;
                    List<int> _t1;

                    if (numazero_b)
                    {
                        _t0 = App.n0enabledT0;
                        _t1 = App.n0enabledT1;
                    }
                    else
                    {
                        _t0 = App.logicalsT0;
                        _t1 = App.logicalsT1;
                    }

                    for (int i = 0; i <_t0.Count; i++)
                    {
                        defBitMask |= (ulong)1 << (_t0[i]);
                        defFullBitMask |= (uint)1 << (_t0[i]);
                        basecores++;
                        App.LogDebug($"Build defBitMask 0x{defBitMask:X16} {_t0[i]}");
                        App.LogDebug($"Build defFullBitMask 0x{defFullBitMask:X16} {_t0[i]}");
                    }
                    for (int i = 0; i < _t1.Count; i++)
                    {
                        defFullBitMask |= (uint)1 << (_t1[i]);
                        addcores++;
                        App.LogDebug($"Build defFullBitMask 0x{defFullBitMask:X16} {_t1[i]}");
                    }
                    */
                    App.LogDebug($"Skipping {System.Reflection.MethodBase.GetCurrentMethod().Name}, not ready");

                    defBitMask = ProcessorInfo.CreateDefaultBitMask();
                    defFullBitMask = ProcessorInfo.CreateFullBitMask();

                    basecores = CountBits(defBitMask);
                    addcores = CountBits(defFullBitMask) - CountBits(defBitMask);
                    
                    setcores = basecores;
                    HighTotalLoadThreshold = (basecores + addcores) * 100 / ProcessorInfo.LogicalCoresCount * HighTotalLoadFactor;
                    //??
                    //setcores = basecores + addcores;

                    /*
                    if (!App.pactive.SysSetHack && numazero_b)
                    {
                        App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name}: NumaZero only, no SSH [{CountBits(defBitMask)}]0x{defBitMask:X16} > [{CountBits(defFullBitMask)}]0x{defFullBitMask:X16}");
                        defBitMask = defFullBitMask;
                    }
                    */
                    App.LogInfo($"{System.Reflection.MethodBase.GetCurrentMethod().Name}: [{whoami}] [Cores:{basecores}+{addcores}={basecores + addcores}] defBitMask [{CountBits(defBitMask)}]0x{defBitMask:X16} defFullBitMask [{CountBits(defFullBitMask)}]0x{defFullBitMask:X16} N0Active={App.pactive.NumaZero} N0={App.numazero_b}");
                }
                else
                {
                    App.LogDebug($"Skipping {System.Reflection.MethodBase.GetCurrentMethod().Name}: [{whoami}] Skipping Active Config not ready");
                }
            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception {System.Reflection.MethodBase.GetCurrentMethod().Name}:", ex);
            }
        }
        public static ulong CreateBitMask(int addcores)
        {
            try
            {

                /*
                ulong newBitMask = defBitMask;
                for (int i = 0; i < addcores; ++i)
                {
                    if (i < morecores ||
                        (ProcessorInfo.HardwareCpuSets[App.logicalsT1[i]].ForcedEnable && !App.numazero_b) ||
                        (ProcessorInfo.HardwareCpuSets[App.logicalsT1[i]].ForcedEnable && App.numazero_b && App.n0enabledT1.Contains(App.logicalsT1[i]) && !App.n0disabledT1.Contains(App.logicalsT1[i])))
                    {
                        {
                            newBitMask |= (uint)1 << (App.logicalsT1[i]);
                            //if (App.numazero_b) App.LogDebug($"N0={App.numazero_b} i={i} logical={App.logicalsT1[i]} T1Forced={ProcessorInfo.HardwareCpuSets[App.logicalsT1[i]].ForcedEnable} enabled e={App.n0enabledT1.Contains(i)} d={App.n0disabledT1.Contains(i)}");
                        }
                    }
                    //App.LogDebug($"i={i} T1Forced={ProcessorInfo.HardwareCpuSets[App.logicalsT1[i]].ForcedEnable} calc newbitMask 0x{newBitMask:X8} Morecores:{morecores}");
                }
                */

                return ProcessorInfo.CreateSSHBitMask(defBitMask, addcores);
            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception {System.Reflection.MethodBase.GetCurrentMethod().Name}:", ex);
                return defBitMask;
            }
        }

        public static ulong CreateCustomBitMask(int _addcores)
        {
            try
            {

                /*
                ulong newBitMask = defBitMask;

                for (int i = 0; i < _addcores; ++i)
                {

                    if (i < addcores ||
                        (ProcessorInfo.HardwareCpuSets[App.logicalsT1[i]].ForcedEnable && !App.numazero_b) ||
                        (ProcessorInfo.HardwareCpuSets[App.logicalsT1[i]].ForcedEnable && App.numazero_b && App.n0enabledT1.Contains(App.logicalsT1[i]) && !App.n0disabledT1.Contains(App.logicalsT1[i])))                    {
                        {
                            newBitMask |= (uint)1 << (App.logicalsT1[i]);
                            //if (App.numazero_b) App.LogDebug($"N0={App.numazero_b} i={i} logical={App.logicalsT1[i]} T1Forced={ProcessorInfo.HardwareCpuSets[App.logicalsT1[i]].ForcedEnable} enabled e={App.n0enabledT1.Contains(i)} d={App.n0disabledT1.Contains(i)}");
                            //App.LogDebug($"i={i} calc newbitMask 0x{newBitMask:X8}");
                        }
                    }
                    //App.LogDebug($"i={i} T1Forced={ProcessorInfo.HardwareCpuSets[App.logicalsT1[i]].ForcedEnable} calc newbitMask 0x{newBitMask:X8}");
                }
                */
                _addcores = _addcores > addcores ? addcores : _addcores;

                return ProcessorInfo.CreateCustomBitMask(defBitMask, _addcores);
            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception {System.Reflection.MethodBase.GetCurrentMethod().Name}:", ex);
                return defBitMask;
            }
        }

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        public static void RunThreadBooster()
        {
            try
            {
                tbtoken = new CancellationToken();
                tbtoken = (CancellationToken)App.tbcts.Token;
                App.mrestb.Set();

                Process.GetCurrentProcess().PriorityBoostEnabled = true;
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;

                while (true) 
                {
                    lock (App.lockApply)
                    {
                        if (!App.mrestb.IsSet)
                        {
                            App.SysCpuSetMask = defFullBitMask;
                            App.SetSysCpuSet(0, "RTB_mrestb_IsSet");
                            //App.lastSysCpuSetMask = defFullBitMask;
                            App.PSAPlanDisable();
                            ZenControlDisable();
                            App.lastPSABiasCurrent = null;
                        }
                    }
                    App.mrestb.Wait();

                    // if (debugtb_steps) App.LogDebug("Stop 001");

                    if (tbtoken.IsCancellationRequested)
                    {
                        App.LogDebug("TB CANCELLATION REQUESTED");
                        tbtoken.ThrowIfCancellationRequested();
                    }

                    // if (debugtb_steps) App.LogDebug("Stop 002");

                    _cpuTotalLoad = ProcessorInfo.cpuTotalLoad;

                    //App.LogDebug($"ID={Thread.CurrentThread.ManagedThreadId} TB ON");

                    // if (debugtb_steps) App.LogDebug("Stop 003");

                    if (tbpoolingStamp != DateTime.MinValue)
                    {
                        _deltaPooling = DateTime.Now - tbpoolingStamp;
                        int _pooling = (int)_deltaPooling.TotalMilliseconds;
                        App.TBPoolingAverage.Push(_pooling);

                        // DISPLAY Governor Stats

                        //App.LogDebug($"POOLING={_pooling}ms UTILITY={App.TBPoolingAverage.Current-ThreadBooster.PoolingInterval:0}ms MAX={App.TBPoolingAverage.GetMax}ms");
                    }
                    tbpoolingStamp = DateTime.Now;

                    // if (debugtb_steps) App.LogDebug("Stop 004");

                    if (!bInit && App.activeconfig_b)
                    {
                        //uint eax = 0;
                        //uint edx = 0;
                        //App.WriteMsrTx(0xC0010202, eax, edx, 0);

                        // if (debugtb_steps) App.LogDebug("Stop 004a");

                        if (pactive.SysSetHack)
                        {
                            ProcessorInfo.SelectionType selection;
                            selection = ProcessorInfo.SelectionType.T1;
                            selection |= ProcessorInfo.SelectionType.ECores;
                            ProcessorInfo.SetDisabled(selection, 0);
                        }

                        App.LogInfo("Start ThreadBooster Init");

                        BuildDefaultMask("RTB1");

                        //App.lastSysCpuSetMask = null;
                        App.SysCpuSetMask = !App.pactive.SysSetHack && numazero_b ? defFullBitMask : defBitMask;
                        App.systemInfo.PSABias = "";

                        // if (debugtb_steps) App.LogDebug("Stop 004b");

                        ZenControlInit();

                        // if (debugtb_steps) App.LogDebug("Stop 004c");

                        if ((int)App.pactive.GameModeBias >= 0)
                        {
                            GModeMinBias = (int)App.pactive.GameModeBias;
                        }
                        else
                        {
                            if ((int)App.pactive.PowerTweak == 0)
                            {
                                GModeMinBias = 0;
                            }
                            else if ((int)App.pactive.PowerTweak == 1)
                            {
                                GModeMinBias = 1;
                            }
                            else
                            {
                                GModeMinBias = 2;
                            }
                        }

                        if ((int)App.pactive.ActiveModeBias >= 0)
                        {
                            AModeMinBias = (int)App.pactive.ActiveModeBias;
                        }
                        else
                        {
                            if ((int)App.pactive.PowerTweak == 0)
                            {
                                AModeMinBias = 0;
                            }
                            else if ((int)App.pactive.PowerTweak == 1)
                            {
                                AModeMinBias = 1;
                            }
                            else
                            {
                                AModeMinBias = 2;
                            }
                        }

                        if (basecores <= 4)
                        {
                            LoadZeroThresholdCount = 2;
                            LoadMediumThresholdCount = 8;
                            LoadHighThresholdCount = 4;
                        }
                        else if (basecores <= 8)
                        {

                            LoadZeroThresholdCount = 2;
                            LoadMediumThresholdCount = 6;
                            LoadHighThresholdCount = 3;
                        }
                        else if (basecores <= 12)
                        {

                            LoadZeroThresholdCount = 3;
                            LoadMediumThresholdCount = 4;
                            LoadHighThresholdCount = 2;
                        }
                        else
                        {
                            LoadZeroThresholdCount = 3;
                            LoadMediumThresholdCount = 3;
                            LoadHighThresholdCount = 2;
                        }

                        // if (debugtb_steps) App.LogDebug("Stop 004d");

                        App.PSAEnable();
                        if (App.psact_plan)
                        {
                            PSAct_Light(false);
                            PSAct_Deep(false);
                        }
                        
                        App.LogInfo("End ThreadBooster Init");
                        bInit = true;
                    }

                    // if (debugtb_steps) App.LogDebug("Stop 005");

                    if (App.reapplyProfile)
                    {
                        lock (lockApply)
                        {
                            //App.lastSysCpuSetMask = 0;
                            App.SysCpuSetMask = !App.pactive.SysSetHack && numazero_b ? defFullBitMask : defBitMask;
                            App.reapplyProfile = false;
                        }
                    }

                    if (App.pactive.PowerSaverActive && !App.psact_plan && !App.PPImportErrStatus)
                    {
                        App.LogDebug("PSA Enabled but not Active...");
                        App.PSAEnable();
                        if (App.psact_plan)
                        {
                            PSAct_Light(false);
                            PSAct_Deep(false);
                            SetPSAActive(0);
                        }
                    }

                    // if (debugtb_steps) App.LogDebug("Stop 006");

                    _deltaStamp = DateTime.Now - prevFullcoresStamp;
                    _deltaUA = DateTime.Now - App.UAStamp;

                    _deltaActivityStamp = DateTime.Now - prevActivityStamp;

                    if (_deltaActivityStamp.TotalSeconds > 1)
                    {
                        prevActivityStamp = DateTime.Now;

                        bool _GameMode = false;
                        bool _ActiveMode = false;
                        bool _FocusAssist = false;
                        bool _UserNotification = false;
                        bool _PLEvtPerfMode = false;

                        // if (debugtb_steps) App.LogDebug("Stop 007");

                        if (App.GetIdleTime() < App.pactive.PSALightSleepSeconds * 1000) _ActiveMode = true;

                        // if (debugtb_steps) App.LogDebug("Stop 008");

                        if (App.pactive.GameMode)
                        {

                            // if (debugtb_steps) App.LogDebug("Stop 008a");

                            FGFullScreenPrimary = App.IsForegroundWindowFullScreen(true);
                            FGFullScreen = false;

                            if (App.pactive.SecondaryMonitor)
                                FGFullScreen = App.IsForegroundWindowFullScreen(false);

                            // if (debugtb_steps) App.LogDebug("Stop 008b");

                            if (FGFullScreenPrimary || (App.pactive.SecondaryMonitor && FGFullScreen))
                            {
                                App.UAStamp = DateTime.Now;
                                _GameMode = true;
                            }

                            if (App.pactive.UserNotification && App.UserNotificationAvailable)
                            {
                                // if (debugtb_steps) App.LogDebug("Stop 008c");

                                unstate = App.GetUserNotificationState();
                                if (unstate == App.QUERY_USER_NOTIFICATION_STATE.QUNS_BUSY ||
                                unstate == App.QUERY_USER_NOTIFICATION_STATE.QUNS_PRESENTATION_MODE ||
                                unstate == App.QUERY_USER_NOTIFICATION_STATE.QUNS_RUNNING_D3D_FULL_SCREEN //||
                                                                                                          //_quns == App.QUERY_USER_NOTIFICATION_STATE.QUNS_FAILED
                                )
                                {
                                    _UserNotification = true;
                                    _GameMode = true;
                                }
                            }

                            if (App.pactive.FocusAssist && App.FocusAssistAvailable)
                            {
                                // if (debugtb_steps) App.LogDebug("Stop 008d");

                                try
                                {
                                    var qhsettings = (IQuietHoursSettings)new QuietHoursSettings();

                                    FocusAssistResult _far = App.GetFocusAssistState();
                                    if ((_far == FocusAssistResult.PRIORITY_ONLY || _far == FocusAssistResult.ALARMS_ONLY) && qhsettings.UserSelectedProfile.ToString() == "Microsoft.QuietHoursProfile.Unrestricted")
                                    {
                                        _FocusAssist = true;
                                        _GameMode = true;
                                    }
                                }
                                catch { }

                            }

                            if (App.pactive.PLPerfMode)
                            {
                                // if (debugtb_steps) App.LogDebug("Stop 008e");

                                if (OpenEventPerfMode(out evtPL))
                                {
                                    if (OpenSingleEvent(evtPL))
                                    {
                                        _GameMode = true;
                                        _PLEvtPerfMode = true;

                                        //if (PLEvtPerfMode == false)
                                            //App.LogDebug("Process Lasso Performance Mode enabled");
                                    }
                                    else
                                    {
                                        if (PLEvtPerfMode == true)
                                        {
                                            _PLEvtPerfMode = false;
                                            //App.LogDebug("Process Lasso Performance Mode disabled");
                                        }
                                    }
                                }
                            }

                            // if (debugtb_steps) App.LogDebug("Stop 008f");

                        }

                        if (_GameMode)
                        {
                            _ActiveMode = true;
                        }

                        if (GameMode != _GameMode ||
                            ActiveMode != _ActiveMode ||
                            UserNotification != _UserNotification ||
                            FocusAssist != _FocusAssist ||
                            PLEvtPerfMode != _PLEvtPerfMode)
                            {
                                lock (App.lockModeApply)
                                {
                                    GameMode = _GameMode;
                                    ActiveMode = _ActiveMode;
                                    UserNotification = _UserNotification;
                                    FocusAssist = _FocusAssist;
                                    PLEvtPerfMode = _PLEvtPerfMode;
                                }
                        }

                    }

                    // if (debugtb_steps) App.LogDebug("Stop 009");

                    if (ActiveMode == true)
                    {
                        App.UAStamp = DateTime.Now;
                    }

                    /*
                    bool loop = (PoolingTick == 40 && App.IsInVisualStudio && debugtb_steps) || (PoolingTick == 4 && !App.IsInVisualStudio) ? true : false;

                    if (loop)
                    {
                    }
                    */
                    
                    // if (debugtb_steps) App.LogDebug("Stop 010");

                    //if (_deltaUA.TotalSeconds > ClearForceThreshold && App.cpuTotalLoad.Current <= ClearForceLoadThreshold)
                    if (App.cpuTotalLoad.Current <= ClearForceLoadThreshold)
                    {
                        /*
                        for (int logical = 0; logical < App.logicalsT1.Count; logical++)
                        {
                            ProcessorInfo.ClearForceEnabled(logical);
                        }
                        */
                        ProcessorInfo.ClearForceEnabled();
                    }

                    // if (debugtb_steps) App.LogDebug("Stop 011");

                    if (App.pactive.PowerSaverActive && App.psact_b && App.psact_plan) PowerSaverActive();

                    // if (debugtb_steps) App.LogDebug("Stop 012");

                    if (zencontrol_b) ZenControl();

                    // if (debugtb_steps) App.LogDebug("Stop 013");

                    if (App.pactive.NumaZero && numazero_b && !App.pactive.SysSetHack && App.SysCpuSetMask != defFullBitMask)
                    {
                        BuildDefaultMask("RTB2");
                        App.LogDebug($"Set NumaZero ThreadBooster Apply mask 0x{defFullBitMask:X16}");
                        //App.lastSysCpuSetMask = 0;
                        App.SysCpuSetMask = defFullBitMask;
                    }

                    // if (debugtb_steps) App.LogDebug("Stop 013a");

                    if ((App.pactive.SysSetHack || App.pactive.PowerSaverActive) && bInit)
                    {
                        int _computedhlt = (int)(CountBits(defFullBitMask) * 100 / ProcessorInfo.LogicalCoresCount * HighTotalLoadFactor);

                        // if (debugtb_steps) App.LogDebug("Stop 014a");

                        if (forceCustomSysMask && forceCustomSysMaskStamp != DateTime.MinValue)
                        {
                            _deltaForceCustomBitMask = DateTime.Now - forceCustomSysMaskStamp;

                            if (_deltaForceCustomBitMask.TotalSeconds > forceCustomSysMaskDuration)
                            {
                                forceCustomSysMask = false;
                                forceCustomSysMaskStamp = DateTime.MinValue;
                            }
                            else
                            {
                                if (CustomSysMask != App.lastSysCpuSetMask && App.pactive.SysSetHack)
                                {
                                    App.LogDebug($"Applying SysCpuSetMask CustomSysMask 0x{CustomSysMask:X16}");
                                    setcores = CountBits(CustomSysMask);
                                    App.SysCpuSetMask = CustomSysMask;
                                }
                            }
                        }
                        else if (_cpuTotalLoad > _computedhlt || _deltaStamp.TotalSeconds < FullLoadHystSecs)
                        {
                            PoolingInterval = PoolingIntervalSlow;

                            if (_cpuTotalLoad > _computedhlt) prevFullcoresStamp = DateTime.Now;
                            //App.LogDebug($"Full CpuLoad: {ProcessorInfo.cpuTotalLoad:0} {_deltaStamp.TotalSeconds}");

                            if (defFullBitMask != App.lastSysCpuSetMask && (App.pactive.SysSetHack))
                            {
                                //App.LogDebug($"SysCpuSetMask defFullBitMask 0x{defFullBitMask:X16}");
                                setcores = basecores + addcores;
                                App.SysCpuSetMask = defFullBitMask;
                            }
                        }
                        else
                        {
                            //App.LogDebug($"_cpuTotalLoad {_cpuTotalLoad} > _computedhlt {_computedhlt}");
                            PoolingInterval = PoolingIntervalDefault;
                            //App.LogDebug($"CpuLoad: {ProcessorInfo.cpuTotalLoad:0}");
                            //App.LogDebug($"\tLoad-0-T0={ProcessorInfo.HardwareCpuSets[0].Load:0} \tLoad-0-T1={ProcessorInfo.HardwareCpuSets[1].Load:0}");

                            if (App.pactive.SysSetHack && InterlockBitMaskUpdate == 0)
                            {
                                // if (debugtb_steps) App.LogDebug("Stop 014b");
                                //needcores = usedcores = newsetcores = morecores = 0;

                                prevMorecores = ProcessorInfo.SSH_ondemandcores + ProcessorInfo.SSH_forcedcores;

                                newsetcores = morecores = 0;
                                //newsetcores = 0;
                                //morecores = prevMorecores;

                                needcores = ProcessorInfo.SSH_needcores;
                                usedcores = ProcessorInfo.SSH_usedcores;
                                TotalEnabledLoadNorm = ProcessorInfo.TotalEnabledLoadNorm;

                                //App.LogDebug($"RTBSSH1 **** prevMorecores {prevMorecores} SSH_ondemandcores {ProcessorInfo.SSH_ondemandcores} SSH_forcedcores {ProcessorInfo.SSH_forcedcores}");

                                //ProcessorInfo.InterlockCpuLoadUpdate = 0;

                                //App.LogDebug($"RTBSSH1 TotalEnabledLoadNorm={TotalEnabledLoadNorm} setcores {setcores} newsetcores {newsetcores} basecores {basecores} usedcores {usedcores} needcores {needcores} morecores {morecores} addcores={addcores}");

                                //morecores += needcores - basecores > 0 ? needcores - basecores : 0;
                                morecores = usedcores - basecores > 0 ? usedcores - basecores : 0;

                                /*
                                if ((needcores >= setcores && TotalEnabledLoadNorm > 95) || (usedcores >= basecores && usedcores/needcores >= 1.7))
                                {
                                    coreratio = (usedcores + needcores) / setcores;
                                    morecores += (coreratio >= 1.9) ? 3 : (coreratio >= 1.6) ? 2 : (coreratio >= 1.3) ? 1 : 0;
                                }
                                */
                                //morecores += (usedcores - needcores) / 2 >= 1 ? (usedcores - needcores) / 2 : 0;

                                //if ( needcores > 0 && needcores >= usedcores-1 && TotalEnabledLoadNorm > 90)
                                if (usedcores >= basecores && needcores > 0 && needcores >= usedcores - 1)
                                {
                                    //morecores += (usedcores - needcores) / 2 >= 1 ? (usedcores - needcores) / 2 : 1;
                                    morecores += (needcores >= usedcores - 1) ? 2 : 1;
                                }

                                //App.LogDebug($"RTBSSH2 setcores {setcores} newsetcores {newsetcores} basecores {basecores} usedcores {usedcores} needcores {needcores} morecores {morecores} addcores={addcores} TotalEnabledLoadNorm={TotalEnabledLoadNorm}");

                                if (morecores > addcores) morecores = addcores;

                                //App.LogDebug($"RTBSSH3 setcores {setcores} newsetcores {newsetcores} basecores {basecores} usedcores {usedcores} needcores {needcores} morecores {morecores} addcores={addcores}");

                                newsetcores = basecores + morecores;

                                //if (newsetcores > usedcores) newsetcores = usedcores;

                                //App.LogDebug($"RTBSSH4 setcores {setcores} newsetcores {newsetcores} basecores {basecores} usedcores {usedcores} needcores {needcores} morecores {morecores} addcores={addcores}");

                                _deltaStamp = DateTime.Now - prevIncreaseStamp;

                                SetHysteresis = (_deltaStamp.TotalSeconds <= IncreaseHysteresis && newsetcores <= setcores) ? true : false;

                                //App.LogDebug($"SetHysteresis {SetHysteresis} {_deltaStamp.TotalSeconds}");

                                //App.LogDebug($"RTBSSH5 setcores {setcores} newsetcores {newsetcores} basecores {basecores} usedcores {usedcores} needcores {needcores} morecores {morecores} addcores={addcores}");

                                deltalesscores = newsetcores - setcores;

                                //App.LogDebug($"RTBSSH6 setcores {setcores} newsetcores {newsetcores} deltalesscores {deltalesscores}");

                                if (deltalesscores < 0 && _deltaStamp.TotalSeconds >= IncreaseHysteresis && !SetHysteresis)
                                {
                                    //App.LogDebug($"Slow decreasing PRE morecores {morecores} SetHysteresis={SetHysteresis}");
                                    morecores = deltalesscores > 8 && morecores > 4 ? morecores - 4 : deltalesscores > 4 && morecores > 2 ? morecores - 2 : morecores - 1;
                                    //if (morecores < prevMorecores && morecores >= 0) prevMorecores = morecores;
                                    //App.LogDebug($"Slow decreasing POST morecores {morecores} SetHysteresis={SetHysteresis}");
                                }
                                else if (newsetcores >= basecores && needcores >= usedcores)
                                {
                                    if (TotalEnabledLoadNorm >= 100)
                                    {
                                        morecores = addcores;
                                    }
                                    else if (TotalEnabledLoadNorm > 98)
                                    {
                                        morecores += 2;
                                    }
                                    else if (TotalEnabledLoadNorm > 95)
                                    {
                                        morecores++;
                                    }
                                    if (morecores > addcores) morecores = addcores;
                                    //App.LogDebug($"Top morecores {morecores} SetHysteresis={SetHysteresis}");
                                }

                                //App.LogDebug($"RTBSSH7 setcores {setcores} newsetcores {newsetcores} basecores {basecores} usedcores {usedcores} needcores {needcores} morecores {morecores} addcores={addcores}");

                                newsetcores = basecores + morecores;

                                //App.LogDebug($"RTBSSH8 setcores {setcores} newsetcores {newsetcores} basecores {basecores} prevMorecores {prevMorecores} morecores {morecores} addcores={addcores}");

                                //if (morecores > 0 || prevMorecores != morecores || ProcessorInfo.SSH_UpdateSysMask)
                                if (setcores != newsetcores || ProcessorInfo.IsForceEnableAny() || ProcessorInfo.SSH_UpdateSysMask)
                                {
                                    //App.LogDebug($"RTBSSH defBitMask 0x{defBitMask:X16} defFullBitMask 0x{defFullBitMask:X16} addcores={addcores}");
                                    newBitMask = ProcessorInfo.CreateSSHBitMask(defBitMask, morecores);
                                    //prevNeedcores = needcores;
                                    //setcores = newsetcores;
                                    //App.LogDebug($"RTBSSH [{CountBits(App.SysCpuSetMask)}->{CountBits(newBitMask)}] newbitMask 0x{newBitMask:X16} prevBitMask 0x{App.SysCpuSetMask:X16} morecores={morecores}");

                                    if (newBitMask != App.SysCpuSetMask && !SetHysteresis)
                                    {
                                        // if (debugtb_steps) App.LogDebug("Stop 014c");
                                        //App.LogDebug($"RTBSSH set newbitMask 0x{newBitMask:X16} prevBitMask 0x{App.SysCpuSetMask:X16} {morecores}");
                                        //App.LogDebug($"setcores {setcores} newsetcores {newsetcores} basecores {basecores} morecores {morecores}");
                                        if (newsetcores > setcores) prevIncreaseStamp = DateTime.Now;
                                        App.SysCpuSetMask = newBitMask;
                                        InterlockBitMaskUpdate = 1;
                                        setcores = newsetcores;
                                        prevNeedcores = needcores;
                                        prevMorecores = morecores;
                                    }
                                }

                                //prevMorecores = morecores;
                                //if (needcores > 0 || morecores > 0 || usedcores > basecores)
                                //    App.LogDebug($"needcores {needcores} morecores {morecores} usedcores {usedcores} setcores {setcores} newsetcores {newsetcores} T0Load {TotalT0LoadNorm}");
                            }

                        }
                    }
                    PoolingTick++;
                    PoolingTick = PoolingTick > 4 ? 0 : PoolingTick;
                    wsleep((uint)(PoolingInterval * 1000));
                    //Thread.Sleep(PoolingInterval);
                    //App.LogDebug($"TB MONITOR TICK {PoolingInterval} ms");
                }
                //While
            }
            catch (OperationCanceledException)
            {
                App.SysCpuSetMask = defFullBitMask;
                App.SetSysCpuSet(0, "RTB_Op_Canc_Ex");
                App.PSAPlanDisable();
                ZenControlDisable();
                App.lastPSABiasCurrent = null;
                App.LogDebug("ThreadBooster cycle exiting due to OperationCanceled");
            }
            catch (Exception ex)
            {
                App.SysCpuSetMask = defFullBitMask;
                App.SetSysCpuSet(0, "RTB_Ex");
                App.PSAPlanDisable();
                ZenControlDisable();
                App.lastPSABiasCurrent = null;
                //App.LogExError($"ThreadBooster cycle Exception: {ex.Message}", ex);
                App.LogError($"ThreadBooster cycle Exception: {ex.Message}");
            }
            finally
            {
                ProcessorInfo.InterlockCpuLoadUpdate = 0;
            }
        }

        public static bool AudioPlaybackCheck()
        {
            bool bACheck = false;

            string checkdevice = "";
            
            App.LogDebug($"AudioPlayBackCheck BassWasapi");
            try
            {
                for (int widx = 0; widx < BassWasapi.DeviceCount; ++widx)
                {
                    WasapiDeviceInfo wdevinfo = new WasapiDeviceInfo();
                    wdevinfo = BassWasapi.GetDeviceInfo(widx);
                    if (wdevinfo.IsEnabled && !wdevinfo.IsInput)
                    {
                        bool bInit = BassWasapi.Init(widx);
                        wdevinfo = BassWasapi.GetDeviceInfo(widx);
                        int currdevice = BassWasapi.GetBassDevice(widx);
                        float wlevel = BassWasapi.GetDeviceLevel(widx);
                        if (bInit)
                        {
                            WasapiInfo winfo = new WasapiInfo();
                            winfo = BassWasapi.Info;
                        }
                        bool bBusy = wdevinfo.IsEnabled && !wdevinfo.IsInitialized ? true : false;

                        if (wlevel > 0)
                        {
                            checkdevice = $"WASAPI Shared Volume={wlevel} Device:{wdevinfo.Name}";
                            bACheck = true;
                            App.LogDebug($"[{widx}] [{currdevice}] {wdevinfo.Name} Busy={bBusy} VolumeLevel={wlevel} Enabled={wdevinfo.IsEnabled} Init={wdevinfo.IsInitialized}");
                        }

                        if (bBusy)
                        {
                            checkdevice = $"WASAPI ASIO/Exclusive Busy device Device:{wdevinfo.Name}";
                            bACheck = true;
                            App.LogDebug($"[{widx}] [{currdevice}] {wdevinfo.Name} Busy={bBusy} VolumeLevel={wlevel} Enabled={wdevinfo.IsEnabled} Init={wdevinfo.IsInitialized}");
                        }
                    }
                    BassWasapi.Free();
                    if (bACheck) continue;
                }

            }
            catch (Exception ex)
            {
                App.LogDebug($"BASS Exception {ex} {Bass.LastError}");
            }

            if (bACheck)
            {
                App.LogDebug($"{checkdevice} is playing Audio");
                return true;
            }

            if (Processes.currentIsRunning("ASIOhost32") || Processes.currentIsRunning("ASIOhost64"))
            {
                App.LogDebug($"foobar2000 is playing ASIO Audio");
                bACheck = true;
            }
            return bACheck;
        }
        public static void SetPSAActive(int? id)
        {
            try
            {
                if (id == null) id = 1;

                // 
                //2 = Booster / hpx
                //1 = Standard / bal
                //0 = Econo / low

                //Bias

                if (pactive.SelectedPersonality == 1)
                {
                    Guid _overlay = id == 0 ? PowerPlan.BetterBatteryLifeOverlay : id == 1 ? PowerPlan.DefaultOverlay : id == 2 ? PowerPlan.MaxPerformanceOverlay : PowerPlan.DefaultOverlay;
                    String _mode = id == 0 ? "Econo" : id == 1 ? "Standard" : id == 2 ? "Booster" : "Standard";


                    if (GameMode)
                    {
                        _overlay = PowerPlan.MaxPerformanceOverlay;
                        _mode = "GameMode";
                    }

                    //Set Overlay
                    if (!App.powerManager.SetActiveOverlay(_overlay))
                    {
                    
                        App.LogInfo($"Failed to set overlay {PowerPlanManager.GetOverlayLabel(_overlay)} for Bias: {_mode}");
                    }
                    ///*
                    else
                    {
                        App.LogInfo($"Successfully to set overlay {PowerPlanManager.GetOverlayLabel(_overlay)} for: {_mode}");
                    }
                    //*/
                }

                uint _value;
                App.systemInfo.SetPSAStatus(true);

                //AHCI Link Power Management - HIPM/DIPM
                _value = (uint)((id == 0) ? 0 : (id == 1) ? 0 : 0);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("0b2d69d7-a2a1-449c-9680-f91c70521c60"), PowerManagerAPI.PowerMode.AC, _value);

                //Primary NVMe Idle Timeout
                _value = (uint)((id == 0) ? 500 : (id == 1) ? 4000 : 8000);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("d639518a-e56d-4345-8af2-b9f32fb26109"), PowerManagerAPI.PowerMode.AC, _value);

                //Secondary NVMe Idle Timeout
                _value = (uint)((id == 0) ? 200 : (id == 1) ? 2000 : 4000);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("d3d55efd-c1ff-424e-9dc3-441be7833010"), PowerManagerAPI.PowerMode.AC, _value);

                //AHCI Link Power Management - Adaptive
                _value = (uint)((id == 0) ? 20 : (id == 1) ? 0 : 0);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("dab60367-53fe-4fbc-825e-521d069d2456"), PowerManagerAPI.PowerMode.AC, _value);

                //Secondary NVMe Power State Transition Latency Tolerance
                _value = (uint)((id == 0) ? 20 : (id == 1) ? 0 : 0);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("dbc9e238-6de9-49e3-92cd-8c2b4946b472"), PowerManagerAPI.PowerMode.AC, _value);

                //Primary NVMe Power State Transition Latency Tolerance
                _value = (uint)((id == 0) ? 10 : (id == 1) ? 0 : 0);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("fc95af4d-40e7-4b6d-835a-56d131dbc80e"), PowerManagerAPI.PowerMode.AC, _value);

                //Maximum Power Level
                _value = (uint)((id == 0) ? 100 : (id == 1) ? 100 : 100);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("51dea550-bb38-4bc4-991b-eacf37be5ec8"), PowerManagerAPI.PowerMode.AC, _value);

                //SEC NVMe Idle Timeout
                _value = (uint)((id == 0) ? 200 : (id == 1) ? 2000 : 4000);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("6b013a00-f775-4d61-9036-a62f7e7a6a5b"), PowerManagerAPI.PowerMode.AC, _value);

                //Hard disk burst ignore time
                _value = (uint)((id == 0) ? 3 : (id == 1) ? 0 : 0);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("80e3c60e-bb94-4ad8-bbe0-0d3195efc663"), PowerManagerAPI.PowerMode.AC, _value);

                //SEC NVMe Maximum Tolerable Trasition Latency
                _value = (uint)((id == 0) ? 20 : (id == 1) ? 0 : 0);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("a9d4776d-785b-4ae8-903b-b742ae4b6b62"), PowerManagerAPI.PowerMode.AC, _value);

                //NVMe NOPPME
                _value = (uint)((id == 0) ? 1 : (id == 1) ? 1 : 1);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("fc7372b6-ab2d-43ee-8797-15e9841f2cca"), PowerManagerAPI.PowerMode.AC, _value);

                //Processor performance time check interval
                _value = (uint)((id == 0) ? 30 : (id == 1) ? 20 : 20);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("4d2b0152-7d5c-498b-88e2-34345392a2c5"), PowerManagerAPI.PowerMode.AC, _value);

                //Processor performance increase threshold
                _value = (uint)((id == 0) ? 60 : (id == 1) ? 60 : 60);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("06cadf0e-64ed-448a-8927-ce7bf90eb35d"), PowerManagerAPI.PowerMode.AC, _value);

                //Processor performance increase threshold for Processor Power Efficiency Class 1
                _value = (uint)((id == 0) ? 85 : (id == 1) ? 60 : 60);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("06cadf0e-64ed-448a-8927-ce7bf90eb35e"), PowerManagerAPI.PowerMode.AC, _value);

                //Minimum processor state
                _value = (uint)((id == 0) ? 80 : (id == 1) ? 100 : 100);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("893dee8e-2bef-41e0-89c6-b55d0929964c"), PowerManagerAPI.PowerMode.AC, _value);

                //Minimum processor state for Processor Power Efficiency Class 1
                _value = (uint)((id == 0) ? 80 : (id == 1) ? 100 : 100);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("893dee8e-2bef-41e0-89c6-b55d0929964d"), PowerManagerAPI.PowerMode.AC, _value);

                //Processor idle promote threshold
                _value = (uint)((id == 0) ? 30 : (id == 1) ? 40 : 50);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("7b224883-b3cc-4d79-819f-8374152cbe7c"), PowerManagerAPI.PowerMode.AC, _value);

                //Processor performance boost policy
                _value = (uint)((id == 0) ? ProcPerfBoostEco : (id == 1) ? 100 : 100);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("45bcc044-d885-43e2-8605-ee0ec6e96b59"), PowerManagerAPI.PowerMode.AC, _value);

                //Processor performance decrease time
                _value = (uint)((id == 0) ? 5 : (id == 1) ? 1 : 1);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("d8edeb9b-95cf-4f95-a73c-b061973693c8"), PowerManagerAPI.PowerMode.AC, _value);

                //Processor performance decrease time for Processor Power Efficiency Class 1
                _value = (uint)((id == 0) ? 1 : (id == 1) ? 1 : 1);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("d8edeb9b-95cf-4f95-a73c-b061973693c9"), PowerManagerAPI.PowerMode.AC, _value);

                //Processor idle time check
                _value = (uint)((id == 0) ? 10000 : (id == 1) ? 5000 : 5000);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("c4581c31-89ab-4597-8e2b-9c9cab440e6b"), PowerManagerAPI.PowerMode.AC, _value);

                //Processor idle demote threshold
                _value = (uint)((id == 0) ? 45 : (id == 1) ? 20 : 20);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("4b92d758-5a24-4851-a470-815d78aee119"), PowerManagerAPI.PowerMode.AC, _value);

                //Processor duty cycling
                _value = (uint)((id == 0) ? App.PPDutyCycling : (id == 1) ? 0 : 0);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("4e4450b3-6179-4e91-b8f1-5bb9938f81a1"), PowerManagerAPI.PowerMode.AC, _value);

                //Processor performance decrease threshold
                _value = (uint)((id == 0) ? 25 : (id == 1) ? 45 : 45);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("12a0ab44-fe28-4fa9-b3bd-4b64f44960a6"), PowerManagerAPI.PowerMode.AC, _value);

                //Processor performance decrease threshold for Processor Power Efficiency Class 1
                _value = (uint)((id == 0) ? 25 : (id == 1) ? 45 : 45);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("12a0ab44-fe28-4fa9-b3bd-4b64f44960a7"), PowerManagerAPI.PowerMode.AC, _value);

                //Processor idle threshold scaling
                _value = (uint)((id == 0) ? 1 : (id == 1) ? 0 : 0);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("6c2993b0-8f48-481f-bcc6-00dd2742aa06"), PowerManagerAPI.PowerMode.AC, _value);

                //Processor performance boost mode
                _value = (uint)((id == 0) ? cpuBoostModeEco : (id == 1) ? cpuBoostModeStd : cpuBoostModeBoost);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("be337238-0d82-4146-a960-4f3749d470c7"), PowerManagerAPI.PowerMode.AC | PowerManagerAPI.PowerMode.DC, _value);

                //Processor performance increase policy
                _value = (uint)((id == 0) ? 0 : (id == 1) ? 2 : 2);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("465e1f50-b610-473a-ab58-00d1077dc418"), PowerManagerAPI.PowerMode.AC, _value);

                //Processor performance decrease policy
                _value = (uint)((id == 0) ? 1 : (id == 1) ? 0 : 0);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("40fbefc7-2e9d-4d25-a185-0cfd8574bac6"), PowerManagerAPI.PowerMode.AC, _value);

                //Processor performance decrease policy for Processor Power Efficiency Class 1
                _value = (uint)((id == 0) ? 1 : (id == 1) ? 0 : 0);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("40fbefc7-2e9d-4d25-a185-0cfd8574bac7"), PowerManagerAPI.PowerMode.AC, _value);

                //Processor performance autonomous mode
                _value = (uint)((id == 0) ? autonomousmode : (id == 1) ? autonomousmode : autonomousmode);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("8baa4a8a-14c6-4451-8e8b-14bdbd197537"), PowerManagerAPI.PowerMode.AC, _value);

                //Processor performance core parking increase time
                _value = (uint)(1);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("2ddd5a84-5a71-437e-912a-db0b8c788732"), PowerManagerAPI.PowerMode.AC, _value);

                //Processor performance increase time
                _value = (uint)1;
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("984cf492-3bed-4488-a8f9-4286c97bf5aa"), PowerManagerAPI.PowerMode.AC, _value);

                //Processor performance increase time for Processor Power Efficiency Class 1
                _value = (uint)1;
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("4009efa7-e72d-4cba-9edf-91084ea8cbc3"), PowerManagerAPI.PowerMode.AC, _value);

                //??
                //Latency sensitivity hint processor performance
                _value = (uint)95;
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("619b7505-003b-4e82-b7a6-4dd29c300971"), PowerManagerAPI.PowerMode.AC, _value);

                //??
                //Latency sensitivity hint min unparked cores/packages for Processor Power Efficiency Class 1
                _value = (uint)95;
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("616cdaa5-695e-4545-97ad-97dc2d1bdd89"), PowerManagerAPI.PowerMode.AC, _value);

                //??
                //Latency sensitivity hint min unparked cores/packages
                _value = (uint)90;
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("616cdaa5-695e-4545-97ad-97dc2d1bdd88"), PowerManagerAPI.PowerMode.AC, _value);

                //??
                //Latency sensitivity hint processor performance for Processor Power Efficiency Class 1
                _value = (uint)90;
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("619b7505-003b-4e82-b7a6-4dd29c300972"), PowerManagerAPI.PowerMode.AC, _value);

                //Processor autonomous activity window
                _value = (uint)((id == 0) ? 15000 : (id == 1) ? 20000 : 30000);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("cfeda3d0-7697-4566-a922-a9086cd49dfa"), PowerManagerAPI.PowerMode.AC, _value);

                //Heterogeneous thread scheduling policy
                _value = (uint)hepfg;
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("93b8b6dc-0698-4d1c-9ee4-0644e900c85d"), PowerManagerAPI.PowerMode.AC, _value);

                //Heterogeneous short running thread scheduling policy
                _value = (uint)hepbg;
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("bae08b81-2d5e-4688-ad6a-13243356654b"), PowerManagerAPI.PowerMode.AC, _value);

                //Processor energy performance preference policy
                _value = (uint)proceppp;
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("36687f9e-e3a5-4dbf-b1dc-15eb381c6863"), PowerManagerAPI.PowerMode.AC, _value);

                //Processor energy performance preference policy for Processor Power Efficiency Class 1
                _value = (uint)0;
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("36687f9e-e3a5-4dbf-b1dc-15eb381c6864"), PowerManagerAPI.PowerMode.AC, _value);

                //USB 3 Link Power Mangement
                _value = (uint)((id == 0) ? 1 : (id == 1) ? 0 : 0);
                App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.USB_SUBGROUP, new Guid("d4e98f31-5ffe-4ce1-be31-1b38b384c009"), PowerManagerAPI.PowerMode.AC, _value);

                //Heterogeneous policy in effect
                powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("7f2f5cfa-f10c-4823-b5e1-e93ae85f46b5"), PowerManagerAPI.PowerMode.AC | PowerManagerAPI.PowerMode.DC, (uint)hetPolicy);

                App.powerManager.SetActiveGuid(App.PPGuid);
            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception SetPSAActive({id}):", ex);
            }

        }
        public static void ZenControlDisable()
        {
            try 
            {
                if (App.systemInfo.Zen == null) return;
                if (App.systemInfo.Zen.powerTable == null) return;

                lock (App.lockZCInit)
                {
                    zencontrol_b = false;
                    ZenControlPBO_lastmode = null;
                    Zen_lastPPT = 0;
                    Zen_lastTDC = 0;
                    Zen_lastEDC = 0;

                    if (zencontrol_activated)
                    {
                        zencontrol_reapply = true;

                        if (zencontrol_b_ppt && systemInfo.ZenStartPPT > 0 && App.systemInfo.Zen.powerTable.PPT != App.systemInfo.ZenStartPPT)
                        {
                            //Thread.Sleep(20);
                            App.wsleep(10000);
                            if (App.systemInfo.Zen != null) App.systemInfo.Zen.SetPPTLimit((uint)systemInfo.ZenStartPPT);
                            App.LogInfo($"Setting PBO Limits back to start values PPT: {systemInfo.ZenStartPPT}");
                        }
                        if (zencontrol_b_tdc && systemInfo.ZenStartTDC > 0 && App.systemInfo.Zen.powerTable.TDC != App.systemInfo.ZenStartTDC)
                        {
                            //Thread.Sleep(20);
                            App.wsleep(10000);
                            if (App.systemInfo.Zen != null) App.systemInfo.Zen.SetTDCVDDLimit((uint)systemInfo.ZenStartTDC);
                            App.LogInfo($"Setting PBO Limits back to start values TDC: {systemInfo.ZenStartTDC}");
                        }
                        if (zencontrol_b_edc && systemInfo.ZenStartEDC > 0 && App.systemInfo.Zen.powerTable.EDC != App.systemInfo.ZenStartEDC)
                        {
                            //Thread.Sleep(20);
                            App.wsleep(10000);
                            if (App.systemInfo.Zen != null) App.systemInfo.Zen.SetEDCVDDLimit((uint)systemInfo.ZenStartEDC);
                            App.LogInfo($"Setting PBO Limits back to start values EDC: {systemInfo.ZenStartEDC}");
                        }
                    }
                }
            }
            catch { }
        }
        public static string ZenControlMode()
        {
            if (App.psact_deep_b) return "deep";
            if (App.psact_light_b) return "light";
            switch (App.PSABiasCurrent)
            {
                case 0:
                    return "low";
                case 1:
                    return "bal";
                default:
                    return "hpx";
            }
        }
        public static void ZenControl()
        {
            if (zencontrol_b)
            {
                if (zencontrol_reapply)
                {
                    string _mode = ZenControlMode();

                    ZenControlPBO(_mode);
                }
            }
        }

        public static void ZenControlInit()
        {
            ZenControlDisable();

            if (App.systemInfo.ZenStates && App.pactive.ZenControl)
            {
                lock (App.lockZCInit)
                {
                    //CHECK IF SMU OP Available
                    if (App.systemInfo.ZenPPT > 0) zencontrol_b_ppt = true;
                    if (App.systemInfo.ZenTDC > 0) zencontrol_b_tdc = true;
                    if (App.systemInfo.ZenEDC > 0) zencontrol_b_edc = true;

                    zenControlModes.Clear();

                    int _ppt, _tdc, _edc, _minppt, _mintdc, _minedc, _percentage;
                    int _autoppt, _autotdc, _autoedc, _ccds;
                    int _ptweak = App.pactive.PowerTweak;
                    int _ratio = _ptweak < 1 ? 90 : _ptweak > 1 ? 120 : 100;
                    float _archratio = 1;
                    string _mode;

                    _ccds = App.systemInfo.ZenCCDTotal > 0 ? App.systemInfo.ZenCCDTotal : 1;

                    if (App.systemInfo.Zen3) _archratio = 1.1f;
                    if (App.systemInfo.Zen4) _archratio = 1.5f;

                    //HIGH PERFORMANCE
                    _mode = "hpx";

                    _autoppt = (ProcessorInfo.PhysicalCoresCount * 15) + (_ccds * 15);
                    _autotdc = (ProcessorInfo.PhysicalCoresCount * 9) + (_ccds * 10);
                    _autoedc = (ProcessorInfo.PhysicalCoresCount * 12) + (_ccds * 10);

                    _autoppt = _ratio == 100 ? _autoppt : ((int)(0.5f + ((_autoppt / 100f) * _ratio)));
                    _autotdc = _ratio == 100 ? _autotdc : ((int)(0.5f + ((_autotdc / 100f) * _ratio)));
                    _autoedc = _ratio == 100 ? _autoedc : ((int)(0.5f + ((_autoedc / 100f) * _ratio)));


                    _autoppt = App.pactive.ZenControlPPTAuto ? _autoppt : (int)App.pactive.ZenControlPPThpx;
                    _autotdc = App.pactive.ZenControlTDCAuto ? _autotdc : (int)App.pactive.ZenControlTDChpx;
                    _autoedc = App.pactive.ZenControlEDCAuto ? _autoedc : (int)App.pactive.ZenControlEDChpx;

                    _ppt = App.pactive.ZenControlPPTAuto && !zencontrol_b_ppt ? 0 : _autoppt;
                    _tdc = App.pactive.ZenControlTDCAuto && !zencontrol_b_tdc ? 0 : _autotdc;
                    _edc = App.pactive.ZenControlEDCAuto && !zencontrol_b_edc ? 0 : _autoedc;

                    zenControlModes.Add(new ZenControlMode(_mode, (int)(_ppt * _archratio), (int)(_tdc * _archratio), (int)(_edc * _archratio)));

                    //BALANCED
                    _mode = "bal";
                    _percentage = 75;

                    _ppt = App.pactive.ZenControlPPTAuto && !zencontrol_b_edc ? 0 : ((int)(0.5f + ((_autoppt / 100f) * _percentage)));
                    _tdc = App.pactive.ZenControlTDCAuto && !zencontrol_b_tdc ? 0 : ((int)(0.5f + ((_autotdc / 100f) * _percentage)));
                    _edc = App.pactive.ZenControlEDCAuto && !zencontrol_b_edc ? 0 : ((int)(0.5f + ((_autoedc / 100f) * _percentage)));

                    zenControlModes.Add(new ZenControlMode(_mode, (int)(_ppt * _archratio), (int)(_tdc * _archratio), (int)(_edc * _archratio)));

                    //LOWPOWER
                    _mode = "low";
                    _percentage = 50;

                    _ppt = App.pactive.ZenControlEDCAuto && !zencontrol_b_ppt ? 0 : ((int)(0.5f + ((_autoppt / 100f) * _percentage)));
                    _tdc = App.pactive.ZenControlEDCAuto && !zencontrol_b_tdc ? 0 : ((int)(0.5f + ((_autotdc / 100f) * _percentage)));
                    _edc = App.pactive.ZenControlEDCAuto && !zencontrol_b_edc ? 0 : ((int)(0.5f + ((_autoedc / 100f) * _percentage)));

                    zenControlModes.Add(new ZenControlMode(_mode, _ppt, _tdc, _edc));

                    //LIGHT SLEEP
                    _mode = "light";
                    _minppt = 45;
                    _mintdc = 35;
                    _minedc = 50;
                    _percentage = 25;

                    _ppt = App.pactive.ZenControlPPTAuto && !zencontrol_b_ppt ? 0 : _minppt < ((int)(0.5f + ((_autoppt / 100f) * _percentage))) ? ((int)(0.5f + ((_autoppt / 100f) * _percentage))) : _minppt;
                    _tdc = App.pactive.ZenControlTDCAuto && !zencontrol_b_tdc ? 0 : _mintdc < ((int)(0.5f + ((_autotdc / 100f) * _percentage))) ? ((int)(0.5f + ((_autotdc / 100f) * _percentage))) : _mintdc;
                    _edc = App.pactive.ZenControlEDCAuto && !zencontrol_b_edc ? 0 : _minedc < ((int)(0.5f + ((_autoedc / 100f) * _percentage))) ? ((int)(0.5f + ((_autoedc / 100f) * _percentage))) : _minedc;

                    zenControlModes.Add(new ZenControlMode(_mode, _ppt, _tdc, _edc));

                    //DEEP SLEEP
                    _mode = "deep";
                    _minppt = 35;
                    _mintdc = 25;
                    _minedc = 35;
                    _percentage = 15;
                    _ppt = App.pactive.ZenControlPPTAuto && !zencontrol_b_ppt ? 0 : _minppt < ((int)(0.5f + ((_autoppt / 100f) * _percentage))) ? ((int)(0.5f + ((_autoppt / 100f) * _percentage))) : _minppt;
                    _tdc = App.pactive.ZenControlTDCAuto && !zencontrol_b_tdc ? 0 : _mintdc < ((int)(0.5f + ((_autotdc / 100f) * _percentage))) ? ((int)(0.5f + ((_autotdc / 100f) * _percentage))) : _mintdc;
                    _edc = App.pactive.ZenControlEDCAuto && !zencontrol_b_edc ? 0 : _minedc < ((int)(0.5f + ((_autoedc / 100f) * _percentage))) ? ((int)(0.5f + ((_autoedc / 100f) * _percentage))) : _minedc;

                    zenControlModes.Add(new ZenControlMode(_mode, _ppt, _tdc, _edc));

                    zencontrol_b = true;
                    zencontrol_activated = true;
                    zencontrol_reapply = true;

                }
            }
        }
        public static void ZenControlPBO(string _mode)
        {
            try
            {
                bool _reapply = zencontrol_reapply;

                if (_mode != ZenControlPBO_lastmode || _reapply == true)
                {
                    ZenControlMode zcmode = GetZenControlMode(_mode);
                    //var sw = Stopwatch.StartNew();

                    if (zcmode != null)
                    {
                        if (zencontrol_b_ppt && zcmode.PPT != Zen_lastPPT)
                        {
                            //Thread.Sleep(5);
                            App.wsleep(5000);
                            App.systemInfo.Zen.SetPPTLimit((uint)zcmode.PPT);
                            //App.LogDebug($"ZenControlPBO PPT {zcmode.PPT}: {sw.ElapsedMilliseconds}ms");
                            Zen_lastPPT = zcmode.PPT;
                        }

                        if (zencontrol_b_tdc && zcmode.TDC != Zen_lastTDC)
                        {
                            //Thread.Sleep(5);
                            App.wsleep(5000);
                            App.systemInfo.Zen.SetTDCVDDLimit((uint)zcmode.TDC);
                            //App.LogDebug($"ZenControlPBO TDC {zcmode.TDC}: {sw.ElapsedMilliseconds}ms");
                            Zen_lastTDC = zcmode.TDC;
                        }
                        if (zencontrol_b_edc && zcmode.EDC != Zen_lastEDC)
                        {
                            //Thread.Sleep(5);
                            App.wsleep(5000);
                            App.systemInfo.Zen.SetEDCVDDLimit((uint)zcmode.EDC);
                            //App.LogDebug($"ZenControlPBO EDC {zcmode.EDC}: {sw.ElapsedMilliseconds}ms");
                            Zen_lastEDC = zcmode.EDC;
                        }
                        //App.LogDebug($"ZenControlPBO Total: {sw.ElapsedMilliseconds}ms");
                    }
                    //sw = null;
                    ZenControlPBO_lastmode = _mode;
                    if (_reapply) zencontrol_reapply = false;
                }
            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception ZenControlPBO:", ex);
            }
        }
        public static ZenControlMode GetZenControlMode(string _mode)
        {
            try
            {
                foreach(ZenControlMode zc in zenControlModes)
                {
                    if (zc.Mode == _mode) return zc;
                }
                return null;
            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception GetZenControlMode:", ex);
                return null;
            }
        }
        public static void PSAPlanCheck()
        {
            try
            {
                if (App.powerManager.GetActiveGuid() != App.PPGuid)
                {
                    App.powerManager.SetActiveGuid(App.PPGuid);
                    App.LogDebug("Power Plan fix: back to Dynamic");
                }
            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception PSAPlanCheck:", ex);
            }
        }

        public static void PowerSaverActive()
        {
            try
            {
                int _cpuTotalLoad = (int)App.cpuTotalLoad.Current;
                int _cpuTotalLoadLong = (int)App.cpuTotalLoadLong.Current;

                _deltaHPX = DateTime.Now - _stampHPX;
                _deltaBAL = DateTime.Now - _stampBAL;
                _deltaAPC = DateTime.Now - _stampAPC;
                _deltaLSL = DateTime.Now - _stampLSL;
                _deltaDSL = DateTime.Now - _stampDSL;

                //App.LogDebug($"PTick {PoolingTick}");
                if (PoolingTick == 4)
                {
                    if (App.psact_b && App.psact_plan) PSAPlanCheck();
                }

                //App.LogDebug($"{_deltaHPX.TotalSeconds}>{App.pactive.PSABiasHpxHysteresis} {_deltaBAL.TotalSeconds}>{App.pactive.PSABiasBalHysteresis}");

                if (App.PSABiasCurrent == null) App.PSABiasCurrent = 2;

                if (_cpuTotalLoadLong > App.pactive.PSABiasHpxThreshold)
                {
                    App.PSABiasCurrent = 2;
                }
                else if (_cpuTotalLoadLong > App.pactive.PSABiasBalThreshold)
                {
                    if (_deltaHPX.TotalSeconds > App.pactive.PSABiasHpxHysteresis) App.PSABiasCurrent = 1;

                }
                else
                {
                    if (_deltaHPX.TotalSeconds > App.pactive.PSABiasHpxHysteresis && _deltaBAL.TotalSeconds > App.pactive.PSABiasBalHysteresis) App.PSABiasCurrent = 0;
                }

                if (GameMode)
                {
                    App.PSABiasCurrent = App.PSABiasCurrent < GModeMinBias ? GModeMinBias : App.PSABiasCurrent;
                }
                else if (ActiveMode)
                {
                    App.PSABiasCurrent = App.PSABiasCurrent < AModeMinBias ? AModeMinBias : App.PSABiasCurrent;
                }

                if (App.PSABiasCurrent != App.lastPSABiasCurrent || App.lastPSABiasCurrent == null || App.lastGameMode != GameMode)
                {
                    PSAPlanCheck();
                    SetPSAActive(App.PSABiasCurrent);
                    App.lastPSABiasCurrent = App.PSABiasCurrent;
                    App.lastGameMode = GameMode;
                    App.LogDebug($"New PSA Bias:{App.GetPSABiasLabel()}");
                    if (App.PSABiasCurrent == 2) _stampHPX = DateTime.Now;
                    if (App.PSABiasCurrent == 1) _stampBAL = DateTime.Now;
                    zencontrol_reapply = true;
                }

                if (_deltaUA.TotalSeconds > App.pactive.PSALightSleepSeconds && !App.psact_light_b && _cpuTotalLoad <= App.pactive.PSALightSleepThreshold && App.pactive.PSALightSleep && !ActiveMode)
                {
                    bool bSleep = true;

                    if (App.pactive.PSAAudioBlocksLightSleep)
                    {
                        if (bAudioCheck && _deltaAPC.TotalSeconds <= App.pactive.PSAAudioBlockHysteresis)
                        {
                            bSleep = false;
                        }
                        else if ((bAudioCheck && _deltaAPC.TotalSeconds > App.pactive.PSAAudioBlockHysteresis) || !bAudioCheck)
                        {
                            if (AudioPlaybackCheck())
                            {
                                bSleep = false;
                                bAudioCheck = true;
                                _stampAPC = DateTime.Now;
                                App.LogDebug($"AudioPlayBackCheck True");
                            }
                            else
                            {
                                bAudioCheck = false;
                            }
                        }
                    }
                    if (bSleep)
                    {
                        App.psact_light_b = true;
                        PSAPlanCheck();
                        PSAct_Light(App.psact_light_b);
                        zencontrol_reapply = true;
                        _stampLSL = DateTime.Now;
                        App.LogDebug($"IN => LIGHT SLEEP AVGLOAD={_cpuTotalLoad}");
                    }
                }

                if (_deltaUA.TotalSeconds > App.pactive.PSADeepSleepSeconds && !App.psact_deep_b && _cpuTotalLoad <= App.pactive.PSADeepSleepThreshold && App.pactive.PSADeepSleep && !ActiveMode)
                {
                    bool bSleep = true;
                    if (App.pactive.PSAAudioBlocksDeepSleep)
                    { 
                        if (bAudioCheck && _deltaAPC.TotalSeconds <= App.pactive.PSAAudioBlockHysteresis)
                        {
                            bSleep = false;
                        }
                        else if ((bAudioCheck && _deltaAPC.TotalSeconds > App.pactive.PSAAudioBlockHysteresis) || !bAudioCheck)
                        {
                            if (AudioPlaybackCheck())
                            {
                                bSleep = false;
                                bAudioCheck = true;
                                _stampAPC = DateTime.Now;
                                App.LogDebug($"AudioPlayBackCheck True");
                            }
                            else
                            {
                                bAudioCheck = false;
                            }
                        }
                    }
                    if (bSleep)
                    {
                        App.psact_deep_b = true;
                        PSAPlanCheck();
                        PSAct_Deep(App.psact_deep_b);
                        zencontrol_reapply = true;
                        App.ResetThrottling(Process.GetCurrentProcess().Id);
                        App.ResetThrottling(0);
                        _stampDSL = DateTime.Now;
                        App.LogDebug($"IN => DEEP SLEEP AVGLOAD={_cpuTotalLoad}");
                    }
                }

                if (App.psact_deep_b && (_deltaUA.TotalSeconds <= App.pactive.PSADeepSleepSeconds || _cpuTotalLoadLong > App.pactive.PSADeepSleepThreshold * 3) && App.pactive.PSADeepSleep && ((!ActiveMode && _deltaDSL.TotalSeconds > 5) || ActiveMode))
                {
                    App.psact_deep_b = false;
                    PSAPlanCheck();
                    PSAct_Deep(App.psact_deep_b);
                    App.SetThrottleExecSpeed(Process.GetCurrentProcess().Id, false);
                    App.SetIgnoreTimer(Process.GetCurrentProcess().Id, false);
                    App.SetThrottleExecSpeed(0, false);
                    App.SetIgnoreTimer(0, false);
                    zencontrol_reapply = true;
                    App.LogDebug($"OUT <= DEEP SLEEP AVGLOAD={_cpuTotalLoadLong}>{App.pactive.PSADeepSleepThreshold * 2} UserActiveLast={(int)_deltaUA.TotalSeconds}<={App.pactive.PSALightSleepSeconds}");
                }

                if (App.psact_light_b && (_deltaUA.TotalSeconds <= App.pactive.PSALightSleepSeconds || _cpuTotalLoadLong > App.pactive.PSALightSleepThreshold * 2) && App.pactive.PSALightSleep && ((!ActiveMode && _deltaLSL.TotalSeconds > 5) || ActiveMode))
                {
                    App.psact_light_b = false;
                    PSAPlanCheck();
                    PSAct_Light(App.psact_light_b);
                    SetPSAActive(App.PSABiasCurrent);
                    zencontrol_reapply = true;
                    App.UAStamp = DateTime.Now;
                    App.LogDebug($"OUT <= LIGHT SLEEP AVGLOAD={_cpuTotalLoadLong}>{App.pactive.PSALightSleepThreshold * 2} UserActiveLast={(int)_deltaUA.TotalSeconds}<={App.pactive.PSALightSleepSeconds}");
                }

            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception PowerSaverActive:", ex);
            }

        }

        public static void PSAct_Light(bool enable)
        {
            uint _value;

            using (Process thisprocess = Process.GetCurrentProcess())
            {
                if (enable) Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
                if (!enable) Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
            }

            //Set Overlay
            if (pactive.SelectedPersonality == 1)
            {
                if (!App.powerManager.SetActiveOverlay(PowerPlan.BetterBatteryLifeOverlay))
                {
                    App.LogDebug("Failed to set overlay BetterBatteryLife for Light Sleep");
                }
                /*
                else 
                {
                    App.LogDebug("Successfully set overlay BetterBatteryLife for Light Sleep");
                }
                */

            }

            //AHCI Link Power Management - HIPM/DIPM
            _value = (uint)(enable ? 2 : 0);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("0b2d69d7-a2a1-449c-9680-f91c70521c60"), PowerManagerAPI.PowerMode.AC, _value);

            //Primary NVMe Idle Timeout
            _value = (uint)(enable ? 100 : 500);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("d639518a-e56d-4345-8af2-b9f32fb26109"), PowerManagerAPI.PowerMode.AC, _value);

            //Secondary NVMe Idle Timeout
            _value = (uint)(enable ? 100 : 4000);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("d3d55efd-c1ff-424e-9dc3-441be7833010"), PowerManagerAPI.PowerMode.AC, _value);

            //AHCI Link Power Management - Adaptive
            _value = (uint)(enable ? 100 : 0);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("dab60367-53fe-4fbc-825e-521d069d2456"), PowerManagerAPI.PowerMode.AC, _value);

            //Secondary NVMe Power State Transition Latency Tolerance
            _value = (uint)(enable ? 100 : 0);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("dbc9e238-6de9-49e3-92cd-8c2b4946b472"), PowerManagerAPI.PowerMode.AC, _value);

            //Primary NVMe Power State Transition Latency Tolerance
            _value = (uint)(enable ? 50 : 0);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("fc95af4d-40e7-4b6d-835a-56d131dbc80e"), PowerManagerAPI.PowerMode.AC, _value);

            //Maximum Power Level
            _value = (uint)(enable ? 80 : 100);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("51dea550-bb38-4bc4-991b-eacf37be5ec8"), PowerManagerAPI.PowerMode.AC, _value);

            //SEC NVMe Idle Timeout
            _value = (uint)(enable ? 20 : 200);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("6b013a00-f775-4d61-9036-a62f7e7a6a5b"), PowerManagerAPI.PowerMode.AC, _value, false);

            //Hard disk burst ignore time
            _value = (uint)(enable ? 30 : 3);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("80e3c60e-bb94-4ad8-bbe0-0d3195efc663"), PowerManagerAPI.PowerMode.AC, _value);

            //SEC NVMe Maximum Tolerable Transition Latency
            _value = (uint)(enable ? 100 : 20);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("a9d4776d-785b-4ae8-903b-b742ae4b6b62"), PowerManagerAPI.PowerMode.AC, _value, false);

            //NVMe NOPPME
            _value = (uint)(enable ? 0 : 1);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("fc7372b6-ab2d-43ee-8797-15e9841f2cca"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance autonomous mode
            _value = (uint)(enable ? 0 : 1);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("8baa4a8a-14c6-4451-8e8b-14bdbd197537"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor energy performance preference policy
            _value = (uint)(enable ? 25 : proceppp);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("36687f9e-e3a5-4dbf-b1dc-15eb381c6863"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor energy performance preference policy for Processor Power Efficiency Class 1
            _value = (uint)(enable ? 25 : 0);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("36687f9e-e3a5-4dbf-b1dc-15eb381c6864"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance increase policy
            _value = (uint)(enable ? 1 : 2);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("465e1f50-b610-473a-ab58-00d1077dc418"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance increase policy for Processor Power Efficiency Class 1
            _value = (uint)(enable ? 1 : 2);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("465e1f50-b610-473a-ab58-00d1077dc419"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance decrease policy
            _value = (uint)(enable ? 0 : 1);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("40fbefc7-2e9d-4d25-a185-0cfd8574bac6"), PowerManagerAPI.PowerMode.AC, _value);

            //Minimum processor state
            _value = (uint)(enable ? 17 : 100);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("893dee8e-2bef-41e0-89c6-b55d0929964c"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor duty cycling
            _value = (uint)(enable ? 1 : 0);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("4e4450b3-6179-4e91-b8f1-5bb9938f81a1"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance decrease threshold
            _value = (uint)(enable ? 60 : 5);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("12a0ab44-fe28-4fa9-b3bd-4b64f44960a6"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance increase threshold for Processor Power Efficiency Class 1
            _value = (uint)(enable ? 95 : 60);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("06cadf0e-64ed-448a-8927-ce7bf90eb35e"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance decrease threshold for Processor Power Efficiency Class 1
            _value = (uint)(enable ? 60 : 5);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("12a0ab44-fe28-4fa9-b3bd-4b64f44960a7"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor idle promote threshold
            _value = (uint)(enable ? 20 : 30);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("7b224883-b3cc-4d79-819f-8374152cbe7c"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance boost policy
            _value = (uint)(enable ? 60 : 100);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("45bcc044-d885-43e2-8605-ee0ec6e96b59"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance decrease time
            _value = (uint)(enable ? 1 : 5);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("d8edeb9b-95cf-4f95-a73c-b061973693c8"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance decrease time for Processor Power Efficiency Class 1
            _value = (uint)(enable ? 1 : 1);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("7f2492b6-60b1-45e5-ae55-773f8cd5caec"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor idle time check
            _value = (uint)(enable ? 15000 : 10000);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("c4581c31-89ab-4597-8e2b-9c9cab440e6b"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor idle threshold scaling
            _value = (uint)(enable ? 1 : 0);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("6c2993b0-8f48-481f-bcc6-00dd2742aa06"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor idle demote threshold
            _value = (uint)(enable ? 55 : 45);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("4b92d758-5a24-4851-a470-815d78aee119"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance boost mode
            _value = (uint)(enable ? cpuBoostModeSleep : cpuBoostModeEco);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("be337238-0d82-4146-a960-4f3749d470c7"), PowerManagerAPI.PowerMode.AC|PowerManagerAPI.PowerMode.DC, _value);

            //Processor performance core parking min cores
            _value = (uint)(enable ? coreparking_min_light : coreparking_min);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("0cc5b647-c1df-4637-891a-dec35c318583"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance core parking min cores for Processor Power Efficiency Class 1
            _value = (uint)(enable ? coreparking_min_light_ec1 : coreparking_min_ec1);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("0cc5b647-c1df-4637-891a-dec35c318584"), PowerManagerAPI.PowerMode.AC, _value);

            //Initial performance for Processor Power Efficiency Class 1 when unparked
            _value = (uint)(100);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("1facfc65-a930-4bc5-9f38-504ec097bbc0"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance core parking concurrency threshold
            _value = (uint)(coreparking_concurrency);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("2430ab6f-a520-44a2-9601-f7f23b5134b1"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance core parking overutilization threshold
            _value = (uint)(60);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("943c8cb6-6f93-4227-ad87-e9a3feec08d1"), PowerManagerAPI.PowerMode.AC, _value);

            //Latency sensitivity hint processor performance
            _value = (uint)(100);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("619b7505-003b-4e82-b7a6-4dd29c300971"), PowerManagerAPI.PowerMode.AC, _value);

            //Latency sensitivity hint min un-parked cores/packages for Processor Power Efficiency Class 1
            _value = (uint)(100);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("619b7505-003b-4e82-b7a6-4dd29c300971"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance core parking increase time
            _value = (uint)(enable ? 7 : 1);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("2ddd5a84-5a71-437e-912a-db0b8c788732"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance core parking decrease policy
            _value = (uint)(0);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("71021b41-c749-4d21-be74-a00f335d582b"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance core parking decrease time
            _value = (uint)(enable ? 20 : 10);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("dfd10d17-d5eb-45dd-877a-9a34ddd15c82"), PowerManagerAPI.PowerMode.AC, _value);

            //Latency sensitivity hint min un-parked cores/packages
            _value = (uint)(enable ? 25 : 100);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("616cdaa5-695e-4545-97ad-97dc2d1bdd88"), PowerManagerAPI.PowerMode.AC, _value);

            //Latency sensitivity hint min un-parked cores/packages for Processor Power Efficiency Class 1
            _value = (uint)(enable ? 5 : 100);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("616cdaa5-695e-4545-97ad-97dc2d1bdd89"), PowerManagerAPI.PowerMode.AC, _value);

            //USB 3 Link Power Management
            _value = (uint)(enable ? 2 : 1);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.USB_SUBGROUP, new Guid("d4e98f31-5ffe-4ce1-be31-1b38b384c009"), PowerManagerAPI.PowerMode.AC, _value);

            App.powerManager.SetActiveGuid(App.PPGuid);
        }
        public static void PSAct_Deep(bool enable)
        {
            uint _value;

            //App.SysCpuSetMask = enable ? 0 : defBitMask;

            //Set Overlay
            if (pactive.SelectedPersonality == 1)
            {
                if (!App.powerManager.SetActiveOverlay(PowerPlan.BetterBatteryLifeOverlay))
                {
                    App.LogDebug("Failed to set overlay BetterBatteryLife for Deep Sleep");
                }
                /*
                else 
                {
                    App.LogDebug("Successfully set overlay BetterBatteryLife for Light Sleep");
                }
                */
            }

            //AHCI Link Power Management - HIPM/DIPM
            _value = (uint)(enable ? 4 : 2);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("0b2d69d7-a2a1-449c-9680-f91c70521c60"), PowerManagerAPI.PowerMode.AC, _value);

            //Maximum Power Level
            _value = (uint)(enable ? 60 : 80);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("51dea550-bb38-4bc4-991b-eacf37be5ec8"), PowerManagerAPI.PowerMode.AC, _value);

            //Heterogeneous thread scheduling policy
            _value = (uint)(enable ? hepfgdeep : hepfg);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("93b8b6dc-0698-4d1c-9ee4-0644e900c85d"), PowerManagerAPI.PowerMode.AC, _value);

            //Heterogeneous short running thread scheduling policy
            _value = (uint)(enable ? hepbgdeep : hepbg);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("bae08b81-2d5e-4688-ad6a-13243356654b"), PowerManagerAPI.PowerMode.AC, _value);

            //Link State Power Management
            _value = (uint)(enable ? 2 : 0);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PCIEXPRESS_SETTINGS_SUBGROUP, new Guid("ee12f906-d277-404b-b6da-e5fa1a576df5"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor energy performance preference policy
            _value = (uint)(enable ? 90 : 25);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("36687f9e-e3a5-4dbf-b1dc-15eb381c6863"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor energy performance preference policy for Processor Power Efficiency Class 1
            _value = (uint)(enable ? 90 : 25);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("36687f9e-e3a5-4dbf-b1dc-15eb381c6864"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance boost policy
            _value = (uint)(enable ? 40 : 60);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("45bcc044-d885-43e2-8605-ee0ec6e96b59"), PowerManagerAPI.PowerMode.AC, _value);

            //Maximum processor state
            _value = (uint)(enable ? 99 : 100);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("bc5038f7-23e0-4960-96da-33abaf5935ec"), PowerManagerAPI.PowerMode.AC, _value);

            //Maximum processor state for Processor Power Efficiency Class 1
            _value = (uint)(enable ? 99 : 100);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("bc5038f7-23e0-4960-96da-33abaf5935ed"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance increase time
            _value = (uint)(enable ? 3 : 1);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("984cf492-3bed-4488-a8f9-4286c97bf5aa"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance increase time for Processor Power Efficiency Class 1
            _value = (uint)(enable ? 3 : 1);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("4009efa7-e72d-4cba-9edf-91084ea8cbc3"), PowerManagerAPI.PowerMode.AC, _value);

            //Latency sensitivity hint processor performance
            _value = (uint)(enable ? 85 : 99);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("619b7505-003b-4e82-b7a6-4dd29c300971"), PowerManagerAPI.PowerMode.AC, _value);

            //Latency sensitivity hint processor performance for Processor Power Efficiency Class 1
            _value = (uint)(enable ? 85 : 99);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("619b7505-003b-4e82-b7a6-4dd29c300972"), PowerManagerAPI.PowerMode.AC, _value);

            //Core parking

            //Processor performance core parking min cores
            _value = (uint)(enable ? 0 : coreparking_min_light);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("0cc5b647-c1df-4637-891a-dec35c318583"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance core parking min cores for Processor Power Efficiency Class 1
            _value = (uint)(enable ? 0 : coreparking_min_light_ec1);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("0cc5b647-c1df-4637-891a-dec35c318584"), PowerManagerAPI.PowerMode.AC, _value);

            //Initial performance for Processor Power Efficiency Class 1 when un-parked
            _value = (uint)(enable ? 50 : 100);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("1facfc65-a930-4bc5-9f38-504ec097bbc0"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance core parking concurrency threshold
            _value = (uint)(enable ? 80 : 97);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("2430ab6f-a520-44a2-9601-f7f23b5134b1"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance core parking over-utilization threshold
            _value = (uint)(enable ? 85 : 60);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("943c8cb6-6f93-4227-ad87-e9a3feec08d1"), PowerManagerAPI.PowerMode.AC, _value);

            //Latency sensitivity hint min un-parked cores/packages
            _value = (uint)(enable ? 5 : 25);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("616cdaa5-695e-4545-97ad-97dc2d1bdd88"), PowerManagerAPI.PowerMode.AC, _value);

            //Latency sensitivity hint min un-parked cores/packages for Processor Power Efficiency Class 1
            _value = (uint)(enable ? 0 : 5);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("616cdaa5-695e-4545-97ad-97dc2d1bdd89"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance core parking increase time
            _value = (uint)(enable ? 7 : 1);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("2ddd5a84-5a71-437e-912a-db0b8c788732"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance core parking decrease policy
            _value = (uint)(enable ? 2 : 0);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("71021b41-c749-4d21-be74-a00f335d582b"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance core parking decrease time
            _value = (uint)(enable ? 50 : 20);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("dfd10d17-d5eb-45dd-877a-9a34ddd15c82"), PowerManagerAPI.PowerMode.AC, _value);

            //USB 3 Link Power Management
            _value = (uint)(enable ? 3 : 2);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.USB_SUBGROUP, new Guid("d4e98f31-5ffe-4ce1-be31-1b38b384c009"), PowerManagerAPI.PowerMode.AC, _value);

            App.powerManager.SetActiveGuid(App.PPGuid);
        }
        public static int CountBits(uint? n = 0)
        {
            n = n == null ? 0 : n;
            int count = 0;
            do
            {
                int has = (int)(n & 1);
                if (has == 1)
                {
                    count++;
                }

            } while ((n >>= 1) != 0);

            return count;
        }
        public static int CountBits(ulong? n = 0)
        {
            n = n == null ? 0 : n;
            int count = 0;
            do
            {
                int has = (int)(n & 1);
                if (has == 1)
                {
                    count++;
                }

            } while ((n >>= 1) != 0);

            return count;
        }
        public static void RunSysCpuSet()
        {
            try
            {
                systoken = new CancellationToken();
                systoken = (CancellationToken)App.syscts.Token;
                App.mressys.Set();

                Process.GetCurrentProcess().PriorityBoostEnabled = true;
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;

                while (true)
                {
                    lock (App.lockApply)
                    {
                        if (!App.mressys.IsSet)
                        {
                            SysCpuSetMask = ThreadBooster.defFullBitMask;
                            //lastSysCpuSetMask = ThreadBooster.defFullBitMask;
                            SetSysCpuSet(ThreadBooster.defFullBitMask, "RSCS_Mressys_IsSet");
                        }
                    }
                    App.mressys.Wait();

                    if (systoken.IsCancellationRequested)
                    {
                        App.LogDebug("SYSCPUSET CANCELLATION REQUESTED");
                        systoken.ThrowIfCancellationRequested();
                    }
                    //App.LogDebug($"ID={Thread.CurrentThread.ManagedThreadId} SYSCPUSET ON SetSystemCpuSet: 0x{App.SysCpuSetMask:X8} Last: 0x{App.lastSysCpuSetMask:X8}");

                    try
                    {
                        if (ThreadBooster.bInit)
                        {
                            if ((App.lastSysCpuSetMask != App.SysCpuSetMask && !App.psact_deep_b) || (App.psact_deep_b && App.SysCpuSetMask == 0))
                            {
                                App.LogInfo($"RunSysCpuSet Action [Cores:{basecores}+{addcores}={basecores + addcores}] 0x{App.lastSysCpuSetMask:X16} -> 0x{App.SysCpuSetMask:X16} - {CountBits(App.lastSysCpuSetMask)} -> {CountBits(App.SysCpuSetMask)}");
                                if (App.SetSysCpuSet(App.SysCpuSetMask, "RSCS_Action") == 0)
                                {
                                    //App.lastSysCpuSetMask = App.SysCpuSetMask;

                                    if (App.SysCpuSetMask != 0)
                                    {
                                        Processes.MaskParse();
                                    }
                                }
                            }
                        }
                        else if (!ThreadBooster.bInit && !pactive.SysSetHack && !pactive.NumaZero)
                        {
                            App.SetSysCpuSet(0, "RSCS_TBInit_False");
                            /*
                            if (App.SetSysCpuSet(0, "RSCS_TBInit_False") == 0)
                            {
                                App.lastSysCpuSetMask = App.SysCpuSetMask;
                            }
                            */
                        }
                    }
                    catch (Exception ex)
                    {
                        App.LogInfo($"Failed SetSystemCpuSet: {ex}");
                        App.SetSysCpuSet(0, "RSCS_Ex");
                    }
                    //Thread.Sleep(SysMaskPooling);
                    wsleep((uint)(SysMaskPooling*1000));
                }
            }
            catch (ObjectDisposedException)
            {
                App.LogDebug("OnSysCpuSet cycle exiting due to ObjectDisposed");
                App.SetSysCpuSet(0, "RSCS_ObjDisp_Ex");
            }
            catch (OperationCanceledException)
            {
                App.LogDebug("OnSysCpuSet cycle exiting due to OperationCanceled");
                App.SetSysCpuSet(0, "RSCS_Op_Canc_Ex");
            }
            catch (Exception ex)
            {
                App.LogExError($"OnSysCpuSet cycle Exception: {ex.Message}", ex);
                App.SetSysCpuSet(0, "RSCS_Ex");
            }
            finally
            {

            }

        }
        /*
        public static void OnSysCpuSet(object sender, ElapsedEventArgs args)
        {
            try
            {

                systoken = new CancellationToken();
                systoken = (CancellationToken)App.syscts.Token;

                if (systoken.IsCancellationRequested)
                {
                    App.LogDebug("SYSCPUSET CANCELLATION REQUESTED");
                    systoken.ThrowIfCancellationRequested();
                }

                //App.LogDebug($"SYSCPUSET ON SetSystemCpuSet: 0x{App.SysCpuSetMask:X8} Last: 0x{App.lastSysCpuSetMask:X8}");

                try
                {
                    if (((App.lastSysCpuSetMask != App.SysCpuSetMask && !App.psact_deep_b) || (App.psact_deep_b && App.SysCpuSetMask == 0)) && ThreadBooster.bInit)
                    {
                        App.LogInfo($"SSH Action [Cores:{basecores}+{addcores}={basecores + addcores}] 0x{App.lastSysCpuSetMask:X8} -> 0x{App.SysCpuSetMask:X8} - {CountBits(App.lastSysCpuSetMask)} -> {CountBits(App.SysCpuSetMask)}");
                        if (App.SetSysCpuSet(App.SysCpuSetMask) == 0)
                        {
                            App.lastSysCpuSetMask = App.SysCpuSetMask;

                            if (App.SysCpuSetMask != 0)
                            {
                                Processes.MaskParse();
                            }
                        }
                    }
                    else if (!ThreadBooster.bInit && !pactive.SysSetHack)
                    {
                        if (App.SetSysCpuSet(0) == 0)
                        {
                            App.lastSysCpuSetMask = App.SysCpuSetMask;
                        }
                    }
                }
                catch (Exception ex)
                {
                    App.LogInfo($"Failed SetSystemCpuSet: {ex}");
                    App.SetSysCpuSet(0);
                }

            }
            catch (ObjectDisposedException)
            {
                App.LogDebug("OnSysCpuSet cycle exiting due to ObjectDisposed");
                App.SetSysCpuSet(0);
            }
            catch (OperationCanceledException)
            {
                App.LogDebug("OnSysCpuSet cycle exiting due to OperationCanceled");
                App.SetSysCpuSet(0);
            }
            catch (Exception ex)
            {
                App.LogExError($"OnSysCpuSet cycle Exception: {ex.Message}", ex);
                App.SetSysCpuSet(0);
            }
        }

        */
        public static void ResetSSH()
        {
            ProcessorInfo.ResetLoadThreads();
            setcores = basecores;
            //App.SetSysCpuSet(defBitMask, "ResetSSH");
            App.SysCpuSetMask = defBitMask;
            App.lastSysCpuSetMask = 0;
        }
        public static void ForceCustomSysMask(bool enable, ulong bitmask = ulong.MaxValue, int duration = int.MaxValue)
        {
            if (bitmask == ulong.MaxValue) bitmask = defFullBitMask;

            if (enable)
            {
                forceCustomSysMask = true;
                forceCustomSysMaskStamp = DateTime.Now;
                forceCustomSysMaskDuration = duration;
                CustomSysMask = bitmask;
                setcores = CountBits(bitmask);
                App.LogDebug($"Enable ForceCustomSysMask 0x{bitmask:X16} SetCores={setcores}");
            }
            else
            {
                forceCustomSysMask = false;
                forceCustomSysMaskStamp = DateTime.MinValue;
                if (pactive.SysSetHack) ResetSSH();
                App.LogDebug($"Disable ForceCustomSysMask 0x{bitmask:X16}");
            }
        }
        public static void ProcMask(string processname, bool sysm, bool sysm_full, bool bitm, bool bitm_full, bool bitm_pfull, int ideal = -2, ulong sysmCustom = 0, ulong bitmCustom = 0, ulong bitmCustomProc = 0)
        {
            if (sysm && (App.pactive.SysSetHack) && !App.IsInVisualStudio) ProcSetSysMask(processname, sysm_full, sysmCustom);
            if (bitm && App.pactive.SysSetHack) ProcSetAffinityMask(processname, bitm_full, bitm_pfull, bitmCustom, bitmCustomProc);
            if (ideal != -2) ProcSetIdealThread(processname, ideal);
        }

        public static void ProcSetAffinityMask(string processname, bool full, bool procfull, ulong customMask = 0, ulong customMaskProc = 0)
        {
            if (!bInit) return;
            ulong mask = full ? defFullBitMask : App.SysCpuSetMask != null ? (uint)App.SysCpuSetMask : defFullBitMask;
            ulong procmask = procfull ? defFullBitMask : mask;
            procmask = customMaskProc > 0 ? customMaskProc : procmask;
            mask = customMask > 0 ? customMask : mask;

            try
            {
                Process[] whitelist = Process.GetProcessesByName(processname);

                if (whitelist.Length > 0)
                {
                    foreach (Process procs in whitelist)
                    {
                        //App.LogInfo($"[PID={threads[0].Id}]: {processname}");
                        Process proc = Process.GetProcessById(procs.Id);
                        //if (!App.IsInVisualStudio) proc.ProcessorAffinity = (IntPtr)App.SysCpuSetMask;
                        if (!App.IsInVisualStudio) proc.ProcessorAffinity = (IntPtr)procmask;
                        ProcessThreadCollection threads;
                        for (int ins = 0; ins < whitelist.Length; ++ins)
                        {
                            threads = whitelist[ins].Threads;
                            //App.LogDebug($"ProcSetAffinityMask: {processname}");
                            for (int t = 0; t < threads.Count; ++t)
                            {
                                //App.LogInfo($"T{t}: {processname}");
                                threads[t].ProcessorAffinity = (IntPtr)mask;
                            }
                        }
                    }
                }
                else {
                    App.LogInfo($"Failed not found ProcSetAffinityMask: {processname} [Full={full}] [Full={procfull}]");
                }
            }
            catch(Exception ex)
            {
                App.LogInfo($"Failed ProcSetAffinityMask: {processname} mask=0x{mask:X16} procmask=0x{procmask:X16} [Full={full}] [Full={procfull}]");
                App.LogDebug($"Failed ProcSetAffinityMask: {processname} mask=0x{mask:X16} procmask=0x{procmask:X16} [Full={full}] [Full={procfull}] {ex}");
            }
        }

        public static void ProcSetIdealThread(string processname, int ideal = -2)
        {
            if (!bInit) return;
            if (ideal == -2) return;
            if (ideal == -1) ideal = App.numazero_b ? App.n0enabledT0.First() : App.logicalsT0.First();

            try
            {
                Process[] procslist = Process.GetProcessesByName(processname);

                if (procslist.Length > 0)
                {
                    //App.LogDebug($"Process: {processname} set ideal Thread to {ideal}");
                    foreach (Process _process in procslist)
                    {
                        foreach (var thread in Process.GetProcessById(_process.Id).Threads.Cast<ProcessThread>())
                        {
                            if (ideal >= 0) thread.IdealProcessor = ideal;
                        }
                    }
                }
                else
                {
                    App.LogDebug($"Failed not found ProcSetIdealThread: {processname} [ideal={ideal}]");
                }
            }
            catch (Exception ex)
            {
                App.LogDebug($"Failed ProcSetIdealThread: {processname} [ideal={ideal}] {ex}");
            }
        }

        public static void ProcSetBoostThread(string processname)
        {
            if (!bInit) return;

            try
            {
                Process[] procslist = Process.GetProcessesByName(processname);

                if (procslist.Length > 0)
                {
                    foreach (Process _process in procslist)
                    {
                        foreach (var thread in Process.GetProcessById(_process.Id).Threads.Cast<ProcessThread>())
                        {
                            thread.PriorityBoostEnabled = true;
                        }
                    }
                }
                else
                {
                    App.LogDebug($"Failed not found ProcSetBoostThread: {processname}");
                }
            }
            catch (Exception ex)
            {
                App.LogDebug($"Failed ProcSetBoostThread: {processname} {ex}");
            }
        }
        public static void ProcSetSysMask(string processname, bool full, ulong customMask = 0)
        {
            if (!bInit) return;
            ulong mask = full ? defFullBitMask : App.SysCpuSetMask != null ? (uint)App.SysCpuSetMask : defFullBitMask;
            mask = customMask > 0 ? customMask : mask;
            try
            {

                Process[] whitelist = Process.GetProcessesByName(processname);
                if (whitelist.Length > 0)
                {
                    foreach (Process procs in whitelist)
                    {

                        int pid = procs.Id;
                        int ret = App.ProcSetCpuSet(pid, mask);
                        //int ret = App.ProcSetCpuSet(pid, (uint)App.SysCpuSetMask);
                        //App.LogDebug($"ProcSysDefMask: {processname}");
                        if (ret != 0) App.LogInfo($"Failed ProcSysDefMask: [Full={full}] {pid} - {processname} = {ret}");
                    }
                }
                else
                {
                    App.LogInfo($"Failed not found ProcSysDefMask: {processname}");
                }
            }
            catch (Exception ex)
            {
                App.LogInfo($"Failed ProcSysDefMask: {processname} Full={full} Custom=0x{customMask:X16} Mask=0x{mask:X16}");
                App.LogDebug($"Failed ProcSysDefMask: {processname} Full={full} {ex}");
            }
        }

        private static bool OpenEventPerfMode(out IntPtr evt)
        {
            uint unEventPermissions = 2031619;
            // Same as EVENT_ALL_ACCESS value in the Win32 realm
            //evt = (IntPtr)Kernel32.OpenEvent(unEventPermissions, false, "Global\\4f90aefd-30ca-4121-afec-106c3903fc0f");
            evt = App.OpenEvent(unEventPermissions, false, "Global\\4f90aefd-30ca-4121-afec-106c3903fc0f");
            if (evt == IntPtr.Zero) return false;
            return true;
        }
        private static bool OpenSingleEvent(IntPtr evt)
        {
            if (evt != IntPtr.Zero)
            {
                AutoResetEvent arEvt = new AutoResetEvent(false);
                arEvt.SafeWaitHandle = new Microsoft.Win32.SafeHandles.SafeWaitHandle(evt, true);
                WaitHandle[] waitHandles = [arEvt];
                int waitResult = WaitHandle.WaitAny(waitHandles, 0, false);
                if (waitResult == 0) return true;
            }
            return false;
        }

        public static void Close()
        {

            //App.systimer.Enabled = false;

            //App.tbtimer.Enabled = false;

            App.tbcts.Cancel();

            try
            {
                App.mrestb.Wait(App.tbcts.Token);
            }
            catch (OperationCanceledException)
            {
                App.LogDebug($"ThreadBooster Closed");
            }
            finally
            {
                App.thrThreadBooster = null;
                App.thrThreadBoosterRunning = false;
                App.tbcts.Dispose();
                App.tbcts = new CancellationTokenSource();                
                App.PSAPlanDisable();
                powerManager.SetActiveOverlay(PowerPlan.DefaultOverlay);
            }

        }
        public static void CloseSysMask()
        {

            //App.systimer.Enabled = false;

            //App.tbtimer.Enabled = false;

            App.syscts.Cancel();

            try
            {
                App.mressys.Wait(App.syscts.Token);
            }
            catch (OperationCanceledException)
            {
                App.LogDebug($"SysMask Closed");
            }
            finally
            {
                App.thrSys = null;
                App.thrSysRunning = false;
                App.syscts.Dispose();
                App.syscts = new CancellationTokenSource();
                Thread.Sleep(100);
                //SysCpuSetMask = 0;
                //lastSysCpuSetMask = ThreadBooster.defFullBitMask;
                SetSysCpuSet(ThreadBooster.defFullBitMask, "Close SCS");
            }
        }
    }
    public class ZenControlMode
    {
        public string Mode { get; set; }
        public int PPT { get; set; }
        public int TDC { get; set; }
        public int EDC { get; set; }
        public ZenControlMode(string _mode, int _ppt, int _tdc, int _edc)
        {
            Mode = _mode;
            PPT = _ppt;
            TDC = _tdc;
            EDC = _edc;
            App.LogDebug($"Added ZenControlMode: {_mode} {_ppt}/{_tdc}/{_edc}");
        }
    }

}
