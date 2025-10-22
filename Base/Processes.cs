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
using net.r_eg.Conari.Types;
using System.Security.Cryptography;
using System.Runtime.Intrinsics.Arm;
using Newtonsoft.Json;
using System.Diagnostics.PerformanceData;
using System.Threading;
using System.Printing;
using static Vanara.PInvoke.Kernel32;
using net.r_eg.Conari.PE;
using ZenStates.Core;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CPUDoc
{
    class Processes
    {
        public static List<CurrentProcessesItem> currentProcesses;
        public static List<ListProcessesItem> listProcesses;
        public static string bench3DMarkActive;
        public static IAppProcessProfile bench3DMarkProfile;
        public static IAppZenProfileOC bench3DMarkZenProfileOC;

        public static bool pInit = false;
        static readonly object lock3DMProfile = new object();
        static readonly object lockCurrent = new object();
        public static void Init()
        {

            listProcesses = new();
            listProcesses.Add(new ListProcessesItem("RTSS", true, false, true, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("RTSSHooksLoader64", true, false, true, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("CapFrameX", true, false, true, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("PresentMon_x64", true, false, true, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("PresentMon-2.3.1-x64", true, false, true, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("CapFrameX", true, false, true, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("nvcontainer", true, false, true, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("NVIDIA Share", true, false, true, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("obs64", true, false, false, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("obs-browser-page", true, true, true, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("obs-ffmpeg-mux", true, true, true, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("cpuz", true, true, false, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("HYDRA", true, false, true, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("MechWarrior-Win64-Shipping", true, false, true, true, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("3DMarkICFWorkload", false, false, false, false, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("3DMarkTimeSpy", false, false, false, false, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("3DMarkNightRaid", false, false, false, false, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("3DMarkPortRoyal", false, false, false, false, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("3DMarkWildLife", false, false, false, false, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("3DMarkWildLifeExtreme", false, false, false, false, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("3DMarkSpeedWay", false, false, false, false, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("3DMarkCPUProfile", false, false, false, false, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("EasyFMSI", true, false, false, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("SystemInfoHelper", true, false, false, false, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("WatchDogsLegion", true, false, true, false, false, false, false, true));
            listProcesses.Add(new ListProcessesItem("csrss", true, false, false, false, false, false, false, true));
            listProcesses.Add(new ListProcessesItem("dwm", true, false, true, false, false, false, false, true));
            listProcesses.Add(new ListProcessesItem("NVDisplay.Container", true, false, true, false, false, false, false, true));
            listProcesses.Add(new ListProcessesItem("SOTTR", true, false, true, false, false, false, false, true));
            listProcesses.Add(new ListProcessesItem("HorizonZeroDawn", false, false, false, false, false, false, true, true));
            listProcesses.Add(new ListProcessesItem("csgo", true, false, true, false, false, false, false, true));
            listProcesses.Add(new ListProcessesItem("bf1", true, false, true, false, false, false, false, true));
            listProcesses.Add(new ListProcessesItem("bf3", true, false, true, false, false, false, false, true));
            listProcesses.Add(new ListProcessesItem("bf4", true, false, true, false, false, false, false, true));
            listProcesses.Add(new ListProcessesItem("bfv", true, false, true, false, false, false, false, true));
            listProcesses.Add(new ListProcessesItem("bfh", true, false, true, false, false, false, false, true));
            listProcesses.Add(new ListProcessesItem("bf2042", true, true, true, true, true, false, false, true));
            listProcesses.Add(new ListProcessesItem("BFBC2Game", true, false, true, false, false, false, false, true));
            listProcesses.Add(new ListProcessesItem("valheim", true, false, true, false, false, false, false, true));
            listProcesses.Add(new ListProcessesItem("huntgame", true, true, true, true, false, false, false, true));
            listProcesses.Add(new ListProcessesItem("FlightSimulator", true, true, true, true, false, false, false, true));
            listProcesses.Add(new ListProcessesItem("ASIOhost32", false, false, false, false, false, false, false, false, true));
            listProcesses.Add(new ListProcessesItem("ASIOhost64", false, false, false, false, false, false, false, false, true));

            //listProcesses.Add(new ListProcessesItem("hunt", true, false, true, false, false, true));

            ImportCXList();

            currentProcesses = new();

            pInit = true;

            Process[] processCollection = Process.GetProcesses();
            foreach (Process p in processCollection)
            {
                PParseIn(p.ProcessName, p.Id);
                //App.LogDebug($"Process: {p.ProcessName}");
            }
            ProfileParse();

            //App.LogDebug("M1");

            MaskParse();

        }
        public static void ImportCXList()
        {
            try
            {
                List<CapframeXList> items;
                string cxlist = SettingsManager.CXList;
                if (!File.Exists(cxlist))
                {
                    App.LogInfo("CapFrameX list not found in Settings folder, import skipped");
                    return;
                }

                if (CalcFileSHA256(cxlist) == App.AppSettings.CapframeXlistSHA)
                {
                    return;
                }

                App.LogInfo("CapFrameX list has a new SHA-256, importing...");

                FileInfo fInfo = new FileInfo(cxlist);
                
                using (System.IO.TextReader readFile = new StreamReader(cxlist))
                {
                    try
                    {
                        var fcontent = readFile.ReadToEnd();
                        items = JsonConvert.DeserializeObject<List<CapframeXList>>(fcontent);

                    }
                    catch (IOException e)
                    {
                        App.LogDebug($"ImportCXList I/O Exception: {e.Message}");
                        return;
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        App.LogDebug($"ImportCXList Access Exception: {e.Message}");
                        return;
                    }
                }

                if (items.Count > 0)
                {
                    int _count = 0, _added = 0;
                    foreach (var item in items)
                    {
                        if (!item.IsBlacklisted)
                        {
                            ListProcessesItem? p = listContains(item.Name);
                            if (p == null)
                            {
                                listProcesses.Add(new ListProcessesItem(item.Name, true, false, true, false, false, false, false, true));
                                //App.LogDebug($"ImportCXList imported {item.DisplayName} filename={item.Name}.");
                                _added++;
                            }
                        }
                        _count++;
                    }
                    App.LogDebug($"ImportCXList finished; {_count} game profiles, imported {_added}.");
                }
                else
                {
                    App.LogDebug($"ImportCXList failed, list empty, file corrupted?");
                    return;
                }

            }
            catch (Exception ex)
            {
                App.LogExError($"ImportCXList exception: {ex.Message}", ex);
            }
        }

        public static void AppLaunched(string processname, int pid)
        {
            if (PParseIn(processname, pid))
                App.LogDebug($"AppLaunched=[{pid}] {processname}");
            //ProfileParse();
            //MaskParse(processname);
        }
        public static void AppClosed(string processname, int pid)
        {
            if (PParseOut(processname, pid))
                App.LogDebug($"AppClosed=[{pid}] {processname}");
            //ProfileParse();
        }
        public static void ProfileParse()
        {
            if (!pInit) return;

            try
            {
#nullable enable
                CurrentProcessesItem? p;
#nullable disable
                lock (lockCurrent)
                {
                    p = currentProcesses.Where(process => process.profile > 0).OrderByDescending(process => process.profile).FirstOrDefault();
                }
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
                void DoParse(CurrentProcessesItem p)
                {
                    if ((p.bitm || p.sysm) && (App.pactive.SysSetHack))
                    {
                        ThreadBooster.ProcMask(p.processName, p.sysm, p.sysm_full, p.bitm, p.bitm_full, p.bitm_pfull);
                        //App.LogDebug($"MaskParse action: [{p.pid}] {p.processName} sysm={p.sysm} sysm_full={p.sysm_full} bitm={p.bitm} bitm_full={p.bitm_full} bitm_pfull={p.bitm_pfull}");
                    }
                }

                if (currentProcesses != null)
                {
                    if (processname.Length > 0)
                    {
                        CurrentProcessesItem? p;
                        lock (lockCurrent)
                        {
                            p = currentProcesses.Where(x => x.processName.ToLower() == processname.ToLower()).FirstOrDefault();
                        }

                        if (p != null) DoParse(p);
                    }
                    else
                    {
                        lock (lockCurrent)
                        {
                            foreach (CurrentProcessesItem p in currentProcesses)
                            {
                                if (p != null) DoParse(p);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.LogExError($"MaskParse exception: {ex.Message}", ex);
            }
        }
        public static void UnMaskAllParse()
        {
            if (!pInit) return;

            try
            {
                void DoUnParse(CurrentProcessesItem p)
                {
                    ThreadBooster.ProcSetAffinityMask(p.processName, true, true);
                }

                if (currentProcesses != null)
                {
                    lock (lockCurrent)
                    {
                        foreach (CurrentProcessesItem p in currentProcesses)
                        {
                            if (p != null) DoUnParse(p);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.LogExError($"UnMaskAllParse exception: {ex.Message}", ex);
            }
        }
        public static bool PParseIn(string processname, int pid)
        {
            if (!pInit || pid <= 1024) return false;

            bool parsed = false;

            try
            {
#nullable enable
                ListProcessesItem? p = listContains(processname);
#nullable disable
                if (p != null)
                {
                    parsed = true;

                    lock(lockCurrent)
                    {
                        currentProcesses.Add(new CurrentProcessesItem(pid, processname, p.pcpusetm, p.pcpusetm_full, p.bitm, p.bitm_full, p.bitm_pfull, p.custom, p.sysm, p.sysm_full, p.profile));
                    }

                    if (App.pactive.IdealThread > -2 && App.pactive.IdealThreadScope == 0 && !p.custom)
                    {
                        App.LogDebug($"Set Application profile {processname} Ideal Thread to {((App.pactive.IdealThread >= 0) ? App.pactive.IdealThread : "Auto")}");
                        ThreadBooster.ProcSetIdealThread(processname, App.pactive.IdealThread);
                    }

                    if (processname.ToLower() == "hydra".ToLower()) {
                        App.ZenBlockRefresh = true;
                        App.ProcessDisableThrottle(pid);
                    }

                    if (processname.ToLower() == "WatchDogsLegion".ToLower()
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
                        || processname.ToLower() == "huntgame".ToLower()
                        || processname.ToLower() == "MechWarrior-Win64-Shipping".ToLower()
                        || processname.ToLower() == "HorizonZeroDawn".ToLower()
                        || processname.ToLower() == "SOTTR".ToLower()
                        )
                    {
                        ThreadBooster.ProcSetBoostThread(processname);
                        App.ProcessDisableThrottle(pid);
                    }

                   
                    if (processname.StartsWith("3DMark") || 
                        processname.ToLower().StartsWith("SystemInfoHelper".ToLower()) || 
                        processname.ToLower().StartsWith("EasyFMSI".ToLower())
                        )
                        {
                        App.LogDebug($"3DMark Proc={processname}");

                        ulong _bitMask = ThreadBooster.defBitMask;
                        int _psysThreads = 0;
                        int _paffThreads = 0;
                        int _paffThreadsProc = 0;
                        int _sysThreads = 0;
                        int _ideal = -2;
                        int _trequest = -1;
                        int _tneeded = 0;
                        int _systforced = 0;
                        int _t0threads = ThreadBooster.basecores;
                        int _t1threads = ThreadBooster.addcores;
                        int _tmin = _t0threads;

                        if (processname.ToLower().StartsWith("3DMarkCPUProfile".ToLower())
                            || processname.ToLower().StartsWith("3DMarkICFWorkload".ToLower()) 
                            || processname.ToLower().StartsWith("3DMarkTimeSpy".ToLower())
                            || processname.ToLower().StartsWith("3DMarkPortRoyal".ToLower())
                            || processname.ToLower().StartsWith("3DMarkNightRaid".ToLower())
                            || processname.ToLower().StartsWith("3DMarkWildLife".ToLower())
                            || processname.ToLower().StartsWith("3DMarkWildLifeExtreme".ToLower())
                            || processname.ToLower().StartsWith("3DMarkSpeedWay".ToLower())
                            || processname.ToLower().StartsWith("3DMarkSkyDiver".ToLower()))
                        {

                            Process[] proc = Process.GetProcessesByName(processname);
                            Process procid = Process.GetProcessById(proc[0].Id);
                            string args = GetCommandLineArgs(procid);
                            var regex = new Regex(@"^.* --in .(.*). --out.*");
                            var match = regex.Match(args);

                            if (match.Success)
                            {
                                Workload3DM workload = new Workload3DM();
                                var workloadfile = match.Groups[1].Value.Trim();
                                bool workloadok = false;

                                App.LogDebug($"3DMark Workload={workloadfile}");

                                int _allcores = _t0threads + _t1threads;

                                if (processname.ToLower().StartsWith("3DMarkCPUProfile".ToLower()))
                                {
                                    if (File.ReadLines(workloadfile).Any(line => line.Contains("_maxthreads.json")))
                                    {
                                        _trequest = _tneeded = _allcores;
                                        Load3DMarkProfile(SettingsManager.GetDictProcessProfile(App.bench3DMark, "CPUProfile"));
                                    }
                                    else if (File.ReadLines(workloadfile).Any(line => line.Contains("_16threads.json")))
                                    {
                                        _trequest = _tneeded = 16;
                                        Load3DMarkProfile(SettingsManager.GetDictProcessProfile(App.bench3DMark, "CPUProfile16"));
                                    }
                                    else if (File.ReadLines(workloadfile).Any(line => line.Contains("_8threads.json")))
                                    {
                                        _trequest = _tneeded = 8;
                                        Load3DMarkProfile(SettingsManager.GetDictProcessProfile(App.bench3DMark, "CPUProfile8"));
                                    }
                                    else if (File.ReadLines(workloadfile).Any(line => line.Contains("_4threads.json")))
                                    {
                                        _trequest = _tneeded = 4;
                                        Load3DMarkProfile(SettingsManager.GetDictProcessProfile(App.bench3DMark, "CPUProfile4"));
                                    }
                                    else if (File.ReadLines(workloadfile).Any(line => line.Contains("_2threads.json")))
                                    {
                                        _trequest = _tneeded = 2;
                                        Load3DMarkProfile(SettingsManager.GetDictProcessProfile(App.bench3DMark, "CPUProfile2"));
                                    }
                                    else
                                    {
                                        _trequest = _tneeded = 1;
                                        Load3DMarkProfile(SettingsManager.GetDictProcessProfile(App.bench3DMark, "CPUProfile1"));
                                    }

                                    _systforced = _trequest == _allcores ? _trequest : _t0threads;
                                    _trequest = _trequest <= _t0threads ? _t0threads : _trequest >= _allcores ? _allcores : _trequest;

                                    App.LogDebug($"3DMarkCPUProfile Auto ThreadsRequest={_trequest} SysCpuSetForced={_systforced}");
                                }
                                else
                                {
                                    if (!File.Exists(workloadfile))
                                    {
                                        App.LogInfo($"3DMark Workload input not found! [{workloadfile}]");
                                    }
                                    else
                                    {
                                        using (System.IO.TextReader readFile = new StreamReader(workloadfile))
                                        {
                                            try
                                            {
                                                var fcontent = readFile.ReadToEnd();
                                                workload = JsonConvert.DeserializeObject<Workload3DM>(fcontent);
                                                if (workload != null)
                                                    workloadok = true;
                                            }
                                            catch (IOException e)
                                            {
                                                App.LogDebug($"Import 3DMWorkLoad I/O Exception: {e.Message}");
                                            }
                                            catch (UnauthorizedAccessException e)
                                            {
                                                App.LogDebug($"Import 3DMWorkLoad Access Exception: {e.Message}");
                                            }
                                        }
                                    }

                                    if (workloadok)
                                    {
                                        if (processname.ToLower().Contains("3DMarkTimeSpy".ToLower()))
                                        {
                                            if (workload.config_path.ToLower().Contains("cpu_extreme.json"))
                                            {
                                                Load3DMarkProfile(SettingsManager.GetDictProcessProfile(App.bench3DMark, "TSECPU"));
                                                _trequest = bench3DMarkProfile.ThreadsRequest;
                                                //_tneeded = _trequest == -1 ? (int)ThreadBooster.CountBits(ThreadBooster.defFullBitMask) : _trequest == 0 ? _t0threads : _trequest;
                                                if (bench3DMarkProfile.CpuSetSysTRequest == -1) _systforced = (int)ThreadBooster.CountBits(ThreadBooster.defFullBitMask);
                                            }
                                            if (workload.config_path.ToLower().Contains("cpu.json"))
                                            {
                                                Load3DMarkProfile(SettingsManager.GetDictProcessProfile(App.bench3DMark, "TSCPU"));
                                                _trequest = bench3DMarkProfile.ThreadsRequest;
                                                _tmin = 26;
                                                _tneeded = _trequest == -1 ? (_tmin > _t0threads ? _tmin : _t0threads ) : _trequest == 0 ? _t0threads : _trequest;
                                                if (bench3DMarkProfile.CpuSetSysTRequest == -1) _systforced = (int)ThreadBooster.CountBits(ThreadBooster.defBitMask);
                                            }
                                            if (workload.config_path.ToLower().Contains("gt") && !workload.config_path.ToLower().Contains("extreme"))
                                            {
                                                if (workload.config_path.ToLower().Contains("gt1"))
                                                {
                                                    Load3DMarkProfile(SettingsManager.GetDictProcessProfile(App.bench3DMark, "TSGT1"));
                                                    _trequest = bench3DMarkProfile.ThreadsRequest;
                                                    _tmin = 12;
                                                    _tneeded = _trequest == -1 ? (_tmin > _t0threads ? _tmin : _t0threads) : _trequest == 0 ? _t0threads : _trequest;
                                                    if (bench3DMarkProfile.CpuSetSysTRequest == -1) _systforced = (int)ThreadBooster.CountBits(ThreadBooster.defBitMask);
                                                }

                                                if (workload.config_path.ToLower().Contains("gt2"))
                                                {
                                                    Load3DMarkProfile(SettingsManager.GetDictProcessProfile(App.bench3DMark, "TSGT2"));
                                                    _trequest = bench3DMarkProfile.ThreadsRequest;
                                                    _tmin = 12;
                                                    _tneeded = _trequest == -1 ? (_tmin > _t0threads ? _tmin : _t0threads) : _trequest == 0 ? _t0threads : _trequest;
                                                    if (bench3DMarkProfile.CpuSetSysTRequest == -1) _systforced = (int)ThreadBooster.CountBits(ThreadBooster.defBitMask);
                                                }
                                            }
                                            if (workload.config_path.ToLower().Contains("gt") && workload.config_path.ToLower().Contains("extreme"))
                                            {
                                                if (workload.config_path.ToLower().Contains("gt1"))
                                                {
                                                    Load3DMarkProfile(SettingsManager.GetDictProcessProfile(App.bench3DMark, "TSEGT1"));
                                                    _trequest = bench3DMarkProfile.ThreadsRequest;
                                                    _tmin = 12;
                                                    _tneeded = _trequest == -1 ? (_tmin > _t0threads ? _tmin : _t0threads) : _trequest == 0 ? _t0threads : _trequest;
                                                    if (bench3DMarkProfile.CpuSetSysTRequest == -1) _systforced = (int)ThreadBooster.CountBits(ThreadBooster.defBitMask);
                                                }
                                                if (workload.config_path.ToLower().Contains("gt2"))
                                                {
                                                    Load3DMarkProfile(SettingsManager.GetDictProcessProfile(App.bench3DMark, "TSEGT2"));
                                                    _trequest = bench3DMarkProfile.ThreadsRequest;
                                                    _tmin = 14;
                                                    _tneeded = _trequest == -1 ? (_tmin > _t0threads ? _tmin : _t0threads) : _trequest == 0 ? _t0threads : _trequest;
                                                    if (bench3DMarkProfile.CpuSetSysTRequest == -1) _systforced = (int)ThreadBooster.CountBits(ThreadBooster.defBitMask);
                                                }
                                            }
                                        }

                                        if (processname.ToLower().Contains("3DMarkWildLife".ToLower()))
                                        {
                                            if (processname.ToLower().Contains("3DMarkWildLifeExtreme".ToLower()))
                                            {
                                                Load3DMarkProfile(SettingsManager.GetDictProcessProfile(App.bench3DMark, "WLEGT1"));
                                                _trequest = bench3DMarkProfile.ThreadsRequest;
                                                _tneeded = _trequest == -1 ? (_tmin > _t0threads ? _tmin : _t0threads) : _trequest == 0 ? _t0threads : _trequest;
                                            }
                                            else
                                            {
                                                Load3DMarkProfile(SettingsManager.GetDictProcessProfile(App.bench3DMark, "WLGT1"));
                                                _trequest = bench3DMarkProfile.ThreadsRequest;
                                                _tneeded = _trequest == -1 ? (_tmin > _t0threads ? _tmin : _t0threads) : _trequest == 0 ? _t0threads : _trequest;
                                            }
                                        }

                                        if (processname.ToLower().Contains("3DMarkNightRaid".ToLower()))
                                        {
                                            if (workload.config_path.ToLower().Contains("cpu.json"))
                                            {
                                                Load3DMarkProfile(SettingsManager.GetDictProcessProfile(App.bench3DMark, "NRCPU"));
                                                _trequest = bench3DMarkProfile.ThreadsRequest;
                                                _tneeded = _trequest == -1 ? (_tmin > _t0threads ? _tmin : _t0threads) : _trequest == 0 ? _t0threads : _trequest;
                                            }
                                            if (workload.config_path.ToLower().Contains("gt"))
                                            {
                                                if (workload.config_path.ToLower().Contains("gt1"))
                                                {
                                                    Load3DMarkProfile(SettingsManager.GetDictProcessProfile(App.bench3DMark, "NRGT1"));
                                                    _trequest = bench3DMarkProfile.ThreadsRequest;
                                                    _tneeded = _trequest == -1 ? (_tmin > _t0threads ? _tmin : _t0threads) : _trequest == 0 ? _t0threads : _trequest;

                                                }
                                                if (workload.config_path.ToLower().Contains("gt2"))
                                                {
                                                    Load3DMarkProfile(SettingsManager.GetDictProcessProfile(App.bench3DMark, "NRGT2"));
                                                    _trequest = bench3DMarkProfile.ThreadsRequest;
                                                    _tneeded = _trequest == -1 ? (_tmin > _t0threads ? _tmin : _t0threads) : _trequest == 0 ? _t0threads : _trequest;

                                                }
                                            }
                                        }

                                    }


                                    if (processname.ToLower().Contains("3DMarkPortRoyal".ToLower()))
                                    {
                                        Load3DMarkProfile(SettingsManager.GetDictProcessProfile(App.bench3DMark, "PRGT1"));
                                        _trequest = bench3DMarkProfile.ThreadsRequest;
                                        _tneeded = _trequest == -1 ? (_tmin > _t0threads ? _tmin : _t0threads) : _trequest == 0 ? _t0threads : _trequest;
                                    }

                                    if (processname.ToLower().Contains("3DMarkSpeedWay".ToLower()))
                                    {
                                        Load3DMarkProfile(SettingsManager.GetDictProcessProfile(App.bench3DMark, "SWGT1"));
                                        _trequest = bench3DMarkProfile.ThreadsRequest;
                                        _tneeded = _trequest == -1 ? (_tmin > _t0threads ? _tmin : _t0threads) : _trequest == 0 ? _t0threads : _trequest;
                                    }

                                }

                            }

                            if (processname.ToLower().Contains("3DMarkICFWorkload".ToLower()))
                            {
                                Load3DMarkProfile(SettingsManager.GetDictProcessProfile(App.bench3DMark, "FSGT1"));
                                App.LogDebug($"3DMProfile {bench3DMarkActive}");
                                _trequest = bench3DMarkProfile.ThreadsRequest;
                                _tneeded = _trequest == -1 ? (_tmin > _t0threads ? _tmin : _t0threads) : _trequest == 0 ? _t0threads : _trequest;
                            }

                        }

                        if (processname.ToLower().Contains("SystemInfoHelper".ToLower()))
                        {
                            Load3DMarkProfile(SettingsManager.GetDictProcessProfile(App.bench3DMark, "SystemInfoHelper"));
                            App.LogDebug($"3DMProfile {bench3DMarkActive}");
                            _trequest = bench3DMarkProfile.ThreadsRequest;
                            _tneeded = _trequest == -1 ? (_tmin > _t0threads ? _tmin : _t0threads) : _trequest == 0 ? _t0threads : _trequest;
                        }
                        if (processname.ToLower().Contains("EasyFMSI".ToLower()))
                        {
                            Load3DMarkProfile(SettingsManager.GetDictProcessProfile(App.bench3DMark, "EasyFMSI"));
                            App.LogDebug($"3DMProfile {bench3DMarkActive}");
                            _trequest = bench3DMarkProfile.ThreadsRequest;
                            _tneeded = _trequest == -1 ? (_tmin > _t0threads ? _tmin : _t0threads) : _trequest == 0 ? _t0threads : _trequest;
                        }

                        App.LogDebug($"3DMProfile {bench3DMarkActive} ThreadRequest={((_trequest > 0) ? _trequest : "Auto")} ThreadNeeded={_tneeded}");

                        bool _forceSysMask = true;

                        _paffThreads = bench3DMarkProfile.AffinityThreadTRequest;
                        _psysThreads = bench3DMarkProfile.CpuSetProcessTRequest;
                        _paffThreadsProc = bench3DMarkProfile.AffinityProcTRequest;

                        _psysThreads = _psysThreads == -1 ? _tneeded : _psysThreads == 0 ? _t0threads : _psysThreads;
                        _paffThreads = _paffThreads == -1 ? _tneeded : _paffThreads == 0 ? _t0threads : _paffThreads;
                        _paffThreadsProc = _paffThreadsProc == -1 ? _tneeded : _paffThreadsProc == 0 ? _t0threads : _paffThreadsProc;

                        if (bench3DMarkProfile.CpuSetSysTRequest <= 0)
                        {
                            if (_systforced > 0)
                            {
                                _sysThreads = _systforced;
                            }
                            else if (_trequest <= 0)
                            {
                                _sysThreads = _t0threads;
                            }
                            else
                            {
                                _sysThreads = _tneeded;
                            }
                        }
                        else
                        {
                            _sysThreads = bench3DMarkProfile.CpuSetSysTRequest;
                        }

                        App.LogInfo($"3DMark {bench3DMarkActive} CpuSetSysThreads={((_sysThreads > 0) ? _sysThreads : "Auto")} T0={_t0threads} T1={_t1threads}");
                        App.LogInfo($"3DMark {bench3DMarkActive} ThreadsRequest={((bench3DMarkProfile.ThreadsRequest == -1) ? "Auto" : (bench3DMarkProfile.ThreadsRequest != 0) ? bench3DMarkProfile.ThreadsRequest : "All T0")}");
                        App.LogInfo($"3DMark {bench3DMarkActive} CpuSetSysTRequest={((bench3DMarkProfile.CpuSetSysTRequest == -1) ? "Auto" : (bench3DMarkProfile.CpuSetSysTRequest != 0) ? bench3DMarkProfile.CpuSetSysTRequest : "All T0")}");
                        App.LogInfo($"3DMark {bench3DMarkActive} CpuSetProcessTRequest={((bench3DMarkProfile.CpuSetProcessTRequest == -1) ? "Auto" : (bench3DMarkProfile.CpuSetProcessTRequest != 0) ? bench3DMarkProfile.CpuSetProcessTRequest : "All T0")}");
                        App.LogInfo($"3DMark {bench3DMarkActive} AffinityThreadTRequest={((bench3DMarkProfile.AffinityThreadTRequest == -1) ? "Auto" : (bench3DMarkProfile.AffinityThreadTRequest != 0) ? bench3DMarkProfile.AffinityThreadTRequest : "All T0")}");
                        App.LogInfo($"3DMark {bench3DMarkActive} AffinityProcTRequest={((bench3DMarkProfile.AffinityProcTRequest == -1) ? "Auto" : (bench3DMarkProfile.AffinityProcTRequest != 0) ? bench3DMarkProfile.AffinityProcTRequest : "All T0")}");

                        if (_sysThreads > 0)
                        {
                            _forceSysMask = true;
                        }
                        else if (_sysThreads == -1)
                        {
                            _sysThreads = _t0threads;
                        }
                        if (_sysThreads > 0)
                        {
                            _bitMask = _sysThreads <= _t0threads ? ThreadBooster.defBitMask : _sysThreads >= (_t0threads + _t1threads) ? ThreadBooster.defFullBitMask : ThreadBooster.CreateCustomBitMask(_sysThreads - _t0threads);

                            App.LogInfo($"3DMark {bench3DMarkActive} CpuSet System BitMask=0x{_bitMask:X16} Threads={ThreadBooster.CountBits(_bitMask)} Forced={_forceSysMask}");
                            ThreadBooster.ForceCustomSysMask(_forceSysMask, _bitMask);
                        }
                        _ideal = bench3DMarkProfile.IdealThread;
                        if (_paffThreads > 0 || _paffThreadsProc > 0 || _psysThreads > 0)
                        {
                            bool _sysm = _psysThreads > 0 ? true : false;
                            bool _bitm = _paffThreads > 0 || _paffThreadsProc > 0 ? true : false;
                            ulong  _sysmCustom = _psysThreads > 0 ? ThreadBooster.CreateCustomBitMask(_psysThreads - _t0threads) : 0;
                            ulong _bitmCustom = _paffThreads > 0 ? ThreadBooster.CreateCustomBitMask(_paffThreads - _t0threads) : 0;
                            ulong _bitCustomProc = _paffThreadsProc > 0 ? ThreadBooster.CreateCustomBitMask(_paffThreadsProc - _t0threads) : 0;
                            App.LogInfo($"3DMark {bench3DMarkActive} Custom Process Masks ProcessCpuSetMask=0x{_sysmCustom:X16}:{ThreadBooster.CountBits(_sysmCustom)} ThreadsAffMask=0x{_bitmCustom:X16}:{ThreadBooster.CountBits(_bitmCustom)} ProcessAffMask=0x{_bitCustomProc:X16}:{ThreadBooster.CountBits(_bitCustomProc)}");
                            App.LogInfo($"3DMark {bench3DMarkActive} Custom Process Threads ProcessCpuSetMaskTRequest={_psysThreads} ThreadsAffMaskTRequest={_paffThreads} ProcessAffMaskTRequest={_paffThreadsProc}");
                            if (_ideal != -2) App.LogDebug($"3DMark Set Ideal Thread to {((_ideal >= 0) ? _ideal : "Auto")}");
                            ThreadBooster.ProcMask(processname, _sysm, false, _bitm, false, false, _ideal, _sysmCustom, _bitmCustom, _bitCustomProc);
                        }
                        else if (_ideal > -2)
                        {
                            App.LogInfo($"3DMark Set Ideal Thread to {((_ideal >= 0) ? _ideal : "Auto")}");
                            ThreadBooster.ProcSetIdealThread(processname, _ideal);
                        }

                        if (bench3DMarkProfile.ProcessBoost)
                            ThreadBooster.ProcSetBoostThread(processname);
                        if (bench3DMarkProfile.ProcessDisableThrottle)
                            App.ProcessDisableThrottle(pid);

                    }
                    else
                    {
                        App.LogDebug($"MaskParse for: {processname}");
                        if (p.sysm_full)
                        {
                            ThreadBooster.ForceCustomSysMask(true, ThreadBooster.defFullBitMask, 120);
                            App.LogInfo($"{processname} force Full SysCpuSet for 120 seconds");
                        }
                        else if(p.sysm)
                        {
                            ThreadBooster.ForceCustomSysMask(true, ThreadBooster.defBitMask, 120);
                            App.LogInfo($"{processname} force T0 SysCpuSet for 120 seconds");
                        }
                        MaskParse(processname);

                    }
                }
                else
                {
                    //Process not found in list of application profiles
                }
                
                return parsed;
            }
            catch (Exception ex)
            {
                App.LogExError($"PParseIn exception: {ex.Message}", ex);
                return false;
            }
        }
        public static bool PParseOut(string processname, int pid)
        {
            if (!pInit) return false;
            
            bool parsed = false;

            try
            {
#nullable enable
                CurrentProcessesItem? p = currentContainsPid(pid);
#nullable disable
                if (p != null)
                {
                    parsed = true;

                    lock (lockCurrent)
                    {
                        currentProcesses.Remove(p);
                    }

                    if (p.processName.StartsWith("HYDRA")) 
                        App.ZenBlockRefresh = false;

                    if (processname.ToLower().Contains("3DMarkCPUProfile".ToLower())
                        || processname.ToLower().Contains("SystemInfoHelper".ToLower())
                        || processname.ToLower().Contains("EasyFMSI".ToLower())
                        || processname.ToLower().Contains("3DMarkICFWorkload".ToLower())
                        || processname.ToLower().Contains("3DMarkTimeSpy".ToLower())
                        || processname.ToLower().Contains("3DMarkPortRoyal".ToLower())
                        || processname.ToLower().Contains("3DMarkNightRaid".ToLower())
                        || processname.ToLower().Contains("3DMarkWildLife".ToLower())
                        || processname.ToLower().Contains("3DMarkSkyDiver".ToLower())
                        || processname.ToLower().Contains("3DMarkSpeedWay".ToLower()))
                    {
                        Unload3DMarkProfile();
                        if (ThreadBooster.forceCustomSysMask)
                            ThreadBooster.ForceCustomSysMask(false);
                    }
                }
                return parsed;
            }
            catch (Exception ex)
            {
                App.LogExError($"PParseIn exception: {ex.Message}", ex);
                return false;
            }
        }
        public static bool Load3DMarkProfile(IAppProcessProfile profile)
        {
            lock (lock3DMProfile)
            {
                if (bench3DMarkActive != "" && bench3DMarkProfile != null)
                {
                    App.LogDebug($"3DM Profile still active {bench3DMarkActive}");
                    if (bench3DMarkProfile.ZenOCModeAfterAction == true)
                    {
                        App.LogDebug($"ZenProfilesOC After Action {bench3DMarkProfile.ZenOCModeAfterSet}");
                    }
                }
                bench3DMarkActive = profile.Name;
                bench3DMarkProfile = profile;
                App.LogInfo($"3DM Profile now loaded: {bench3DMarkProfile.Name}");
                if (bench3DMarkProfile != null)
                {
                    if (bench3DMarkProfile.ZenOCModeBeforeAction == true)
                    {
                        App.LogDebug($"ZenProfilesOC After Action {bench3DMarkProfile.ZenOCModeBeforeSet}");
                    }
                    if (profile.ZenStaticProfile >= 0)
                    {
                        App.ZenProfilesOC[profile.ZenStaticProfile].CopyPropertiesTo(bench3DMarkZenProfileOC);
                        App.LogDebug($"ZenProfilesOC Set to {profile.ZenStaticProfile}");
                    }
                    else
                    {
                        bench3DMarkZenProfileOC = null;
                    }
                }
                else
                {
                    App.LogDebug($"3DM Profile not loaded!");
                }
            }
            return true;
        }
        public static bool Unload3DMarkProfile()
        {
            lock (lock3DMProfile)
            {
                if (bench3DMarkProfile != null)
                {
                    App.LogDebug($"3DM Profile unloading: {bench3DMarkActive}");
                    if (bench3DMarkProfile.ZenOCModeAfterAction == true)
                    {
                        App.LogDebug($"ZenProfilesOC After Action {bench3DMarkProfile.ZenOCModeAfterSet}");
                    }
                    bench3DMarkActive = "";
                    bench3DMarkProfile = null;
                    bench3DMarkZenProfileOC = null;
                }
            }
            return true;
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
                lock (lockCurrent)
                {
                    foreach (ListProcessesItem p in listProcesses)
                    {
                        if (processname.ToLower() == p.processName.ToLower()) return p;
                    }
                }
            }
            return null;
        }
        public static CurrentProcessesItem? currentContains(string processname)
        {
            if (processname != null && processname.Length > 0)
            {
                lock (lockCurrent)
                {
                    foreach (CurrentProcessesItem p in currentProcesses)
                    {
                        if (processname.ToLower() == p.processName.ToLower()) return p;
                    }
                }
            }
            return null;
        }
        public static CurrentProcessesItem? currentContainsPid(int pid)
        {
            if (pid > 1024)
            {
                foreach (CurrentProcessesItem p in currentProcesses)
                {
                    if (p.pid == pid) return p;
                }
            }
            return null;
        }

        public static bool currentIsRunning(string processname)
        {
            if (processname != null && processname.Length > 0)
            {
                lock (lockCurrent)
                {
                    foreach (CurrentProcessesItem p in currentProcesses)
                    {
                        if (processname.ToLower() == p.processName.ToLower()) return true;
                    }
                }
            }
            return false;
        }

        public static string ByteArrayToString(byte[] array)
        {
            string _return = "";
            for (int i = 0; i < array.Length; i++)
            {
                _return += ($"{array[i]:X2}");
                if ((i % 4) == 3) _return += (" ");
            }
            return _return;
        }
        public static string CalcFileSHA256(string filename)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                FileInfo fInfo = new FileInfo(filename);
                using (FileStream fileStream = fInfo.Open(FileMode.Open))
                {
                    try
                    {
                        // Create a fileStream for the file.
                        // Be sure it's positioned to the beginning of the stream.
                        fileStream.Position = 0;
                        // Compute the hash of the fileStream.
                        byte[] hashValue = mySHA256.ComputeHash(fileStream);
                        // Return the hash value of the file
                        return ByteArrayToString(hashValue);
                    }
                    catch (IOException e)
                    {
                        App.LogDebug($"CalcFileSHA256 I/O Exception: {e.Message}");
                        return "";
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        App.LogDebug($"CalcFileSHA256 Access Exception: {e.Message}");
                        return "";
                    }
                }
            }
        }

    }

    public class CurrentProcessesItem
    {
        public int pid { get; set; }
        public string processName { get; set; }
        public bool sysm { get; set; }
        public bool sysm_full { get; set; }
        public bool pcpusetm { get; set; }
        public bool pcpusetm_full { get; set; }
        public bool bitm { get; set; }
        public bool bitm_full { get; set; }
        public bool bitm_pfull { get; set; }
        public bool custom { get; set; }
        public int? profile { get; set; }

        public CurrentProcessesItem(int _pid, string _processName, bool _pcpusetm, bool _pcpusetm_full, bool _bitm, bool _bitm_full, bool _bitm_pfull, bool _sysm, bool _sysm_full, bool _custom = false, int? _profile = null)
        {
            pid = _pid;
            processName = _processName;
            sysm = _sysm;
            sysm_full = _sysm_full;
            pcpusetm = _pcpusetm;
            pcpusetm_full = _pcpusetm_full;
            bitm = _bitm;
            bitm_full = _bitm_full;
            bitm_pfull = _bitm_pfull;
            custom = _custom;
            profile = _profile;
        }
    }
    public class ListProcessesItem
    {
        public string processName { get; set; }
        public bool sysm { get; set; }
        public bool sysm_full { get; set; }
        public bool pcpusetm { get; set; }
        public bool pcpusetm_full { get; set; }
        public bool bitm { get; set; }
        public bool bitm_full { get; set; }
        public bool bitm_pfull { get; set; }
        public bool factory { get; set; }
        public bool custom { get; set; }
        public int? profile { get; set; }
        public ListProcessesItem(string _processName, bool _pcpusetm, bool _pcpusetm_full, bool _bitm, bool _bitm_full, bool _bitm_pfull, bool _sysm, bool _sysm_full, bool _factory, bool _custom = false, int? _profile = null)
        {
            processName = _processName;
            sysm = _sysm;
            sysm_full = _sysm_full;
            pcpusetm = _pcpusetm;
            pcpusetm_full = _pcpusetm_full;
            bitm = _bitm;
            bitm_full = _bitm_full;
            bitm_pfull = _bitm_pfull;
            factory = _factory;
            custom = _custom;
            profile = _profile;
        }
    }
    public class CapframeXList
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsBlacklisted { get; set; } = true;
    }

    public class Workload3DM
    {
        [JsonProperty("abuffer_remap_subtile")]
        public bool abuffer_remap_subtile { get; set; }

        [JsonProperty("allow_fail")]
        public bool allow_fail { get; set; }

        [JsonProperty("config_path")]
        public string config_path { get; set; }

        [JsonProperty("cpu_config_path")]
        public string cpu_config_path { get; set; }

        [JsonProperty("cpu_instruction_set")]
        public string cpu_instruction_set { get; set; }

        [JsonProperty("data_folder")]
        public string data_folder { get; set; }

        [JsonProperty("debug_log_path")]
        public string debug_log_path { get; set; }

        [JsonProperty("directx_feature_level")]
        public string directx_feature_level { get; set; }

        [JsonProperty("disable_async_compute")]
        public bool disable_async_compute { get; set; }

        [JsonProperty("disable_command_submission")]
        public bool disable_command_submission { get; set; }

        [JsonProperty("disable_explicit_resource_heaps")]
        public bool disable_explicit_resource_heaps { get; set; }

        [JsonProperty("disable_particles")]
        public bool disable_particles { get; set; }

        [JsonProperty("display_id")]
        public string display_id { get; set; }

        [JsonProperty("dlc_name")]
        public string dlc_name { get; set; }

        [JsonProperty("draw_loading_screen")]
        public bool draw_loading_screen { get; set; }

        [JsonProperty("dxgi_adapter_luid")]
        public int dxgi_adapter_luid { get; set; }

        [JsonProperty("edition")]
        public string edition { get; set; }

        [JsonProperty("enable_fixed_fps")]
        public bool enable_fixed_fps { get; set; }

        [JsonProperty("enable_frame_output")]
        public bool enable_frame_output { get; set; }

        [JsonProperty("enable_systeminfo_collect")]
        public bool enable_systeminfo_collect { get; set; }

        [JsonProperty("enable_systeminfo_monitor")]
        public bool enable_systeminfo_monitor { get; set; }

        [JsonProperty("enable_vsync")]
        public bool enable_vsync { get; set; }

        [JsonProperty("fixed_fps")]
        public int fixed_fps { get; set; }

        [JsonProperty("frame_output_file_name")]
        public string frame_output_file_name { get; set; }

        [JsonProperty("frame_output_fixed_fps")]
        public int frame_output_fixed_fps { get; set; }

        [JsonProperty("frame_output_sequence_begin")]
        public int frame_output_sequence_begin { get; set; }

        [JsonProperty("frame_output_sequence_end")]
        public int frame_output_sequence_end { get; set; }

        [JsonProperty("is_audio_enabled")]
        public bool is_audio_enabled { get; set; }

        [JsonProperty("is_borderless")]
        public bool is_borderless { get; set; }

        [JsonProperty("is_debug_log_enabled")]
        public bool is_debug_log_enabled { get; set; }

        [JsonProperty("is_graphics_api_debug_layer_enabled")]
        public bool is_graphics_api_debug_layer_enabled { get; set; }

        [JsonProperty("is_looping_enabled")]
        public bool is_looping_enabled { get; set; }

        [JsonProperty("is_multi_threaded_shader_and_pso_compilation_enabled")]
        public bool is_multi_threaded_shader_and_pso_compilation_enabled { get; set; }

        [JsonProperty("is_pso_cache_enabled")]
        public bool is_pso_cache_enabled { get; set; }

        [JsonProperty("is_shader_cache_enabled")]
        public bool is_shader_cache_enabled { get; set; }

        [JsonProperty("is_windowed")]
        public bool is_windowed { get; set; }

        [JsonProperty("is_wireframe_enabled")]
        public bool is_wireframe_enabled { get; set; }

        [JsonProperty("launch_binary_x64")]
        public string launch_binary_x64 { get; set; }

        [JsonProperty("loading_badge_path")]
        public string loading_badge_path { get; set; }

        [JsonProperty("loading_screen_path")]
        public string loading_screen_path { get; set; }

        [JsonProperty("loop_count")]
        public int loop_count { get; set; }

        [JsonProperty("max_af_anisotropy")]
        public int max_af_anisotropy { get; set; }

        [JsonProperty("max_gpu_count")]
        public int max_gpu_count { get; set; }

        [JsonProperty("max_shadow_map_size")]
        public int max_shadow_map_size { get; set; }

        [JsonProperty("max_tessellation_factor")]
        public double max_tessellation_factor { get; set; }

        [JsonProperty("mode_frame_sequence")]
        public bool mode_frame_sequence { get; set; }

        [JsonProperty("mode_single_frame")]
        public bool mode_single_frame { get; set; }

        [JsonProperty("pre_workload_apps_to_run")]
        public List<object> pre_workload_apps_to_run { get; set; }

        [JsonProperty("preset")]
        public string preset { get; set; }

        [JsonProperty("relative_dat_folder")]
        public string relative_dat_folder { get; set; }

        [JsonProperty("rendering_resolution")]
        public List<int> rendering_resolution { get; set; }

        [JsonProperty("run_all_performance_sections")]
        public bool run_all_performance_sections { get; set; }

        [JsonProperty("scaling_mode")]
        public string scaling_mode { get; set; }

        [JsonProperty("set_name")]
        public string set_name { get; set; }

        [JsonProperty("single_frame_step")]
        public int single_frame_step { get; set; }

        [JsonProperty("swapchain_resolution")]
        public List<int> swapchain_resolution { get; set; }

        [JsonProperty("temp_path")]
        public string temp_path { get; set; }

        [JsonProperty("tessellation_factor_scale")]
        public double tessellation_factor_scale { get; set; }

        [JsonProperty("test_id")]
        public string test_id { get; set; }

        [JsonProperty("texture_filtering_mode")]
        public string texture_filtering_mode { get; set; }

        [JsonProperty("thread_count")]
        public int thread_count { get; set; }

        [JsonProperty("use_refrast")]
        public bool use_refrast { get; set; }

        [JsonProperty("use_triple_buffering")]
        public bool use_triple_buffering { get; set; }

        [JsonProperty("use_warp")]
        public bool use_warp { get; set; }

        [JsonProperty("ux_dat_path")]
        public string ux_dat_path { get; set; }

        [JsonProperty("window_message_id")]
        public int window_message_id { get; set; }

        [JsonProperty("workload_name")]
        public string workload_name { get; set; }
    }

    class HandleInfo
    {
        [DllImport("ntdll.dll", CharSet = CharSet.Auto)]
        public static extern uint NtQuerySystemInformation(int SystemInformationClass, IntPtr SystemInformation, int SystemInformationLength, out int ReturnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr VirtualAlloc(IntPtr address, uint numBytes, uint commitOrReserve, uint pageProtectionMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool VirtualFree(IntPtr address, uint numBytes, uint pageFreeMode);

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_HANDLE_INFORMATION
        {
            public int ProcessId;
            public byte ObjectTypeNumber;
            public byte Flags; // 1 = PROTECT_FROM_CLOSE, 2 = INHERIT
            public short Handle;
            public int Object;
            public int GrantedAccess;
        }

        static uint MEM_COMMIT = 0x1000;
        static uint PAGE_READWRITE = 0x04;
        static uint MEM_DECOMMIT = 0x4000;
        static int SystemHandleInformation = 16;
        static uint STATUS_INFO_LENGTH_MISMATCH = 0xC0000004;

        public HandleInfo()
        {
            IntPtr memptr = VirtualAlloc(IntPtr.Zero, 100, MEM_COMMIT, PAGE_READWRITE);

            int returnLength = 0;
            bool success = false;

            uint result = NtQuerySystemInformation(SystemHandleInformation, memptr, 100, out returnLength);
            if (result == STATUS_INFO_LENGTH_MISMATCH)
            {
                success = VirtualFree(memptr, 0, MEM_DECOMMIT);
                memptr = VirtualAlloc(IntPtr.Zero, (uint)(returnLength + 256), MEM_COMMIT, PAGE_READWRITE);
                result = NtQuerySystemInformation(SystemHandleInformation, memptr, returnLength, out returnLength);
            }

            int handleCount = Marshal.ReadInt32(memptr);
            SYSTEM_HANDLE_INFORMATION[] returnHandles = new SYSTEM_HANDLE_INFORMATION[handleCount];

            using (StreamWriter sw = new StreamWriter(@"C:\NtQueryDbg.txt"))
            {
                sw.WriteLine("@ Offset\tProcess Id\tHandle Id\tHandleType");
                for (int i = 0; i < handleCount; i++)
                {
                    SYSTEM_HANDLE_INFORMATION thisHandle = (SYSTEM_HANDLE_INFORMATION)Marshal.PtrToStructure(
                        new IntPtr(memptr.ToInt32() + 4 + i * Marshal.SizeOf(typeof(SYSTEM_HANDLE_INFORMATION))),
                        typeof(SYSTEM_HANDLE_INFORMATION));
                    sw.WriteLine("{0}\t{1}\t{2}\t{3}", i.ToString(), thisHandle.ProcessId.ToString(), thisHandle.Handle.ToString(), thisHandle.ObjectTypeNumber.ToString());
                }
            }

            success = VirtualFree(memptr, 0, MEM_DECOMMIT);
        }
    }
}
