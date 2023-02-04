using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace CPUDoc
{
    class Processes
    {
        public static List<CurrentProcessesItem> currentProcesses;
        public static List<ListProcessesItem> listProcesses;
        public static bool pInit = false;

        public static void Init()
        {

            listProcesses = new();
            listProcesses.Add(new ListProcessesItem("RTSS", true, false, true, false, true, true));
            listProcesses.Add(new ListProcessesItem("RTSSHooksLoader64", true, false, true, false, true, true));
            listProcesses.Add(new ListProcessesItem("CapFrameX", true, false, true, false, true, true));
            listProcesses.Add(new ListProcessesItem("nvcontainer", true, false, true, false, true, true));
            listProcesses.Add(new ListProcessesItem("NVIDIA Share", true, false, true, false, true, true));
            listProcesses.Add(new ListProcessesItem("obs64", true, false, false, false, true, true));
            listProcesses.Add(new ListProcessesItem("obs-browser-page", true, true, true, false, true, true));
            listProcesses.Add(new ListProcessesItem("obs-ffmpeg-mux", true, true, true, false, true, true));
            listProcesses.Add(new ListProcessesItem("cpuz", true, true, false, false, true, true));
            listProcesses.Add(new ListProcessesItem("HYDRA", true, false, true, false, true, true));
            listProcesses.Add(new ListProcessesItem("3DMarkICFWorkload", false, false, false, false, true, true));
            listProcesses.Add(new ListProcessesItem("3DMarkTimeSpy", false, false, false, false, true, true));
            listProcesses.Add(new ListProcessesItem("3DMarkNightRaid", false, false, false, false, true, true));
            listProcesses.Add(new ListProcessesItem("3DMarkCPUProfile", true, false, false, false, true, true));
            listProcesses.Add(new ListProcessesItem("EasyFMSI", true, false, false, false, true, true));
            listProcesses.Add(new ListProcessesItem("SystemInfoHelper", true, false, false, false, true, true));
            listProcesses.Add(new ListProcessesItem("WatchDogsLegion", true, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("csrss", true, false, false, false, false, true));
            listProcesses.Add(new ListProcessesItem("dwm", true, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("NVDisplay.Container", true, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("SOTTR", true, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("HorizonZeroDawn", true, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("csgo", true, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("bf1", true, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("bf3", true, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("bf4", true, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("bfv", true, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("bfh", true, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("bf2042", true, true, true, true, true, true));
            listProcesses.Add(new ListProcessesItem("BFBC2Game", true, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("valheim", true, false, true, false, false, true));
            //listProcesses.Add(new ListProcessesItem("hunt", true, false, true, false, false, true));

            currentProcesses = new();

            pInit = true;

            Process[] processCollection = Process.GetProcesses();
            foreach (Process p in processCollection)
            {
                PParseIn(p.ProcessName, p.Id);
                //App.LogDebug($"Process: {p.ProcessName}");
            }
            ProfileParse();
            App.LogDebug("M1");
            MaskParse();            

        }
        public static void AppLaunched(string processname, int pid)
        {
            PParseIn(processname, pid);
            ProfileParse();
            MaskParse(processname);
        }
        public static void AppClosed(string processname, int pid)
        {
            PParseOut(processname, pid);
            ProfileParse();
        }
        public static void ProfileParse()
        {
            if (!pInit) return;

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
        public static void MaskParse(string processname = "")
        {
            if (!pInit) return;

            try
            {
#nullable enable
                void DoParse(CurrentProcessesItem p)
                {
                    if ((p.bitm || p.sysm) && (App.pactive.SysSetHack ?? false))
                    {
                        ThreadBooster.ProcMask(p.processName, p.sysm, p.sysm_full, p.bitm, p.bitm_full, p.bitm_pfull);
                        App.LogDebug($"MaskParse action: [{p.pid}] {p.processName} sysm={p.sysm} sysm_full={p.sysm_full} bitm={p.bitm} bitm_full={p.bitm_full} bitm_pfull={p.bitm_pfull}");
                    }
                }

                if (currentProcesses != null)
                {
                    if (processname.Length > 0)
                    {
                        CurrentProcessesItem? p = currentProcesses.Where(x => x.processName.ToLower() == processname.ToLower()).FirstOrDefault();
                        if (p != null) DoParse(p);
                    }
                    else
                    {
                        foreach (CurrentProcessesItem p in currentProcesses)
                        {
                            if (p != null) DoParse(p);
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
            if (!pInit) return;

            try
            {
#nullable enable
                ListProcessesItem? p = listContains(processname);
                if (p != null)
                {
                    currentProcesses.Add(new CurrentProcessesItem(pid, processname, p.sysm, p.sysm_full, p.bitm, p.bitm_full, p.bitm_pfull, p.profile));

                    if (processname.ToLower() == "EasyFMSI".ToLower() 
                        || processname.ToLower() == "SystemInfoHelper".ToLower() 
                        || processname.ToLower() == "WatchDogsLegion".ToLower()
                        || processname.ToLower() == "csgo".ToLower()
                        || processname.ToLower() == "bf1".ToLower()
                        || processname.ToLower() == "bf3".ToLower()
                        || processname.ToLower() == "bf4".ToLower()
                        || processname.ToLower() == "bfv".ToLower()
                        || processname.ToLower() == "bfh".ToLower()
                        || processname.ToLower() == "bf2042".ToLower()
                        || processname.ToLower() == "BFBC2Game".ToLower()
                        || processname.ToLower().StartsWith("obs")
                        || processname.ToLower() == "valheim".ToLower()
                        )
                        App.setPowerThrottlingExecSpeed(pid, false);
                    if (processname.StartsWith("3DMark"))
                    {
                        App.setPowerThrottlingExecSpeed(pid, false);
                        if (processname.ToLower().Contains("3DMarkCPUProfile".ToLower()) 
                            || processname.ToLower().Contains("3DMarkICFWorkload".ToLower()) 
                            || processname.ToLower().Contains("3DMarkTimeSpy".ToLower()) 
                            || processname.ToLower().Contains("3DMarkNightRaid".ToLower()))
                        {
                            Process[] proc = Process.GetProcessesByName(processname);
                            Process procid = Process.GetProcessById(proc[0].Id);
                            string args = GetCommandLineArgs(procid);
                            var regex = new Regex(@"^.* --in .(.*). --out.*");
                            var match = regex.Match(args);
                            if (match.Success)
                            {
                                //App.LogDebug($"3DMark JSON={match.Groups[1].Value}");
                                if (processname.ToLower().Contains("3DMarkCPUProfile".ToLower()))
                                {
                                    uint _bitMask = ThreadBooster.defBitMask;
                                    int _corereq = 1;
                                    if (File.ReadLines(match.Groups[1].Value).Any(line => line.Contains("_maxthreads.json")))
                                    {
                                        _corereq = ThreadBooster.basecores + ThreadBooster.addcores;
                                    }
                                    else if (File.ReadLines(match.Groups[1].Value).Any(line => line.Contains("_16threads.json")))
                                    {
                                        _corereq = 16;
                                    }
                                    else if (File.ReadLines(match.Groups[1].Value).Any(line => line.Contains("_8threads.json")))
                                    {
                                        _corereq = 8;
                                    }
                                    else if (File.ReadLines(match.Groups[1].Value).Any(line => line.Contains("_4threads.json")))
                                    {
                                        _corereq = 4;
                                    }
                                    else if (File.ReadLines(match.Groups[1].Value).Any(line => line.Contains("_2threads.json")))
                                    {
                                        _corereq = 2;
                                    }
                                    _bitMask = _corereq <= ThreadBooster.basecores ? ThreadBooster.defBitMask : _corereq >= (ThreadBooster.basecores + ThreadBooster.addcores) ? ThreadBooster.defFullBitMask : ThreadBooster.CreateBitMask(_corereq - ThreadBooster.basecores);
                                    App.LogDebug($"_corereq={_corereq}");
                                    ThreadBooster.ForceCustomBitMask(true, _bitMask);
                                }
                                if ((processname.ToLower().Contains("3DMarkICFWorkload".ToLower()) && ThreadBooster.basecores < 16)
                                    || (processname.ToLower().Contains("3DMarkTimeSpy".ToLower()) && ThreadBooster.basecores < 24) 
                                    || (processname.ToLower().Contains("3DMarkNightRaid".ToLower()) && ThreadBooster.basecores < 8))
                                {
                                    //App.LogDebug($"3DMark CPU test");
                                    if (File.ReadLines(match.Groups[1].Value).Any(line => line.Contains("configs/cpu")))
                                    {
                                        //App.LogDebug($"3DMark FullMask");
                                        ThreadBooster.ForceCustomBitMask(true);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.LogExError($"PParseIn exception: {ex.Message}", ex);
            }
        }
        public static void PParseOut(string processname, int pid)
        {
            if (!pInit) return;

            try
            {
#nullable enable
                CurrentProcessesItem? p = currentContainsPid(pid);
                if (p != null)
                {
                    currentProcesses.Remove(p);
                    if (ThreadBooster.forceCustomBitMask)
                    {
                        if (processname.Contains("3DMarkCPUProfile") || processname.Contains("3DMarkICFWorkload") || processname.Contains("3DMarkTimeSpy") || processname.Contains("3DMarkNightRaid"))
                        {
                            ThreadBooster.ForceCustomBitMask(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.LogExError($"PParseIn exception: {ex.Message}", ex);
            }
        }

        public static string GetCommandLineArgs(Process process)
        {
            if (process == null)
            {
                throw new ArgumentNullException(nameof(process));
            }

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
                using (var objects = searcher.Get())
                {
                    var result = objects.Cast<ManagementBaseObject>().SingleOrDefault();
                    return result?["CommandLine"]?.ToString() ?? "";
                }
            }
            catch
            {
                return string.Empty;
            }
        }
        public static ListProcessesItem? listContains(string processname)
        {
            if (processname != null && processname.Length > 0)
            {
                foreach (ListProcessesItem p in listProcesses)
                {
                    if (processname.ToLower() == p.processName.ToLower()) return p;
                }
            }
            return null;
        }
        public static CurrentProcessesItem? currentContains(string processname)
        {
            if (processname != null && processname.Length > 0)
            {
                foreach (CurrentProcessesItem p in currentProcesses)
                {
                    if (processname.ToLower() == p.processName.ToLower()) return p;
                }
            }
            return null;
        }
        public static CurrentProcessesItem? currentContainsPid(int pid)
        {
            if (pid > 0)
            {
                foreach (CurrentProcessesItem p in currentProcesses)
                {
                    if (p.pid == pid) return p;
                }
            }
            return null;
        }
    }

    public class CurrentProcessesItem
    {
        public int pid { get; set; }
        public string processName { get; set; }
        public bool sysm { get; set; }
        public bool sysm_full { get; set; }
        public bool bitm { get; set; }
        public bool bitm_full { get; set; }
        public bool bitm_pfull { get; set; }
        public int? profile { get; set; }

        public CurrentProcessesItem(int _pid, string _processName, bool _sysm, bool _sysm_full, bool _bitm, bool _bitm_full, bool _bitm_pfull, int? _profile = null)
        {
            pid = _pid;
            processName = _processName;
            sysm = _sysm;
            bitm = _bitm;
            sysm_full = _sysm_full;
            bitm_full = _bitm_full;
            bitm_pfull = _bitm_pfull;
            profile = _profile;
        }
    }
    public class ListProcessesItem
    {
        public string processName { get; set; }
        public bool sysm { get; set; }
        public bool sysm_full { get; set; }
        public bool bitm { get; set; }
        public bool bitm_full { get; set; }
        public bool bitm_pfull { get; set; }
        public bool factory { get; set; }
        public int? profile { get; set; }
        public ListProcessesItem(string _processName, bool _sysm, bool _sysm_full, bool _bitm, bool _bitm_full, bool _bitm_pfull, bool _factory, int? _profile = null)
        {
            processName = _processName;
            sysm = _sysm;
            bitm = _bitm;
            sysm_full = _sysm_full;
            bitm_full = _bitm_full;
            bitm_pfull = _bitm_pfull;
            factory = _factory;
            profile = _profile;
        }
    }
}
