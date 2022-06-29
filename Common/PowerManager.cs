using System;
using System.Linq;
using System.Diagnostics;
using System.Management;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.ServiceProcess;

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

    public interface IPowerManager
    {
        /// <returns>
        /// All supported power plans.
        /// </returns>
        List<PowerPlan> GetPlans();

        PowerPlan GetCurrentPlan();
        Guid GetActiveGuid();
        string GetPowerPlanName(Guid guid);

        /// <returns>Battery charge value in percent, 
        /// i.e. values in a 0..100 range</returns>
        void SetActive(PowerPlan plan);

    }

    public class PowerManagerProvider
    {
        public static IPowerManager CreatePowerManager()
        {
            return new PowerManager();
        }
    }

    class PowerManager : IPowerManager
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

        public PowerManager()
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
            return new List<PowerPlan>(new PowerPlan[] {
                MaximumPerformance,
                Balanced,
                PowerSourceOptimized
            });
        }

        public Guid GetActiveGuid()
        {
            Guid ActiveScheme = Guid.Empty;
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
            if (PowerGetActiveScheme((IntPtr)null, out ptr) == 0)
            {
                ActiveScheme = (Guid)Marshal.PtrToStructure(ptr, typeof(Guid));
                if (ptr != null)
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
            return ActiveScheme;
        }

        public PowerPlan GetCurrentPlan()
        {
            Guid guid = GetActiveGuid();
            return GetPlans().Find(p => (p.guid == guid));
        }
        private void PowerModeChangedHandler(object sender, PowerModeChangedEventArgs e)
        {
            App.LogInfo("Detected PowerMode Change");
            
            if (e.Mode == PowerModes.Suspend)
            {
                App.LogInfo("Detected Suspend");
            }
            else if (e.Mode == PowerModes.Resume)
            {
                App.LogInfo("Detected Resume");
            }
        }
        private void SystemEvents_UserPreferenceChanging(object sender, UserPreferenceChangingEventArgs e)
        {
            // 10 = Power 
            //Console.WriteLine("The user preference is changing. Category={0}", e.Category);
            App.LogInfo("Detected Change UserPref Category: " + e.Category);

        }

        public string GetPowerPlanName(Guid guid)
        {
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

        #endregion
    }
}
