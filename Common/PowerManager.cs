using System;
using System.Linq;
using System.Diagnostics;
using System.Management;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.ServiceProcess;
using PowerManagerAPI;

namespace CPUDoc
{

    public class PowerPlan
    {
        public readonly string name;
        public Guid guid;

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
        void SetDynamic(SettingSubgroup subgroup, Guid setting, PowerMode powerMode, uint value);

        /// <returns>Battery charge value in percent, 
        /// i.e. values in a 0..100 range</returns>
        void SetActive(PowerPlan plan);

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

        public void SetDynamic(SettingSubgroup subgroup, Guid settingId, PowerMode powerMode, uint value)
        {
            Guid subgroupId = SettingIdLookup.SettingSubgroupGuids[subgroup];            

            if (powerMode.HasFlag(PowerMode.AC))
            {
                var res = PowerWriteACValueIndex(IntPtr.Zero, ref App.PPGuid, ref subgroupId, ref settingId, value);
                if (res != (uint)ErrorCode.SUCCESS)
                    App.LogDebug($"Error setting PSA AC Guid {settingId} to {value}");
            }
            if (powerMode.HasFlag(PowerMode.DC))
            {
                var res = PowerWriteDCValueIndex(IntPtr.Zero, ref App.PPGuid, ref subgroupId, ref settingId, value);
                if (res != (uint)ErrorCode.SUCCESS)
                    App.LogDebug($"Error setting PSA DC Guid {settingId} to {value}");
            }
        }
        public string GetActivePlanFriendlyName()
        {
            var activePlanGuid = PowerManager.GetActivePlan();
            return PowerManager.GetPlanName(activePlanGuid);
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
            // 10 = Power 
            App.LogInfo($"Detected Change UserPref Category: {e.Category} Object: {e}");
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
        #endregion
    }
}
