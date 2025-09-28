using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using ZenStates.Core;
using System.Xml.Linq;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Data;
using LibreHardwareMonitor.Hardware;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using OSVersionExtension;
using AutoUpdaterDotNET;
using Hardcodet.Wpf.TaskbarNotification.Interop;
using static CPUDoc.ProcessorInfo;
using static CPUDoc.MemoryConfig;
using System.Windows.Markup;
using CPUDoc.Windows;
using System.Windows.Shapes;
using static ZenStates.Core.Cpu;
using net.r_eg.Conari.Types;
using Windows.Devices.HumanInterfaceDevice;
using System.Windows.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using net.r_eg.Conari.Extension;
using System.Windows.Media.Imaging;

namespace CPUDoc
{
    public class SystemInfo : INotifyPropertyChanged
    {
        public Computer LibreComputer = new Computer();
        public string AppVersion { get; set; }
        public string LastVersionOnServer { get; set; }
        public bool bECores { get; set; }
        public bool bECoresLast { get; set; }
        public string BoardManufacturer { get; set; }
        public string BoardModel { get; set; }
        public string BoardBIOS { get; set; }
        public string CPUBits { get; set; } = "N/A";
        public string CPUDescription { get; set; }
        public string CPUName { get; set; }
        public int CPUFamily { get; set; }
        public int CPUCores { get; set; }
        public int CPUEnabledCores { get; set; }
        public int CPUThreads { get; set; }
        public string CPUSocket { get; set; }
        public int CPULogicalProcessors { get; set; }
        public string CPULabel { get; set; }
        public string BoardLabel { get; set; }
        public string SystemLabel { get; set; }
        public string PPlanLabel { get; set; }
        public static Guid PPlanGuid { get; set; }
        public string ProcessorsLabel { get; set; }
        public string MemoryLabel { get; set; }
        public string WindowsLabel { get; set; }
        public bool HyperThreading { get; set; }
        public int[,] CPPC { get; set; }
        public int[] CPPCActiveOrder { get; set; }
        public int[] CPPCOrder { get; set; }
        public int[] CPPCOrder1 { get; set; }
        public int[] CPPCCustomOrder { get; set; }
        public bool CPPCCustomEnabled { get; set; }
        public string CPPCLabel { get; set; }
        public string CPPCActiveLabel { get; set; }
        public string CPPCTagsLabel { get; set; }
        public string CPPCPerfLabel { get; set; }
        public Cpu Zen { get; set; }
        public bool ZenDLLFail { get; set; }
        public bool ZenStates { get; set; }
        public string ZenSMUVer { get; set; }
        public int ZenPTVersion { get; set; }
        public int ZenBoost { get; set; }
        public int ZenScalar { get; set; }
        public int ZenStartPPT { get; set; }
        public int ZenStartTDC { get; set; }
        public int ZenStartEDC { get; set; }
        public int ZenStartTHM { get; set; }
        public int ZenStartBoost { get; set; }
        public int ZenPPT { get; set; }
        public int ZenTDC { get; set; }
        public int ZenEDC { get; set; }
        public int ZenTHM { get; set; }
        public int ZenMaxBoost { get; set; }
        public int ZenMaxPPT { get; set; }
        public int ZenMaxTDC { get; set; }
        public int ZenMaxEDC { get; set; }
        public int ZenMaxTHM { get; set; }
        public double ZenFCLK { get; set; }
        public double ZenUCLK { get; set; }
        public double ZenMCLK { get; set; }
        public int ZenVDDP { get; set; }
        public int ZenVCCD { get; set; }
        public int ZenVIOD { get; set; }
        public int ZenVDDG { get; set; }
        public float ZenMemRatio { get; set; }
        public bool ZenCOb { get; set; }
        public bool ZenPerCCDTemp { get; set; }
        public int ZenCCDTotal { get; set; }
        public int ZenCCXTotal { get; set; }
        public int ZenCoresPerCCX { get; set; }
        public int[] ZenCO { get; set; }
        public int[] ZenCoreMap { get; set; }
        public string ZenCoreMapLabel { get; set; }
        public int[] CPPCTags { get; set; }
        public double WinMaxSize { get; set; }
        public string ZenCOLabel { get; set; }
        public string CPUSensorsSource { get; set; }
        public bool Zen1X { get; set; }
        public bool ZenPlus { get; set; }
        public bool Zen3 { get; set; }
        public bool Zen4 { get; set; }
        public string ZenCpuTemp { get; set; }
        public string ZenCcd1Temp { get; set; }
        public string ZenCcd2Temp { get; set; }
        public string ZenCpuVcore { get; set; }
        public string ZenCpuVsoc { get; set; }

        public bool IntelAVX512 { get; set; }
        public bool IntelHybrid { get; set; }
        public List<int> Pcores { get; set; }
        public List<int> Ecores { get; set; }
        public List<int> Plogicals { get; set; }
        public List<int> Elogicals { get; set; }

        private int[] cpuTload;
        public int[] CpuTload { get { return cpuTload; } }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        public AsusWMI AsusWmi = new AsusWMI();

        public MemoryConfig MEMCFG = new MemoryConfig();

        public List<MemoryModule> modules = new List<MemoryModule>();

        public List<BiosACPIFunction> biosFunctions = new List<BiosACPIFunction>();

        public BiosMemController BMC;
        public List<String> MemPartNumbers { get; set; }
        public string MemVdimm { get; set; }
        public string MemVtt { get; set; }
        public string MemProcODT { get; set; }
        public string MemClkDrvStren { get; set; }
        public string MemAddrCmdDrvStren { get; set; }
        public string MemCsOdtCmdDrvStren { get; set; }
        public string MemCkeDrvStren { get; set; }

        public string MemRttNom { get; set; }
        public string MemRttWr { get; set; }
        public string MemRttPark { get; set; }

        public string MemAddrCmdSetup { get; set; }
        public string MemCsOdtSetup { get; set; }
        public string MemCkeSetup { get; set; }
        public string MemVddio { get; set; }
        public string MemVddq { get; set; }
        public string MemVpp { get; set; }
        public string MemCadBusDrvStren { get; set; }
        public string MemDramDataDrvStren { get; set; }
        public string MemProcDataDrvStren { get; set; }
        public string MemRttWrD5 { get; set; }
        public string MemRttNomWr { get; set; }
        public string MemRttNomRd { get; set; }
        public string MemRttParkD5 { get; set; }
        public string MemRttParkDqs { get; set; }
        public string CpuVtt { get; set; }
        public double CpuBusClock { get; set; }
        public bool CpuSHAExt { get; set; }
        public bool CpuVAESExt { get; set; }
        public double TBLoopTime { get; set; }
        public double TBLoopEvery { get; set; }

        public string ThreadBoosterStatus { get; set; }
        public string ThreadBoosterButton { get; set; }
        public string SleepAllowed { get; set; }
        public string HiberAllowed { get; set; }

        public string PSABias { get; set; }
        public string PSAStatus { get; set; }
        public string SSHStatus { get; set; }
        public string N0Status { get; set; }
        public string ToggleSSH { get; set; }
        public string TogglePSA { get; set; }
        public string ToggleN0 { get; set; }
        public string LiveCPUTemp { get; set; }
        public string LiveCPUClock { get; set; }
        public string LiveCPUPower { get; set; }
        public string LiveCPUAdditional { get; set; }
        public string LiveFinished { get; set; }
        public string LiveCpuLoad { get; set; }


        private int EmptyTags()
        {
            int _remaining = CPUCores;
            for (int i = 0; i <= CPUCores - 1; i++)
            {
                if (CPPCTags[i] > 0) _remaining--;
            }
            return _remaining;
        }
        private bool CPPCFoundAlready(int _core)
        {
            for (int i = 0; i < CPPC.GetLength(0); i++)
            {
                if (CPPC[i, 0] == _core) return true;
            }
            return false;
        }

        private int[] RemoveIndices(int[] IndicesArray, int RemoveAt)
        {
            int[] newIndicesArray = new int[IndicesArray.Length - 1];

            int i = 0;
            int j = 0;
            while (i < IndicesArray.Length)
            {
                if (i != RemoveAt)
                {
                    newIndicesArray[j] = IndicesArray[i];
                    j++;
                }

                i++;
            }

            return newIndicesArray;
        }

        public static IEnumerable<T[]> Filter<T>(T[,] source, Func<T[], bool> predicate)
        {
            for (int i = 0; i < source.GetLength(0); ++i)
            {
                T[] values = new T[source.GetLength(1)];
                for (int j = 0; j < values.Length; ++j)
                {
                    values[j] = source[i, j];
                }
                if (predicate(values))
                {
                    yield return values;
                }
            }
        }
        public SystemInfo()
        {
            CPUDescription = "N/A";
            CPUName = "N/A";
            CPUCores = ProcessorInfo.PhysicalCoresCount;
            CPUEnabledCores = ProcessorInfo.PhysicalCoresCount;
            CPUThreads = ProcessorInfo.LogicalCoresCount;
            CPUSocket = "N/A";
            CPULogicalProcessors = ProcessorInfo.LogicalCoresCount;
            PPlanGuid = new Guid("175B2BEF-94E4-43FF-B545-BD1F233E48BD");
            PPlanLabel = "";

            ToggleN0 = "Toggle NumaZero";
            TogglePSA = "Toggle PowerSaverActive";
            ToggleSSH = "Toggle SysSetHack";

            cpuTload = new int[ProcessorInfo.LogicalCoresCount];

            CPUFamily = 0;
            BoardBIOS = "N/A";
            BoardManufacturer = "N/A";
            MemoryLabel = "";
            BoardModel = "N/A";
            CPPCTagsLabel = "";
            WindowsLabel = "";
            HyperThreading = false;
            CpuVtt = "";
            CpuBusClock = 0;

            MemPartNumbers = new();
            MemVdimm = "";
            MemVtt = "";
            MemProcODT = "";
            MemClkDrvStren = "";
            MemAddrCmdDrvStren = "";
            MemCsOdtCmdDrvStren = "";
            MemCkeDrvStren = "";

            MemRttNom = "";
            MemRttWr = "";
            MemRttPark = "";

            MemAddrCmdSetup = "";
            MemCsOdtSetup = "";
            MemCkeSetup = "";

            MemVddio = "";
            MemVddq = "";
            MemVpp = "";
            MemCadBusDrvStren = "";
            MemDramDataDrvStren = "";
            MemProcDataDrvStren = "";
            MemRttWrD5 = "";
            MemRttNomWr = "";
            MemRttNomRd = "";
            MemRttParkD5 = "";
            MemRttParkDqs = "";

            LastVersionOnServer = "N/A";

            CPPCOrder = new int[CPUCores];
            CPPCOrder1 = new int[CPUCores];
            CPPCTags = new int[CPUCores];

            IntelHybrid = false;
            Pcores = new();
            Ecores = new();
            Plogicals = new();
            Elogicals = new();

            CpuSHAExt = false;
            CpuVAESExt = false;

            ZenStates = false;
            ZenPerCCDTemp = false;
            ZenBoost = 0;
            ZenScalar = 0;
            ZenPPT = 0;
            ZenTDC = 0;
            ZenEDC = 0;
            ZenTHM = 0;
            ZenStartBoost = 0;
            ZenStartPPT = 0;
            ZenStartTDC = 0;
            ZenStartEDC = 0;
            ZenStartTHM = 0;
            ZenMaxBoost = 0;
            ZenMaxPPT = 0;
            ZenMaxTDC = 0;
            ZenMaxEDC = 0;
            ZenMaxTHM = 0;
            ZenFCLK = 0;
            ZenUCLK = 0;
            ZenMCLK = 0;
            ZenVDDP = 0;
            ZenVDDG = 0;
            ZenVCCD = 0;
            ZenVIOD = 0;
            ZenCCDTotal = 0;
            ZenCCXTotal = 0;
            ZenCoresPerCCX = 0;
            ZenCO = new int[CPUCores];
            ZenCoreMap = new int[CPUCores];
            ZenCOb = false;
            ZenCOLabel = "";
            ZenSMUVer = "N/A";
            ZenPTVersion = 0;
            ZenCoreMapLabel = "";
            Zen1X = false;
            ZenPlus = false;
            Zen3 = false;
            Zen4 = false;
            LiveCPUTemp = "N/A";
            LiveCPUClock = "N/A";
            LiveCPUPower = "N/A";
            LiveCPUAdditional = "N/A";

            WinMaxSize = 600;

            bECores = true;

            try
            {
                PPlanGuid = App.powerManager.GetActiveGuid();
                PPlanLabel = App.powerManager.GetActivePlanFriendlyName();

                CPUSensorsSource = "LibreHardwareMonitor";
                HWMonitor.CPUSource = HWSensorSource.Libre;
                HWMonitor.NewSensors();

                SetProgress(12, "WMI basics query");

                App.LogInfo("SystemInfo: WMI basics query");

                WmiBasics();

                if (CPULogicalProcessors > CPUCores) HyperThreading = true;

                App.LogInfo("SystemInfo: CPPC Tags Init");

                CPPCTagsInit();

                SetProgress(15, "CPUID Init");

                App.LogInfo("SystemInfo: CPUID Init");

                CpuIdInit();

                SetProgress(40, "CPUSet Init");

                App.LogInfo("SystemInfo: CPUSet Init");

                CpuSetInit();

                SetProgress(65, "OS Info Init");

                App.LogInfo("SystemInfo: Windows OS Info Init");

                GetWindowsLabel();

                SetProgress(70, "Zen Init");

                App.LogInfo($"SystemInfo: ZenMain Init dllDisable={App.inpoutdlldisable}");

                if (!App.inpoutdlldisable) ZenMainInit();


                if (!MemPartNumbers.Any())
                {
                    if (HWMonitor.computer.SMBios.MemoryDevices.Length > 0)
                    {
                        foreach (var module in HWMonitor.computer.SMBios.MemoryDevices)
                        {
                            if (module.Size > 0)
                            {
                                MemPartNumbers.Add(
                                    $"{module.BankLocator}: {module.PartNumber} ({module.Size / 1024}GB, {module.Speed}MHz)");
                            }
                        }
                    }
                }
                HWMonitor.computer.Close();
                HWMonitor.computer = null;
            }
            catch (Exception ex)
            {
                App.LogExError($"SystemInfo Exception: {ex.Message}", ex);
            }
            finally
            {
                RefreshLabels();
            }
        }
        static int CountSetBits(uint n)
        {
            uint count = 0;
            while (n > 0)
            {
                count += n & 1;
                n >>= 1;
            }
            return (int)count;
        }
        static uint BitSlice(uint arg, int start, int end)
        {
            uint mask = (2U << end - start) - 1U;
            return arg >> start & mask;
        }
        public bool ZenRefreshStatic(bool refresh)
        {
            if (!ZenStates || App.ZenBlockRefresh || Zen == null) return false;

            if (refresh)
            {
                bool _refreshpt = ZenRefreshPowerTable();

                if (!_refreshpt) return false;
            }

            double? _busClock = Zen.GetBclk();
            CpuBusClock = _busClock != null ? (double)_busClock : CpuBusClock;

            int _zenPPT = Zen.GetPPTLimit();

            ZenBoost = Zen.GetBoostLimit(0);

            if (ZenPTVersion == 0x380804)
            {
                ZenPPT = _zenPPT > 0 ? _zenPPT : (int)Zen.powerTable.Table[0];
                ZenTDC = (int)Zen.powerTable.Table[2];
                ZenTHM = (int)Zen.powerTable.Table[4];
                ZenEDC = (int)Zen.powerTable.Table[8];
                ZenFCLK = (int)Zen.powerTable.Table[74];
                ZenUCLK = (int)Zen.powerTable.Table[78];
                ZenMCLK = (int)Zen.powerTable.Table[82];
                ZenScalar = (int)GetPBOScalar();
                ZenVDDP = (int)(Zen.powerTable.Table[137] * 1000);
                ZenVIOD = (int)(Zen.powerTable.Table[138] * 1000);
                ZenVCCD = (int)(Zen.powerTable.Table[139] * 1000);
            }
            else if (ZenPTVersion == 0x380904)
            {
                ZenPPT = _zenPPT > 0 ? _zenPPT : (int)Zen.powerTable.Table[0];
                ZenTDC = (int)Zen.powerTable.Table[2];
                ZenTHM = (int)Zen.powerTable.Table[4];
                ZenEDC = (int)Zen.powerTable.Table[8];
                ZenFCLK = (int)Zen.powerTable.Table[74];
                ZenUCLK = (int)Zen.powerTable.Table[78];
                ZenMCLK = (int)Zen.powerTable.Table[82];
                ZenScalar = (int)GetPBOScalar();
                ZenVDDP = (int)(Zen.powerTable.Table[137] * 1000);
                ZenVIOD = (int)(Zen.powerTable.Table[138] * 1000);
                ZenVCCD = (int)(Zen.powerTable.Table[139] * 1000);
            }
            else if (ZenPTVersion == 0x380805)
            {
                ZenPPT = _zenPPT > 0 ? _zenPPT : (int)Zen.powerTable.Table[0];
                ZenTDC = (int)Zen.powerTable.Table[2];
                ZenTHM = (int)Zen.powerTable.Table[4];
                ZenEDC = (int)Zen.powerTable.Table[8];
                ZenFCLK = (int)Zen.powerTable.Table[74];
                ZenUCLK = (int)Zen.powerTable.Table[78];
                ZenMCLK = (int)Zen.powerTable.Table[82];
                ZenScalar = (int)GetPBOScalar();
                ZenVDDP = (int)(Zen.powerTable.Table[137] * 1000);
                ZenVIOD = (int)(Zen.powerTable.Table[138] * 1000);
                ZenVCCD = (int)(Zen.powerTable.Table[139] * 1000);
            }
            else if (ZenPTVersion == 0x380905)
            {
                ZenPPT = _zenPPT > 0 ? _zenPPT : (int)Zen.powerTable.Table[0];
                ZenTDC = (int)Zen.powerTable.Table[2];
                ZenTHM = (int)Zen.powerTable.Table[4];
                ZenEDC = (int)Zen.powerTable.Table[8];
                ZenFCLK = (int)Zen.powerTable.Table[74];
                ZenUCLK = (int)Zen.powerTable.Table[78];
                ZenMCLK = (int)Zen.powerTable.Table[82];
                ZenScalar = (int)GetPBOScalar();
                ZenVDDP = (int)(Zen.powerTable.Table[137] * 1000);
                ZenVIOD = (int)(Zen.powerTable.Table[138] * 1000);
                ZenVCCD = (int)(Zen.powerTable.Table[139] * 1000);
            }
            else if (ZenPTVersion == 0x540100
                || ZenPTVersion == 0x540101
                || ZenPTVersion == 0x540102
                || ZenPTVersion == 0x540103
                || ZenPTVersion == 0x540104
                || ZenPTVersion == 0x540105
                || ZenPTVersion == 0x540000
                || ZenPTVersion == 0x540001
                || ZenPTVersion == 0x540002
                || ZenPTVersion == 0x540003
                || ZenPTVersion == 0x540004
                || ZenPTVersion == 0x540005)
            {
                ZenPPT = _zenPPT > 0 ? _zenPPT : (int)Zen.powerTable.Table[2];
                ZenTDC = (int)Zen.powerTable.Table[8];
                ZenTHM = (int)Zen.powerTable.Table[10];
                ZenEDC = (int)Zen.powerTable.Table[61];
                ZenFCLK = (int)Zen.powerTable.Table[70];
                ZenUCLK = (int)Zen.powerTable.Table[74];
                ZenMCLK = (int)Zen.powerTable.Table[78];
                ZenScalar = (int)GetPBOScalar();
                ZenVDDP = (int)(Zen.powerTable.Table[268] * 1000);
            }
            else if (ZenPTVersion == 0x400004 || ZenPTVersion == 0x400005)
            {
                ZenPPT = _zenPPT > 0 ? _zenPPT : (int)Zen.powerTable.Table[4];
                ZenTDC = (int)Zen.powerTable.Table[8];
                ZenTHM = (int)Zen.powerTable.Table[16];
                ZenEDC = (int)Zen.powerTable.Table[12];
                ZenFCLK = (int)Zen.powerTable.Table[409];
                ZenUCLK = (int)Zen.powerTable.Table[410];
                ZenMCLK = (int)Zen.powerTable.Table[411];
                ZenScalar = (int)GetPBOScalar();
                ZenVDDP = (int)(Zen.powerTable.Table[565] * 1000);
            }
            else if (ZenPTVersion == 0x240903)
            {
                ZenPPT = _zenPPT > 0 ? _zenPPT : (int)Zen.powerTable.Table[0];
                ZenTDC = (int)Zen.powerTable.Table[2];
                ZenTHM = (int)Zen.powerTable.Table[4];
                ZenEDC = (int)Zen.powerTable.Table[8];
                ZenFCLK = (int)Zen.powerTable.Table[70];
                ZenUCLK = (int)Zen.powerTable.Table[74];
                ZenMCLK = (int)Zen.powerTable.Table[78];
                ZenScalar = (int)GetPBOScalar();
                ZenVDDP = (int)(Zen.powerTable.Table[125] * 1000);
                ZenVDDG = (int)(Zen.powerTable.Table[126] * 1000);
            }
            else if (ZenPTVersion == 0x240803)
            {
                ZenPPT = _zenPPT > 0 ? _zenPPT : (int)Zen.powerTable.Table[0];
                ZenTDC = (int)Zen.powerTable.Table[2];
                ZenTHM = (int)Zen.powerTable.Table[4];
                ZenEDC = (int)Zen.powerTable.Table[8];
                ZenFCLK = (int)Zen.powerTable.Table[70];
                ZenUCLK = (int)Zen.powerTable.Table[74];
                ZenMCLK = (int)Zen.powerTable.Table[78];
                ZenScalar = (int)GetPBOScalar();
                ZenVDDP = (int)(Zen.powerTable.Table[125] * 1000);
                ZenVDDG = (int)(Zen.powerTable.Table[126] * 1000);
            }
            else if (ZenSMUVer == "25.86.0")
            {
                if (Zen1X || ZenPlus)
                {
                    ZenPPT = (int)Zen.powerTable.Table[0];
                    ZenTDC = (int)Zen.powerTable.Table[2];
                }
                if (ZenPlus)
                {
                    ZenEDC = (int)Zen.powerTable.Table[8];
                }
                ZenTHM = (int)Zen.powerTable.Table[4];
                ZenFCLK = (int)Zen.powerTable.Table[33];
                ZenUCLK = (int)Zen.powerTable.Table[33];
                ZenMCLK = (int)Zen.powerTable.Table[33];
            }

            if (ZenFCLK > 0 && CpuBusClock > 0) ZenFCLK = Math.Round(ZenFCLK / 100 * CpuBusClock, 0);
            if (ZenUCLK > 0 && CpuBusClock > 0) ZenUCLK = Math.Round(ZenUCLK / 100 * CpuBusClock, 0);
            if (ZenMCLK > 0 && CpuBusClock > 0) ZenMCLK = Math.Round(ZenMCLK / 100 * CpuBusClock, 0);

            if (ZenCOb) ZenRefreshCO();

            ZenRefreshSensors();

            //App.LogDebug($"ZenRefreshStatic done");
            return true;
        }

        public void ZenRefreshSensors()
        {
            Zen.RefreshSensors();

            ZenCpuTemp = Zen.cpuTemp != null ? $"{Zen.cpuTemp:F1}°C" : "";
            ZenCcd1Temp = Zen.ccd1Temp != null ? $"{Zen.ccd1Temp:F1}°C" : "";
            ZenCcd2Temp = Zen.ccd2Temp != null ? $"{Zen.ccd2Temp:F1}°C" : "";
            ZenCpuVcore = Zen.cpuVcore != null ? $"{Zen.cpuVcore:F3}V" : "";
            ZenCpuVsoc = Zen.cpuVsoc != null ? $"{Zen.cpuVsoc:F3}V" : "";

            if (App.MainWindowOpen)
            {
                OnChange("ZenCpuTemp");
                OnChange("ZenCcd1Temp");
                OnChange("ZenCcd2Temp");
                OnChange("ZenCpuVcore");
                OnChange("ZenCpuVsoc");
            }
        }
        public bool ZenRefreshPowerTable()
        {
            try
            {
                App.LogDebug("ZenRefreshPowerTable...");

                SMU.Status? status = SMU.Status.UNKNOWN_CMD;

                if (Ring0.WaitPciBusMutex(50))
                {
                    status = Zen.RefreshPowerTable();
                    Ring0.ReleasePciBusMutex();
                }

                if (status != SMU.Status.OK)
                {
                    App.LogDebug("ZenRefreshPowerTable retry");

                    for (int r = 0; r < 10; ++r)
                    {
                        if (r > 0) App.LogDebug($"ZenRefreshPowerTable retry n.{r}");
                        Thread.Sleep(25);
                        if (Ring0.WaitPciBusMutex(50))
                        {
                            status = ZenRefreshPowerTable2();
                            Ring0.ReleasePciBusMutex();
                        }
                        if (status == SMU.Status.OK) r = 99;
                    }
                }

                if (status == SMU.Status.OK) App.LogDebug("ZenRefreshPowerTable OK");
                if (status == SMU.Status.OK) return true;
                return false;
            }
            catch (Exception ex)
            {
                App.LogExError($"ZenRefreshPowerTable Exception: {ex.Message}", ex);
                return false;
            }

        }
        public SMU.Status ZenRefreshPowerTable2()
        {
            try
            {
                SMU.Status status = Zen.RefreshPowerTable();

                if (status != SMU.Status.OK)
                {
                    for (int r = 0; r < 10; ++r)
                    {
                        status = Zen.RefreshPowerTable();
                        if (status == SMU.Status.OK) r = 99;
                    }
                }

                return status;
            }
            catch (Exception ex)
            {
                App.LogExError($"ZenRefreshPowerTable2 Exception: {ex.Message}", ex);
                return SMU.Status.FAILED;
            }

        }
        public uint[] MakeCmdArgs(uint[] args)
        {
            uint[] cmdArgs = new uint[6];
            int length = args.Length > 6 ? 6 : args.Length;

            for (int i = 0; i < length; i++)
                cmdArgs[i] = args[i];

            return cmdArgs;
        }
        public uint[] MakeCmdArgs(uint arg = 0)
        {
            return MakeCmdArgs(new uint[1] { arg });
        }
        private uint MakePsmMarginArg(int margin)
        {
            int _maxm = Zen4 ? 60 : 30;
            int _minm = Zen4 ? -60 : -30;
            if (margin > _maxm)
                margin = _maxm;
            else if (margin < _minm)
                margin = _minm;

            int offset = margin < 0 ? 0x100000 : 0;
            return Convert.ToUInt32(offset + margin) & 0xffff;
        }
        private int MakePsmCount(int margin)
        {
            int _maxm = Zen4 ? 30 : 30;
            int _minm = Zen4 ? -16959 : -30;
            if (margin > _maxm)
                margin = _maxm;
            else if (margin < _minm)
                margin = _minm;

            return margin;
        }

        public int GetPBOScalar()
        {
            /*
            int _wait = Zen.GetOcMode() ? 50 : 10;
            float _scalar = 0;
            if (Ring0.WaitPciBusMutex(_wait))
            {
                if (!Zen.GetOcMode())
                {
                    _scalar = Zen.GetPBOScalar();
                }
                else
                {
                    _scalar = 0f;
                }
                Ring0.ReleasePciBusMutex();
            }
            return _scalar;
            */
            return Zen.GetPBOScalar();
        }
        public int? GetPsmCount(int core)
        {
            /*
            int _wait = Zen.GetOcMode() ? 50 : 10;

            if (Ring0.WaitPciBusMutex(_wait))
            {
                int? _count = Zen.GetPsmMarginSingleCore(Zen.GetCoreMask(core));
                Ring0.ReleasePciBusMutex();
                return _count;
            }
            return null;
            */
            int? _count = Zen.GetPsmMarginSingleCore(Zen.GetCoreMask(core));
            return _count;
        }
        public bool SetPsmCount(int core, int margin)
        {
            /*
            int _wait = Zen.GetOcMode() ? 50 : 10;

            if (Ring0.WaitPciBusMutex(_wait))
            {
                bool status = Zen.SetPsmMarginSingleCore(Zen.GetCoreMask(core), MakePsmCount(margin));
                Ring0.ReleasePciBusMutex();
                if (status) return true;
            }
            */
            bool status = Zen.SetPsmMarginSingleCore(Zen.GetCoreMask(core), MakePsmCount(margin));
            if (status) return true;
            return false;
        }
        public bool SetPsmCounts(int margin)
        {
            /*
            int _wait = Zen.GetOcMode() ? 50 : 10;

            if (Ring0.WaitPciBusMutex(_wait))
            {
                bool status = Zen.SetPsmMarginAllCores(MakePsmCount(margin));
                Ring0.ReleasePciBusMutex();
                if (status) return true;
            }
            */
            bool status = Zen.SetPsmMarginAllCores(MakePsmCount(margin));
            if (status) return true;
            return false;
        }

        public void ZenRefreshCO()
        {
            try
            {
                if ((Zen.smu.SMU_TYPE == SMU.SmuType.TYPE_CPU3 || Zen.smu.SMU_TYPE == SMU.SmuType.TYPE_CPU4) 
                    && CPUCores <= ZenCoreMap.Length 
                    && (Zen.smu.Rsmu.SMU_MSG_GetDldoPsmMargin != 0x0 || Zen.smu.Mp1Smu.SMU_MSG_GetDldoPsmMargin != 0x0
                    && Zen.info.topology.coreFullMap != null
                    && Zen.info.topology.coreFullMap.GetLength(0) >= CPUCores ))
                {
                    ZenCOLabel = "";
                    for (int ix = 0; ix < CPUCores; ix++)
                    {
                        int? count = GetPsmCount(ZenCoreMap[ix]);
                        ZenCO[ix] = count != null ? (int)count : 0;
                    }
                    int ccd = 0, _ccd;
                    for (int ic = 0; ic < CPUCores; ic++)
                    {
                        _ccd = (int)Zen.info.topology.coreFullMap[ic, 2];
                        if (ccd != _ccd)
                        {
                            ccd++;
                            ZenCOLabel = ccd == 1 ? $"CCD{_ccd}: {ZenCOLabel}" : $"{ZenCOLabel}\nCCD{_ccd}: ";
                        }
                        ZenCOLabel += String.Format("{0,3}#{1,-4}", $"C{ic}", ZenCO[ic].ToString("+#;-#;+0"));
                    }
                    ZenCOb = true;
                    //App.LogDebug($"ZenRefreshCO: {string.Join(", ", ZenCO)}");
                    OnChange("ZenCOLabel");
                }
            }
            catch (Exception ex)
            {
                ZenCOb = false;
                ZenCOLabel = "";
                App.LogExError($"ZenRefreshCO Exception: {ex.Message}", ex);
            }
        }
        public void RefreshLabels()
        {
            try
            {
                CPULabel = $"{CPUName} [Socket {CPUSocket}]\n{CPUDescription} x{CPUBits}";
                BoardLabel = $"{BoardManufacturer}\n{BoardModel}";
                    if (BoardBIOS.Length > 0) BoardLabel += $" [BIOS Version {BoardBIOS}]";

                if (WindowsLabel.Length > 0)
                    SystemLabel = $"{WindowsLabel}";
                SystemLabel = SystemLabel.Length > 0 ? SystemLabel + "\n" : "";

                if (App.powerManager.GetActiveGuid() != PPlanGuid)
                {
                    PPlanLabel = App.powerManager.GetActivePlanFriendlyName();
                    PPlanGuid = App.powerManager.GetActiveGuid();
                }
                if (PPlanLabel.Length > 0)
                    SystemLabel += $"{PPlanLabel}";

                ProcessorsLabel = $"{CPUCores}";
                if (HyperThreading) ProcessorsLabel += $" [Threads: {CPULogicalProcessors}]";
                if (IntelHybrid)
                {
                    ProcessorsLabel += $" P-Cores: {Pcores.Count}";
                    if (Plogicals.Count > Pcores.Count) ProcessorsLabel += $" [{Plogicals.Count}T]";
                    ProcessorsLabel += $" E-Cores: {Ecores.Count}";
                    if (Elogicals.Count > Ecores.Count) ProcessorsLabel += $" [{Elogicals.Count}T]";
                }

                string _MemoryLabel1 = "";
                string _MemoryLabel2 = "";
                string _MemoryLabel3 = "";

                bool _done = false;

                if (MemPartNumbers.Any())
                {
                    foreach (var mempart in MemPartNumbers)
                    {
                        if (_MemoryLabel1.Length > 0 && mempart.Length > 0) _MemoryLabel1 += $"\n";
                        _MemoryLabel1 += $"{mempart}";
                        if (MEMCFG.FrequencyString != null)
                            if (MEMCFG.FrequencyString.Length > 0 && !_done) { _MemoryLabel1 += $" Clock: {MEMCFG.FrequencyString} MHz"; _done = true; }
                    }
                }

                if (MemVdimm.Length > 0)
                {
                    _MemoryLabel2 += $"VDIMM: {MemVdimm}";
                    if (MemVtt.Length > 0)
                        _MemoryLabel2 += $" VTT: {MemVtt}";
                }

                if (ZenStates && (MEMCFG.Type == MemType.DDR4 || MEMCFG.Type == MemType.DDR5))
                {
                    if (MEMCFG.Type == MemType.DDR4)
                    {
                        _MemoryLabel3 += $"RTT [Nom: {MemRttNom} Wr: {MemRttWr} Park: {MemRttPark}] pODT: {MemProcODT}";
                    }

                    if (MEMCFG.Type == MemType.DDR5)
                    {
                        _MemoryLabel3 += $"RTT [NomRd: {MemRttNomRd} NomWr: {MemRttNomWr} Wr: {MemRttWrD5} Park: {MemRttParkD5} ParkDqs: {MemRttParkDqs}] pODT: {MemProcODT} ";
                        _MemoryLabel3 += $"\nVDDIO: {MemVddio} VDDQ: {MemVddq} VPP: {MemVpp}";
                    }
                }

                MemoryLabel = _MemoryLabel1;
                if (_MemoryLabel1.Length > 0 && _MemoryLabel2.Length > 0) MemoryLabel += "\n";
                if (_MemoryLabel2.Length > 0) MemoryLabel += $"{_MemoryLabel2}";

                if (_MemoryLabel3.Length > 0 && MemoryLabel.Length > 0) MemoryLabel += "\n";
                if (_MemoryLabel3.Length > 0) MemoryLabel += $"{_MemoryLabel3}";

                if (MemoryLabel.Length == 0) MemoryLabel = "N/A";

                if (App.inpoutdlldisable == false)  
                    {                    

                    if (object.ReferenceEquals(null, Zen)) ZenInit();

                    if (ZenStates)
                    {

                        for (int r = 0; r < 20; ++r)
                        {
                            bool _ok = ZenRefreshStatic(true);
                            r = _ok ? 99 : r;
                            Thread.Sleep(25);
                        }

                        string _CPULabel = "";
                        if (ZenPPT > 0 && ZenMaxPPT > 0 && ZenPPT != ZenMaxPPT)
                        {
                            _CPULabel += $"PPT: {string.Format("{0:D0}/{1:D0}W", ZenPPT, ZenMaxPPT)} ";
                        } 
                        else if (ZenPPT > 0)
                        {
                            _CPULabel += $"PPT: {string.Format("{0:D0}W", ZenPPT)} ";
                        }
                        if (Zen.powerTable.TDC > 0 && ZenMaxTDC > 0 && Zen.powerTable.TDC != ZenMaxTDC)
                        {
                            _CPULabel += $"TDC: {string.Format("{0:D0}/{1:D0}A", Zen.powerTable.TDC, ZenMaxTDC)} ";
                        }
                        else if (Zen.powerTable.TDC > 0)
                        {
                            _CPULabel += $"TDC: {string.Format("{0:D0}A", Zen.powerTable.TDC)} ";
                        }
                        if (Zen.powerTable.EDC > 0 && ZenMaxEDC > 0 && Zen.powerTable.EDC != ZenMaxEDC)
                        {
                            _CPULabel += $"EDC: {string.Format("{0:D0}/{1:D0}A", Zen.powerTable.EDC, ZenMaxEDC)} ";
                        }
                        else if (Zen.powerTable.EDC > 0)
                        {
                            _CPULabel += $"EDC: {string.Format("{0:D0}A", Zen.powerTable.EDC)} ";
                        }
                        if (ZenScalar > 0) _CPULabel += $"Scalar: {ZenScalar}x ";
                        if (Zen.powerTable.THM > 0 && ZenMaxTHM > 0 && Zen.powerTable.THM != ZenMaxTHM)
                        {
                            _CPULabel += $"THM: {string.Format("{0:D0}/{1:D0}°C", Zen.powerTable.THM, ZenMaxTHM)} ";
                        }
                        else if (Zen.powerTable.THM > 0)
                        {
                            _CPULabel += $"THM: {string.Format("{0:D0}°C", Zen.powerTable.THM)} ";
                        }

                        if (_CPULabel.Length > 0) CPULabel += $"\n{_CPULabel}";

                        _CPULabel = "";

                        if (ZenMCLK > 0 || ZenFCLK > 0 || ZenUCLK > 0) _CPULabel += $"MCLK/FCLK/UCLK: {ZenMCLK.ToString("0.##")}/{ZenFCLK.ToString("0.##")}/{ZenUCLK.ToString("0.##")} ";
                        if (ZenBoost > 0 && ZenMaxBoost > 0 && ZenBoost != ZenMaxBoost)
                        {
                            _CPULabel += $"Boost Clock: {ZenBoost}/{ZenMaxBoost} MHz ";
                        }
                        else if (ZenBoost > 0)
                        {
                            _CPULabel += $"Boost Clock: {ZenBoost} MHz ";
                        }

                        if (_CPULabel.Length > 0) CPULabel += $"\n{_CPULabel}";

                        _CPULabel = "";

                        if (ZenVDDP > 0) _CPULabel += $"VDDP: {ZenVDDP}mV ";
                        if (ZenVDDG > 0) _CPULabel += $"VDDG: {ZenVDDG}mV ";
                        if (ZenVCCD > 0) _CPULabel += $"VDDG CCD: {ZenVCCD}mV ";
                        if (ZenVIOD > 0) _CPULabel += $"VDDG IOD: {ZenVIOD}mV ";

                        if (_CPULabel.Length > 0) CPULabel += $"\n{_CPULabel}";

                        _CPULabel = "";
                        string _SMUVer = ZenSMUVer;

                        if (CpuVtt.Length > 0) _CPULabel += $"VDD18: {CpuVtt} ";
                        if (ZenSMUVer.Length > 0) {
                            if (ZenSMUVer != "N/A") _SMUVer = $"v{ZenSMUVer}"; 
                            _CPULabel += $"SMU: {_SMUVer} ";
                        }
                        if (ZenPTVersion > 0) _CPULabel += $"PT: 0x{ZenPTVersion:X} ";

                        if (_CPULabel.Length > 0) CPULabel += $"\n{_CPULabel}";

                        if (ZenCoreMapLabel.Length > 0) ProcessorsLabel += $"\nCoreMap: {ZenCoreMapLabel} ";
                    }
                }

                OnChange("CPULabel");
                OnChange("BoardLabel");
                OnChange("SystemLabel");
                OnChange("ProcessorsLabel");
                OnChange("MemoryLabel");
                //App.LogDebug($"RefreshLabels done");
            }
            catch (Exception ex)
            {
                App.LogExError($"RefreshLabels Exception: {ex.Message}", ex);
            }

        }
        public void WmiBasics()
        {
            try
            {
                string ClassName = "Win32_BIOS";

                ManagementClass SIManagementClass = new ManagementClass(ClassName);
                //Create a ManagementObjectCollection to loop through
                ManagementObjectCollection SIManagemenobjCol = SIManagementClass.GetInstances();
                //Get the properties in the class
                PropertyDataCollection SIproperties = SIManagementClass.Properties;

                foreach (ManagementObject obj in SIManagemenobjCol)
                {
                    foreach (PropertyData property in SIproperties)
                    {
                        try
                        {
                            if (property.Name == "Name" && obj.Properties[property.Name].Value != null)
                            {
                                if (obj.Properties[property.Name].Value.ToString().Length > 0)
                                    BoardBIOS = obj.Properties[property.Name].Value.ToString().Trim();
                            }
                        }
                        catch (Exception ex)
                        {
                            App.LogExError($"WmiBasics Win32_BIOS Exception: {ex.Message}", ex);
                        }
                    }
                }

                SetProgress(12);

                ClassName = "Win32_BaseBoard";

                SIManagementClass = new ManagementClass(ClassName);
                SIManagemenobjCol = SIManagementClass.GetInstances();
                SIproperties = SIManagementClass.Properties;

                foreach (ManagementObject obj in SIManagemenobjCol)
                {
                    foreach (PropertyData property in SIproperties)
                    {
                        try
                        {
                            if (property.Name == "Manufacturer" && obj.Properties[property.Name].Value != null)
                            {
                                if (obj.Properties[property.Name].Value.ToString().Length > 0)
                                    BoardManufacturer = obj.Properties[property.Name].Value.ToString().Trim();
                            }
                            if (property.Name == "Product" && obj.Properties[property.Name].Value != null)
                            {
                                if (obj.Properties[property.Name].Value.ToString().Length > 0)
                                    BoardModel = obj.Properties[property.Name].Value.ToString().Trim();
                            }
                        }
                        catch (Exception ex)
                        {
                            App.LogExError($"WmiBasics Win32_BaseBoard Exception: {ex.Message}", ex);
                        }
                    }
                }

                SetProgress(13);

                ClassName = "Win32_Processor";

                SIManagementClass = new ManagementClass(ClassName);
                SIManagemenobjCol = SIManagementClass.GetInstances();
                SIproperties = SIManagementClass.Properties;

                foreach (ManagementObject obj in SIManagemenobjCol)
                {
                    foreach (PropertyData property in SIproperties)
                    {
                        try
                        {
                            if (property.Name == "AddressWidth" && obj.Properties[property.Name].Value != null)
                            {
                                if (obj.Properties[property.Name].Value.ToString().Length > 0)
                                    CPUBits = obj.Properties[property.Name].Value.ToString().Trim();
                            }
                            if (property.Name == "Description" && obj.Properties[property.Name].Value != null)
                            {
                                if (obj.Properties[property.Name].Value.ToString().Length > 0)
                                    CPUDescription = obj.Properties[property.Name].Value.ToString().Trim();
                            }
                            if (property.Name == "Name" && obj.Properties[property.Name].Value != null)
                            {
                                if (obj.Properties[property.Name].Value.ToString().Length > 0)
                                    CPUName = obj.Properties[property.Name].Value.ToString().Trim();
                            }
                            /*
                            if (property.Name == "NumberOfCores" && obj.Properties[property.Name].Value != null)
                            {
                                if (obj.Properties[property.Name].Value.ToString().Length > 0)
                                    CPUCores = Int32.Parse(obj.Properties[property.Name].Value.ToString().Trim());
                            }
                            */
                            if (property.Name == "NumberOfEnabledCore" && obj.Properties[property.Name].Value != null)
                            {
                                if (obj.Properties[property.Name].Value.ToString().Length > 0)
                                    CPUEnabledCores = Int32.Parse(obj.Properties[property.Name].Value.ToString().Trim());
                            }
                            /*
                            if (property.Name == "NumberOfLogicalProcessors" && obj.Properties[property.Name].Value != null)
                            {
                                if (obj.Properties[property.Name].Value.ToString().Length > 0)
                                    CPUThreads = Int32.Parse(obj.Properties[property.Name].Value.ToString().Trim());
                            }
                            */
                            if (property.Name == "SocketDesignation" && obj.Properties[property.Name].Value != null)
                            {
                                if (obj.Properties[property.Name].Value.ToString().Length > 0)
                                    CPUSocket = obj.Properties[property.Name].Value.ToString().Trim();
                            }
                            /*
                            if (property.Name == "NumberOfLogicalProcessors" && obj.Properties[property.Name].Value != null)
                            {
                                if (obj.Properties[property.Name].Value.ToString().Length > 0)
                                    CPULogicalProcessors = Int32.Parse(obj.Properties[property.Name].Value.ToString().Trim());
                            }
                            */
                            if (property.Name == "Family" && obj.Properties[property.Name].Value != null)
                            {
                                if (obj.Properties[property.Name].Value.ToString().Length > 0)
                                {
                                    CPUFamily = Int32.Parse(obj.Properties[property.Name].Value.ToString().Trim());
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            App.LogExError($"WmiBasics Win32_Processor Exception: {ex.Message}", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.LogExError($"WmiBasics Exception: {ex.Message}", ex);
            }
        }
        public void CPPCTagsInit()
        {
            CPPCTags = new int[CPUCores];

            string eventLogName = "System";

            string evtquery = "*[System/Provider/@Name=\"Microsoft-Windows-Kernel-Processor-Power\"]";

            try
            {
                EventLogQuery elq = new EventLogQuery(eventLogName, PathType.LogName, evtquery);
                elq.Session = new EventLogSession();

                elq.ReverseDirection = true;

                using (EventLogReader elr = new EventLogReader(elq))
                {
                    StringBuilder sb = new StringBuilder();

                    EventRecord ev;
                    while ((ev = elr.ReadEvent()) != null)
                    {
                        if (ev.FormatDescription() != null && ev.Id == 55)
                        {
                            int _procid = 0;
                            int _procgroup = 0;
                            int _procperf = 0;

                            string _rawmessage = ev.FormatDescription();
                            sb.Append(_rawmessage);
                            sb.AppendLine();
                            sb.AppendLine();

                            XDocument doc = XDocument.Parse(ev.ToXml());
                            Dictionary<string, string> dataDictionary = new Dictionary<string, string>();

                            foreach (XElement element in doc.Descendants().Where(p => p.HasElements == false))
                            {
                                int keyInt = 0;
                                string keyName = element.Name.LocalName;
                                while (dataDictionary.ContainsKey(keyName))
                                {
                                    keyName = element.Name.LocalName + "_" + keyInt++;
                                }
                                dataDictionary.Add(keyName, element.Value);

                                if (element.HasAttributes)
                                {
                                    var lmsAttribute = element.FirstAttribute;
                                    if (lmsAttribute != null)
                                    {
                                        dataDictionary.Add($"{keyName}_{lmsAttribute.Name.LocalName}", lmsAttribute.Value);
                                    }
                                }
                            }

                            foreach (KeyValuePair<string, string> kvp in dataDictionary)
                            {
                                sb.AppendLine($"Key = {kvp.Key}, Value = {kvp.Value}");
                            }

                            _procid = Convert.ToInt32(dataDictionary["Data_0"]);
                            _procgroup = Convert.ToInt32(dataDictionary["Data"]);
                            _procperf = Convert.ToInt32(dataDictionary["Data_4"]);

                            sb.AppendLine();
                            sb.Append($"Group: {_procgroup} Processor: {_procid} Processor: {_procperf}");
                            sb.AppendLine();
                            sb.AppendLine();
                            sb.AppendLine();
                            sb.AppendLine();

                            if (_procid == 0 || !HyperThreading || (HyperThreading && (_procid % 2 == 0)))
                            {
                                int __procid = _procid == 0 ? _procid : HyperThreading ? _procid / 2 : _procid;
                                //App.LogInfo($"Add Tag={__procid} {_procperf}");
                                CPPCTags[__procid] = _procperf;
                            }
                            //App.LogInfo($"EmptyTags={EmptyTags()}");

                            if (EmptyTags() <= 0) break;
                        }

                    }

                    for (int i = 0; i < CPPCTags.Length; i++)
                    {
                        CPPCTagsLabel += (CPPCTags.Length > 8 && i == CPPCTags.Length / 2) ? "\n" : "";
                        CPPCTagsLabel += String.Format("{0,3}:{1,-3} ", $"C{i}", CPPCTags[i]);
                    }
                    App.LogInfo($"CPPC: {CPPCTagsLabel}");

                    CPPC = new int[CPUCores, 2];

                    for (int ii = 0; ii < CPPC.GetLength(0); ii++)
                    {
                        CPPC[ii, 0] = -10;
                        CPPC[ii, 1] = -10;
                    }

                    CPPCOrder = new int[CPUCores];
                    CPPCOrder1 = new int[CPUCores];

                    for (int i = 0; i < CPPC.GetLength(0); i++)
                    {
                        int _highestcpu = 0;
                        int _highestperf = 0;
                        for (int ix = CPPC.GetLength(0) - 1; ix > -1; ix--)
                        {
                            //App.LogInfo($"CPPC Testing ix: {ix} perf: {CPPCTags[ix]} IsIn: {CPPCFoundAlready(ix)}");
                            if (_highestperf <= CPPCTags[ix] && !CPPCFoundAlready(ix))
                            {
                                //App.LogInfo($"CPPC Highest ix: {ix} perf: {CPPCTags[ix]}");
                                _highestperf = CPPCTags[ix];
                                _highestcpu = ix;
                            }
                        }
                        CPPCOrder[i] = _highestcpu;
                        CPPCOrder1[i] = _highestcpu + 1;
                        CPPC[i, 0] = _highestcpu;
                        CPPC[i, 1] = _highestperf;

                        string _cppctrace = "";
                        for (int iz = 0; iz < CPPC.GetLength(0); iz++)
                        {
                            _cppctrace += String.Format("{0}#{1} ", CPPC[iz, 0], CPPC[iz, 1]);
                        }
                        //App.LogInfo($"CPPC: {_cppctrace}");

                        //App.LogInfo($"CPPC i: {i} Core: {CPPC[i, 0]} Perf: {CPPC[i, 1]}");
                    }

                    CPPCLabels();

                    string path = @".\Logs\dumpcppc.txt";
                    if (!File.Exists(path)) File.Delete(path);

                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine(sb.ToString());
                    }


                }
            }
            catch (Exception ex)
            {
                CPPCTagsLabel = "Failed parsing CPPC Tags from System Event Log";

                CPPC = new int[CPUCores, 2];
                for (int i = 0; i < CPPC.GetLength(0); i++)
                {
                    CPPC[i, 0] = i;
                    CPPC[i, 1] = 100;
                }
                for (int i = 0; i < CPPC.GetLength(0); i++)
                    CPPCPerfLabel += String.Format("[{0} {1}] ", CPPC[i, 0], CPPC[i, 1]);

                for (int i = 0; i < CPPC.GetLength(0); i++)
                {
                    CPPCLabel += String.Format("{0} ", CPPC[i, 0]);
                    if (i != CPPC.GetLength(0) - 1) CPPCLabel += ", ";

                }

                App.LogExError($"CPPC Tags EventReader Exception: {ex.Message}", ex);
            }
        }

        public void CPPCLabels()
        {
            try
            {
                CPPCLabel = "";
                CPPCPerfLabel = "";

                for (int i = 0; i < CPPC.GetLength(0); i++)
                {
                    CPPCPerfLabel += String.Format("{0}#{1}", CPPC[i, 0], CPPC[i, 1]);
                    if (i != CPPC.GetLength(0) - 1) CPPCPerfLabel += ", ";
                }

                for (int i = 0; i < CPPC.GetLength(0); i++)
                {
                    CPPCLabel += String.Format("{0} ", CPPC[i, 0]);
                    if (i != CPPC.GetLength(0) - 1) CPPCLabel += ", ";

                }
            }
            catch (Exception ex)
            {
                App.LogExError($"CPPCLabels Exception: {ex.Message}", ex);
            }
        }

        public void GetWindowsLabel()
        {
            try
            {
                using (var objOS = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject objMgmt in objOS.Get())
                    {
                        string Win10Prop = "";
                        string WinBuild = "";
                        string WinLabel = "";
                        if (OSVersion.MajorVersion10Properties().DisplayVersion.Length > 0) Win10Prop = $" {OSVersion.MajorVersion10Properties().DisplayVersion}";
                        if (OSVersion.BuildNumber > 0) WinBuild = $" build {OSVersion.BuildNumber}";
                        if (OSVersion.MajorVersion10Properties().UBR.Length > 0) WinBuild += $".{OSVersion.MajorVersion10Properties().UBR}";
                        WinLabel += $"{objMgmt.Properties["Caption"].Value}{Win10Prop}{WinBuild}";
                        WindowsLabel += $"{WinLabel}";
                        App.LogInfo($"OS: {WinLabel} [{OSVersion.GetOSVersion().Version.Major}.{OSVersion.GetOSVersion().Version.Minor}.{OSVersion.GetOSVersion().Version.Build}]");
                    }
                }
            }
            catch (Exception ex)
            {
                App.LogExError($"GetWindowsLabel Exception: {ex.Message}", ex);
            }
        }

        public void CpuIdInit()
        {
            try
            {
                App.LogInfo("");
                App.LogInfo("CpuIdInit");
                App.LogInfo("");

                HWMonitor.computer = new Computer
                {
                    IsCpuEnabled = true,
                    IsGpuEnabled = false,
                    IsMemoryEnabled = false,
                    IsMotherboardEnabled = true,
                    IsControllerEnabled = false,
                    IsNetworkEnabled = false,
                    IsStorageEnabled = false
                };
                HWMonitor.computer.Open();
                HWMonitor.computer.Accept(new UpdateVisitor());

                uint hybridreg = 0x0;
                uint hybridflag = 0x0;
                uint coretypereg = 0x0;
                ulong coretype = 0x0;
                string hybridstr = "";
                string coretypestr = "";
                uint avx512 = 0x0;
                uint avx512reg = 0x0;
                uint shaflag = 0x0;
                uint vaesflag = 0x0;
                string shastr = "";
                string vaesstr = "";
                string avx512str = "No";
                string cpumanufacturer = HWMonitor.computer.SMBios.Processors[0].ManufacturerName;

                App.LogInfo($"CPU Manufacturer: {cpumanufacturer}");
                App.LogInfo("");

                int _deltap = 20 / CPULogicalProcessors;

                for (int j = 0; j < CPULogicalProcessors; j++)
                {
                    SetProgress(16 + _deltap * j);

                    App.LogInfo($" CPU Logical Processor: {j}");

                    LibreHardwareMonitor.Hardware.CPU.CpuId _cpuid = LibreHardwareMonitor.Hardware.CPU.CpuId.Get(0, j);
                    App.LogInfo($" CPUID_0 {_cpuid.Data[0, 0]:X}");
                    App.LogInfo($" CPUID_EXT {_cpuid.ExtData[0, 0]:X}");

                    uint offset = LibreHardwareMonitor.Hardware.CPU.CpuId.CPUID_0;
                    uint offsetext = LibreHardwareMonitor.Hardware.CPU.CpuId.CPUID_EXT;

                    if (_cpuid.Vendor == LibreHardwareMonitor.Hardware.CPU.Vendor.Intel)
                    {
                        try
                        {
                            if (_cpuid.Data.GetLength(0) >= 0x7)
                            {
                                hybridreg = _cpuid.Data[0x7, 3];
                                hybridflag = BitSlice(hybridreg, 15, 15);
                                vaesflag = BitSlice(_cpuid.Data[0x7, 2], 9, 9);
                                shaflag = BitSlice(_cpuid.Data[0x7, 1], 29, 29);
                            }
                            if (_cpuid.Data.GetLength(0) >= 0x1A)
                            {
                                coretypereg = _cpuid.Data[0x1A, 0];
                                coretype = BitSlice(coretypereg, 24, 31);
                            }
                            if (_cpuid.Data.GetLength(0) >= 0xD)
                            {
                                avx512reg = _cpuid.Data[0xD, 3];
                                avx512 = BitSlice(hybridreg, 5, 5);
                            }
                        }
                        catch (Exception ex)
                        {
                            App.LogExWarn($"Error Reading Hybrid/CoreType: {ex.Message}", ex);
                            hybridreg = 3;
                            coretypereg = 0;
                        }
                        switch (hybridflag)
                        {
                            case 0:
                                hybridstr = "No";
                                break;
                            case 1:
                                IntelHybrid = true;
                                hybridstr = "Yes";
                                break;
                            default:
                                hybridstr = "Unknown";
                                break;
                        }
                        switch (coretype)
                        {
                            case 0x20:
                                if (!Ecores.Contains(ProcessorInfo.PhysicalCore(j)))
                                    Ecores.Add(ProcessorInfo.PhysicalCore(j));
                                Elogicals.Add(j + 1);
                                coretypestr = "E-Core";
                                break;
                            case 0x40:
                                if (!Pcores.Contains(ProcessorInfo.PhysicalCore(j)))
                                    Pcores.Add(ProcessorInfo.PhysicalCore(j));
                                Plogicals.Add(j + 1);
                                coretypestr = "P-Core";
                                break;
                            default:
                                coretypestr = "Unknown";
                                break;
                        }
                        switch (avx512)
                        {
                            case 0:
                                avx512str = "No";
                                break;
                            case 1:
                                IntelAVX512 = true;
                                avx512str = "Yes";
                                break;
                            default:
                                avx512str = "Unknown";
                                break;
                        }
                        switch (shaflag)
                        {
                            case 0:
                                shastr = "No";
                                break;
                            case 1:
                                CpuSHAExt = true;
                                shastr = "Yes";
                                break;
                            default:
                                shastr = "Unknown";
                                break;
                        }
                        switch (vaesflag)
                        {
                            case 0:
                                vaesstr = "No";
                                break;
                            case 1:
                                CpuVAESExt = true;
                                vaesstr = "Yes";
                                break;
                            default:
                                vaesstr = "Unknown";
                                break;
                        }
                        App.LogInfo($" Hybrid: [{hybridstr}] CoreType: [{coretypestr}] AVX-512: [{avx512str}] SHA: [{shastr}] VAES: [{vaesstr}]");
                    }
                    if (_cpuid.Vendor == LibreHardwareMonitor.Hardware.CPU.Vendor.AMD)
                    {
                        try
                        {
                            if (_cpuid.Data.GetLength(0) >= 0x7)
                            {
                                if (_cpuid.Family == 0x17 || _cpuid.Family == 0x19)
                                {
                                    shaflag = BitSlice(_cpuid.Data[0x7, 1], 29, 29);
                                }
                                if (_cpuid.Family == 0x19)
                                {
                                    vaesflag = BitSlice(_cpuid.Data[0x7, 2], 9, 9);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            App.LogExWarn($"Error Reading SHA/VAES: {ex.Message}", ex);
                            shaflag = 3;
                            vaesflag = 3;
                        }
                        switch (shaflag)
                        {
                            case 0:
                                shastr = "No";
                                break;
                            case 1:
                                CpuSHAExt = true;
                                shastr = "Yes";
                                break;
                            default:
                                shastr = "Unknown";
                                break;
                        }
                        switch (vaesflag)
                        {
                            case 0:
                                vaesstr = "No";
                                break;
                            case 1:
                                CpuVAESExt = true;
                                vaesstr = "Yes";
                                break;
                            default:
                                vaesstr = "Unknown";
                                break;
                        }
                        App.LogInfo($" Family: {_cpuid.Family:X}h SHA: [{shastr}] VAES: [{vaesstr}]");
                    }
                    App.LogInfo("");
                    App.LogInfo(" Function  EAX       EBX       ECX       EDX");
                    string _line = "";
                    for (int i = 0; i < _cpuid.Data.GetLength(0); i++)
                    {
                        _line = " ";
                        _line += (i + offset).ToString("X8", CultureInfo.InvariantCulture);
                        for (int ij = 0; ij < 4; ij++)
                        {
                            _line += "  ";
                            _line += _cpuid.Data[i, ij].ToString("X8", CultureInfo.InvariantCulture);
                        }
                        App.LogInfo(_line);


                    }
                    App.LogInfo(" Function  EAX       EBX       ECX       EDX");
                    for (int i = 0; i < _cpuid.ExtData.GetLength(0); i++)
                    {
                        _line = " ";
                        _line += (i + offsetext).ToString("X8", CultureInfo.InvariantCulture);
                        for (int ij = 0; ij < 4; ij++)
                        {
                            _line += "  ";
                            _line += _cpuid.ExtData[i, ij].ToString("X8", CultureInfo.InvariantCulture);
                        }
                        App.LogInfo(_line);

                    }
                    App.LogInfo("");

                }

                if (IntelHybrid)
                {
                    string pcoresstr = String.Join(", ", Pcores.ToArray());
                    string plogicalsstr = String.Join(", ", Plogicals.ToArray());
                    string ecoresstr = String.Join(", ", Ecores.ToArray());
                    string elogicalsstr = String.Join(", ", Elogicals.ToArray());
                    App.LogInfo($"P-Cores: {pcoresstr}");
                    App.LogInfo($"P-Logicals: {plogicalsstr}");
                    App.LogInfo($"E-Cores: {ecoresstr}");
                    App.LogInfo($"E-Logicals: {elogicalsstr}");
                }

                App.LogInfo($"Looking for CPU VTT");

                foreach (IHardware hardware in HWMonitor.computer.Hardware)
                {
                    foreach (IHardware subhardware in hardware.SubHardware)
                    {
                        foreach (ISensor sensor in subhardware.Sensors)
                        {
                            if (sensor.Name == "VTT" && hardware.HardwareType == HardwareType.Motherboard && sensor.SensorType == SensorType.Voltage)
                            {
                                if (sensor.Value > 1)
                                {
                                    CpuVtt = $"{Math.Round((decimal)sensor.Value, 4)}V";
                                    App.LogInfo($"Found CPU VTT: {CpuVtt}");
                                }
                                else
                                {
                                    App.LogInfo($"Found CPU VTT below 1V: {Math.Round((decimal)sensor.Value, 4)}");
                                }
                            }
                        }
                    }

                    foreach (ISensor sensor in hardware.Sensors)
                    {
                        if (sensor.Name == "VTT" && hardware.HardwareType == HardwareType.Motherboard && sensor.SensorType == SensorType.Voltage)
                        {
                            if (sensor.Value > 1)
                            {
                                CpuVtt = $"{Math.Round((decimal)sensor.Value, 4)}V";
                                App.LogInfo($"Found CPU VTT: {CpuVtt}");
                            }
                            else
                            {
                                App.LogInfo($"Found CPU VTT below 1V: {Math.Round((decimal)sensor.Value, 4)}");
                            }
                        }
                    }
                }

                App.LogInfo("");
                App.LogInfo("CpuIdInit Done");
                App.LogInfo("");
            }
            catch (Exception ex)
            {
                App.LogExError($"CpuIdInit Exception: {ex.Message}", ex);
            }
        }

        public void CpuSetInit()
        {
            try
            {
                App.LogInfo($"CpuSetInfo");
                App.LogInfo($"");

                App.LogInfo($"CoresByScheduling:");
                var coresbysched = ProcessorInfo.CoresByScheduling();

                if (ProcessorInfo.IsCoresBySchedulingAllZeros())
                {
                    App.LogInfo($"Not available");
                }
                else
                {
                    for (int ix = 0; ix < coresbysched.Count(); ++ix)
                    {
                        App.LogInfo($"#{coresbysched[ix][0]}:{coresbysched[ix][1]}");
                    }

                    CPPC = new int[CPUCores, 2];

                    for (int ii = 0; ii < CPPC.GetLength(0); ii++)
                    {
                        CPPC[ii, 0] = -10;
                        CPPC[ii, 1] = -10;
                    }

                    CPPCOrder = new int[CPUCores];
                    CPPCOrder1 = new int[CPUCores];

                    for (int i = 0; i < CPPC.GetLength(0); i++)
                    {
                        int _highestcpu = 0;
                        int _highestperf = 0;
                        int _highestindex = 0;
                        int _tag = 0;
                        for (int ix = CPPC.GetLength(0) - 1; ix > -1; ix--)
                        {
                            //App.LogInfo($"CPPC Scheduler Testing ix: {ix} core: {coresbysched[ix][0]} perf: {coresbysched[ix][1]} IsIn: {CPPCFoundAlready(ix)}");
                            if (_highestperf <= coresbysched[ix][1] && !CPPCFoundAlready(coresbysched[ix][0]))
                            {
                                //App.LogInfo($"CPPC Highest ix: {ix} perf: {coresbysched[ix][1]}");
                                _highestperf = coresbysched[ix][1];
                                _tag = CPPCTags[coresbysched[ix][0]];
                                _highestcpu = coresbysched[ix][0];
                                _highestindex = ix;
                            }
                        }
                        CPPCOrder[i] = _highestcpu;
                        CPPCOrder1[i] = _highestcpu + 1;
                        CPPC[i, 0] = _highestcpu;
                        CPPC[i, 1] = _tag;

                        string _cppctrace = "";
                        for (int iz = 0; iz < CPPC.GetLength(0); iz++)
                        {
                            _cppctrace += String.Format("{0}#{1} ", CPPC[iz, 0], CPPC[iz, 1]);
                        }
                        App.LogInfo($"Scheduler CPPC: {_cppctrace}");
                        //App.LogInfo($"Scheduler CPPC i: {i} Core: {CPPC[i, 0]} Tag: {CPPC[i, 1]}");
                    }
                }
                App.LogInfo($"");

                App.LogInfo($"CoresByEfficiency:");
                var coresbyeff = ProcessorInfo.CoresByEfficiency();
                if (ProcessorInfo.IsCoresByEfficiencyAllZeros())
                {
                    App.LogInfo($"Not available");
                }
                else
                {
                    for (int ix = 0; ix < coresbyeff.Count(); ++ix)
                    {
                        App.LogInfo($"#{coresbyeff[ix][0]}:{coresbyeff[ix][1]}");
                    }
                }
                App.LogInfo($"");
                App.LogInfo($"ProcessorInfo.Clusters: {ProcessorInfo.Clusters}");
                App.LogInfo($"");

                if (ProcessorInfo.Clusters > 1)
                {
                    string _outstr = "";
                    for (int ix = 0; ix < ProcessorInfo.LogicalsClusters(1).Count(); ++ix)
                    {
                        _outstr = _outstr + $"{ProcessorInfo.LogicalsClusters(1)[ix].ToString()}#";
                    }
                    App.LogInfo($"ProcessorInfo.LogicalsCluster(1): {_outstr}");
                    App.LogInfo($"");
                    _outstr = "";
                    for (int ix = 0; ix < ProcessorInfo.LogicalsClusters(ProcessorInfo.Clusters).Count(); ++ix)
                    {
                        _outstr = _outstr + $"{ProcessorInfo.LogicalsClusters(ProcessorInfo.Clusters)[ix].ToString()}#";
                    }
                    App.LogInfo($"ProcessorInfo.LogicalsCluster({ProcessorInfo.Clusters}): {_outstr}");
                    App.LogInfo($"");
                    _outstr = "";
                    for (int ix = 0; ix < ProcessorInfo.LogicalsClustersOut(1).Count(); ++ix)
                    {
                        _outstr = _outstr + $"{ProcessorInfo.LogicalsClustersOut(1)[ix].ToString()}#";
                    }
                    App.LogInfo($"ProcessorInfo.LogicalsClusterOut(1):  {_outstr}");
                    App.LogInfo($"");
                    _outstr = "";
                    for (int ix = 0; ix < ProcessorInfo.LogicalsClustersOut(ProcessorInfo.Clusters).Count(); ++ix)
                    {
                        _outstr = _outstr + $"{ProcessorInfo.LogicalsClustersOut(ProcessorInfo.Clusters)[ix].ToString()}#";
                    }
                    App.LogInfo($"ProcessorInfo.LogicalsClusterOut({ProcessorInfo.Clusters}):  {_outstr}");
                    App.LogInfo($"");

                }

                int _deltap = 20 / CPULogicalProcessors;

                for (int i = 0; i < CPULogicalProcessors; ++i)
                {
                    SetProgress(42 + _deltap * i);
                    App.LogInfo($"CPU Logical Processor: {i + 1}");
                    App.LogInfo($" ProcessorInfo.LogicalProcessorIndex: {ProcessorInfo.CpuSetLogicalProcessorIndex(i) + 1}");
                    App.LogInfo($" ProcessorInfo.EfficiencyClass: {ProcessorInfo.CpuSetEfficiencyClass(i)}");
                    App.LogInfo($" ProcessorInfo.CoreIndex: {ProcessorInfo.CpuSetCoreIndex(i)}");
                    App.LogInfo($" ProcessorInfo.NumaNodeIndex: {ProcessorInfo.CpuSetNumaNodeIndex(i)}");
                    App.LogInfo($" ProcessorInfo.LastLevelCacheIndex: {ProcessorInfo.CpuSetLastLevelCacheIndex(i)}");
                    App.LogInfo($" ProcessorInfo.Group: {ProcessorInfo.CpuSetGroup(i)}");
                    App.LogInfo($" ProcessorInfo.SchedulingClass: {ProcessorInfo.CpuSetSchedulingClass(i)}");
                    App.LogInfo($" ProcessorInfo.AllocationTag: {ProcessorInfo.CpuSetAllocationTag(i)}");
                    App.LogInfo($" ProcessorInfo.Parked: {ProcessorInfo.CpuSetParked(i)}");
                    App.LogInfo($" ProcessorInfo.Allocated: {ProcessorInfo.CpuSetAllocated(i)}");
                    App.LogInfo($" ProcessorInfo.Cluster: {ProcessorInfo.CpuSetCluster(i)}");
                }

                CPPCLabels();
            }
            catch (Exception ex)
            {
                App.LogExError($"CpuSetsInit Exception: {ex.Message}", ex);
            }
        }

        public bool ZenInit(bool docheck = false)
        {
            try
            {
                if (!ZenDLLFail && App.inpoutdlldisable == false)
                {
                    ZenStates = false;
                    Zen?.Dispose();
                    Zen = null;
                    Zen = new Cpu();
                    bool smucheck = false;
                    int retries = 0;
                    smucheck = Zen.smu.Version != 0U;

                    if ((!smucheck && docheck) || (!smucheck && ZenSMUVer != "N/A"))
                    {
                        for (int i = 0; i < 25; ++i)
                        {
                            if (Ring0.WaitPciBusMutex(10))
                            {
                                Thread.Sleep(25);
                                smucheck = Zen.GetSmuVersion() != 0U;
                                if (smucheck)
                                {
                                    Zen.Dispose();
                                    Zen = null;
                                    Zen = new Cpu();
                                    smucheck = Zen.smu.Version != 0U;
                                }
                                Ring0.ReleasePciBusMutex();
                            }
                            if (smucheck)
                            {
                                retries = i;
                                i = 100;
                            }
                            else
                            {
                                retries++;
                            }
                        }
                    }
                    if (Zen.Status == IOModule.LibStatus.OK) ZenStates = true;
                    App.LogInfo($"ZenInit({docheck}) ZenStates={ZenStates} SMU Check is {smucheck} retries: {retries}");
                    return smucheck;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("inpoutx64.dll")) ZenDLLFail = true;
                App.LogDebug($"ZenInit ZenDLLFail: {ZenDLLFail} ");
                App.LogDebug($"ZenInit Exception: {ex.Message} ");
                return false;
            }
        }
        public void ZenMainInit()
        {
            try
            {
                if (CPUSocket == "AM4" || CPUSocket == "AM5")
                {
                    for (int r = 0; r < 20; ++r)
                    {
                        if (Ring0.WaitPciBusMutex(100))
                        {
                            r = 99;
                        }
                        else
                        {
                            Thread.Sleep(25);
                        }
                    }

                    bool smucheck = false;

                    try
                    {
                        Zen = new Cpu();

                        smucheck = ZenInit();

                        if (Zen.smu.SMU_TYPE == SMU.SmuType.TYPE_CPU3 || (Zen.info.family == Cpu.Family.FAMILY_19H && CPUSocket == "AM4") || Zen.info.codeName == Cpu.CodeName.Vermeer || Zen.info.codeName == Cpu.CodeName.Cezanne) Zen3 = true;
                        if (Zen.smu.SMU_TYPE == SMU.SmuType.TYPE_CPU4 || (Zen.info.family == Cpu.Family.FAMILY_19H && CPUSocket == "AM5") || Zen.info.codeName == Cpu.CodeName.Raphael) Zen4 = true;

                        if (!Zen.info.family.Equals(Cpu.Family.UNSUPPORTED) && !Zen.info.codeName.Equals(Cpu.CodeName.Unsupported))
                        {
                            App.LogInfo($"Zen Name: {Zen.info.cpuName}");
                            App.LogInfo($"Zen CodeName: {Zen.info.codeName}");
                            App.LogInfo($"Zen Family: {Zen.info.family}");
                            App.LogInfo($"Zen Model: {Zen.info.model}");
                            App.LogInfo($"Zen BaseModel: {Zen.info.baseModel}");
                            App.LogInfo($"Zen ExtModel: {Zen.info.extModel}");
                            App.LogInfo($"Zen Socket: {Zen.info.packageType}");
                            App.LogInfo($"Zen Zen3 flag: {Zen3}");
                            App.LogInfo($"Zen Zen4 flag: {Zen4}");
                            App.LogInfo($"Zen CpuID: {Zen.info.cpuid:X8}");
                            App.LogInfo($"Zen SVI2: {Zen.info.svi2.coreAddress:X8}:{Zen.info.svi2.socAddress:X8}");
                            App.LogInfo($"Zen SMU Type: {Zen.smu.SMU_TYPE}");
                            App.LogInfo($"Zen HSMP Version: {Zen.smu.Hsmp.InterfaceVersion}");
                            if (Zen.smu.Hsmp.InterfaceVersion > 0)
                            {
                                uint[] _args = { 0, 0, 0, 0, 0, 0, 0, 0 };
                                _args[0] = 0;
                                Zen.smu.SendHsmpCommand(Zen.smu.Hsmp.ReadCurrentFclkMemclk, ref _args);
                                App.LogInfo($"Zen HSMP FCLK: {_args[0]} MCLK: {_args[1]}");
                            }
                            App.LogInfo($"Zen OCMode: {Zen.GetOcMode()}");
                            App.LogInfo($"Zen Cores: Physical={Zen.info.topology.cores} Logical={Zen.info.topology.logicalCores} ThreadsPerCore={Zen.info.topology.threadsPerCore}");
                            App.LogInfo($"Zen Topology: CCDs={Zen.info.topology.ccds} CCXs={Zen.info.topology.ccxs} CoresPerCcx={Zen.info.topology.coresPerCcx} CcxPerCcd={Zen.info.topology.ccxPerCcd} Nodes={Zen.info.topology.cpuNodes}");
                            App.LogInfo($"Zen EnabledCores: {Zen.info.topology.enabledCores}");
                            App.LogInfo($"Zen CoreEnabledMap: 0x{Zen.info.topology.coreEnabledMap:X8}");
                            App.LogInfo($"Zen CoreDisableMap: 0x{Zen.info.topology.coreDisableMap:X8}");

                            StringBuilder sbz = new StringBuilder();

                            sbz.Append($"Zen CoreIds Map:");
                            for (int i = 0; i < Zen.info.topology.coreIds.Length; ++i)
                                sbz.Append($" [{i}]=[{Zen.info.topology.coreIds[i]}]");

                            App.LogInfo($"{sbz}");

                            sbz.Clear();

                            sbz.Append($"Zen Cores2apicId Map:");
                            for (int i = 0; i < Zen.info.topology.cores2apicId.Length; ++i)
                                sbz.Append($" [{i}]=[{Zen.info.topology.cores2apicId[i]}]");

                            App.LogInfo($"{sbz}");

                            sbz.Clear();

                            sbz.Append($"Zen Logical2apicIds Map:");
                            for (int i = 0; i < Zen.info.topology.logical2apicIds.Length; ++i)
                                sbz.Append($" [{i}]=[{Zen.info.topology.logical2apicIds[i]}]");

                            App.LogInfo($"{sbz}");
                            
                            sbz.Clear();

                            sbz.AppendLine($"Zen coreFullMap:");

                            string _header = $"{(char)0x2554}{(char)0x2550}{(char)0x2550}{(char)0x2550}{(char)0x2550}{(char)0x2566}{(char)0x2550}{(char)0x2550}{(char)0x2550}{(char)0x2566}{(char)0x2550}{(char)0x2550}{(char)0x2550}{(char)0x2557}";
                            string _divider = $"{(char)0x2560}{(char)0x2550}{(char)0x2550}{(char)0x2550}{(char)0x2550}{(char)0x256C}{(char)0x2550}{(char)0x2550}{(char)0x2550}{(char)0x256C}{(char)0x2550}{(char)0x2550}{(char)0x2550}{(char)0x2563}";
                            string _footer = $"{(char)0x255A}{(char)0x2550}{(char)0x2550}{(char)0x2550}{(char)0x2550}{(char)0x2569}{(char)0x2550}{(char)0x2550}{(char)0x2550}{(char)0x2569}{(char)0x2550}{(char)0x2550}{(char)0x2550}{(char)0x255D}";
                            sbz.AppendLine(_header);
                            sbz.AppendLine($"{(char)0x2551}CORE{(char)0x2551}CCD{(char)0x2551}CCX{(char)0x2551}");
                            sbz.AppendLine(_divider);
                            for (int i = 0; i < Zen.info.topology.coreFullMap.GetLength(0); ++i)
                            {
                                sbz.AppendLine(String.Format((char)0x2551 + "{0,4}" + (char)0x2551 + "{1,-3}" + (char)0x2551 + "{2,-3}" + (char)0x2551, $"C{i}", Zen.info.topology.coreFullMap[i, 2], Zen.info.topology.coreFullMap[i, 1]));
                                if (i < (Zen.info.topology.coreFullMap.GetLength(0) - 1)) sbz.AppendLine(_divider);
                                //sbz.AppendLine($" C{i}={Zen.info.topology.coreFullMap[i, 0]}-CCX{Zen.info.topology.coreFullMap[i, 1]}-CCD{Zen.info.topology.coreFullMap[i, 2]}");
                            }

                            sbz.AppendLine(_footer);
                            App.LogInfo($"{sbz}");

                            sbz.Clear();

                            sbz.AppendLine($"Zen coreCcxMap:");
                            for (int i = 0; i < Zen.info.topology.coreCcxMap.GetLength(0); ++i)
                            {
                                sbz.AppendLine($"C{i}=b:{Zen.info.topology.coreCcxMap[i, 0]} ccxSharing:{Zen.info.topology.coreCcxMap[i, 1]} logical:{Zen.info.topology.coreCcxMap[i, 2]} numSharingCache:{Zen.info.topology.coreCcxMap[i, 3]} numSharingCacheId:{Zen.info.topology.coreCcxMap[i, 4]} prevNumSharingCacheId:{Zen.info.topology.coreCcxMap[i, 5]} _logicalCoreIdLog2:{Zen.info.topology.coreCcxMap[i, 6]} numSharingCacheLog2:{Zen.info.topology.coreCcxMap[i, 7]}");
                            }

                            App.LogInfo($"{sbz}");

                            sbz.Clear();

                            //App.Current.Shutdown();
                            //Environment.Exit(0);

                            sbz = null;

                            ZenCCDTotal = (int)Zen.info.topology.ccds;
                            ZenCCXTotal = (int)Zen.info.topology.ccxs;
                            ZenCoresPerCCX = (int)Zen.info.topology.coresPerCcx;

                        }
                        else
                        {
                            App.LogInfo($"ZenStates-Core DLL: CPU not supported");
                        }

                    }
                    catch (Exception ex)
                    {
                        App.LogInfo($"ZenStates-Core DLL couldn't be loaded: {ex.Message}");
                    }

                    if (smucheck)
                    {
                        ZenStates = true;

                        bool _sensors = false;
                        for (int r = 0; r < 20; ++r)
                        {
                            if (Ring0.WaitPciBusMutex(50))
                            {
                                _sensors = Zen.RefreshSensors();
                                r = 99;
                                Ring0.ReleasePciBusMutex();
                            } 
                            else
                            {
                                Thread.Sleep(25);
                            }
                        }

                        CpuBusClock = Zen.cpuBusClock;

                        App.LogInfo($"Zen BCLK: {CpuBusClock}");

                        if (CpuBusClock <= 0) CpuBusClock = 100;

                        double _bclkmulti = CpuBusClock / 100;

                        ReadMemoryModulesInfo();

                        if (modules.Count > 0)
                            ReadTimings(modules[0].DctOffset);
                        else
                            ReadTimings();

                        if (!AsusWmi.Init())
                        {
                            AsusWmi.Dispose();
                            AsusWmi = null;
                        }
                        BMC = new BiosMemController();
                        ReadMemoryConfig();

                        uint smu_ver = Zen.smu.Version;
                        uint ver_maj = smu_ver >> 16 & 255U;
                        uint ver_min = smu_ver >> 8 & 255U;
                        uint ver_rev = smu_ver & 255U;
                        ZenSMUVer = string.Format("{0}.{1}.{2}", ver_maj, ver_min, ver_rev);

                        string line = $"SMU Ver [{ZenSMUVer}]";
                        App.LogInfo(line);

                        ZenBoost = Zen.GetBoostLimit(0);

                        App.LogInfo($"ZenCoreLayoutInit: {Zen.info.topology.coreLayout:X8}");
                        App.LogInfo($"ZenCoreLayout: {Zen.info.topology.coreLayoutInit:X8}");

                        if (ZenCCDTotal > 0)
                        {
                            uint cores_t = Zen.info.topology.coreLayout;
                            ZenCoreMap = new int[CountSetBits(~Zen.info.topology.coreLayout)];
                            int last = ZenCCDTotal * 8;
                            for (int i = 0, k = 0; i < last; cores_t = cores_t >> 1)
                            {
                                ZenCoreMapLabel += (i == 0) ? "[" : (i % 8 != 0) ? "." : "";
                                if ((cores_t & 1) == 1)
                                {
                                    ZenCoreMap[k++] = i;
                                    ZenCoreMapLabel += $"{i}";
                                }
                                else
                                {
                                    ZenCoreMapLabel += "x";
                                }
                                i++;
                                ZenCoreMapLabel += (i % 8 == 0 && i != last) ? "][" : i == last ? "]" : "";
                            }
                            App.LogInfo($"ZenCoreMap: {string.Join(", ", ZenCoreMap)}");
                            App.LogInfo($"ZenCoreMapLabel: {ZenCoreMapLabel}");
                        }

                        App.LogInfo("ZenRefreshCO...");
                        ZenRefreshCO();

                        bool _refreshpt = false, _done = false;

                        int _ptr = 0;

                        while (!_done)
                        {
                            _refreshpt = ZenRefreshPowerTable();
                            _ptr++;
                            App.LogInfo($"ZenRefreshPT Init Loop n.{_ptr}...");
                            if ((Zen.powerTable != null && _refreshpt) || _ptr > 10) _done = true;
                        }

                        StringBuilder sbz = new StringBuilder();

                        if (Zen.powerTable != null)
                        {
                            App.LogInfo("ZenPT Dump...");

                            try
                            {
                                sbz.AppendLine($"Zen PM Table dump:");
                                for (var i = 0; i < Zen.powerTable.Table.Length; ++i)
                                {
                                    var temp = BitConverter.GetBytes(Zen.powerTable.Table[i]);
                                    sbz.AppendLine($"Offset {i:D4} {i * 0x4:X3}: {BitConverter.ToSingle(temp, 0):F8}");
                                }
                            }
                            catch
                            {
                                sbz.AppendLine("Dump FAILED");
                            }
                            App.LogInfo($"{sbz}");

                            sbz.Clear();
                        }

                        App.LogInfo("ZenPT Init...");

                        if (_refreshpt || ver_maj > 0)
                        {
                            bool ZenPTKnown = false;

                            ZenPTVersion = (int)Zen.GetTableVersion();

                            line = $"PT Ver [0x{ZenPTVersion:X}] SMU Ver [{ZenSMUVer}]";
                            App.LogInfo(line);

                            if (ZenSMUVer == "25.86.0")
                            {
                                ZenPTKnown = true;
                                int _maxcores = 8;
                                App.LogInfo($"Configuring Zen Source for PT [0x{ZenPTVersion:X}] [{_maxcores}] Zen1");

                                CPUSensorsSource = "Zen PowerTable";
                                HWMonitor.CPUSource = HWSensorSource.Zen;

                                string model_pattern = @"^AMD Ryzen (?<series>\d+) (?<generation>\d)(?<model>\d*)(?<ex>.?) .*";
                                Regex model_rgx = new Regex(model_pattern, RegexOptions.Multiline);
                                Match model_m = model_rgx.Match(Zen.info.cpuName);

                                string generation = "";
                                string ex = "";
                                string model = "";
                                string series = "";

                                if (model_m.Success)
                                {
                                    string[] results = model_rgx.GetGroupNames();

                                    foreach (var name in results)
                                    {
                                        Group grp = model_m.Groups[name];
                                        if (name == "ex" && grp.Value.Length > 0)
                                        {
                                            ex = grp.Value.TrimEnd('\r', '\n').Trim();
                                        }
                                        if (name == "generation" && grp.Value.Length > 0)
                                        {
                                            generation = grp.Value.TrimEnd('\r', '\n').Trim();
                                        }
                                        if (name == "model" && grp.Value.Length > 0)
                                        {
                                            model = grp.Value.TrimEnd('\r', '\n').Trim();
                                        }
                                        if (name == "series" && grp.Value.Length > 0)
                                        {
                                            series = grp.Value.TrimEnd('\r', '\n').Trim();
                                        }
                                    }
                                    if (ex == "X") Zen1X = true;
                                    if (ex == "2") ZenPlus = true;
                                }

                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresC0, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Load, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresEffClocks, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresStretch, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPT, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Power, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPTLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD1L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD2L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));

                                ZenRefreshStatic(false);

                                if (ZenPlus)
                                {
                                    App.hwsensors.InitZen(HWSensorName.CPUPPT, 1);
                                    App.hwsensors.InitZen(HWSensorName.CPUTDC, 3);
                                    App.hwsensors.InitZen(HWSensorName.CPUPPTLimit, -1, 1, false);
                                    App.hwsensors.InitZen(HWSensorName.CPUTDCLimit, -1, 1, false);
                                    App.hwsensors.InitZen(HWSensorName.CPUEDC, 9);
                                    App.hwsensors.InitZen(HWSensorName.CPUEDCLimit, -1, 1, false);
                                }
                                App.hwsensors.InitZen(HWSensorName.CPUPower, 22);
                                App.hwsensors.InitZen(HWSensorName.SOCVoltage, 26);
                                App.hwsensors.InitZen(HWSensorName.CCD1L3Temp, 127);
                                App.hwsensors.InitZen(HWSensorName.CCD2L3Temp, 128);
                                App.hwsensors.InitZen(HWSensorName.CPUFSB, -1);
                                App.hwsensors.InitZen(HWSensorName.CPUVoltage, 40);
                                App.hwsensors.InitZen(HWSensorName.CPUTemp, -1);
                                App.hwsensors.InitZen(HWSensorName.CPUClock, -1);
                                App.hwsensors.InitZen(HWSensorName.CPUEffClock, -1);
                                App.hwsensors.InitZen(HWSensorName.CPULoad, -1);

                                for (int _core = 1; _core <= CPUCores; ++_core)
                                {
                                    int _coreoffset = _core - 1;
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresPower, 41 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresVoltages, 57 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresTemps, 65 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresClocks, 81 + _coreoffset, _core, 1000 * _bclkmulti);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresEffClocks, 89 + _coreoffset, _core, 1000 * _bclkmulti);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresStretch, -1, _core, 1);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresC0, 97 + _coreoffset, _core);
                                }

                                for (int _cpu = 1; _cpu <= CPULogicalProcessors; ++_cpu)
                                {
                                    App.hwsensors.InitZenMulti(HWSensorName.CPULogicalsLoad, -1, _cpu);
                                }

                                float tempoffset = 0;

                                if (ex == "X" && model == "700" && generation == "2") tempoffset = -10.0f;
                                if (ex == "X" && generation == "1") tempoffset = -20.0f;
                                if (tempoffset != 0)
                                {
                                    App.LogInfo($"Setting Zen Temp Offset={tempoffset}");
                                    App.hwsensors.SetValueOffset(HWSensorName.CCD1L3Temp, tempoffset);
                                    App.hwsensors.SetValueOffset(HWSensorName.CCD2L3Temp, tempoffset);
                                    App.hwsensors.SetValueOffset(HWSensorName.CPUCoresTemps, tempoffset);
                                }

                                App.LogInfo($"Zen Flags series={series} generation={generation} model={model} ex={ex} ZenPlus={ZenPlus} Zen1X={Zen1X}");

                                App.LogInfo($"Configuring Zen Source done");
                            }
                            else if (ZenPTVersion == 0x380804)
                            {
                                ZenPTKnown = true;
                                int _maxcores = 16;
                                App.LogInfo($"Configuring Zen Source for PT [0x{ZenPTVersion:X}] [{_maxcores}] Zen3");

                                ZenPerCCDTemp = true;

                                CPUSensorsSource = "Zen PowerTable";
                                HWMonitor.CPUSource = HWSensorSource.Zen;

                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresC0, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Load, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresEffClocks, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresStretch, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPT, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Power, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPTLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD1L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD2L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));

                                ZenRefreshStatic(false);

                                App.hwsensors.InitZen(HWSensorName.CPUPPT, 1);
                                App.hwsensors.InitZen(HWSensorName.CPUTDC, 3);
                                App.hwsensors.InitZen(HWSensorName.CPUEDC, 9);
                                App.hwsensors.InitZen(HWSensorName.CPUPPTLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUTDCLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUEDCLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUPower, 29);
                                int _vsoc = (int)Zen.powerTable.Table[45] == 0 ? 44 : 45;
                                App.hwsensors.InitZen(HWSensorName.SOCVoltage, _vsoc);
                                App.hwsensors.InitZen(HWSensorName.CCD1L3Temp, 525);
                                App.hwsensors.InitZen(HWSensorName.CCD2L3Temp, 526);
                                App.hwsensors.InitZen(HWSensorName.CPUFSB, -1);
                                App.hwsensors.InitZen(HWSensorName.CPUVoltage, 41);
                                App.hwsensors.InitZen(HWSensorName.CPUTemp, -1);

                                App.hwsensors.InitZen(HWSensorName.CPUClock, -1);
                                App.hwsensors.InitZen(HWSensorName.CPUEffClock, -1);
                                App.hwsensors.InitZen(HWSensorName.CCD1Temp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CCD2Temp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CCDSTemp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CPULoad, -1);

                                for (int _core = 1; _core <= CPUCores; ++_core)
                                {
                                    int _coreoffset = ZenCoreMap[_core - 1];
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresPower, 169 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresVoltages, 185 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresTemps, 201 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresClocks, 249 + _coreoffset, _core, 1000 * _bclkmulti);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresEffClocks, 265 + _coreoffset, _core, 1000 * _bclkmulti);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresStretch, -1, _core, 1);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresC0, 281 + _coreoffset, _core);
                                }
                                for (int _cpu = 1; _cpu <= CPULogicalProcessors; ++_cpu)
                                {
                                    App.hwsensors.InitZenMulti(HWSensorName.CPULogicalsLoad, -1, _cpu);
                                }
                                App.LogInfo($"Configuring Zen Source done");

                            }
                            else if (ZenPTVersion == 0x380904)
                            {
                                ZenPTKnown = true;
                                int _maxcores = 8;
                                App.LogInfo($"Configuring Zen Source for PT [0x{ZenPTVersion:X}] [{_maxcores}] Zen3");

                                ZenPerCCDTemp = true;

                                CPUSensorsSource = "Zen PowerTable";
                                HWMonitor.CPUSource = HWSensorSource.Zen;

                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresC0, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Load, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresEffClocks, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresStretch, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPT, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Power, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPTLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD1L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));

                                ZenRefreshStatic(false);

                                App.hwsensors.InitZen(HWSensorName.CPUPPT, 1);
                                App.hwsensors.InitZen(HWSensorName.CPUTDC, 3);
                                App.hwsensors.InitZen(HWSensorName.CPUEDC, 9);
                                App.hwsensors.InitZen(HWSensorName.CPUPPTLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUTDCLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUEDCLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUPower, 29);
                                int _vsoc = (int)Zen.powerTable.Table[45] == 0 ? 44 : 45;
                                App.hwsensors.InitZen(HWSensorName.SOCVoltage, _vsoc);
                                App.hwsensors.InitZen(HWSensorName.CCD1L3Temp, 347);
                                App.hwsensors.InitZen(HWSensorName.CPUFSB, -1);
                                App.hwsensors.InitZen(HWSensorName.CPUVoltage, 41);
                                App.hwsensors.InitZen(HWSensorName.CPUTemp, -1);

                                App.hwsensors.InitZen(HWSensorName.CPUClock, -1);
                                App.hwsensors.InitZen(HWSensorName.CPUEffClock, -1);
                                App.hwsensors.InitZen(HWSensorName.CCD1Temp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CCD2Temp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CCDSTemp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CPULoad, -1);

                                for (int _core = 1; _core <= CPUCores; ++_core)
                                {
                                    int _coreoffset = ZenCoreMap[_core - 1];
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresPower, 169 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresVoltages, 177 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresTemps, 185 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresClocks, 209 + _coreoffset, _core, 1000 * _bclkmulti);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresEffClocks, 217 + _coreoffset, _core, 1000 * _bclkmulti);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresStretch, -1, _core, 1);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresC0, 225 + _coreoffset, _core);
                                }
                                for (int _cpu = 1; _cpu <= CPULogicalProcessors; ++_cpu)
                                {
                                    App.hwsensors.InitZenMulti(HWSensorName.CPULogicalsLoad, -1, _cpu);
                                }
                                App.LogInfo($"Configuring Zen Source done");

                            }
                            else if (ZenPTVersion == 0x380805)
                            {
                                ZenPTKnown = true;
                                int _maxcores = 16;
                                App.LogInfo($"Configuring Zen Source for PT [0x{ZenPTVersion:X}] [{_maxcores}] Zen3");

                                ZenPerCCDTemp = true;

                                CPUSensorsSource = "Zen PowerTable";
                                HWMonitor.CPUSource = HWSensorSource.Zen;

                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresC0, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Load, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresEffClocks, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresStretch, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPT, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Power, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPTLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD1L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD2L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));

                                ZenRefreshStatic(false);

                                App.hwsensors.InitZen(HWSensorName.CPUPPT, 1);
                                App.hwsensors.InitZen(HWSensorName.CPUTDC, 3);
                                App.hwsensors.InitZen(HWSensorName.CPUEDC, 9);
                                App.hwsensors.InitZen(HWSensorName.CPUPPTLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUTDCLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUEDCLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUPower, 29);
                                int _vsoc = (int)Zen.powerTable.Table[45] == 0 ? 44 : 45;
                                App.hwsensors.InitZen(HWSensorName.SOCVoltage, _vsoc);
                                App.hwsensors.InitZen(HWSensorName.CCD1L3Temp, 544);
                                App.hwsensors.InitZen(HWSensorName.CCD2L3Temp, 545);
                                App.hwsensors.InitZen(HWSensorName.CPUFSB, -1);
                                App.hwsensors.InitZen(HWSensorName.CPUVoltage, 41);
                                App.hwsensors.InitZen(HWSensorName.CPUTemp, -1);

                                App.hwsensors.InitZen(HWSensorName.CPUClock, -1);
                                App.hwsensors.InitZen(HWSensorName.CPUEffClock, -1);
                                App.hwsensors.InitZen(HWSensorName.CCD1Temp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CCD2Temp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CCDSTemp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CPULoad, -1);

                                for (int _core = 1; _core <= CPUCores; ++_core)
                                {
                                    int _coreoffset = ZenCoreMap[_core - 1];
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresPower, 172 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresVoltages, 188 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresTemps, 204 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresClocks, 252 + _coreoffset, _core, 1000 * _bclkmulti);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresEffClocks, 268 + _coreoffset, _core, 1000 * _bclkmulti);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresStretch, -1, _core, 1);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresC0, 284 + _coreoffset, _core);
                                }

                                for (int _cpu = 1; _cpu <= CPULogicalProcessors; ++_cpu)
                                {
                                    App.hwsensors.InitZenMulti(HWSensorName.CPULogicalsLoad, -1, _cpu);
                                }

                                App.LogInfo($"Configuring Zen Source done");

                            }
                            else if (ZenPTVersion == 0x540100 || ZenPTVersion == 0x540101 || ZenPTVersion == 0x540102 || ZenPTVersion == 0x540103 || ZenPTVersion == 0x540104 || ZenPTVersion == 0x540105)
                            {
                                ZenPTKnown = true;
                                int _maxcores = 16;
                                App.LogInfo($"Configuring Zen Source for PT [0x{ZenPTVersion:X}] [{_maxcores}] Zen4");

                                ZenPerCCDTemp = true;

                                CPUSensorsSource = "Zen PowerTable";
                                HWMonitor.CPUSource = HWSensorSource.Zen;

                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresC0, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Load, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresEffClocks, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresStretch, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPT, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Power, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPTLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD1L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD2L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));

                                ZenRefreshStatic(false);

                                App.hwsensors.InitZen(HWSensorName.CPUPPT, 3);
                                App.hwsensors.InitZen(HWSensorName.CPUTDC, 9);
                                App.hwsensors.InitZen(HWSensorName.CPUEDC, 62);
                                App.hwsensors.InitZen(HWSensorName.CPUPPTLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUTDCLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUEDCLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUPower, 29);
                                int _vsoc = (int)Zen.powerTable.Table[52] == 0 ? 51 : 52;
                                App.hwsensors.InitZen(HWSensorName.SOCVoltage, _vsoc);
                                App.hwsensors.InitZen(HWSensorName.CCD1L3Temp, 544);
                                App.hwsensors.InitZen(HWSensorName.CCD2L3Temp, 545);
                                App.hwsensors.InitZen(HWSensorName.CPUFSB, -1);
                                App.hwsensors.InitZen(HWSensorName.CPUVoltage, 41);
                                App.hwsensors.InitZen(HWSensorName.CPUTemp, -1);

                                App.hwsensors.InitZen(HWSensorName.CPUClock, -1);
                                App.hwsensors.InitZen(HWSensorName.CPUEffClock, -1);
                                App.hwsensors.InitZen(HWSensorName.CCD1Temp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CCD2Temp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CCDSTemp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CPULoad, -1);

                                for (int _core = 1; _core <= CPUCores; ++_core)
                                {
                                    int _coreoffset = ZenCoreMap[_core - 1];
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresPower, 172 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresVoltages, 188 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresTemps, 204 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresClocks, 252 + _coreoffset, _core, 1000 * _bclkmulti);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresEffClocks, 268 + _coreoffset, _core, 1000 * _bclkmulti);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresStretch, -1, _core, 1);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresC0, 284 + _coreoffset, _core);
                                }

                                for (int _cpu = 1; _cpu <= CPULogicalProcessors; ++_cpu)
                                {
                                    App.hwsensors.InitZenMulti(HWSensorName.CPULogicalsLoad, -1, _cpu);
                                }

                                App.LogInfo($"Configuring Zen Source done");

                            }
                            else if (ZenPTVersion == 0x540000 || ZenPTVersion == 0x540001 || ZenPTVersion == 0x540002 || ZenPTVersion == 0x540003 || ZenPTVersion == 0x540004 || ZenPTVersion == 0x540005)
                            {
                                ZenPTKnown = true;
                                int _maxcores = 16;
                                App.LogInfo($"Configuring Zen Source for PT [0x{ZenPTVersion:X}] [{_maxcores}] Zen4");

                                ZenPerCCDTemp = true;

                                CPUSensorsSource = "Zen PowerTable";
                                HWMonitor.CPUSource = HWSensorSource.Zen;

                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresC0, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Load, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresEffClocks, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresStretch, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPT, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Power, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPTLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD1L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD2L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));

                                ZenRefreshStatic(false);

                                App.hwsensors.InitZen(HWSensorName.CPUPPT, 3);
                                App.hwsensors.InitZen(HWSensorName.CPUTDC, 9);
                                App.hwsensors.InitZen(HWSensorName.CPUEDC, 62);
                                App.hwsensors.InitZen(HWSensorName.CPUPPTLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUTDCLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUEDCLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUPower, 29);
                                int _vsoc = (int)Zen.powerTable.Table[52] == 0 ? 51 : 52;
                                App.hwsensors.InitZen(HWSensorName.SOCVoltage, _vsoc);
                                App.hwsensors.InitZen(HWSensorName.CCD1L3Temp, 544);
                                App.hwsensors.InitZen(HWSensorName.CCD2L3Temp, 545);
                                App.hwsensors.InitZen(HWSensorName.CPUFSB, -1);
                                App.hwsensors.InitZen(HWSensorName.CPUVoltage, 41);
                                App.hwsensors.InitZen(HWSensorName.CPUTemp, -1);

                                App.hwsensors.InitZen(HWSensorName.CPUClock, -1);
                                App.hwsensors.InitZen(HWSensorName.CPUEffClock, -1);
                                App.hwsensors.InitZen(HWSensorName.CCD1Temp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CCD2Temp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CCDSTemp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CPULoad, -1);

                                for (int _core = 1; _core <= CPUCores; ++_core)
                                {
                                    int _coreoffset = ZenCoreMap[_core - 1];
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresPower, 172 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresVoltages, 188 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresTemps, 204 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresClocks, 252 + _coreoffset, _core, 1000 * _bclkmulti);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresEffClocks, 268 + _coreoffset, _core, 1000 * _bclkmulti);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresStretch, -1, _core, 1);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresC0, 284 + _coreoffset, _core);
                                }

                                for (int _cpu = 1; _cpu <= CPULogicalProcessors; ++_cpu)
                                {
                                    App.hwsensors.InitZenMulti(HWSensorName.CPULogicalsLoad, -1, _cpu);
                                }

                                App.LogInfo($"Configuring Zen Source done");

                            }
                            else if (ZenPTVersion == 0x380905)
                            {
                                ZenPTKnown = true;
                                int _maxcores = 8;
                                App.LogInfo($"Configuring Zen Source for PT [0x{ZenPTVersion:X}] [{_maxcores}] Zen3");

                                ZenPerCCDTemp = true;

                                CPUSensorsSource = "Zen PowerTable";
                                HWMonitor.CPUSource = HWSensorSource.Zen;

                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresC0, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Load, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresEffClocks, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresStretch, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPT, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Power, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPTLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD1L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));

                                ZenRefreshStatic(false);

                                App.hwsensors.InitZen(HWSensorName.CPUPPT, 1);
                                App.hwsensors.InitZen(HWSensorName.CPUTDC, 3);
                                App.hwsensors.InitZen(HWSensorName.CPUEDC, 9);
                                App.hwsensors.InitZen(HWSensorName.CPUPPTLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUTDCLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUEDCLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUPower, 29);
                                int _vsoc = (int)Zen.powerTable.Table[45] == 0 ? 44 : 45;
                                App.hwsensors.InitZen(HWSensorName.SOCVoltage, _vsoc);
                                App.hwsensors.InitZen(HWSensorName.CCD1L3Temp, 358);
                                App.hwsensors.InitZen(HWSensorName.CPUFSB, -1);
                                App.hwsensors.InitZen(HWSensorName.CPUVoltage, 41);
                                App.hwsensors.InitZen(HWSensorName.CPUTemp, -1);

                                App.hwsensors.InitZen(HWSensorName.CPUClock, -1);
                                App.hwsensors.InitZen(HWSensorName.CPUEffClock, -1);
                                App.hwsensors.InitZen(HWSensorName.CCD1Temp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CCD2Temp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CCDSTemp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CPULoad, -1);

                                for (int _core = 1; _core <= CPUCores; ++_core)
                                {
                                    int _coreoffset = ZenCoreMap[_core - 1];
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresPower, 172 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresVoltages, 180 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresTemps, 188 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresClocks, 212 + _coreoffset, _core, 1000 * _bclkmulti);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresEffClocks, 220 + _coreoffset, _core, 1000 * _bclkmulti);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresStretch, -1, _core, 1);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresC0, 228 + _coreoffset, _core);
                                }
                                for (int _cpu = 1; _cpu <= CPULogicalProcessors; ++_cpu)
                                {
                                    App.hwsensors.InitZenMulti(HWSensorName.CPULogicalsLoad, -1, _cpu);
                                }
                                App.LogInfo($"Configuring Zen Source done");

                            }
                            else if (ZenPTVersion == 0x400005 || ZenPTVersion == 0x400004)
                            {
                                ZenPTKnown = true;
                                int _maxcores = 8;
                                App.LogInfo($"Configuring Zen Source for PT [0x{ZenPTVersion:X}] [{_maxcores}] Zen3 APU");

                                ZenPerCCDTemp = true;

                                CPUSensorsSource = "Zen PowerTable";
                                HWMonitor.CPUSource = HWSensorSource.Zen;

                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresC0, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Load, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresEffClocks, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresStretch, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPT, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Power, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPTLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD1L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));

                                ZenRefreshStatic(false);

                                App.hwsensors.InitZen(HWSensorName.CPUPPT, 5);
                                App.hwsensors.InitZen(HWSensorName.CPUTDC, 9);
                                App.hwsensors.InitZen(HWSensorName.CPUEDC, 12);
                                App.hwsensors.InitZen(HWSensorName.CPUPPTLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUTDCLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUEDCLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUPower, 38);
                                int _vsoc = (int)Zen.powerTable.Table[103] == 0 ? 102 : 103;
                                App.hwsensors.InitZen(HWSensorName.SOCVoltage, _vsoc);
                                App.hwsensors.InitZen(HWSensorName.CCD1L3Temp, 386);
                                App.hwsensors.InitZen(HWSensorName.CPUFSB, -1);
                                App.hwsensors.InitZen(HWSensorName.CPUVoltage, 99);
                                //App.hwsensors.InitZen(HWSensorName.CPUTemp, 17);
                                App.hwsensors.InitZen(HWSensorName.CPUTemp, -1);
                                App.hwsensors.InitZen(HWSensorName.CPUClock, -1);
                                App.hwsensors.InitZen(HWSensorName.CPUEffClock, -1);
                                App.hwsensors.InitZen(HWSensorName.CCD1Temp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CCD2Temp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CCDSTemp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CPULoad, -1);

                                for (int _core = 1; _core <= CPUCores; ++_core)
                                {
                                    int _coreoffset = ZenCoreMap[_core - 1];
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresPower, 200 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresVoltages, 208 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresTemps, 216 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresClocks, 240 + _coreoffset, _core, 1000 * _bclkmulti);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresEffClocks, 248 + _coreoffset, _core, 1000 * _bclkmulti);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresStretch, -1, _core, 1);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresC0, 256 + _coreoffset, _core);
                                }
                                for (int _cpu = 1; _cpu <= CPULogicalProcessors; ++_cpu)
                                {
                                    App.hwsensors.InitZenMulti(HWSensorName.CPULogicalsLoad, -1, _cpu);
                                }
                                App.LogInfo($"Configuring Zen Source done");

                            }
                            else if (ZenPTVersion == 0x240903)
                            {
                                ZenPTKnown = true;
                                int _maxcores = 8;
                                App.LogInfo($"Configuring Zen Source for PT [0x{ZenPTVersion:X}] [{_maxcores}] Zen2");

                                ZenPerCCDTemp = true;

                                CPUSensorsSource = "Zen PowerTable";
                                HWMonitor.CPUSource = HWSensorSource.Zen;

                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresC0, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Load, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresEffClocks, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresStretch, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPT, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Power, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPTLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD1L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));

                                ZenRefreshStatic(false);

                                App.hwsensors.InitZen(HWSensorName.CPUPPT, 1);
                                App.hwsensors.InitZen(HWSensorName.CPUTDC, 3);
                                App.hwsensors.InitZen(HWSensorName.CPUEDC, 9);
                                App.hwsensors.InitZen(HWSensorName.CPUPPTLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUTDCLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUEDCLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUPower, 29);
                                int _vsoc = (int)Zen.powerTable.Table[45] == 0 ? 44 : 45;
                                App.hwsensors.InitZen(HWSensorName.SOCVoltage, _vsoc);
                                App.hwsensors.InitZen(HWSensorName.CCD1L3Temp, 303);
                                App.hwsensors.InitZen(HWSensorName.CPUFSB, -1);
                                App.hwsensors.InitZen(HWSensorName.CPUVoltage, 40);
                                //App.hwsensors.InitZen(HWSensorName.CPUTemp, 5);
                                App.hwsensors.InitZen(HWSensorName.CPUTemp, -1);

                                App.hwsensors.InitZen(HWSensorName.CPUClock, -1);
                                App.hwsensors.InitZen(HWSensorName.CPUEffClock, -1);
                                App.hwsensors.InitZen(HWSensorName.CCD1Temp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CCD2Temp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CCDSTemp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CPULoad, -1);

                                for (int _core = 1; _core <= CPUCores; ++_core)
                                {
                                    int _coreoffset = ZenCoreMap[_core - 1];
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresPower, 147 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresVoltages, 155 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresTemps, 163 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresClocks, 187 + _coreoffset, _core, 1000 * _bclkmulti);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresEffClocks, 195 + _coreoffset, _core, 1000 * _bclkmulti);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresStretch, -1, _core, 1);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresC0, 203 + _coreoffset, _core);
                                }
                                for (int _cpu = 1; _cpu <= CPULogicalProcessors; ++_cpu)
                                {
                                    App.hwsensors.InitZenMulti(HWSensorName.CPULogicalsLoad, -1, _cpu);
                                }
                                App.LogInfo($"Configuring Zen Source done");

                            }
                            else if (ZenPTVersion == 0x240803)
                            {
                                ZenPTKnown = true;
                                int _maxcores = 16;
                                App.LogInfo($"Configuring Zen Source for PT [0x{ZenPTVersion:X}] [{_maxcores}] Zen2");

                                ZenPerCCDTemp = true;

                                CPUSensorsSource = "Zen PowerTable";
                                HWMonitor.CPUSource = HWSensorSource.Zen;

                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresC0, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Load, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresEffClocks, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUCoresStretch, HWSensorValues.MultiCore, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Clock, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPT, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Power, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDC, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Amperage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUPPTLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUTDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CPUEDCLimit, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Percentage, HWSensorSource.Zen));
                                App.hwsensors.Add(new HWSensorItem(HWSensorName.CCD1L3Temp, HWSensorValues.Single, HWSensorConfig.Auto, HWSensorDevice.CPU, HWSensorType.Temperature, HWSensorSource.Zen));

                                ZenRefreshStatic(false);

                                App.hwsensors.InitZen(HWSensorName.CPUPPT, 1);
                                App.hwsensors.InitZen(HWSensorName.CPUTDC, 3);
                                App.hwsensors.InitZen(HWSensorName.CPUEDC, 9);
                                App.hwsensors.InitZen(HWSensorName.CPUPPTLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUTDCLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUEDCLimit, -1, 1, false);
                                App.hwsensors.InitZen(HWSensorName.CPUPower, 29);
                                int _vsoc = (int)Zen.powerTable.Table[45] == 0 ? 44 : 45;
                                App.hwsensors.InitZen(HWSensorName.SOCVoltage, _vsoc);
                                App.hwsensors.InitZen(HWSensorName.CCD1L3Temp, 459);
                                App.hwsensors.InitZen(HWSensorName.CPUFSB, -1);
                                App.hwsensors.InitZen(HWSensorName.CPUVoltage, 40);
                                App.hwsensors.InitZen(HWSensorName.CPUTemp, -1);

                                App.hwsensors.InitZen(HWSensorName.CPUClock, -1);
                                App.hwsensors.InitZen(HWSensorName.CPUEffClock, -1);
                                App.hwsensors.InitZen(HWSensorName.CCD1Temp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CCD2Temp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CCDSTemp, -1, 1);
                                App.hwsensors.InitZen(HWSensorName.CPULoad, -1);

                                for (int _core = 1; _core <= CPUCores; ++_core)
                                {
                                    int _coreoffset = ZenCoreMap[_core - 1];
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresPower, 147 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresVoltages, 163 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresTemps, 179 + _coreoffset, _core);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresClocks, 227 + _coreoffset, _core, 1000 * _bclkmulti);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresEffClocks, 243 + _coreoffset, _core, 1000 * _bclkmulti);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresStretch, -1, _core, 1);
                                    App.hwsensors.InitZenMulti(HWSensorName.CPUCoresC0, 259 + _coreoffset, _core);
                                }
                                for (int _cpu = 1; _cpu <= CPULogicalProcessors; ++_cpu)
                                {
                                    App.hwsensors.InitZenMulti(HWSensorName.CPULogicalsLoad, -1, _cpu);
                                }
                                App.LogInfo($"Configuring Zen Source done");
                            }

                            float? _cpuVcore, _cpuVsoc;
                            _cpuVcore = Zen.cpuVcore;
                            _cpuVsoc = Zen.cpuVsoc;
                            App.LogInfo($"Zen cpuVcore: {_cpuVcore} _cpuVsoc: {_cpuVsoc}");

                            if (_cpuVcore > 0)
                            {
                                App.hwsensors.InitZen(HWSensorName.CPUVoltage, -1);
                            }
                            if (_cpuVsoc > 0)
                            {
                                App.hwsensors.InitZen(HWSensorName.SOCVoltage, -1);
                            }

                            ZenStartBoost = Zen.GetMaxBoostLimit();
                            ZenStartPPT = Zen.GetPPTLimit();
                            ZenStartTDC = Zen.info.TDCSupported ? Zen.powerTable.TDC : 0;
                            ZenStartEDC = Zen.info.EDCSupported ? Zen.powerTable.EDC : 0;
                            ZenStartTHM = Zen.info.THMSupported ? Zen.powerTable.THM : 0;
                            ZenMaxBoost = Zen.GetMaxBoostLimit();
                            ZenMaxPPT = Zen.GetMaxPPTLimit();
                            ZenMaxTDC = Zen.GetMaxTDCLimit();
                            ZenMaxEDC = Zen.GetMaxEDCLimit();
                            ZenMaxTHM = Zen.GetMaxTHMLimit();

                            App.LogInfo($"Zen PowerTable Known: {ZenPTKnown}");
                            App.LogInfo($"Zen HSMP isSupported: {Zen.smu.Hsmp.IsSupported}");
                            App.LogInfo($"Zen Boost: {ZenBoost}/{ZenMaxBoost}");
                            App.LogInfo($"Zen PPT: {ZenPPT}/{ZenMaxPPT}");
                            App.LogInfo($"Zen TDC: {ZenTDC}/{ZenMaxTDC}");
                            App.LogInfo($"Zen EDC: {ZenEDC}/{ZenMaxEDC}");
                            App.LogInfo($"Zen THM: {ZenTHM}/{ZenMaxTHM}");
                            App.LogInfo($"Zen PPT PowerTable Supported? {Zen.info.PPTSupported}: {Zen.powerTable.PPT}");
                            App.LogInfo($"Zen TDC PowerTable Supported? {Zen.info.TDCSupported}: {Zen.powerTable.TDC}");
                            App.LogInfo($"Zen EDC PowerTable Supported? {Zen.info.EDCSupported}: {Zen.powerTable.EDC}");
                            App.LogInfo($"Zen THM PowerTable Supported? {Zen.info.THMSupported}: {Zen.powerTable.THM}");

                            ZenMaxBoost = ZenMaxBoost > 0 ? ZenMaxBoost : ZenBoost > 0 ? ZenBoost : 0;
                            ZenMaxPPT = ZenMaxPPT > 0 ? ZenMaxPPT : ZenPPT > 0 ? ZenPPT : 0;
                            ZenMaxTDC = ZenMaxTDC > 0 ? ZenMaxTDC : ZenTDC > 0 ? ZenTDC : 0;
                            ZenMaxEDC = ZenMaxEDC > 0 ? ZenMaxEDC : ZenEDC > 0 ? ZenEDC : 0;
                            ZenMaxTHM = ZenMaxTHM > 0 ? ZenMaxTHM : ZenTHM > 0 ? ZenTHM : 0;

                            App.LogInfo($"Zen Max Boost: {ZenMaxBoost}");
                            App.LogInfo($"Zen Max PPT: {ZenMaxPPT}");
                            App.LogInfo($"Zen Max TDC: {ZenMaxTDC}");
                            App.LogInfo($"Zen Max EDC: {ZenMaxEDC}");
                            App.LogInfo($"Zen Max THM: {ZenMaxTHM}");

                            App.LogInfo($"Zen Start PPT: {ZenStartPPT}");
                            App.LogInfo($"Zen Start TDC: {ZenStartTDC}");
                            App.LogInfo($"Zen Start EDC: {ZenStartEDC}");
                            App.LogInfo($"Zen Start THM: {ZenStartTHM}");
                        }
                        else
                        {
                            App.LogInfo($"Failed SMU PowerTable refresh: {_refreshpt} ver: {ver_maj}");
                        }
                    }

                    Ring0.ReleasePciBusMutex();

                }
            }
            catch (Exception ex)
            {
                App.LogExError($"ZenMainInit Exception: {ex.Message}", ex);
            }
        }

        public void DumpZenPowerTable(bool writefile = false)
        {
            try
            {
                var sb = new StringBuilder();
                string line = "";
                App.LogInfo("Dump ZenPowerTable");
                for (int it = 0; it < Zen.powerTable.Table.Length; ++it)
                {
                    line = $"\t\t[{it * 4:X3}][{it}] = [{Zen.powerTable.Table[it]}]";
                    App.LogInfo(line);
                    sb.AppendLine(line);
                }
                if (writefile)
                {
                    string path = @".\Logs\dumpzenpt.txt";
                    if (!File.Exists(path)) File.Delete(path);

                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine(sb.ToString());
                    }
                }
                sb.Clear();
                sb = null;
            }
            catch { }
        }
        private void ReadMemoryModulesInfo()
        {
            using (var searcher = new ManagementObjectSearcher("select * from Win32_PhysicalMemory"))
            {
                try
                {
                    WMI.Connect(@"root\cimv2");

                    foreach (var queryObject in searcher.Get().Cast<ManagementObject>())
                    {
                        var capacity = 0UL;
                        var clockSpeed = 0U;
                        var partNumber = "N/A";
                        var bankLabel = "";
                        var manufacturer = "";
                        var deviceLocator = "";

                        var temp = WMI.TryGetProperty(queryObject, "Capacity");
                        if (temp != null) capacity = (ulong)temp;

                        temp = WMI.TryGetProperty(queryObject, "ConfiguredClockSpeed");
                        if (temp != null) clockSpeed = (uint)temp;

                        temp = WMI.TryGetProperty(queryObject, "partNumber");
                        if (temp != null) partNumber = (string)temp;

                        temp = WMI.TryGetProperty(queryObject, "BankLabel");
                        if (temp != null) bankLabel = (string)temp;

                        temp = WMI.TryGetProperty(queryObject, "Manufacturer");
                        if (temp != null) manufacturer = (string)temp;

                        temp = WMI.TryGetProperty(queryObject, "DeviceLocator");
                        if (temp != null) deviceLocator = (string)temp;

                        modules.Add(new MemoryModule(partNumber.Trim(), bankLabel.Trim(), manufacturer.Trim(),
                            deviceLocator, capacity, clockSpeed));

                        //string bl = bankLabel.Length > 0 ? new string(bankLabel.Where(char.IsDigit).ToArray()) : "";
                        //string dl = deviceLocator.Length > 0 ? new string(deviceLocator.Where(char.IsDigit).ToArray()) : "";

                        //comboBoxPartNumber.Items.Add($"#{bl}: {partNumber}");
                        //comboBoxPartNumber.SelectedIndex = 0;
                    }
                }
                catch (Exception ex)
                {
                    App.LogExError($"ReadMemoryModuleInfo Exception: {ex.Message}", ex);
                }
            }

            if (modules.Count > 0)
            {
                bool rchan = ReadChannelsInfo();
                if (!rchan) 
                { 
                    for (int i = 0; i <= 10; ++i)
                    {
                        Thread.Sleep(25);
                        rchan = ReadChannelsInfo();
                        if (rchan) i = 10;
                    }
                }

                ulong totalCapacity = 0UL;

                foreach (var module in modules)
                {
                    totalCapacity += module.Capacity;
                    MemPartNumbers.Add(
                        $"{module.Slot}: {module.PartNumber} ({module.Capacity / 1024 / (1024 * 1024)}GB, {module.Rank})");
                }

                if (modules[0].ClockSpeed != 0)
                    MEMCFG.Frequency = modules[0].ClockSpeed;

                if (totalCapacity != 0)
                    MEMCFG.TotalCapacity = $"{totalCapacity / 1024 / (1024 * 1024)}GB";
            }
        }

        private void ReadMemoryConfig()
        {
            string scope = @"root\wmi";
            string className = "AMD_ACPI";

            App.LogDebug("Zen ReadMemoryConfig");

            try
            {
                WMI.Connect($@"{scope}");

                string instanceName = WMI.GetInstanceName(scope, className);

                ManagementObject classInstance = new ManagementObject(scope,
                    $"{className}.InstanceName='{instanceName}'",
                    null);

                // Get possible values (index) of a memory option in BIOS
                /*pack = WMI.InvokeMethod(classInstance, "Getdvalues", "pack", "ID", 0x20007);
				if (pack != null)
				{
					uint[] DValuesBuffer = (uint[])pack.GetPropertyValue("DValuesBuffer");
					for (var i = 0; i < DValuesBuffer.Length; i++)
					{
						Debug.WriteLine("{0}", DValuesBuffer[i]);
					}
				}
				*/


                // Get function names with their IDs
                string[] functionObjects = { "GetObjectID", "GetObjectID2" };
                foreach (string functionObject in functionObjects)
                {
                    try
                    {
                        ManagementBaseObject pack = WMI.InvokeMethodAndGetValue(classInstance, functionObject, "pack", null, 0);
                        if (pack != null)
                        {
                            uint[] ID = (uint[])pack.GetPropertyValue("ID");
                            string[] IDString = (string[])pack.GetPropertyValue("IDString");
                            byte Length = (byte)pack.GetPropertyValue("Length");

                            for (int i = 0; i < Length; ++i)
                            {
                                biosFunctions.Add(new BiosACPIFunction(IDString[i], ID[i]));
                                Debug.WriteLine("{0}: {1:X8}", IDString[i], ID[i]);
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }

                AOD aod = Zen.info.aod;

                if (MEMCFG.Type == MemType.DDR4)
                {
                    // Get APCB config from BIOS. Holds memory parameters.
                    BiosACPIFunction cmd = GetFunctionByIdString("Get APCB Config");
                    if (cmd == null)
                    {
                        // throw new Exception("Could not get memory controller config");
                        // Use AOD table as an alternative path for now
                        BMC.Table = Zen.info.aod.Table.rawAodTable;
                    }
                    else
                    {
                        byte[] apcbConfig = WMI.RunCommand(classInstance, cmd.ID);
                        // BiosACPIFunction cmd = new BiosACPIFunction("Get APCB Config", 0x00010001);
                        cmd = GetFunctionByIdString("Get memory voltages");
                        if (cmd != null)
                        {
                            byte[] voltages = WMI.RunCommand(classInstance, cmd.ID);

                            // MEM_VDDIO is ushort, offset 27
                            // MEM_VTT is ushort, offset 29
                            for (int i = 27; i <= 30; i++)
                            {
                                byte value = voltages[i];
                                if (value > 0)
                                    apcbConfig[i] = value;
                            }
                        }

                        BMC.Table = apcbConfig ?? new byte[] { };
                    }

                    float vdimm = Convert.ToSingle(Convert.ToDecimal(BMC.Config.MemVddio) / 1000);
                    if (vdimm > 0 && vdimm < 3)
                    {
                        MemVdimm = $"{vdimm:F4}V";
                        App.LogInfo($"Zen ReadMemoryConfig VDIMM BMC {MemVdimm}");
                    }
                    else if (AsusWmi != null && AsusWmi.Status == 1)
                    {
                        AsusSensorInfo sensor = AsusWmi.FindSensorByName("DRAM Voltage");
                        float temp = 0;
                        bool valid = sensor != null && float.TryParse(sensor.Value, out temp);

                        if (valid && temp > 0 && temp < 3)
                        {
                            MemVdimm = sensor.Value;
                            App.LogInfo($"Zen ReadMemoryConfig VDIMM ASUSWMI {MemVdimm}");
                        }
                    }

                    float vtt = Convert.ToSingle(Convert.ToDecimal(BMC.Config.MemVtt) / 1000);
                    if (vtt > 0)
                    {
                        MemVtt = $"{vtt:F4}V";
                        App.LogInfo($"Zen ReadMemoryConfig VTT BMC {MemVtt}");
                    }

                    // When ProcODT is 0, then all other resistance values are 0
                    // Happens when one DIMM installed in A1 or A2 slot
                    if (BMC.Table == null || Utils.AllZero(BMC.Table) || BMC.Config.ProcODT < 1) return;

                    MemProcODT = BMC.GetProcODTString(BMC.Config.ProcODT);

                    MemClkDrvStren = BMC.GetDrvStrenString(BMC.Config.ClkDrvStren);
                    MemAddrCmdDrvStren = BMC.GetDrvStrenString(BMC.Config.AddrCmdDrvStren);
                    MemCsOdtCmdDrvStren = BMC.GetDrvStrenString(BMC.Config.CsOdtCmdDrvStren);
                    MemCkeDrvStren = BMC.GetDrvStrenString(BMC.Config.CkeDrvStren);

                    MemRttNom = BMC.GetRttString(BMC.Config.RttNom);
                    MemRttWr = BMC.GetRttWrString(BMC.Config.RttWr);
                    MemRttPark = BMC.GetRttString(BMC.Config.RttPark);

                    MemAddrCmdSetup = $"{BMC.Config.AddrCmdSetup}";
                    MemCsOdtSetup = $"{BMC.Config.CsOdtSetup}";
                    MemCkeSetup = $"{BMC.Config.CkeSetup}";
                }
                else
                {
                    if (Utils.AllZero(Zen.info.aod.Table.rawAodTable))
                        return;

                    AOD.AodData Data = Zen.info.aod.Table.Data;

                    MemVddio = $"{Data.MemVddio / 1000.0:F4}V";
                    MemVddq = $"{Data.MemVddq / 1000.0:F4}V";
                    MemVpp = $"{Data.MemVpp / 1000.0:F4}V";

                    MemProcODT = AOD.GetProcODTString(Data.ProcODT);
                    MemCadBusDrvStren = AOD.GetCadBusDrvStrenString(Data.CadBusDrvStren);
                    MemDramDataDrvStren = AOD.GetDramDataDrvStrenString(Data.DramDataDrvStren);
                    MemProcDataDrvStren = AOD.GetProcDataDrvStrenString(Data.ProcDataDrvStren);

                    MemRttWrD5 = RttToString(Data.RttWr);
                    MemRttNomWr = RttToString(Data.RttNomWr);
                    MemRttNomRd = RttToString(Data.RttNomRd);
                    MemRttParkD5 = RttToString(Data.RttPark);
                    MemRttParkDqs = RttToString(Data.RttParkDqs);

                }
            }
            catch (Exception ex)
            {
                App.LogExError($"ReadMemoryConfig Exception: {ex.Message}", ex);
            }

            BMC.Dispose();
        }
        private string RttToString(int rtt)
        {
            if (rtt > 0)
                return $"{AOD.GetRttString(rtt)} ({240 / rtt})";
            return $"{AOD.GetRttString(rtt)}";
        }
        private bool ReadChannelsInfo()
        {
            try
            {
                int dimmIndex = 0;
                int channelsPerDimm = MEMCFG.Type == MemType.DDR5 ? 2 : 1;

                // Get the offset by probing the IMC0 to IMC7
                // It appears that offsets 0x80 and 0x84 are DIMM config registers
                // When a DIMM is DR, bit 0 is set to 1
                // 0x50000
                // offset 0, bit 0 when set to 1 means DIMM1 is installed
                // offset 8, bit 0 when set to 1 means DIMM2 is installed
                for (var i = 0; i < 8; i++)
                {
                    uint channelOffset = (uint)i << 20;
                    bool channel = Utils.GetBits(Zen.ReadDword(channelOffset | 0x50DF0), 19, 1) == 0;
                    bool dimm1 = Utils.GetBits(Zen.ReadDword(channelOffset | 0x50000), 0, 1) == 1;
                    bool dimm2 = Utils.GetBits(Zen.ReadDword(channelOffset | 0x50008), 0, 1) == 1;

                    try
                    {
                        if (channel && (dimm1 || dimm2))
                        {
                            if (dimm1)
                            {
                                MemoryModule module = modules[dimmIndex++];
                                module.Slot = $"{Convert.ToChar(i / channelsPerDimm + 65)}1";
                                module.DctOffset = channelOffset;
                                module.Rank = (MemRank)Utils.GetBits(Zen.ReadDword(channelOffset | 0x50080), 0, 1);
                            }

                            if (dimm2)
                            {
                                MemoryModule module = modules[dimmIndex++];
                                module.Slot = $"{Convert.ToChar(i / channelsPerDimm + 65)}2";
                                module.DctOffset = channelOffset;
                                module.Rank = (MemRank)Utils.GetBits(Zen.ReadDword(channelOffset | 0x50084), 0, 1);
                            }
                        }
                    }
                    catch { }

                }
                return true;
            }
            catch (Exception ex)
            {
                App.LogExError($"ReadChannelsInfo Exception: {ex.Message}", ex);
                return false;
            }
        }
        private void ReadTimings(uint offset = 0)
        {
            try
            {
                uint config = Zen.ReadDword(offset | 0x50100);

                MEMCFG.Type = (MemType)Utils.GetBits(config, 0, 2);

                uint powerDown = Zen.ReadDword(offset | 0x5012C);
                uint umcBase = Zen.ReadDword(offset | 0x50200);
                uint bgsa0 = Zen.ReadDword(offset | 0x500D0);
                uint bgsa1 = Zen.ReadDword(offset | 0x500D4);
                uint bgs0 = Zen.ReadDword(offset | 0x50050);
                uint bgs1 = Zen.ReadDword(offset | 0x50058);
                uint timings5 = Zen.ReadDword(offset | 0x50204);
                uint timings6 = Zen.ReadDword(offset | 0x50208);
                uint timings7 = Zen.ReadDword(offset | 0x5020C);
                uint timings8 = Zen.ReadDword(offset | 0x50210);
                uint timings9 = Zen.ReadDword(offset | 0x50214);
                uint timings10 = Zen.ReadDword(offset | 0x50218);
                uint timings11 = Zen.ReadDword(offset | 0x5021C);
                uint timings12 = Zen.ReadDword(offset | 0x50220);
                uint timings13 = Zen.ReadDword(offset | 0x50224);
                uint timings14 = Zen.ReadDword(offset | 0x50228);
                uint timings15 = Zen.ReadDword(offset | 0x50230);
                uint timings16 = Zen.ReadDword(offset | 0x50234);
                uint timings17 = Zen.ReadDword(offset | 0x50250);
                uint timings18 = Zen.ReadDword(offset | 0x50254);
                uint timings19 = Zen.ReadDword(offset | 0x50258);
                uint trfcTimings0 = Zen.ReadDword(offset | 0x50260);
                uint trfcTimings1 = Zen.ReadDword(offset | 0x50264);
                uint trfcTimings2 = Zen.ReadDword(offset | 0x50268);
                uint trfcTimings3 = Zen.ReadDword(offset | 0x5026C);
                uint timings22 = Zen.ReadDword(offset | 0x5028C);
                uint trfcRegValue = 0;

                if (MEMCFG.Type == MemType.DDR4)
                {
                    trfcRegValue = trfcTimings0 != trfcTimings1 ? (trfcTimings0 != 0x21060138 ? trfcTimings0 : trfcTimings1) : trfcTimings0;
                }
                else if (MEMCFG.Type == MemType.DDR5)
                {
                    uint[] ddr5Regs = { trfcTimings0, trfcTimings1, trfcTimings2, trfcTimings3 };
                    foreach (uint reg in ddr5Regs)
                    {
                        if (reg != 0x00C00138)
                        {
                            trfcRegValue = reg;
                            break;
                        }
                    }
                }

                float configured = MEMCFG.Frequency;
                float ratio = MEMCFG.Type == MemType.DDR4 ? Utils.GetBits(umcBase, 0, 7) / 3.0f : Utils.GetBits(umcBase, 0, 16) / 100.0f;
                float freqFromRatio = ratio * 200;

                MEMCFG.Ratio = ratio;

                // Fallback to ratio when ConfiguredClockSpeed fails
                if (configured == 0.0f || freqFromRatio > configured)
                {
                    MEMCFG.Frequency = freqFromRatio;
                }

                if (CpuBusClock > 0)
                {
                    MEMCFG.Frequency = (float)Math.Round(MEMCFG.Frequency / 100 * CpuBusClock, 0);
                }

                MEMCFG.BGS = bgs0 == 0x87654321 && bgs1 == 0x87654321 ? "Disabled" : "Enabled";
                MEMCFG.BGSAlt = Utils.GetBits(bgsa0, 4, 7) > 0 || Utils.GetBits(bgsa1, 4, 7) > 0
                    ? "Enabled"
                    : "Disabled";
                int GDM_BIT = MEMCFG.Type == MemType.DDR4 ? 11 : 18;
                MEMCFG.GDM = Utils.GetBits(umcBase, GDM_BIT, 1) > 0 ? "Enabled" : "Disabled";
                int CMD2T_BIT = MEMCFG.Type == MemType.DDR4 ? 10 : 17;
                MEMCFG.Cmd2T = Utils.GetBits(umcBase, CMD2T_BIT, 1) > 0 ? "2T" : "1T";

                MEMCFG.CL = Utils.GetBits(timings5, 0, 6);
                MEMCFG.RAS = Utils.GetBits(timings5, 8, 7);
                MEMCFG.RCDRD = Utils.GetBits(timings5, 16, 6);
                MEMCFG.RCDWR = Utils.GetBits(timings5, 24, 6);

                MEMCFG.RC = Utils.GetBits(timings6, 0, 8);
                MEMCFG.RP = Utils.GetBits(timings6, 16, 6);

                MEMCFG.RRDS = Utils.GetBits(timings7, 0, 5);
                MEMCFG.RRDL = Utils.GetBits(timings7, 8, 5);
                MEMCFG.RTP = Utils.GetBits(timings7, 24, 5);

                MEMCFG.FAW = Utils.GetBits(timings8, 0, 7);

                MEMCFG.CWL = Utils.GetBits(timings9, 0, 6);
                MEMCFG.WTRS = Utils.GetBits(timings9, 8, 5);
                MEMCFG.WTRL = Utils.GetBits(timings9, 16, 7);

                MEMCFG.WR = Utils.GetBits(timings10, 0, 7);

                MEMCFG.TRCPAGE = Utils.GetBits(timings11, 20, 12);

                MEMCFG.RDRDDD = Utils.GetBits(timings12, 0, 4);
                MEMCFG.RDRDSD = Utils.GetBits(timings12, 8, 4);
                MEMCFG.RDRDSC = Utils.GetBits(timings12, 16, 4);
                MEMCFG.RDRDSCL = Utils.GetBits(timings12, 24, 6);

                MEMCFG.WRWRDD = Utils.GetBits(timings13, 0, 4);
                MEMCFG.WRWRSD = Utils.GetBits(timings13, 8, 4);
                MEMCFG.WRWRSC = Utils.GetBits(timings13, 16, 4);
                MEMCFG.WRWRSCL = Utils.GetBits(timings13, 24, 6);

                MEMCFG.RDWR = Utils.GetBits(timings14, 8, 6);
                MEMCFG.WRRD = Utils.GetBits(timings14, 0, 4);

                MEMCFG.REFI = Utils.GetBits(timings15, 0, 16);

                MEMCFG.MODPDA = Utils.GetBits(timings16, 24, 6);
                MEMCFG.MRDPDA = Utils.GetBits(timings16, 16, 6);
                MEMCFG.MOD = Utils.GetBits(timings16, 8, 6);
                MEMCFG.MRD = Utils.GetBits(timings16, 0, 6);

                MEMCFG.STAG = Utils.GetBits(timings17, 16, 11);

                MEMCFG.XP = Utils.GetBits(timings18, 0, 6);
                MEMCFG.CKE = Utils.GetBits(timings18, 24, 5);

                MEMCFG.PHYWRL = Utils.GetBits(timings19, 8, 8);
                MEMCFG.PHYRDL = Utils.GetBits(timings19, 16, 8);
                MEMCFG.PHYWRD = Utils.GetBits(timings19, 24, 3);

                if (MEMCFG.Type == MemType.DDR4)
                {
                    MEMCFG.RFC = Utils.GetBits(trfcRegValue, 0, 11);
                    MEMCFG.RFC2 = Utils.GetBits(trfcRegValue, 11, 11);
                    MEMCFG.RFC4 = Utils.GetBits(trfcRegValue, 22, 11);
                }

                if (MEMCFG.Type == MemType.DDR5)
                {
                    MEMCFG.RFC = Utils.GetBits(trfcRegValue, 0, 16);
                    MEMCFG.RFC2 = Utils.GetBits(trfcRegValue, 16, 16);
                    uint[] temp = {
                    Utils.GetBits(Zen.ReadDword(offset | 0x502c0), 0, 11),
                    Utils.GetBits(Zen.ReadDword(offset | 0x502c4), 0, 11),
                    Utils.GetBits(Zen.ReadDword(offset | 0x502c8), 0, 11),
                    Utils.GetBits(Zen.ReadDword(offset | 0x502cc), 0, 11),
                };
                    foreach (uint value in temp)
                    {
                        if (value != 0)
                        {
                            MEMCFG.RFCsb = value;
                            break;
                        }
                    }
                }

                MEMCFG.PowerDown = Utils.GetBits(powerDown, 28, 1) == 1 ? "Enabled" : "Disabled";
            }
            catch (Exception ex)
            {
                App.LogExError($"ReadTimings Exception: {ex.Message}", ex);
            }

        }
        private BiosACPIFunction GetFunctionByIdString(string name)
        {
            return biosFunctions.Find(x => x.IDString == name);
        }
        public void UpdateLoadThread(int _thread, int _value)
        {
            if (!App.MainWindowOpen) return;
            try
            {
                cpuTload[_thread] = _value;
                //App.LogInfo($"{_value}");
            }
            catch { }
        }
        public void UpdateLoadThreads()
        {
            if (!App.MainWindowOpen) return;
            try
            {
                OnChange("CpuTload");
            }
            catch { }
        }
        public void UpdateLiveCPUTemp(string _value)
        {
            try
            {
                LiveCPUTemp = _value.Length > 0 ? _value : "N/A";
                //App.LogInfo($"{_value}");
                OnChange("LiveCPUTemp");
            }
            catch { }
        }
        public void UpdateLiveCPUPower(string _value)
        {
            try
            {
                LiveCPUPower = _value.Length > 0 ? _value : "N/A";
                //App.LogInfo($"{_value}");
                OnChange("LiveCPUPower");
            }
            catch { }
        }
        public void UpdateLiveCPUClock(string _value)
        {
            try
            {
                LiveCPUClock = _value.Length > 0 ? _value : "N/A";
                //App.LogInfo($"{_value}");
                OnChange("LiveCPUClock");
            }
            catch { }
        }
        public void UpdateLiveCPUAdditional(string _value)
        {
            try
            {
                LiveCPUAdditional = _value.Length > 0 ? _value : "N/A";
                //App.LogInfo($"{_value}");
                OnChange("LiveCPUAdditional");
            }
            catch { }
        }
        public void UpdateLiveFinished(string _value)
        {
            try
            {
                LiveFinished = _value.Length > 0 ? _value : "N/A";
                //App.LogInfo($"{_value}");
                OnChange("LiveFinished");
            }
            catch { }
        }
       
        public void UpdateCpuTotalLoad(double _value)
        {
            if (!App.MainWindowOpen) return;
            try
            {
                DispatcherOperationStatus result = App.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Send,
                    new Action(() => {
                        App.systemInfo.LiveCpuLoad = _value > 0 ? $"{_value:F0}%" : "N/A";
                    })).Wait();
                if (result == DispatcherOperationStatus.Completed) { OnChange("LiveCpuLoad"); }
               
            }
            catch { }
        }
        public void SetProgress(int progress, string content = "")
        {
            return;
            try
            {
                DispatcherOperationStatus result = App.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Input,
                    new Action(() => {
                        App.splashInfo.SetProgress(progress, content);
                    })).Wait();
                if (result == DispatcherOperationStatus.Completed) { return; }
            }
            catch { }
        }

        public void SetLastVersionOnServer(string _value)
        {
            try
            {
                LastVersionOnServer = _value.Length > 0 ? _value : "N/A";
                //App.LogInfo($"{_value}");
                OnChange("LastVersionOnServer");
            }
            catch { }
        }

        public void SetThreadBoosterStatus(string _value)
        {
            try
            {
                ThreadBoosterStatus = _value.Length > 0 ? _value : "N/A";
                //App.LogInfo($"{_value}");
                //if (App.thrThreadBooster.ThreadState == System.Threading.ThreadState.Running || App.thrThreadBooster.ThreadState == System.Threading.ThreadState.Background || App.tbtimer.Enabled == true)
                if (App.thrThreadBoosterRunning)
                {
                    ThreadBoosterButton = "Stop";
                }
                else
                {
                    ThreadBoosterButton = "Start";
                }
                OnChange("ThreadBoosterStatus");
                OnChange("ThreadBoosterButton");
            }
            catch { }
        }

        public void SetSleepsAllowed()
        {
            if (!App.MainWindowOpen) return;
            try
            {
                bool _sleep = App.IsPwrSuspendAllowed();
                bool _hibern = App.IsPwrHibernateAllowed();
                SleepAllowed = _sleep ? "System Standby is available" : "System Standby is disabled";
                HiberAllowed = _hibern ? "Hibernation is available" : "Hibernation is disabled";

                OnChange("SleepAllowed");
                OnChange("HiberAllowed");
            }
            catch { }
        }

        public void UpdateSSHStatus(bool _status)
        {
            try
            {
                SSHStatus = _status == true ? $"Enabled" : "Disabled";
                if (_status == true) SSHStatus = App.thrSysRunning && App.pactive.SysSetHack ? $"{SSHStatus} {ThreadBooster.CountBits(App.lastSysCpuSetMask)}/{ThreadBooster.CountBits(ThreadBooster.defFullBitMask)}" : $"{SSHStatus} (Inactive)";
                SSHStatus = App.thrThreadBoosterRunning ? SSHStatus : $"<Inactive> {SSHStatus}";
            }
            catch { }
        }

        public void SetSSHStatus(bool _status)
        {
            try
            {
                UpdateSSHStatus(_status);
                TogglePSA = _status ? $"Disable SSH" : "Enable SSH";
                App.StripItemToggleSSH.Text = _status ? $"Disable SSH" : "Enable SSH";
                App.StripItemToggleSSH.Checked = _status;
                OnChange("SSHStatus");
                OnChange("ToggleSSH");
            }
            catch { }
        }

        public void UpdatePSAStatus(bool _status)
        {
            try
            {
                string sleep = "", mode = "";
                if (_status)
                {
                    PSABias = App.GetPSABiasLabel();
                    sleep = App.psact_deep_b ? " [Deep Sleep]" : App.psact_light_b ? " [Light Sleep]" : "";
                    mode = ThreadBooster.GameMode ? " [GameMode]" : ThreadBooster.ActiveMode ? " [ActiveMode]" : "";
                    mode += ThreadBooster.FocusAssist ? " [FocusAssist]" : "";
                    mode += ThreadBooster.UserNotification ? " [UserNotification]" : "";
                    mode += ThreadBooster.PLEvtPerfMode ? " [PL PerfMode]" : "";
                    if (App.pactive.SelectedPersonality == 1)
                    {
                        mode += App.pactive.SelectedPersonality == 1 ? $" [{ThreadBooster.CurrentOverlay}]" : "";
                    }
                    mode += ThreadBooster.PLEvtPerfMode ? " [PL PerfMode]" : "";
                }

                if (App.PPImportErrStatus) sleep = "[Error Initialization]";
                PSAStatus = _status ? $"Enabled {PSABias}{sleep}{mode}" : "Disabled";
                TogglePSA = _status ? $"Disable PSA" : "Enable PSA";
                PSAStatus = App.psact_b ? PSAStatus : $"<Inactive> {PSAStatus}";
            }
            catch { }
        }
        public void SetPSAStatus(bool _status)
        {
            try
            {
                UpdatePSAStatus(_status);
                App.StripItemTogglePSA.Text = _status ? $"Disable PSA" : "Enable PSA";
                App.StripItemTogglePSA.Checked = _status;
                OnChange("PSAStatus");
                OnChange("PSABias");
                OnChange("TogglePSA");
            }
            catch { }
        }
        public void UpdateN0Status(bool _status)
        {
            try
            {
                N0Status = _status ? $"Enabled" : "Disabled";
                if (_status == true) N0Status = App.numazero_b ? $"{N0Status} Selected: {App.n0enabledT0.Count() + App.n0enabledT1.Count()}T Excluded: {App.n0disabledT0.Count() + App.n0disabledT1.Count()}T" : $"{N0Status} (Inactive)";
                N0Status = App.thrThreadBoosterRunning ? N0Status : $"<Inactive> {N0Status}";
            }
            catch { }
        }
        public void SetN0Status(bool _status)
        {
            try
            {
                UpdateN0Status(_status);
                ToggleN0 = _status ? $"Disable NumaZero" : "Enable NumaZero";
                OnChange("ToggleN0");
                OnChange("N0Status");
                App.StripItemToggleNumaZero.Text = _status ? $"Disable NumaZero" : "Enable NumaZero";
                App.StripItemToggleNumaZero.Checked = _status;
                /*
                if (App.Current.Dispatcher.CheckAccess())
                    App.StripItemToggleNumaZero.Text = _status ? $"Disable NumaZero" : "Enable NumaZero";
                else
                    App.Current.Dispatcher.Invoke(new Action(() => App.StripItemToggleNumaZero.Text = _status ? $"Disable NumaZero" : "Enable NumaZero"));
                */
            }
            catch { }
        }

        protected void OnChange(string info)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(info));
                }
            }
            catch { }
        }

        public class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }
            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
            }
            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }
    }
}
