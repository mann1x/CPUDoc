using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CPUDoc
{
    class Processes
    {
        public static List<CurrentProcessesItem> currentProcesses;
        public static List<ListProcessesItem> listProcesses;

        public static void Init()
        {

            listProcesses = new();
            listProcesses.Add(new ListProcessesItem("RTSS", true, true, true));
            listProcesses.Add(new ListProcessesItem("RTSSHooksLoader64", true, true, true));
            listProcesses.Add(new ListProcessesItem("CapFrameX", true, true, true));
            listProcesses.Add(new ListProcessesItem("cpuz", false , true, true));
            currentProcesses = new();

            Process[] processCollection = Process.GetProcesses();
            foreach (Process p in processCollection)
            {
                PParseIn(p.ProcessName, p.Id);
                //App.LogDebug($"Process: {p.ProcessName}");
            }
            ProfileParse();
            MaskParse();
        }
        public static void AppLaunched(string processname, int pid)
        {
            PParseIn(processname, pid);
            ProfileParse();
            MaskParse();
        }
        public static void AppClosed(string processname, int pid)
        {
            PParseOut(processname, pid);
            ProfileParse();
        }
        public static void ProfileParse()
        {
            try
            {
#nullable enable
                CurrentProcessesItem? p = currentProcesses.Where(process => process.profile > 0).OrderByDescending(process => process.profile).FirstOrDefault();
                if (p != null)
                {
                    //App profile = p.profile
                }
                else
                {

                }
                //App profile 0
            }
            catch (Exception ex)
            {
                App.LogExError($"ProfileParse exception: {ex.Message}", ex);
            }
        }
        public static void MaskParse()
        {
            try
            {
#nullable enable
                if (currentProcesses != null)
                {
                    foreach (CurrentProcessesItem p in currentProcesses)
                    {
                        if ((p.bitm || p.sysm) && App.pactive.SysSetHack)
                        {
                            ThreadBooster.ProcMask(p.processName, p.sysm, p.bitm);
                            App.LogDebug($"MaskParse action: {p.processName} sysm={p.sysm} bitm={p.bitm}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.LogExError($"MaskParse exception: {ex.Message}", ex);
            }
        }
        public static void PParseIn(string processname, int pid)
        {
            try
            {
#nullable enable
                ListProcessesItem? p = listContains(processname);
                if (p != null)
                {
                    currentProcesses.Add(new CurrentProcessesItem(pid, processname, p.sysm, p.bitm, p.profile));
                }
            }
            catch (Exception ex)
            {
                App.LogExError($"PParseIn exception: {ex.Message}", ex);
            }
        }
        public static void PParseOut(string processname, int pid)
        {
            try
            {
#nullable enable
                CurrentProcessesItem? p = currentContainsPid(pid);
                if (p != null)
                {
                    currentProcesses.Remove(p);
                }
            }
            catch (Exception ex)
            {
                App.LogExError($"PParseIn exception: {ex.Message}", ex);
            }
        }

        public static ListProcessesItem? listContains(string processname)
        {
            foreach(ListProcessesItem p in listProcesses) {
                if (processname.Contains(p.processName)) return p;
            }
            return null;
        }
        public static CurrentProcessesItem? currentContains(string processname)
        {
            foreach(CurrentProcessesItem p in currentProcesses)
            {
                if (processname.Contains(p.processName)) return p;
            }
            return null;
        }
        public static CurrentProcessesItem? currentContainsPid(int pid)
        {
            foreach (CurrentProcessesItem p in currentProcesses)
            {
                if (p.pid == pid) return p;
            }
            return null;
        }
    }

    public class CurrentProcessesItem
    {
        public int pid { get; set; }
        public string processName { get; set; }
        public bool sysm { get; set; }
        public bool bitm { get; set; }
        public int? profile { get; set; }

        public CurrentProcessesItem(int _pid, string _processName, bool _sysm, bool _bitm, int? _profile = null)
        {
            pid = _pid;
            processName = _processName;
            sysm = _sysm;
            bitm = _bitm;
            profile = _profile;
        }
    }
    public class ListProcessesItem
    {
        public string processName { get; set; }
        public bool sysm { get; set; }
        public bool bitm { get; set; }
        public bool factory { get; set; }
        public int? profile { get; set; }
        public ListProcessesItem(string _processName, bool _sysm, bool _bitm, bool _factory, int? _profile = null)
        {
            processName = _processName;
            sysm = _sysm;
            bitm = _bitm;
            factory = _factory;
            profile = _profile;
        }
    }
}
