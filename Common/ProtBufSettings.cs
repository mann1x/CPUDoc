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
        public int BootType { get; set; }
        [ProtoMember(2)]
        public bool LogInfo { get; set; }
        [ProtoMember(3)]
        public bool LogTrace { get; set; }
        [ProtoMember(4)]
        public bool AUNotifications { get; set; }
        public void Init()
        {

            BootType = 1;
            LogInfo = false;
            LogTrace = false;
            AUNotifications = true;

        }

    }
    [ProtoContract]
    public class appConfigs
    {
        [ProtoMember(1)]
        public bool ThreadBooster { get; set; }
        [ProtoMember(2)]
        public bool SysSetHack { get; set; }
        [ProtoMember(3)]
        public bool PowerSaverActive { get; set; }
        [ProtoMember(4)]
        public bool PSALightSleep { get; set; }
        [ProtoMember(5)]
        public bool PSADeepSleep { get; set; }
        [ProtoMember(6)]
        public int PSALightSleepSeconds { get; set; }
        [ProtoMember(7)]
        public int PSADeepSleepSeconds { get; set; }
        [ProtoMember(8)]
        public bool WHEASuppressor { get; set; }
        [ProtoMember(9)]
        public bool COStandbySafe { get; set; }
        [ProtoMember(10)]
        public bool NumaZero { get; set; }
        [ProtoMember(11)]
        public int NumaZeroType { get; set; }
        [ProtoMember(12)]
        public bool Enabled { get; set; }
        [ProtoMember(13)]
        public int PSALightSleepThreshold { get; set; }
        [ProtoMember(14)]
        public int PSADeepSleepThreshold { get; set; }
        [ProtoMember(15)]
        public int id { get; set; }
        [ProtoMember(16)]
        public string Description { get; set; }
        [ProtoMember(17)]
        public int PSABias { get; set; }
        [ProtoMember(18)]
        public bool PSABiasAuto { get; set; }
        [ProtoMember(19)]
        public int PSABiasHpxHysteresis { get; set; }
        [ProtoMember(20)]
        public int PSABiasHpxThreshold { get; set; }
        [ProtoMember(21)]
        public int PSABiasBalHysteresis { get; set; }
        [ProtoMember(22)]
        public int PSABiasBalThreshold { get; set; }
        [ProtoMember(23)]
        public bool ManualPoolingRate { get; set; }
        [ProtoMember(24)]
        public int PoolingRate { get; set; }
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
        public void Init() 
        {
            WHEASuppressor = false;
            ThreadBooster = true;
            SysSetHack = true;
            PSALightSleep = true;
            PSADeepSleep = true;
            PowerSaverActive = true;
            NumaZero = false;
            NumaZeroType = 0;
            PSALightSleepSeconds = 15;
            PSADeepSleepSeconds = 60;
            PSALightSleepThreshold = 14;
            PSADeepSleepThreshold = 8;
            PSABias = 1;
            PSABiasAuto = true;
            PSABiasHpxHysteresis = 30;
            PSABiasHpxThreshold = 65;
            PSABiasBalHysteresis = 10;
            PSABiasBalThreshold = 25;
            COStandbySafe = true;
            ManualPoolingRate = false;
            PoolingRate = 3;
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

        public static void ReadSettings()
        {
            try
            {
                if (!File.Exists(AppSettingsPath))
                {
                    App.AppSettings = new appSettings();
                    App.AppSettings.Init();
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
                    App.AddConfig(0, true);
                    App.AppConfigs[0].Init();
                    using (var file = File.Create(AppConfigsPath))
                    {
                        Serializer.Serialize(file, App.AppConfigs);
                    }
                }
                else
                {
                    using (var file = File.OpenRead(AppConfigsPath))
                    {
                        App.AppConfigs = Serializer.Deserialize<List<appConfigs>>(file);
                    }
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
                
            }
            catch (Exception ex)
            {
                App.AppSettings = new appSettings();
                App.AppProfiles = new appProfiles();
                App.AddConfig(0, true);
                App.LogExError("ReadSettings exception:", ex);
            }
        }
    }
}