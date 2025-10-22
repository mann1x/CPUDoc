using CPUDoc.Properties;
using Microsoft.Win32;
using PowerManagerAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.RightsManagement;
using System.ServiceProcess;
using System.Text;
using Vanara.PInvoke;
using static Vanara.PInvoke.PowrProf;

namespace CPUDoc
{

    public class PowerPlan
    {
        public readonly string name;
        public Guid guid;

        public static Guid DefaultOverlay = new Guid("00000000-0000-0000-0000-000000000000");
        public static Guid BetterBatteryLifeOverlay = new Guid("961cc777-2547-4f9d-8174-7d86181b8a7a");
        public static Guid BetterPerformanceOverlay = new Guid("3af9B8d9-7c97-431d-ad78-34a8bfea439f");
        public static Guid MaxPerformanceOverlay = new Guid("ded574b5-45a0-4f42-8737-46345c09c238");

        public PowerPlan(string name, Guid guid)
        {
            this.name = name;
            this.guid = guid;
        }
    }

    public interface IPowerPlanManager
    {
        /// <returns>
        /// All supported power plans.
        /// </returns>
        List<PowerPlan> GetPlans();

        PowerPlan GetCurrentPlan();
        Guid GetActiveGuid();
        string GetPowerPlanName(Guid guid);
        string GetActivePlanFriendlyName();
        bool PlanExists(Guid guid);
        bool DeletePlan(Guid guid);
        bool SetActiveGuid(Guid guid);
        void SetDynamic(SettingSubgroup subgroup, Guid setting, PowerMode powerMode, uint value, bool alert = true);
        void SetActive(PowerPlan plan);
        bool SetActiveOverlay(Guid guid);
        Guid GetActiveOverlay();


    }

    public class PowerManagerProvider
    {
        public static IPowerPlanManager CreatePowerManager()
        {
            return new PowerPlanManager();
        }
    }

    class PowerPlanManager : IPowerPlanManager
    {
        uint EFFECTIVE_POWER_MODE_V1 = 0x00000001;
        uint EFFECTIVE_POWER_MODE_V2 = 0x00000002;

        /// <summary>
        /// Indicates that almost no power savings measures will be used.
        /// </summary>
        private readonly PowerPlan MaximumPerformance;

        /// <summary>
        /// Indicates that fairly aggressive power savings measures will be used.
        /// </summary>
        private readonly PowerPlan Balanced;

        /// <summary>
        /// Indicates that very aggressive power savings measures will be used to help
        /// stretch battery life.                                                     
        /// </summary>
        private readonly PowerPlan PowerSourceOptimized;

        public PowerPlanManager()
        {
            // See GUID values in WinNT.h.
            MaximumPerformance = NewPlan("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
            Balanced = NewPlan("381b4222-f694-41f0-9685-ff5bb260df2e");
            PowerSourceOptimized = NewPlan("a1841308-3541-4fab-bc81-f71556f20b4a");

            // Add handler for power mode state changing.
            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(PowerModeChangedHandler);
            SystemEvents.UserPreferenceChanging += new UserPreferenceChangingEventHandler(SystemEvents_UserPreferenceChanging);
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(SystemEvents_UserPreferenceChanged);
        }

        private PowerPlan NewPlan(string guidString)
        {
            Guid guid = new Guid(guidString);
            return new PowerPlan(GetPowerPlanName(guid), guid);
        }

        public void SetActive(PowerPlan plan)
        {
            PowerSetActiveScheme(IntPtr.Zero, ref plan.guid);
        }

        public bool SetActiveOverlay(Guid overlay)
        {
            HRESULT hr = PowerSetActiveOverlayScheme(overlay);

            if (hr != HRESULT.S_OK) return false;

            ThreadBooster.CurrentOverlay = GetOverlayLabel(overlay);

            return true;
        }
        public Guid GetActiveOverlay()
        {
            Guid overlay;
            HRESULT hr = PowerGetActualOverlayScheme(out overlay);

            if (hr != HRESULT.S_OK) return Guid.Empty;

            return overlay;
        }
        public string GetActiveOverlayString()
        {
            return GetOverlayLabel(GetActiveOverlay());
        }
        public static string GetOverlayLabel(Guid overlay)
        {
            String _label = overlay == PowerPlan.DefaultOverlay ? "Better" : overlay == PowerPlan.BetterBatteryLifeOverlay ? "Saving" : overlay == PowerPlan.MaxPerformanceOverlay ? "Max" : "Better";
            return _label;
        }

        /// <returns>
        /// All supported power plans.
        /// </returns>
        public List<PowerPlan> GetPlans()
        {
            var list = PowerManager.ListPlans();
            var plans = new List<PowerPlan>();
            if (list.Count > 0)
            {
                foreach(var _plan in list)
                {
                    var newplan = new PowerPlan(PowerManager.GetPlanName(_plan), _plan);
                    plans.Add(newplan);
                }
            }
            return plans;
            /*
            return new List<PowerPlan>(new PowerPlan[] {
                MaximumPerformance,
                Balanced,
                PowerSourceOptimized
            });
            */
        }
        public bool PlanExists(Guid plan)
        {
            try
            {
                if (PowerManager.PlanExists(plan)) return true;
                return false;
            }
            catch
            {
                return false;
            }
        }

        public Guid GetActiveGuid()
        {
            Guid ActiveScheme = Guid.Empty;
            ActiveScheme = PowerManager.GetActivePlan();
            /*
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
            if (PowerGetActiveScheme((IntPtr)null, out ptr) == 0)
            {
                ActiveScheme = (Guid)Marshal.PtrToStructure(ptr, typeof(Guid));
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
            */
            return ActiveScheme;
        }
        public bool SetActiveGuid(Guid guid)
        {
            PowerManager.SetActivePlan(guid);
            bool isactive = PowerManager.GetActivePlan() == guid ? true : false;
            return isactive;
        }

        public void SetDynamic(SettingSubgroup subgroup, Guid settingId, PowerMode powerMode, uint value, bool alert)
        {
            Guid subgroupId = SettingIdLookup.SettingSubgroupGuids[subgroup];            

            if (powerMode.HasFlag(PowerMode.AC))
            {
                var res = PowerWriteACValueIndex(IntPtr.Zero, ref App.PPGuid, ref subgroupId, ref settingId, value);
                if (res != (uint)ErrorCode.SUCCESS && alert)
                    App.LogDebug($"Error setting PSA AC Guid {settingId} to {value}");
            }
            if (powerMode.HasFlag(PowerMode.DC))
            {
                var res = PowerWriteDCValueIndex(IntPtr.Zero, ref App.PPGuid, ref subgroupId, ref settingId, value);
                if (res != (uint)ErrorCode.SUCCESS && alert)
                    App.LogDebug($"Error setting PSA DC Guid {settingId} to {value}");
            }
        }
        public string GetActivePlanFriendlyName()
        {
            try 
            {
                var activePlanGuid = PowerManager.GetActivePlan();
                //App.LogDebug($"Active Plan Guid {activePlanGuid}");
                return PowerManager.GetPlanName(activePlanGuid);
            }
            catch (Exception ex)
            {
                App.LogDebug($"GetActivePlanFriendlyName Exception: {ex.Message}");
                return "Error retrieving active Power Plan name";
            }
        }

        public bool DeletePlan(Guid plan)
        {
            PowerManager.DeletePlanIfExists(plan);
            return !PowerManager.PlanExists(plan);
        }
        public PowerPlan GetCurrentPlan()
        {
            Guid guid = GetActiveGuid();
            return GetPlans().Find(p => (p.guid == guid));
        }
        private void PowerModeChangedHandler(object sender, PowerModeChangedEventArgs e)
        {
            App.LogDebug("Detected PowerMode Change");
            
            if (e.Mode == PowerModes.Suspend)
            {
                App.LogInfo("Detected going to Standby");
            }
            else if (e.Mode == PowerModes.Resume)
            {
                App.LogInfo("Detected Resume from Standby");
                App.reapplyProfile = true;
            }
        }
        private void SystemEvents_UserPreferenceChanging(object sender, UserPreferenceChangingEventArgs e)
        {
            App.LogInfo($"Detected Changing UserPref Category: {e.Category}");
        }
        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            App.LogInfo($"Detected Changed UserPref Category: {e.Category}");
        }

        public string GetPowerPlanName(Guid guid)
        {
            try
            {
                string name = string.Empty;
                if (PowerManager.GetPlanName(guid).Length > 0) name = PowerManager.GetPlanName(guid);
                return name;
            }
            catch (Exception ex)
            {
                App.LogExError($"GetPowerPlanName exception:", ex);
                return "";
            }
            /*
            string name = string.Empty;
            IntPtr lpszName = (IntPtr)null;
            uint dwSize = 0;

            PowerReadFriendlyName((IntPtr)null, ref guid, (IntPtr)null, (IntPtr)null, lpszName, ref dwSize);
            if (dwSize > 0)
            {
                lpszName = Marshal.AllocHGlobal((int)dwSize);
                if (0 == PowerReadFriendlyName((IntPtr)null, ref guid, (IntPtr)null, (IntPtr)null, lpszName, ref dwSize))
                {
                    name = Marshal.PtrToStringUni(lpszName);
                }
                if (lpszName != IntPtr.Zero)
                    Marshal.FreeHGlobal(lpszName);
            }

            return name;
            */
        }
    
        #region DLL imports

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern int GetSystemDefaultLCID();

        [DllImportAttribute("powrprof.dll", EntryPoint = "PowerSetActiveScheme")]
        public static extern uint PowerSetActiveScheme(IntPtr UserPowerKey, ref Guid ActivePolicyGuid);

        [DllImportAttribute("powrprof.dll", EntryPoint = "PowerGetActiveScheme")]
        public static extern uint PowerGetActiveScheme(IntPtr UserPowerKey, out IntPtr ActivePolicyGuid);

        [DllImportAttribute("powrprof.dll", EntryPoint = "PowerReadFriendlyName")]
        public static extern uint PowerReadFriendlyName(IntPtr RootPowerKey, ref Guid SchemeGuid, IntPtr SubGroupOfPowerSettingsGuid, IntPtr PowerSettingGuid, IntPtr Buffer, ref uint BufferSize);

        [DllImport("powrprof.dll")]
        private static extern uint PowerWriteACValueIndex(
        [In, Optional] IntPtr RootPowerKey,
        [In] ref Guid SchemeGuid,
        [In, Optional] ref Guid SubGroupOfPowerSettingsGuid,
        [In, Optional] ref Guid PowerSettingGuid,
        [In] uint AcValueIndex
        );

        [DllImport("powrprof.dll")]
        private static extern uint PowerWriteDCValueIndex(
            [In, Optional] IntPtr RootPowerKey,
            [In] ref Guid SchemeGuid,
            [In, Optional] ref Guid SubGroupOfPowerSettingsGuid,
            [In, Optional] ref Guid PowerSettingGuid,
            [In] uint DcValueIndex
        );

        [DllImportAttribute("powrprof.dll", EntryPoint = "PowerSetActiveOverlayScheme")]
        public static extern uint PowerSetActiveOverlayScheme(Guid OverlaySchemeGuid);
        
        [DllImportAttribute("powrprof.dll", EntryPoint = "PowerGetActualOverlayScheme")]
        public static extern uint PowerGetActualOverlayScheme(out Guid ActualOverlayGuid);

        /*
        
        [DllImportAttribute("powrprof.dll", EntryPoint = "PowerGetActualOverlaySchemes")]
        public static extern uint PowerGetActualOverlaySchemes(out Guid ActualOverlayGuid);
        
        [DllImport(@"User32", SetLastError = true,
        EntryPoint = "RegisterPowerSettingNotification",
        CallingConvention = CallingConvention.StdCall)]

        private static extern IntPtr RegisterPowerSettingNotification(
        IntPtr hRecipient,
        ref Guid PowerSettingGuid,
        Int32 Flags);

        static readonly Guid GUID_LIDCLOSE_ACTION =
            new Guid(0xBA3E0F4D, 0xB817, 0x4094, 0xA2, 0xD1,
                     0xD5, 0x63, 0x79, 0xE6, 0xA0, 0xF3);

        private const int WM_POWERBROADCAST = 0x0218;
        private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
        const int PBT_POWERSETTINGCHANGE = 0x8013; // DPPE

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        internal struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public uint DataLength;
            public byte Data;
        }
        */

        #endregion

    }

    public enum EffectivePowerModeV1
    {
        BatterySaver,
        BetterBattery,
        Balanced,
        HighPerformance,
        MaxPerformance
    }
    public enum EffectivePowerModeV2
    {
        BatterySaver,
        BetterBattery,
        SaverStandard,
        Balanced,
        HighPerformance,
        MaxPerformance,
        GameMode,
        MixedReality
    }
}
