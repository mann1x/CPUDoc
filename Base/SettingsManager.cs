using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ComponentModel;
using Config.Net;
using System.Reflection;

namespace CPUDoc
{

    public interface ICommandLineArgs
    {
        [Option(DefaultValue = -1)]
        int LogTrace { get; }
        
        [Option(DefaultValue = 0)]
        int inpoutdlldisable { get; set; }

        [Option(DefaultValue = 0)]
        int rebootpid { get; set; }
    }

    public interface IAppSettings : INotifyPropertyChanged
    {
        [Option(Alias="Startup.BootType", DefaultValue = 1)]
        int BootType { get; set; }

        [Option(Alias = "Startup.BootProfile", DefaultValue = 0)]
        int BootProfile { get; set; }

        [Option(Alias="Logging.Logtrace", DefaultValue = false)]
        bool LogTrace { get; set; }

        [Option(Alias = "CPUDoc.TopmostUI", DefaultValue = false)]
        bool TopmostUI { get; set; }

        [Option(Alias = "Updates.AUNotifications", DefaultValue = true)]
        bool AUNotifications { get; set; }

        [Option(Alias = "Startup.DisableInpoutx64dll", DefaultValue = false)]
        bool inpoutdlldisable { get; set; }

        [Option(Alias = "CPUDoc.CapframeXlistSHA", DefaultValue = "")]
        string CapframeXlistSHA { get; set; }

        [Option(Alias = "ZenOC.vCoreOffset", DefaultValue = 0)]
        int ZenOCvCoreOffset { get; set; }
    }

    public interface IApp3DMark : INotifyPropertyChanged
    {

        [Option(Alias = "EasyFMSI")]
        IAppProcessProfile EasyFMSI { get; set; }

        [Option(Alias = "SystemInfoHelper")]
        IAppProcessProfile SystemInfoHelper { get; set; }

        [Option(Alias = "TimeSpyExtreme.GT1")]
        IAppProcessProfile TSEGT1 { get; set; }

        [Option(Alias = "TimeSpyExtreme.GT2")]
        IAppProcessProfile TSEGT2 { get; set; }

        [Option(Alias = "TimeSpyExtreme.CPU")]
        IAppProcessProfile TSECPU { get; set; }

        [Option(Alias = "TimeSpy.GT1")]
        IAppProcessProfile TSGT1 { get; set; }

        [Option(Alias = "TimeSpy.GT2")]
        IAppProcessProfile TSGT2 { get; set; }

        [Option(Alias = "TimeSpy.CPU")]
        IAppProcessProfile TSCPU { get; set; }

        [Option(Alias = "SpeedWay")]
        IAppProcessProfile SWGT1 { get; set; }

        [Option(Alias = "PortRoyal")]
        IAppProcessProfile PRGT1 { get; set; }

        [Option(Alias = "FireStrike.GT1")]
        IAppProcessProfile FSGT1 { get; set; }

        [Option(Alias = "FireStrike.GT2")]
        IAppProcessProfile FSGT2 { get; set; }

        [Option(Alias = "FireStrike.Physics")]
        IAppProcessProfile FSPHY { get; set; }

        [Option(Alias = "FireStrike.Combi")]
        IAppProcessProfile FSCOMBI { get; set; }

        [Option(Alias = "FireStrikeUltra.GT1")]
        IAppProcessProfile FSUGT1 { get; set; }

        [Option(Alias = "FireStrikeUltra.GT2")]
        IAppProcessProfile FSUGT2 { get; set; }

        [Option(Alias = "FireStrikeUltra.Physics")]
        IAppProcessProfile FSUPHY { get; set; }

        [Option(Alias = "FireStrikeUltra.Combi")]
        IAppProcessProfile FSUCOMBI { get; set; }

        [Option(Alias = "FireStrikeExtreme.GT1")]
        IAppProcessProfile FSEGT1 { get; set; }

        [Option(Alias = "FireStrikeExtreme.GT2")]
        IAppProcessProfile FSEGT2 { get; set; }

        [Option(Alias = "FireStrikeExtreme.Physics")]
        IAppProcessProfile FSEPHY { get; set; }

        [Option(Alias = "FireStrikeExtreme.Combi")]
        IAppProcessProfile FSECOMBI { get; set; }

        [Option(Alias = "WildLife")]
        IAppProcessProfile WLGT1 { get; set; }

        [Option(Alias = "WildLifeExtreme")]
        IAppProcessProfile WLEGT1 { get; set; }

        [Option(Alias = "NightRaid.GT1")]
        IAppProcessProfile NRGT1 { get; set; }

        [Option(Alias = "NightRaid.GT2")]
        IAppProcessProfile NRGT2 { get; set; }

        [Option(Alias = "NightRaid.CPU")]
        IAppProcessProfile NRCPU { get; set; }

        [Option(Alias = "SkyDiver.GT1")]
        IAppProcessProfile SDGT1 { get; set; }

        [Option(Alias = "SkyDiver.GT2")]
        IAppProcessProfile SDGT2 { get; set; }

        [Option(Alias = "SkyDiver.Physics")]
        IAppProcessProfile SDPHY { get; set; }

        [Option(Alias = "SkyDiver.Combi")]
        IAppProcessProfile SDCOMBI { get; set; }

        [Option(Alias = "CloudGate.GT1")]
        IAppProcessProfile CGGT1 { get; set; }

        [Option(Alias = "CloudGate.GT2")]
        IAppProcessProfile CGGT2 { get; set; }

        [Option(Alias = "CloudGate.Physics")]
        IAppProcessProfile CGPHY { get; set; }

        [Option(Alias = "IceStorm.GT1")]
        IAppProcessProfile ISGT1 { get; set; }

        [Option(Alias = "IceStorm.GT2")]
        IAppProcessProfile ISGT2 { get; set; }

        [Option(Alias = "IceStorm.Physics")]
        IAppProcessProfile ISPHY { get; set; }

        [Option(Alias = "IceStormExtreme.GT1")]
        IAppProcessProfile ISEGT1 { get; set; }

        [Option(Alias = "IceStormExtreme.GT2")]
        IAppProcessProfile ISEGT2 { get; set; }

        [Option(Alias = "IceStormExtreme.Physics")]
        IAppProcessProfile ISEPHY { get; set; }

        [Option(Alias = "CPUProfileMax")]
        IAppProcessProfile CPUProfile { get; set; }

        [Option(Alias = "CPUProfile16")]
        IAppProcessProfile CPUProfile16 { get; set; }

        [Option(Alias = "CPUProfile8")]
        IAppProcessProfile CPUProfile8 { get; set; }

        [Option(Alias = "CPUProfile4")]
        IAppProcessProfile CPUProfile4 { get; set; }
        
        [Option(Alias = "CPUProfile2")]
        IAppProcessProfile CPUProfile2 { get; set; }

        [Option(Alias = "CPUProfile1")]
        IAppProcessProfile CPUProfile1 { get; set; }
    }

    public interface IAppProcessProfile : INotifyPropertyChanged
    {

        [Option(Alias = "Name", DefaultValue = "")]
        string Name { get; set; }

        [Option(Alias = "ProcesseNames", DefaultValue = "")]
        string[] ProcessNames { get; set; }

        [Option(Alias = "RegExMode", DefaultValue = false)]
        bool RegExMode { get; set; }

        [Option(Alias = "RegEx", DefaultValue = "")]
        string RegEx { get; set; }

        [Option(Alias = "ThreadsRequest", DefaultValue = -1)]
        int ThreadsRequest { get; set; }

        [Option(Alias = "IdealThread", DefaultValue = -1)]
        int IdealThread { get; set; }

        [Option(Alias = "CpuSetSysForced", DefaultValue = false)]
        bool CpuSetSysForced { get; set; }

        [Option(Alias = "CpuSetSysTRequest", DefaultValue = -1)]
        int CpuSetSysTRequest { get; set; }

        [Option(Alias = "CpuSetProcessTRequest", DefaultValue = -1)]
        int CpuSetProcessTRequest { get; set; }

        [Option(Alias = "AffinityProcTRequest", DefaultValue = -1)]
        int AffinityProcTRequest { get; set; }

        [Option(Alias = "AffinityThreadTRequest", DefaultValue = -1)]
        int AffinityThreadTRequest { get; set; }

        [Option(Alias = "ZenOCModeBeforeAction", DefaultValue = false)]
        bool ZenOCModeBeforeAction { get; set; }

        [Option(Alias = "ZenOCModeBeforeSet", DefaultValue = false)]
        bool ZenOCModeBeforeSet { get; set; }

        [Option(Alias = "ZenOCModeAfterAction", DefaultValue = false)]
        bool ZenOCModeAfterAction { get; set; }

        [Option(Alias = "ZenOCModeAfterSet", DefaultValue = false)]
        bool ZenOCModeAfterSet { get; set; }

        [Option(Alias = "ZenStaticProfile", DefaultValue = -1)]
        int ZenStaticProfile { get; set; }

        [Option(Alias = "ZenBCLKBeforeSet", DefaultValue = false)]
        bool ZenBCLKBeforeSet { get; set; }

        [Option(Alias = "ZenBCLKBefore", DefaultValue = 100)]
        int ZenBCLKBefore { get; set; }

        [Option(Alias = "ZenBCLKAfterSet", DefaultValue = false)]
        bool ZenBCLKAfterSet { get; set; }

        [Option(Alias = "ZenBCLKAfter", DefaultValue = 100)]
        int ZenBCLKAfter { get; set; }

        [Option(Alias = "SendHotKey", DefaultValue = false)]
        bool SendHotKey { get; set; }

        [Option(Alias = "HotKey", DefaultValue = "")]
        bool HotKey { get; set; }

        [Option(Alias = "SendHotKey2", DefaultValue = false)]
        bool SendHotKey2 { get; set; }

        [Option(Alias = "HotKey2", DefaultValue = "")]
        bool HotKey2 { get; set; }

        [Option(Alias = "ProcessBoost", DefaultValue = true)]
        bool ProcessBoost { get; set; }

        [Option(Alias = "ProcessDisableThrottle", DefaultValue = true)]
        bool ProcessDisableThrottle { get; set; }
    }

    public interface IAppZenProfileOC : INotifyPropertyChanged
    {

        [Option(Alias = "Name", DefaultValue = "StaticProfile")]
        string Name { get; set; }

        [Option(Alias = "Profile.id", DefaultValue = 1)]
        int id { get; set; }

        [Option(Alias = "vCoreVoltage", DefaultValue = -1)]
        int vCoreVoltage { get; set; }

        [Option(Alias = "SetBCLK", DefaultValue = false)]
        bool SetBCLK { get; set; }

        [Option(Alias = "BCLK", DefaultValue = 100)]
        int BCLK { get; set; }

        [Option(Alias = "SetMode", DefaultValue = 0)]
        int SetMode { get; set; }

        [Option(Alias = "FrequencyCCD0", DefaultValue = -1)]
        int FrequencyCCD0 { get; set; }

        [Option(Alias = "FrequencyCCD1", DefaultValue = -1)]
        int FrequencyCCD1 { get; set; }

        [Option(Alias = "FrequencyCCD2", DefaultValue = -1)]
        int FrequencyCCD2 { get; set; }

        [Option(Alias = "FrequencyCCD3", DefaultValue = -1)]
        int FrequencyCCD3 { get; set; }

        [Option(Alias = "FrequencyCCX0", DefaultValue = -1)]
        int FrequencyCCX0 { get; set; }

        [Option(Alias = "FrequencyCCX1", DefaultValue = -1)]
        int FrequencyCCX1 { get; set; }

        [Option(Alias = "FrequencyCCX2", DefaultValue = -1)]
        int FrequencyCCX2 { get; set; }

        [Option(Alias = "FrequencyCCX3", DefaultValue = -1)]
        int FrequencyCCX3 { get; set; }

        [Option(Alias = "FrequencyCCX4", DefaultValue = -1)]
        int FrequencyCCX4 { get; set; }

        [Option(Alias = "FrequencyCCX5", DefaultValue = -1)]
        int FrequencyCCX5 { get; set; }

        [Option(Alias = "FrequencyCCX6", DefaultValue = -1)]
        int FrequencyCCX6 { get; set; }

        [Option(Alias = "FrequencyCCX7", DefaultValue = -1)]
        int FrequencyCCX7 { get; set; }

        [Option(Alias = "FrequencyCore0", DefaultValue = -1)]
        int FrequencyCore0 { get; set; }

        [Option(Alias = "FrequencyCore1", DefaultValue = -1)]
        int FrequencyCore1 { get; set; }

        [Option(Alias = "FrequencyCore2", DefaultValue = -1)]
        int FrequencyCore2 { get; set; }

        [Option(Alias = "FrequencyCore3", DefaultValue = -1)]
        int FrequencyCore3 { get; set; }

        [Option(Alias = "FrequencyCore4", DefaultValue = -1)]
        int FrequencyCore4 { get; set; }

        [Option(Alias = "FrequencyCore5", DefaultValue = -1)]
        int FrequencyCore5 { get; set; }

        [Option(Alias = "FrequencyCore6", DefaultValue = -1)]
        int FrequencyCore6 { get; set; }

        [Option(Alias = "FrequencyCore7", DefaultValue = -1)]
        int FrequencyCore7 { get; set; }

        [Option(Alias = "FrequencyCore8", DefaultValue = -1)]
        int FrequencyCore8 { get; set; }

        [Option(Alias = "FrequencyCore9", DefaultValue = -1)]
        int FrequencyCore9 { get; set; }

        [Option(Alias = "FrequencyCore10", DefaultValue = -1)]
        int FrequencyCore10 { get; set; }

        [Option(Alias = "FrequencyCore11", DefaultValue = -1)]
        int FrequencyCore11 { get; set; }

        [Option(Alias = "FrequencyCore12", DefaultValue = -1)]
        int FrequencyCore12 { get; set; }

        [Option(Alias = "FrequencyCore13", DefaultValue = -1)]
        int FrequencyCore13 { get; set; }

        [Option(Alias = "FrequencyCore14", DefaultValue = -1)]
        int FrequencyCore14 { get; set; }

        [Option(Alias = "FrequencyCore15", DefaultValue = -1)]
        int FrequencyCore15 { get; set; }

    }
    public interface IAppZenProfilesOC : INotifyPropertyChanged
    {

        [Option(Alias = "Profile0")]
        IAppZenProfileOC profile0 { get; set; }

        [Option(Alias = "Profile1")]
        IAppZenProfileOC profile1 { get; set; }

        [Option(Alias = "Profile2")]
        IAppZenProfileOC profile2 { get; set; }

        [Option(Alias = "Profile3")]
        IAppZenProfileOC profile3 { get; set; }

        [Option(Alias = "Profile4")]
        IAppZenProfileOC profile4 { get; set; }

        [Option(Alias = "Profile5")]
        IAppZenProfileOC profile5 { get; set; }

        [Option(Alias = "Profile6")]
        IAppZenProfileOC profile6 { get; set; }

        [Option(Alias = "Profile7")]
        IAppZenProfileOC profile7 { get; set; }

        [Option(Alias = "Profile8")]
        IAppZenProfileOC profile8 { get; set; }

        [Option(Alias = "Profile9")]
        IAppZenProfileOC profile9 { get; set; }

        [Option(Alias = "Profile10")]
        IAppZenProfileOC profile10 { get; set; }

        [Option(Alias = "Profile11")]
        IAppZenProfileOC profile11 { get; set; }

        [Option(Alias = "Profile12")]
        IAppZenProfileOC profile12 { get; set; }

        [Option(Alias = "Profile13")]
        IAppZenProfileOC profile13 { get; set; }

        [Option(Alias = "Profile14")]
        IAppZenProfileOC profile14 { get; set; }

        [Option(Alias = "Profile15")]
        IAppZenProfileOC profile15 { get; set; }

        [Option(Alias = "Profile16")]
        IAppZenProfileOC profile16 { get; set; }

        [Option(Alias = "Profile17")]
        IAppZenProfileOC profile17 { get; set; }

        [Option(Alias = "Profile18")]
        IAppZenProfileOC profile18 { get; set; }

        [Option(Alias = "Profile19")]
        IAppZenProfileOC profile19 { get; set; }
    }

    public interface IAppConfigs : INotifyPropertyChanged
    {

        [Option(Alias = "Profile0")]
        IAppConfig profile0 { get; set; }

        [Option(Alias = "Profile1")]
        IAppConfig profile1 { get; set; }

        [Option(Alias = "Profile2")]
        IAppConfig profile2 { get; set; }

        [Option(Alias = "Profile3")]
        IAppConfig profile3 { get; set; }

        [Option(Alias = "Profile4")]
        IAppConfig profile4 { get; set; }

        [Option(Alias = "Profile5")]
        IAppConfig profile5 { get; set; }

        [Option(Alias = "Profile6")]
        IAppConfig profile6 { get; set; }

        [Option(Alias = "Profile7")]
        IAppConfig profile7 { get; set; }

        [Option(Alias = "Profile8")]
        IAppConfig profile8 { get; set; }

        [Option(Alias = "Profile9")]
        IAppConfig profile9 { get; set; }
    }
    public interface IAppConfig : INotifyPropertyChanged
    {
        [Option(Alias = "CPUDoc.ThreadBooster", DefaultValue = true)]
        bool ThreadBooster { get; set; }

        [Option(Alias = "CPUDoc.SysSetHack", DefaultValue = false)]
        bool SysSetHack { get; set; }

        [Option(Alias = "CPUDoc.PowerSaverActive", DefaultValue = true)]
        bool PowerSaverActive { get; set; }

        [Option(Alias = "CPUDoc.IdealThread", DefaultValue = -2)]
        int IdealThread { get; set; }

        // 1 = Global, 0 = Only Profiles
        [Option(Alias = "CPUDoc.IdealThreadScope", DefaultValue = 0)]
        int IdealThreadScope { get; set; }

        [Option(Alias = "PSA.LightSleep", DefaultValue = true)]
        bool PSALightSleep { get; set; }

        [Option(Alias = "PSA.DeepSleep", DefaultValue = true)]
        bool PSADeepSleep { get; set; }

        [Option(Alias = "PSA.AudioBlocksLightSleep", DefaultValue = false)]
        bool PSAAudioBlocksLightSleep { get; set; }

        [Option(Alias = "PSA.AudioBlocksDeepSleep", DefaultValue = true)]
        bool PSAAudioBlocksDeepSleep { get; set; }

        [Option(Alias = "PSA.AudioBlocksHysteresisSecs", DefaultValue = 120)]
        int PSAAudioBlockHysteresis { get; set; }

        [Option(Alias = "PSA.LightSleepSeconds", DefaultValue = 15)]
        int PSALightSleepSeconds { get; set; }

        [Option(Alias = "PSA.DeepSleepSeconds", DefaultValue = 60)]
        int PSADeepSleepSeconds { get; set; }

        [Option(Alias = "ZenControl.WHEASuppressor", DefaultValue = false)]
        bool WHEASuppressor { get; set; }

        [Option(Alias = "ZenControl.COStandbySafe", DefaultValue = true)]
        bool COStandbySafe { get; set; }

        [Option(Alias = "CPUDoc.NumaZero", DefaultValue = false)]
        bool NumaZero { get; set; }

        [Option(Alias = "CPUDoc.NumaZeroType", DefaultValue = 0)]
        int NumaZeroType { get; set; }

        [Option(Alias = "CPUDoc.NumaZeroAutoType", DefaultValue = 0)]
        int NumaZeroAutoType { get; set; }

        [Option(Alias = "CPUDoc.NumaZeroExcludeType", DefaultValue = 0)]
        int NumaZeroExcludeType { get; set; }

        [Option(Alias = "Profile.Enabled", DefaultValue = true)]
        bool Enabled { get; set; }

        [Option(Alias = "PSA.LightSleepThreshold", DefaultValue = 14)]
        int PSALightSleepThreshold { get; set; }

        [Option(Alias = "PSA.DeepSleepThreshold", DefaultValue = 8)]
        int PSADeepSleepThreshold { get; set; }

        [Option(Alias = "Profile.id", DefaultValue = 1)]
        int id { get; set; }

        [Option(Alias = "Profile.Description", DefaultValue = "")]
        string Description { get; set; }

        [Option(Alias = "PSA.Bias", DefaultValue = 1)]
        int PSABias { get; set; }

        [Option(Alias = "PSA.BiasAuto", DefaultValue = true)]
        bool PSABiasAuto { get; set; }

        [Option(Alias = "PSA.BiasBoosterHysteresis", DefaultValue = 60)]
        int PSABiasHpxHysteresis { get; set; }

        [Option(Alias = "PSA.BiasBoosterThreshold", DefaultValue = 65)]
        int PSABiasHpxThreshold { get; set; }

        [Option(Alias = "PSA.BiasStandardThreshold", DefaultValue = 30)]
        int PSABiasBalHysteresis { get; set; }

        [Option(Alias = "PSA.BiasStandardThreshold", DefaultValue = 25)]
        int PSABiasBalThreshold { get; set; }

        [Option(Alias = "CPUDoc.ManualPoolingRate", DefaultValue = false)]
        bool ManualPoolingRate { get; set; }

        [Option(Alias = "CPUDoc.PoolingRate", DefaultValue = 3)]
        int PoolingRate { get; set; }

        [Option(Alias = "CPUDoc.ZenControl", DefaultValue = false)]
        bool ZenControl { get; set; }

        [Option(Alias = "ZenControl.PPTAuto", DefaultValue = true)]
        bool ZenControlPPTAuto { get; set; }

        [Option(Alias = "ZenControl.PPTBooster", DefaultValue = 0)]
        int ZenControlPPThpx { get; set; }

        [Option(Alias = "ZenControl.PPTStandard", DefaultValue = 0)]
        int ZenControlPPTbal { get; set; }

        [Option(Alias = "ZenControl.PPTEconomizer", DefaultValue = 0)]
        int ZenControlPPTlow { get; set; }

        [Option(Alias = "ZenControl.PPTLightSleep", DefaultValue = 0)]
        int ZenControlPPTlight { get; set; }

        [Option(Alias = "ZenControl.PPTDeepSleep", DefaultValue = 0)]
        int ZenControlPPTdeep { get; set; }

        [Option(Alias = "ZenControl.EDCAuto", DefaultValue = true)]
        bool ZenControlEDCAuto { get; set; }

        [Option(Alias = "ZenControl.EDCBooster", DefaultValue = 0)]
        int ZenControlEDChpx { get; set; }

        [Option(Alias = "ZenControl.EDCStandard", DefaultValue = 0)]
        int ZenControlEDCbal { get; set; }

        [Option(Alias = "ZenControl.EDCEconomizer", DefaultValue = 0)]
        int ZenControlEDClow { get; set; }

        [Option(Alias = "ZenControl.EDCLightSleep", DefaultValue = 0)]
        int ZenControlEDClight { get; set; }

        [Option(Alias = "ZenControl.EDCDeepSleep", DefaultValue = 0)]
        int ZenControlEDCdeep { get; set; }

        [Option(Alias = "ZenControl.TDCAuto", DefaultValue = true)]
        bool ZenControlTDCAuto { get; set; }

        [Option(Alias = "ZenControl.TDCBooster", DefaultValue = 0)]
        int ZenControlTDChpx { get; set; }

        [Option(Alias = "ZenControl.TDCStandard", DefaultValue = 0)]
        int ZenControlTDCbal { get; set; }

        [Option(Alias = "ZenControl.TDCEconomizer", DefaultValue = 0)]
        int ZenControlTDClow { get; set; }

        [Option(Alias = "ZenControl.TDCLightSleep", DefaultValue = 0)]
        int ZenControlTDClight { get; set; }

        [Option(Alias = "ZenControl.TDCDeepSleep", DefaultValue = 0)]
        int ZenControlTDCdeep { get; set; }

        [Option(Alias = "ZenControl.CPUTempTrayIcon", DefaultValue = false)]
        bool ZenControlCPUTempTrayIcon { get; set; }

        [Option(Alias = "PSA.PowerTweak", DefaultValue = 1)]
        int PowerTweak { get; set; }

        [Option(Alias = "PSA.GameMode", DefaultValue = true)]
        bool GameMode { get; set; }

        [Option(Alias = "PSA.FocusAssist", DefaultValue = true)]
        bool FocusAssist { get; set; }

        [Option(Alias = "PSA.UserNotification", DefaultValue = true)]
        bool UserNotification { get; set; }

        [Option(Alias = "PSA.SecondaryMonitor", DefaultValue = false)]
        bool SecondaryMonitor { get; set; }

        [Option(Alias = "PSA.GameModeBias", DefaultValue = -1)]
        int GameModeBias { get; set; }

        [Option(Alias = "PSA.ActiveModeBias", DefaultValue = -1)]
        int ActiveModeBias { get; set; }

        [Option(Alias = "PSA.Personality", DefaultValue = 0)]
        int Personality { get; set; }

        [Option(Alias = "PSA.SelectedPersonality", DefaultValue = 1)]
        int SelectedPersonality { get; set; }

        [Option(Alias = "PSA.SleepIdle", DefaultValue = -1)]
        int SleepIdle { get; set; }

        [Option(Alias = "PSA.MonitorIdle", DefaultValue = -1)]
        int MonitorIdle { get; set; }

        [Option(Alias = "PSA.HyberIdle", DefaultValue = -1)]
        int HyberIdle { get; set; }

        [Option(Alias = "PSA.WakeTimers", DefaultValue = false)]
        bool WakeTimers { get; set; }

        [Option(Alias = "PSA.SleepIdleDC", DefaultValue = -1)]
        int SleepIdleDC { get; set; }

        [Option(Alias = "PSA.MonitorIdleDC", DefaultValue = -1)]
        int MonitorIdleDC { get; set; }

        [Option(Alias = "PSA.HyberIdleDC", DefaultValue = -1)]
        int HyberIdleDC { get; set; }

        [Option(Alias = "PSA.WakeTimersDC", DefaultValue = false)]
        bool WakeTimersDC { get; set; }

        [Option(Alias = "PSA.PLPerfMode", DefaultValue = true)]
        bool PLPerfMode { get; set; }

        [Option(Alias = "CPUDoc.CoreControl", DefaultValue = false)]
        bool CoreControl { get; set; }

        [Option(Alias = "CoreControl.TurboBoostMode", DefaultValue = 0)]
        int TurboBoostMode { get; set; }
    }

    public class appProfiles
    {
        public bool Sysm { get; set; } = false;
        public bool Bitm { get; set; } = false;
        public string ProcessName { get; set; } = "";
        public string WindowTile { get; set; } = "";
        public string WindowName { get; set; } = "";
        public int Config { get; set; } = 0;
        public bool Factory { get; set; } = false;

    }
    class SettingsManager
    {
        public static string SettingsFolder = @".\Settings";

        public static string CXList = SettingsFolder + "\\CapframeX.json";

        public static string AppSettingsPath = SettingsFolder + "\\AppSettings.bin";
        public static string AppConfigsPath = SettingsFolder + "\\AppConfigs.bin";
        public static string AppProfilesPath = SettingsFolder + "\\AppProfiles.bin";

        public static string fileAppSettingsINI = SettingsFolder + "\\AppSettings.ini";
        public static string fileAppConfigsINI = SettingsFolder + "\\AppConfigs.ini";
        public static string fileAppProfilesINI = SettingsFolder + "\\AppProfiles.ini";
        public static string fileZenProfilesOCINI = SettingsFolder + "\\ZenProfilesOC.ini";
        public static string file3DMarkINI = SettingsFolder + "\\3DMark.ini";

        public SettingsManager()
        {
            if (!Directory.Exists(SettingsFolder)) Directory.CreateDirectory(SettingsFolder);
        }
        public static IAppConfig GetAppConfigProfile(int id)
        {
            var members = typeof(IAppConfigs).GetMembers().Where(x => x.Name == $"profile{id}").First();
            return members as IAppConfig;
        }
        public static string GetDictByKey(Dictionary<int, string> dict, int key)
        {
            return dict.TryGetValue(key, out string output) ? output : "N/A";
        }
        public static IAppProcessProfile GetDictProcessProfile(Dictionary<string, IAppProcessProfile> dict, string key)
        {
            return dict.TryGetValue(key, out IAppProcessProfile output) ? output : null;
        }
        public static void SanitizeProfiles()
        {
            foreach(var config in App.AppConfigs)
            {
                config.PowerTweak = config.PowerTweak < 0 ? 0 : config.PowerTweak;
                config.PowerTweak = config.PowerTweak > 2 ? 2 : config.PowerTweak;
                config.GameModeBias = config.GameModeBias < -1 ? -1 : config.GameModeBias;
                config.GameModeBias = config.GameModeBias > 2 ? 2 : config.GameModeBias;
                config.ActiveModeBias = config.ActiveModeBias < -1 ? -1 : config.ActiveModeBias;
                config.ActiveModeBias = config.ActiveModeBias > 2 ? 2 : config.ActiveModeBias;
                config.Personality = config.Personality < 0 ? 0 : config.Personality;
                config.Personality = config.Personality > 3 ? 0 : config.Personality;
                config.SelectedPersonality = config.SelectedPersonality > 2 ? 2 : config.SelectedPersonality;
                config.SelectedPersonality = config.SelectedPersonality < 1 ? 1 : config.SelectedPersonality;
                config.PSALightSleepSeconds = config.PSALightSleepSeconds < 0 ? 0 : config.PSALightSleepSeconds;
                config.PSADeepSleepSeconds = config.PSADeepSleepSeconds < 0 ? 0 : config.PSADeepSleepSeconds;
                config.PSALightSleepThreshold = config.PSALightSleepThreshold < 0 ? 0 : config.PSALightSleepThreshold;
                config.PSALightSleepThreshold = config.PSALightSleepThreshold > 99 ? 14 : config.PSALightSleepThreshold;
                config.PSADeepSleepThreshold = config.PSADeepSleepThreshold < 0 ? 0 : config.PSADeepSleepThreshold;
                config.PSADeepSleepThreshold = config.PSADeepSleepThreshold > 99 ? 8 : config.PSADeepSleepThreshold;
            }
        }
        
        /*
        public static bool WriteSettings2()
        {
            try
            {
                if (File.Exists(AppSettingsPath)) File.Delete(AppSettingsPath);
                using (var file = File.Create(AppSettingsPath))
                {
                    Serializer.Serialize(file, App.AppSettings);
                }
                if (File.Exists(AppConfigsPath)) File.Delete(AppConfigsPath);
                using (var file = File.Create(AppConfigsPath))
                {
                    Serializer.Serialize(file, App.AppConfigs);
                }
                if (File.Exists(AppProfilesPath)) File.Delete(AppProfilesPath);
                using (var file = File.Create(AppProfilesPath))
                {
                    Serializer.Serialize(file, App.AppProfiles);
                }
                return true;
            }
            catch (Exception ex)
            {
                App.LogExError("WriteSettings exception:", ex);
                return false;
            }
        }

        public static bool WriteSettings3()
        {
            try
            {
                if (File.Exists(AppSettingsPath)) File.Delete(AppSettingsPath);
                using (var file = File.Create(AppSettingsPath))
                {
                    Serializer.Serialize(file, App.AppSettings);
                }
                if (File.Exists(AppConfigsPath)) File.Delete(AppConfigsPath);
                using (var file = File.Create(AppConfigsPath))
                {
                    Serializer.Serialize(file, App.AppConfigs);
                }
                if (File.Exists(AppProfilesPath)) File.Delete(AppProfilesPath);
                using (var file = File.Create(AppProfilesPath))
                {
                    Serializer.Serialize(file, App.AppProfiles);
                }
                return true;
            }
            catch (Exception ex)
            {
                App.LogExError("WriteSettings exception:", ex);
                return false;
            }
        }
        */

        public static void ResetSettings()
        {
            if (File.Exists(fileAppSettingsINI)) File.Delete(fileAppSettingsINI);
            if (File.Exists(fileAppConfigsINI)) File.Delete(fileAppConfigsINI);
            if (File.Exists(fileAppProfilesINI)) File.Delete(fileAppProfilesINI);
        }

        /*
        public static bool ReadSettings2(int configId = 0)
        {
            try
            {
                /*
                if (!File.Exists(AppSettingsPath))
                {
                    App.AppSettings = new appSettings();
                    using (var file = File.Create(AppSettingsPath))
                    {
                        Serializer.Serialize(file, App.AppSettings);
                    }
                }
                else
                {
                    using (var file = File.OpenRead(AppSettingsPath))
                    {
                        App.AppSettings = Serializer.Deserialize<appSettings>(file);
                    }
                }
                if (!File.Exists(AppConfigsPath))
                {
                    App.AppConfigs = new List<appConfigs>();
                    using (var file = File.Create(AppConfigsPath))
                    {
                        Serializer.Serialize(file, App.AppConfigs);
                    }
                    App.AppConfigs[configId].Init();
                }
                else
                {
                    List<appConfigs> _AppConfigs = new List<appConfigs>();
                    using (var file = File.OpenRead(AppConfigsPath))
                    {
                        _AppConfigs = Serializer.Deserialize<List<appConfigs>>(file);
                    }
                    var dump = ObjectDumper.Dump(_AppConfigs[configId]);
                    App.LogDebug($"Dump During Read AppConfigs={configId}\n{dump}");
                    _AppConfigs[configId].Init();
                    App.AppConfigs = new List<appConfigs>(_AppConfigs);
                }

                if (!File.Exists(AppProfilesPath))
                {
                    App.AppProfiles = new appProfiles();
                    using (var file = File.Create(AppProfilesPath))
                    {
                        Serializer.Serialize(file, App.AppProfiles);
                    }
                }
                else
                {
                    using (var file = File.OpenRead(AppProfilesPath))
                    {
                        App.AppProfiles = Serializer.Deserialize<appProfiles>(file);
                    }
                }
                App.LogInfo($"ReadSettings configId={configId}");
                return true;
            }
            catch (Exception ex)
            {
                //App.AppConfigs = new List<appConfigs>();
                App.AddConfig(configId, true);
                //App.AppSettings = new appSettings();
                App.AppProfiles = new appProfiles();
                App.LogExError($"ReadSettings configId={configId} exception:", ex);
                return false;
            }
        }
                */
    }

    public static class ExtensionMethods
    {
        public static void CopyPropertiesTo<T>(this T source, T dest)
        {
            var plist = from prop in typeof(T).GetProperties() where prop.CanRead && prop.CanWrite select prop;

            foreach (PropertyInfo prop in plist)
            {
                var value = prop.GetValue(source, null);
                prop.SetValue(dest, value, null);
            }
        }
        public static void WritePropertiesToItself<T>(this T source)
        {
            var plist = from prop in typeof(T).GetProperties() where prop.CanRead && prop.CanWrite select prop;

            foreach (PropertyInfo prop in plist)
            {
                var value = prop.GetValue(source, null);
                prop.SetValue(source, value, null);
            }
        }
        public static void WriteProcessProfileNames<T>(this T source)
        {
            if (typeof(T) != typeof(IApp3DMark)) return;
            
            var plist = from prop in typeof(T).GetProperties() where prop.CanRead && prop.CanWrite select prop;

            foreach (PropertyInfo prop in plist)
            {
                if (prop.GetType() == typeof(IAppProcessProfile))
                {
                    var processprofile = prop as IAppProcessProfile;
                    processprofile.Name = prop.Name;
                }
            }
        }
    }

    public class ShallowCopy
    {
        public static void Copy<From, To>(From from, To to)
            where To : class
            where From : class
        {
            Type toType = to.GetType();
            foreach (var propertyInfo in from.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance))
            {
                toType.GetProperty(propertyInfo.Name).SetValue(to, propertyInfo.GetValue(from, null), null);
            }
        }
    }

}