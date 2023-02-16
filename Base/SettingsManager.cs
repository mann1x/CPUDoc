using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using CPUDoc.Properties;
using Windows.Foundation.Metadata;
using System.ComponentModel;
using net.r_eg.Conari.Extension;
using Config.Net;
using LibreHardwareMonitor.Hardware;
using System.Runtime.CompilerServices;
using static CPUDoc.ProcessorInfo;
using System.Collections;
using System.Reflection;

namespace CPUDoc
{

    public interface ICommandLineArgs
    {
        [Option(DefaultValue = -1)]
        int LogTrace { get; }
        
        [Option(DefaultValue = 0)]
        int inpoutdlldisable { get; set; }
    }

    public interface IAppSettings : INotifyPropertyChanged
    {
        [Option(Alias="Startup.BootType", DefaultValue = 1)]
        int BootType { get; set; }

        [Option(Alias = "Startup.BootProfile", DefaultValue = 0)]
        int BootProfile { get; set; }

        [Option(Alias="Logging.Logtrace", DefaultValue = false)]
        bool LogTrace { get; set; }

        [Option(Alias = "Updates.AUNotifications", DefaultValue = true)]
        bool AUNotifications { get; set; }

        [Option(Alias = "Startup.DisableInpoutx64dll", DefaultValue = false)]
        bool inpoutdlldisable { get; set; }

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

        [Option(Alias = "CPUDoc.SysSetHack", DefaultValue = true)]
        bool SysSetHack { get; set; }

        [Option(Alias = "CPUDoc.PowerSaverActive", DefaultValue = true)]
        bool PowerSaverActive { get; set; }

        [Option(Alias = "PSA.LightSleep", DefaultValue = true)]
        bool PSALightSleep { get; set; }

        [Option(Alias = "PSA.DeepSleep", DefaultValue = true)]
        bool PSADeepSleep { get; set; }

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
        [ProtoMember(41)]
        int ZenControlTDClow { get; set; }

        [Option(Alias = "ZenControl.TDCLightSleep", DefaultValue = 0)]
        int ZenControlTDClight { get; set; }

        [Option(Alias = "ZenControl.TDCDeepSleep", DefaultValue = 0)]
        int ZenControlTDCdeep { get; set; }

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

    }

    [ProtoContract]
    public class appProfiles
    {
        [ProtoMember(1)]
        public bool Sysm { get; set; } = false;
        [ProtoMember(2)]
        public bool Bitm { get; set; } = false;
        [ProtoMember(3)]
        public string ProcessName { get; set; } = "";
        [ProtoMember(4)]
        public string WindowTile { get; set; } = "";
        [ProtoMember(5)]
        public string WindowName { get; set; } = "";
        [ProtoMember(6)]
        public int Config { get; set; } = 0;
        [ProtoMember(7)]
        public bool Factory { get; set; } = false;

    }
    class SettingsManager
    {
        public static string SettingsFolder = @".\Settings";
        public static string AppSettingsPath = SettingsFolder + "\\AppSettings.bin";
        public static string AppConfigsPath = SettingsFolder + "\\AppConfigs.bin";
        public static string AppProfilesPath = SettingsFolder + "\\AppProfiles.bin";

        public static string fileAppSettingsINI = SettingsFolder + "\\AppSettings.ini";
        public static string fileAppConfigsINI = SettingsFolder + "\\AppConfigs.ini";
        public static string fileAppProfilesINI = SettingsFolder + "\\AppProfiles.ini";

        public SettingsManager()
        {
            if (!Directory.Exists(SettingsFolder)) Directory.CreateDirectory(SettingsFolder);
        }
        public static IAppConfig GetProfile(int id)
        {
            var members = typeof(IAppConfigs).GetMembers().Where(x => x.Name == $"profile{id}").First();
            return members as IAppConfig;
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
                config.Personality = config.Personality > 2 ? 2 : config.Personality;
                config.PSALightSleepSeconds = config.PSALightSleepSeconds < 0 ? 0 : config.PSALightSleepSeconds;
                config.PSADeepSleepSeconds = config.PSADeepSleepSeconds < 0 ? 0 : config.PSADeepSleepSeconds;
                config.PSALightSleepThreshold = config.PSALightSleepThreshold < 0 ? 0 : config.PSALightSleepThreshold;
                config.PSALightSleepThreshold = config.PSALightSleepThreshold > 99 ? 14 : config.PSALightSleepThreshold;
                config.PSADeepSleepThreshold = config.PSADeepSleepThreshold < 0 ? 0 : config.PSADeepSleepThreshold;
                config.PSADeepSleepThreshold = config.PSADeepSleepThreshold > 99 ? 8 : config.PSADeepSleepThreshold;
            }
        }

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
        public static void ResetSettings()
        {
            if (File.Exists(fileAppSettingsINI)) File.Delete(fileAppSettingsINI);
            if (File.Exists(fileAppConfigsINI)) File.Delete(fileAppConfigsINI);
            if (File.Exists(fileAppProfilesINI)) File.Delete(fileAppProfilesINI);
        }

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
                */

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
    }

    public static class ExtensionMethods
    {
        public static void CopyPropertiesTo<T>(this T source, T dest)
        {
            var plist = from prop in typeof(T).GetProperties() where prop.CanRead && prop.CanWrite select prop;

            foreach (PropertyInfo prop in plist)
            {
                prop.SetValue(dest, prop.GetValue(source, null), null);
            }
        }
        public static void WritePropertiesToItself<T>(this T source)
        {
            var plist = from prop in typeof(T).GetProperties() where prop.CanRead && prop.CanWrite select prop;

            foreach (PropertyInfo prop in plist)
            {
                prop.SetValue(source, prop.GetValue(source, null), null);
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