using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;

namespace CPUDoc
{

    [ProtoContract]
    public class appSettings
    {
        [ProtoMember(1)]
        public int? BootType { get; set; }

        [ProtoMember(2)]
        public bool? LogInfo { get; set; }

        [ProtoMember(3)]
        public bool? LogTrace { get; set; }

        [ProtoMember(4)]
        public bool? AUNotifications { get; set; }

        public void Init(bool realinit)
        {

            if (realinit || BootType == null) BootType = 1;
            if (realinit || LogInfo == null) LogInfo = false;
            if (realinit || LogTrace == null) LogTrace = false;
            if (realinit || AUNotifications == null) AUNotifications = true;

        }

    }
    [ProtoContract]
    public class appConfigs
    {
        [ProtoMember(1)]
        public bool? ThreadBooster { get; set; }

        [ProtoMember(2)]
        public bool? SysSetHack { get; set; }

        [ProtoMember(3)]
        public bool? PowerSaverActive { get; set; }

        [ProtoMember(4)]
        public bool? PSALightSleep { get; set; }

        [ProtoMember(5)]
        public bool? PSADeepSleep { get; set; }

        [ProtoMember(6)]
        public int? PSALightSleepSeconds { get; set; }

        [ProtoMember(7)]
        public int? PSADeepSleepSeconds { get; set; }

        [ProtoMember(8)]
        public bool? WHEASuppressor { get; set; }

        [ProtoMember(9)]
        public bool? COStandbySafe { get; set; }

        [ProtoMember(10)]
        public bool? NumaZero { get; set; }

        [ProtoMember(11)]
        public int? NumaZeroType { get; set; }

        [ProtoMember(12)]
        public bool? Enabled { get; set; }

        [ProtoMember(13)]
        public int? PSALightSleepThreshold { get; set; }

        [ProtoMember(14)]
        public int? PSADeepSleepThreshold { get; set; }

        [ProtoMember(15)]
        public int? id { get; set; }

        [ProtoMember(16)]
        public string? Description { get; set; }

        [ProtoMember(17)]
        public int? PSABias { get; set; }

        [ProtoMember(18)]
        public bool? PSABiasAuto { get; set; }

        [ProtoMember(19)]
        public int? PSABiasHpxHysteresis { get; set; }

        [ProtoMember(20)]
        public int? PSABiasHpxThreshold { get; set; }

        [ProtoMember(21)]
        public int? PSABiasBalHysteresis { get; set; }

        [ProtoMember(22)]
        public int? PSABiasBalThreshold { get; set; }

        [ProtoMember(23)]
        public bool? ManualPoolingRate { get; set; }

        [ProtoMember(24)]
        public int? PoolingRate { get; set; }

        [ProtoMember(25)]
        public bool? ZenControl { get; set; }

        [ProtoMember(26)]
        public bool? ZenControlPPTAuto { get; set; }

        [ProtoMember(27)]
        public int? ZenControlPPThpx { get; set; }

        [ProtoMember(28)]
        public int? ZenControlPPTbal { get; set; }

        [ProtoMember(29)]
        public int? ZenControlPPTlow { get; set; }

        [ProtoMember(30)]
        public int? ZenControlPPTlight { get; set; }

        [ProtoMember(31)]
        public int? ZenControlPPTdeep { get; set; }

        [ProtoMember(32)]
        public bool? ZenControlEDCAuto { get; set; }

        [ProtoMember(33)]
        public int? ZenControlEDChpx { get; set; }

        [ProtoMember(34)]
        public int? ZenControlEDCbal { get; set; }

        [ProtoMember(35)]
        public int? ZenControlEDClow { get; set; }

        [ProtoMember(36)]
        public int? ZenControlEDClight { get; set; }

        [ProtoMember(37)]
        public int? ZenControlEDCdeep { get; set; }

        [ProtoMember(38)]
        public bool? ZenControlTDCAuto { get; set; }

        [ProtoMember(39)]
        public int? ZenControlTDChpx { get; set; }

        [ProtoMember(40)]
        public int? ZenControlTDCbal { get; set; }

        [ProtoMember(41)]
        public int? ZenControlTDClow { get; set; }

        [ProtoMember(42)]
        public int? ZenControlTDClight { get; set; }

        [ProtoMember(43)]
        public int? ZenControlTDCdeep { get; set; }

        [ProtoMember(44)]
        public int? PowerTweak { get; set; }

        [ProtoMember(45)]
        public bool? GameMode { get; set; }

        [ProtoMember(46)]
        public bool? FocusAssist { get; set; }

        [ProtoMember(47)]
        public bool? UserNotification { get; set; }

        [ProtoMember(48)]
        public bool? SecondaryMonitor { get; set; }

        [ProtoMember(49)]
        public int? GameModeBias { get; set; }

        [ProtoMember(50)]
        public int? ActiveModeBias { get; set; }

        [ProtoMember(51)]
        public int? Personality { get; set; }

        [ProtoMember(52)]
        public int? SleepIdle { get; set; }
        [ProtoMember(53)]
        public int? MonitorIdle { get; set; }
        [ProtoMember(54)]
        public int? HyberIdle { get; set; }
        /*
        public static T DeepClone<T>(this T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }
        */
        public void Init(bool realinit) 
        {
            if (realinit || WHEASuppressor == null) WHEASuppressor = false;
            if (realinit || ThreadBooster == null) ThreadBooster = true;
            if (realinit || SysSetHack == null) SysSetHack = true;
            if (realinit || PSALightSleep == null) PSALightSleep = true;
            if (realinit || PSADeepSleep == null) PSADeepSleep = true;
            if (realinit || PowerSaverActive == null) PowerSaverActive = true;
            if (realinit || NumaZero == null) NumaZero = false;
            if (realinit || NumaZeroType == null) NumaZeroType = 0;
            if (realinit || PSALightSleepSeconds == null) PSALightSleepSeconds = 15;
            if (realinit || PSADeepSleepSeconds == null) PSADeepSleepSeconds = 60;
            if (realinit || PSALightSleepThreshold == null) PSALightSleepThreshold = 14;
            if (realinit || PSADeepSleepThreshold == null) PSADeepSleepThreshold = 8;
            if (realinit || PSABias == null) PSABias = 1;
            if (realinit || PSABiasAuto == null) PSABiasAuto = true;
            if (realinit || PSABiasHpxHysteresis == null) PSABiasHpxHysteresis = 30;
            if (realinit || PSABiasHpxThreshold == null) PSABiasHpxThreshold = 65;
            if (realinit || PSABiasBalHysteresis == null) PSABiasBalHysteresis = 10;
            if (realinit || PSABiasBalThreshold == null) PSABiasBalThreshold = 25;
            if (realinit || COStandbySafe == null) COStandbySafe = true;
            if (realinit || ManualPoolingRate == null) ManualPoolingRate = false;
            if (realinit || PoolingRate == null) PoolingRate = 3;
            if (realinit || ZenControl == null) ZenControl = false;
            if (realinit || ZenControlPPTAuto == null) ZenControlPPTAuto = true;
            if (realinit || ZenControlTDCAuto == null) ZenControlTDCAuto = true;
            if (realinit || ZenControlEDCAuto == null) ZenControlEDCAuto = true;
            if (realinit || PowerTweak == null) PowerTweak = 1;
            PowerTweak = PowerTweak < 0 ? 0 : PowerTweak;
            PowerTweak = PowerTweak > 2 ? 2 : PowerTweak;
            if (realinit || GameMode == null) GameMode = true;
            if (realinit || FocusAssist == null) FocusAssist = true;
            if (realinit || UserNotification == null) UserNotification = true;
            if (realinit || SecondaryMonitor == null) SecondaryMonitor = false;
            if (realinit || GameModeBias == null) GameModeBias = -1;
            GameModeBias = GameModeBias < -1 ? -1 : GameModeBias;
            GameModeBias = GameModeBias > 2 ? 2 : GameModeBias;
            if (realinit || ActiveModeBias == null) ActiveModeBias = -1;
            ActiveModeBias = ActiveModeBias < -1 ? -1 : ActiveModeBias;
            ActiveModeBias = ActiveModeBias > 2 ? 2 : ActiveModeBias;
            if (realinit || Personality == null) Personality = 0;
            Personality = Personality < 0 ? 0 : Personality;
            Personality = Personality > 2 ? 2 : Personality;
        }

    }

    [ProtoContract]
    public class appProfiles
    {
        [ProtoMember(1)]
        public bool Sysm { get; set; }
        [ProtoMember(2)]
        public bool Bitm { get; set; }
        [ProtoMember(3)]
        public string ProcessName { get; set; }
        [ProtoMember(4)]
        public string WindowTile { get; set; }
        [ProtoMember(5)]
        public string WindowName { get; set; }
        [ProtoMember(6)]
        public int Config { get; set; }
        [ProtoMember(7)]
        public bool Factory { get; set; }

    }
    class ProtBufSettings
    {
        public static string SettingsFolder = @".\Settings";
        public static string AppSettingsPath = @".\Settings\AppSettings.bin";
        public static string AppConfigsPath = @".\Settings\AppConfigs.bin";
        public static string AppProfilesPath = @".\Settings\AppProfiles.bin";

        public ProtBufSettings()
        {
            if (!Directory.Exists(SettingsFolder)) Directory.CreateDirectory(SettingsFolder);
        }
        public static bool WriteSettings()
        {
            try
            {
                using (var file = File.Create(AppSettingsPath))
                {
                    Serializer.Serialize(file, App.AppSettings);
                }
                using (var file = File.Create(AppConfigsPath))
                {
                    Serializer.Serialize(file, App.AppConfigs);
                }
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
            if (File.Exists(AppSettingsPath)) File.Delete(AppSettingsPath);
            if (File.Exists(AppConfigsPath)) File.Delete(AppConfigsPath);
            if (File.Exists(AppProfilesPath)) File.Delete(AppProfilesPath);
        }

        public static bool ReadSettings()
        {
            try
            {
                if (!File.Exists(AppSettingsPath))
                {
                    App.AppSettings = new appSettings();
                    using (var file = File.Create(AppSettingsPath))
                    {
                        Serializer.Serialize(file, App.AppSettings);
                    }
                    App.AppSettings.Init(true);
                }
                else
                {
                    using (var file = File.OpenRead(AppSettingsPath))
                    {
                        App.AppSettings = Serializer.Deserialize<appSettings>(file);
                    }
                    App.AppSettings.Init(false);
                }
                if (!File.Exists(AppConfigsPath))
                {
                    App.AddConfig(0, true);
                    using (var file = File.Create(AppConfigsPath))
                    {
                        Serializer.Serialize(file, App.AppConfigs);
                    }
                    App.AppConfigs[0].Init(true);
                }
                else
                {
                    using (var file = File.OpenRead(AppConfigsPath))
                    {
                        App.AppConfigs = Serializer.Deserialize<List<appConfigs>>(file);
                    }
                    App.AppConfigs[0].Init(false);
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
                return true;
            }
            catch (Exception ex)
            {
                App.AppSettings = new appSettings();
                App.AppProfiles = new appProfiles();
                App.AddConfig(0, true);
                App.LogExError("ReadSettings exception:", ex);
                return false;
            }
        }
    }
}