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
        public string CPUBits { get; set; }
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
        public string CpuVtt { get; set; }
        public double CpuBusClock { get; set; }
        public bool CpuSHAExt { get; set; }
        public bool CpuVAESExt { get; set; }
        public double TBLoopTime { get; set; }
        public double TBLoopEvery { get; set; }
        public string ThreadBoosterStatus { get; set; }
        public string ThreadBoosterButton { get; set; }
        public bool ThreadBoosterRunning { get; set; }

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
            CPUBits = "N/A";
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

                WmiBasics();

                if (CPULogicalProcessors > CPUCores) HyperThreading = true;

                CPPCTagsInit();

                CpuIdInit();

                CpuSetInit();

                GetWindowsLabel();

                ZenMainInit();

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
        private uint BitSlice(uint arg, int start, int end)
        {
            uint mask = (2U << end - start) - 1U;
            return arg >> start & mask;
        }
        public bool ZenRefreshStatic(bool refresh)
        {
            if (!ZenStates) return false;

            if (refresh)
            {
                bool _refreshpt = ZenRefreshPowerTable();

                if (!_refreshpt) return false;
            }

            if (ZenPTVersion == 0x380804)
            {
                ZenPPT = (int)Zen.powerTable.Table[0];
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
                ZenPPT = (int)Zen.powerTable.Table[0];
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
                ZenPPT = (int)Zen.powerTable.Table[0];
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
                ZenPPT = (int)Zen.powerTable.Table[0];
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
                || ZenPTVersion == 0x540000
                || ZenPTVersion == 0x540001
                || ZenPTVersion == 0x540002
                || ZenPTVersion == 0x540003
                || ZenPTVersion == 0x540004)
            {
                ZenPPT = (int)Zen.powerTable.Table[2];
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
                ZenPPT = (int)Zen.powerTable.Table[4];
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
                ZenPPT = (int)Zen.powerTable.Table[0];
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
                ZenPPT = (int)Zen.powerTable.Table[0];
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

            App.LogDebug($"ZenRefreshStatic done");
            return true;
        }
        public bool ZenRefreshPowerTable()
        {
            try
            {
                SMU.Status? status = SMU.Status.UNKNOWN_CMD;

                if (Ring0.WaitPciBusMutex(50))
                {
                    status = Zen.RefreshPowerTable();
                    Ring0.ReleasePciBusMutex();
                }

                if (status != SMU.Status.OK)
                {
                    for (int r = 0; r < 10; ++r)
                    {
                        Thread.Sleep(25);
                        if (Ring0.WaitPciBusMutex(50))
                        {
                            status = ZenRefreshPowerTable2();
                            Ring0.ReleasePciBusMutex();
                        }
                        if (status == SMU.Status.OK) r = 99;
                    }
                }

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
            if (margin > 30)
                margin = 30;
            else if (margin < -30)
                margin = -30;

            int offset = margin < 0 ? 0x100000 : 0;
            return Convert.ToUInt32(offset + margin) & 0xffff;
        }

        public float GetPBOScalar()
        {
            int _wait = Zen.GetOcMode() ? 500 : 10;
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
        }
        public bool SetPsmCounts(int margin)
        {
            uint m = MakePsmMarginArg(margin);
            uint[] args = MakeCmdArgs(m);
            int _wait = Zen.GetOcMode() ? 500 : 10;
            if (Ring0.WaitPciBusMutex(_wait))
            {
                SMU.Status status = Zen.smu.SendMp1Command(Zen.smu.Mp1Smu.SMU_MSG_SetAllDldoPsmMargin, ref args);
                Ring0.ReleasePciBusMutex();
                if (status == SMU.Status.OK) return true;
            }
            return false;
        }
        public int? GetPsmCount(uint core)
        {
            uint[] args = new uint[6];
            args[0] = (uint)((uint)((core & 8) << 5 | (core & 7)) << 20);

            int _wait = Zen.GetOcMode() ? 500 : 10;
            if (Ring0.WaitPciBusMutex(_wait))
            {
                if (Zen.smu.SendMp1Command(Zen.smu.Mp1Smu.SMU_MSG_GetDldoPsmMargin, ref args) == SMU.Status.OK)
                {
                    Ring0.ReleasePciBusMutex();
                    return (int)args[0];
                }
                Ring0.ReleasePciBusMutex();
            }
            return null;
        }
        public bool SetPsmCount(int core, int count)
        {
            uint[] args = new uint[6];
            args[0] = (uint)(((core & 8) << 5 | (core & 7)) << 20 | (count & 65535));
            int _wait = Zen.GetOcMode() ? 500 : 10;
            if (Ring0.WaitPciBusMutex(_wait))
            {
                SMU.Status status = Zen.smu.SendMp1Command(Zen.smu.Mp1Smu.SMU_MSG_SetDldoPsmMargin, ref args);
                Ring0.ReleasePciBusMutex();
                if (status == SMU.Status.OK) return true;
            }
            return false;
        }

        public void ZenRefreshCO()
        {
            try
            {
                if (Zen.smu.SMU_TYPE == SMU.SmuType.TYPE_CPU3 && CPUCores <= ZenCoreMap.Length)
                {
                    ZenCOLabel = "";
                    for (int ix = 0; ix < CPUCores; ix++)
                    {
                        int? count = GetPsmCount((uint)ZenCoreMap[ix]);
                        ZenCO[ix] = count != null ? (int)count : 0;
                    }
                    for (int ic = 0; ic < CPUCores; ic++)
                    {
                        ZenCOLabel += String.Format("{0}#{1} ", ic, ZenCO[ic].ToString("+#;-#;0"));
                        if (ic != CPUCores - 1) ZenCOLabel += ", ";
                    }
                    ZenCOb = true;
                    App.LogDebug($"ZenRefreshCO: {string.Join(", ", ZenCO)}");
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
                BoardLabel = $"{BoardManufacturer}\n{BoardModel} [BIOS Version {BoardBIOS}]";

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

                if (MemPartNumbers.Any())
                {
                    foreach (var mempart in MemPartNumbers)
                    {
                        if (_MemoryLabel1.Length > 0) _MemoryLabel1 += $"\n";
                        _MemoryLabel1 += $"{mempart}";
                    }
                }

                if (MemVdimm.Length > 0)
                {
                    _MemoryLabel2 += $"VDIMM: {MemVdimm}";
                    if (MemVtt.Length > 0)
                        _MemoryLabel2 += $" VTT: {MemVtt}";
                }

                if (ZenStates)
                {
                    if (_MemoryLabel2.Length > 0) _MemoryLabel2 += " ";
                    if (_MemoryLabel2.Length == 0 && MEMCFG.Frequency > 0) _MemoryLabel2 += $"\n";
                    if (MEMCFG.Frequency > 0)
                        _MemoryLabel2 += $"Clock: {MEMCFG.Frequency} MHz";
                }

                MemoryLabel = _MemoryLabel1;
                if (_MemoryLabel1.Length > 0 && _MemoryLabel2.Length > 0) MemoryLabel += "\n";
                if (_MemoryLabel2.Length > 0) MemoryLabel += $"{_MemoryLabel2}";

                if (MemoryLabel.Length == 0) MemoryLabel = "N/A";

                if (object.ReferenceEquals(null, Zen)) ZenInit();

                if (ZenStates)
                {

                    for (int r = 0; r < 20; ++r)
                    {
                        if (Ring0.WaitPciBusMutex(50))
                        {
                            Zen.RefreshPowerTable();
                            ZenPPT = Zen.GetPPTLimit();
                            ZenBoost = Zen.GetBoostLimit();
                            ZenScalar = Zen.GetPBOScalar();
                            r = 99;
                            Ring0.ReleasePciBusMutex();
                        }
                        else
                        {
                            Thread.Sleep(25);
                        }
                    }

                    string _CPULabel = "";
                    if (ZenPPT > 0 && ZenMaxPPT > 0 && ZenPPT != ZenMaxPPT)
                    {
                        _CPULabel += $"PPT: {string.Format("{0:N0}/{1:N0}W", ZenPPT, ZenMaxPPT)} ";
                    } 
                    else if (ZenPPT > 0)
                    {
                        _CPULabel += $"PPT: {string.Format("{0:N0}W", ZenPPT)} ";
                    }
                    if (Zen.powerTable.TDC > 0 && ZenMaxTDC > 0 && Zen.powerTable.TDC != ZenMaxTDC)
                    {
                        _CPULabel += $"TDC: {string.Format("{0:N0}/{1:N0}A", Zen.powerTable.TDC, ZenMaxTDC)} ";
                    }
                    else if (Zen.powerTable.TDC > 0)
                    {
                        _CPULabel += $"TDC: {string.Format("{0:N0}A", Zen.powerTable.TDC)} ";
                    }
                    if (Zen.powerTable.EDC > 0 && ZenMaxEDC > 0 && Zen.powerTable.EDC != ZenMaxEDC)
                    {
                        _CPULabel += $"EDC: {string.Format("{0:N0}/{1:N0}A", Zen.powerTable.EDC, ZenMaxEDC)} ";
                    }
                    else if (Zen.powerTable.EDC > 0)
                    {
                        _CPULabel += $"EDC: {string.Format("{0:N0}A", Zen.powerTable.EDC)} ";
                    }
                    if (ZenScalar > 0) _CPULabel += $"Scalar: {ZenScalar}x ";
                    if (Zen.powerTable.THM > 0 && ZenMaxTHM > 0 && Zen.powerTable.THM != ZenMaxTHM)
                    {
                        _CPULabel += $"THM: {string.Format("{0:N0}/{1:N0}°C", Zen.powerTable.THM, ZenMaxTHM)} ";
                    }
                    else if (Zen.powerTable.THM > 0)
                    {
                        _CPULabel += $"THM: {string.Format("{0:N0}°C", Zen.powerTable.THM)} ";
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
                        CPPCTagsLabel += String.Format("{0}#{1}", i, CPPCTags[i]);
                        if (i != CPPCTags.Length - 1) CPPCTagsLabel += ", ";
                    }
                    App.LogInfo($"CPPC: {CPPCTagsLabel}");

                    CPPC = new int[CPUCores, 2];

                    for (int ii = 0; ii < CPPC.GetLength(0); ii++)
                    {
                        CPPC[ii, 0] = -100;
                        CPPC[ii, 1] = -100;
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

                for (int j = 0; j < CPULogicalProcessors; j++)
                {
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
                        CPPC[ii, 0] = -100;
                        CPPC[ii, 1] = -100;
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
                for (int i = 0; i < CPULogicalProcessors; ++i)
                {
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
                if (!ZenDLLFail)
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
                App.LogDebug($"ZenInit Exception: {ex.Message}");
                if (ex.Message.Contains("inpoutx64.dll")) ZenDLLFail = true;
                return false;
            }
        }
        public void ZenMainInit()
        {
            try
            {
                if (CPUSocket == "AM4" || CPUSocket == "AM5")
                {
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
                            App.LogInfo($"Zen OCMode: {Zen.GetOcMode()}");
                            //App.LogInfo($"Zen Base Clock: {Zen.baseClock} MHz [x{Zen.baseMulti}]");
                        }
                        else
                        {
                            App.LogInfo($"ZenCore DLL: CPU not supported");
                        }

                    }
                    catch (Exception ex)
                    {
                        App.LogInfo($"ZenCore DLL couldn't be loaded: {ex.Message}");
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

                        for (int r = 0; r < 20; ++r)
                        {
                            if (Ring0.WaitPciBusMutex(50))
                            {
                                ZenMaxBoost = Zen.GetMaxBoostLimit();
                                ZenMaxPPT = Zen.GetMaxPPTLimit();
                                ZenMaxTDC = Zen.GetMaxTDCLimit();
                                ZenMaxEDC = Zen.GetMaxEDCLimit();
                                ZenMaxTHM = Zen.GetMaxTHMLimit();
                                r = 99;
                                Ring0.ReleasePciBusMutex();
                            }
                            else
                            {
                                Thread.Sleep(25);
                            }
                        }

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

                        uint[] args = new uint[6];
                        uint cmd = Zen.smu.Hsmp.ReadBoostLimit;

                        SMU.Status status = Zen.smu.SendHsmpCommand(cmd, ref args);
                        if (status == SMU.Status.OK)
                        {
                            ZenBoost = (int)args[0];
                        }
                        else
                        {
                            App.LogInfo($"Failed SMU check for BoostClock: {status}");
                        }

                        uint ccd_fuse1a = 0x5D218;
                        uint ccd_fuse2a = 0x5D21C;
                        uint core_fuse1a = 0x30081800 + 0x238;
                        uint core_fuse2a = 0x30081800 + 0x238 + 0x2000000;

                        if (Zen.info.family == Cpu.Family.FAMILY_17H && Zen.info.model != 0x71)
                        {
                            ccd_fuse1a += 0x40;
                            ccd_fuse2a += 0x40;
                        }
                        uint ZenCCD_Fuse1 = Zen.ReadDword(ccd_fuse1a);
                        uint ZenCCD_Fuse2 = Zen.ReadDword(ccd_fuse2a);

                        if (Zen.info.family == Cpu.Family.FAMILY_19H)
                        {
                            core_fuse1a = 0x30081800 + 0x598;
                            core_fuse2a = 0x30081800 + 0x598 + 0x2000000;
                        }
                        uint ZenCore_Fuse1 = Zen.ReadDword(core_fuse1a);
                        uint ZenCore_Fuse2 = Zen.ReadDword(core_fuse2a);
                        uint ZenCCDS_Total = BitSlice(ZenCCD_Fuse1, 22, 23);
                        uint ZenCCDS_Disabled = BitSlice(ZenCCD_Fuse1, 30, 31);
                        uint ZenCCDS_Total2 = BitSlice(ZenCCD_Fuse2, 22, 23);
                        uint ZenCCDS_Disabled2 = BitSlice(ZenCCD_Fuse2, 30, 31);
                        uint ZenCCD1_Fuse = BitSlice(ZenCore_Fuse1, 0, 7);
                        uint ZenCCD2_Fuse = BitSlice(ZenCore_Fuse2, 0, 7);

                        uint ZenCore_Layout = ZenCCD1_Fuse | ZenCCD2_Fuse << 8 | 0xFFFF0000;
                        int ZenCores_per_ccd = (ZenCCD1_Fuse == 0 || ZenCCD2_Fuse == 0) ? 8 : 6;
                        int ZenCCD_Total = CountSetBits(ZenCCDS_Total);
                        int ZenCCD_Total2 = CountSetBits(ZenCCDS_Total2);
                        int ZenCCD_Disabled = CountSetBits(ZenCCDS_Disabled);

                        uint cores_t = ZenCore_Layout;

                        ZenCCDTotal = (int)Zen.info.topology.ccds;

                        App.LogInfo($"ZenCCD_Total {ZenCCD_Total:X2}");
                        App.LogInfo($"ZenCCD_Total2 {ZenCCD_Total2:X2}");
                        App.LogInfo($"ZenCore_Fuse1 {ZenCore_Fuse1:X8}");
                        App.LogInfo($"ZenCore_Fuse2 {ZenCore_Fuse2:X8}");
                        App.LogInfo($"ZenCCD_Disabled {ZenCCDS_Disabled:X2}");
                        App.LogInfo($"ZenCCD_Disabled2 {ZenCCDS_Disabled2:X2}");
                        App.LogInfo($"ZenCCD_Fuse1 {ZenCCD_Fuse1:X8}");
                        App.LogInfo($"ZenCCD_Fuse2 {ZenCCD_Fuse2:X8}");
                        App.LogInfo($"ZenCCD1_Fuse {ZenCCD1_Fuse:X8}");
                        App.LogInfo($"ZenCCD2_Fuse {ZenCCD2_Fuse:X8}");
                        App.LogInfo($"ZenCore_Layout {ZenCore_Layout:X8}");
                        App.LogInfo($"ZenCores_per_ccd {ZenCores_per_ccd}");

                        if (ZenCCD_Total > 0 && cores_t > 0)
                        {
                            ZenCoreMap = new int[CountSetBits(~ZenCore_Layout)];
                            int last = ZenCCD_Total * 8;
                            for (int i = 0, k = 0; i < ZenCCD_Total * 8; cores_t = cores_t >> 1)
                            {
                                ZenCoreMapLabel += (i == 0) ? "[" : (i % 8 != 0) ? "." : "";
                                if ((cores_t & 1) == 0)
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

                        ZenRefreshCO();

                        bool _refreshpt = ZenRefreshPowerTable();

                        if (_refreshpt || ver_maj > 0)
                        {
                            bool ZenPTKnown = false;

                            StringBuilder sb = new StringBuilder();

                            ZenPTVersion = (int)Zen.GetTableVersion();

                            line = $"SMU Ver [{ZenSMUVer}] PT Ver [0x{ZenPTVersion:X}]";
                            App.LogInfo(line);

                            sb.AppendLine(line);
                            sb.AppendLine($"CPUName {CPUName}");
                            sb.AppendLine($"CPUFamily {CPUFamily:X}h");

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
                            else if (ZenPTVersion == 0x540100 || ZenPTVersion == 0x540101 || ZenPTVersion == 0x540102 || ZenPTVersion == 0x540103 || ZenPTVersion == 0x540104)
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
                            else if (ZenPTVersion == 0x540000 || ZenPTVersion == 0x540001 || ZenPTVersion == 0x540002 || ZenPTVersion == 0x540003 || ZenPTVersion == 0x540004)
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

                            float _cpuVcore, _cpuVsoc;
                            _cpuVcore = Zen.cpuVcore;
                            _cpuVsoc = Zen.cpuVsoc;
                            //App.LogInfo($"_cpuVcore: {_cpuVcore} _cpuVsoc: {_cpuVsoc}");

                            if (_cpuVcore > 0)
                            {
                                App.hwsensors.InitZen(HWSensorName.CPUVoltage, -1);
                            }
                            if (_cpuVsoc > 0)
                            {
                                App.hwsensors.InitZen(HWSensorName.SOCVoltage, -1);
                            }

                            sb.Clear();
                            sb = null;

                            App.LogInfo($"Zen PowerTable Known: {ZenPTKnown}");
                            App.LogInfo($"Zen Boost: {ZenBoost}/{ZenMaxBoost}");
                            App.LogInfo($"Zen PPT: {ZenPPT}/{ZenMaxPPT} Supported?({Zen.info.PPTSupported})");
                            App.LogInfo($"Zen TDC: {ZenTDC}/{ZenMaxTDC} Supported?({Zen.info.TDCSupported})");
                            App.LogInfo($"Zen EDC: {ZenEDC}/{ZenMaxEDC} Supported?({Zen.info.EDCSupported})");
                            App.LogInfo($"Zen THM: {ZenTHM}/{ZenMaxTHM} Supported?({Zen.info.THMSupported})");

                        }
                        else
                        {
                            App.LogInfo($"Failed SMU PowerTable refresh: {status}");
                        }
                    }
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

                var totalCapacity = 0UL;

                foreach (var module in modules)
                {
                    var rank = module.DualRank ? "DR" : "SR";
                    totalCapacity += module.Capacity;
                    MemPartNumbers.Add(
                        $"{module.Slot}: {module.PartNumber} ({module.Capacity / 1024 / (1024 * 1024)}GB, {rank})");
                }

                if (modules[0].ClockSpeed != 0)
                    MEMCFG.Frequency = modules[0].ClockSpeed;

                if (totalCapacity != 0)
                    MEMCFG.TotalCapacity = $"{totalCapacity / 1024 / (1024 * 1024)}GB";
            }
        }

        private void ReadMemoryConfig()
        {
            var scope = @"root\wmi";
            var className = "AMD_ACPI";

            App.LogDebug("Zen ReadMemoryConfig");

            try
            {
                WMI.Connect($@"{scope}");

                var instanceName = WMI.GetInstanceName(scope, className);

                var classInstance = new ManagementObject(scope,
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
                foreach (var functionObject in functionObjects)
                {
                    try
                    {
                        var pack = WMI.InvokeMethod(classInstance, functionObject, "pack", null, 0);
                        if (pack != null)
                        {
                            var ID = (uint[])pack.GetPropertyValue("ID");
                            var IDString = (string[])pack.GetPropertyValue("IDString");
                            var Length = (byte)pack.GetPropertyValue("Length");

                            for (var i = 0; i < Length; ++i)
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

                // Get APCB config from BIOS. Holds memory parameters.
                BiosACPIFunction cmd = GetFunctionByIdString("Get APCB Config");
                if (cmd == null)
                    throw new Exception();

                var apcbConfig = WMI.RunCommand(classInstance, cmd.ID);

                cmd = GetFunctionByIdString("Get memory voltages");
                if (cmd != null)
                {
                    var voltages = WMI.RunCommand(classInstance, cmd.ID);

                    // MEM_VDDIO is ushort, offset 27
                    // MEM_VTT is ushort, offset 29
                    for (var i = 27; i <= 30; i++)
                    {
                        var value = voltages[i];
                        if (value > 0)
                            apcbConfig[i] = value;
                    }
                }

                BMC.Table = apcbConfig;

                // When ProcODT is 0, then all other resistance values are 0
                // Happens when one DIMM installed in A1 or A2 slot
                if (BMC.Table == null || Utils.AllZero(BMC.Table) || BMC.Config.ProcODT < 1) return;

                var vdimm = Convert.ToSingle(Convert.ToDecimal(BMC.Config.MemVddio) / 1000);
                if (vdimm > 0)
                {
                    MemVdimm = $"{vdimm:F3}V";
                    App.LogInfo($"Zen ReadMemoryConfig VDIMM BMC {MemVdimm}");
                }
                else if (AsusWmi != null && AsusWmi.Status == 1)
                {
                    var sensor = AsusWmi.FindSensorByName("DRAM Voltage");
                    if (sensor != null)
                    {
                        MemVdimm = sensor.Value;
                        App.LogInfo($"Zen ReadMemoryConfig VDIMM ASUSWMI {MemVdimm}");
                    }
                }

                var vtt = Convert.ToSingle(Convert.ToDecimal(BMC.Config.MemVtt) / 1000);
                if (vtt > 0)
                {
                    MemVtt = $"{vtt:F3}V";
                    App.LogInfo($"Zen ReadMemoryConfig VTT BMC {MemVtt}");
                }

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
            catch (Exception ex)
            {
                App.LogExError($"ReadMemoryConfig Exception: {ex.Message}", ex);
            }

            BMC.Dispose();
        }

        private bool ReadChannelsInfo()
        {
            try
            {
                int dimmIndex = 0;

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

                    if (channel && (dimm1 || dimm2))
                    {
                        if (dimm1 && modules.Count > 0)
                        {
                            MemoryModule module = modules[dimmIndex++];
                            module.Slot = $"{Convert.ToChar(i + 65)}1";
                            module.DctOffset = channelOffset;
                            module.DualRank = Utils.GetBits(Zen.ReadDword(channelOffset | 0x50080), 0, 1) == 1;
                        }

                        if (dimm2 && modules.Count > 1)
                        {
                            MemoryModule module = modules[dimmIndex++];
                            module.Slot = $"{Convert.ToChar(i + 65)}2";
                            module.DctOffset = channelOffset;
                            module.DualRank = Utils.GetBits(Zen.ReadDword(channelOffset | 0x50084), 0, 1) == 1;
                        }
                    }
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
                uint timings20 = Zen.ReadDword(offset | 0x50260);
                uint timings21 = Zen.ReadDword(offset | 0x50264);
                uint timings22 = Zen.ReadDword(offset | 0x5028C);
                uint timings23 = timings20 != timings21 ? (timings20 != 0x21060138 ? timings20 : timings21) : timings20;

                float configured = MEMCFG.Frequency;
                float ratio = Utils.GetBits(umcBase, 0, 7) / 3.0f;
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
                MEMCFG.GDM = Utils.GetBits(umcBase, 11, 1) > 0 ? "Enabled" : "Disabled";
                MEMCFG.Cmd2T = Utils.GetBits(umcBase, 10, 1) > 0 ? "2T" : "1T";

                MEMCFG.CL = Utils.GetBits(timings5, 0, 6);
                MEMCFG.RAS = Utils.GetBits(timings5, 8, 7);
                MEMCFG.RCDRD = Utils.GetBits(timings5, 16, 6);
                MEMCFG.RCDWR = Utils.GetBits(timings5, 24, 6);

                MEMCFG.RC = Utils.GetBits(timings6, 0, 8);
                MEMCFG.RP = Utils.GetBits(timings6, 16, 6);

                MEMCFG.RRDS = Utils.GetBits(timings7, 0, 5);
                MEMCFG.RRDL = Utils.GetBits(timings7, 8, 5);
                MEMCFG.RTP = Utils.GetBits(timings7, 24, 5);

                MEMCFG.FAW = Utils.GetBits(timings8, 0, 8);

                MEMCFG.CWL = Utils.GetBits(timings9, 0, 6);
                MEMCFG.WTRS = Utils.GetBits(timings9, 8, 5);
                MEMCFG.WTRL = Utils.GetBits(timings9, 16, 7);

                MEMCFG.WR = Utils.GetBits(timings10, 0, 8);

                MEMCFG.TRCPAGE = Utils.GetBits(timings11, 20, 12);

                MEMCFG.RDRDDD = Utils.GetBits(timings12, 0, 4);
                MEMCFG.RDRDSD = Utils.GetBits(timings12, 8, 4);
                MEMCFG.RDRDSC = Utils.GetBits(timings12, 16, 4);
                MEMCFG.RDRDSCL = Utils.GetBits(timings12, 24, 6);

                MEMCFG.WRWRDD = Utils.GetBits(timings13, 0, 4);
                MEMCFG.WRWRSD = Utils.GetBits(timings13, 8, 4);
                MEMCFG.WRWRSC = Utils.GetBits(timings13, 16, 4);
                MEMCFG.WRWRSCL = Utils.GetBits(timings13, 24, 6);

                MEMCFG.RDWR = Utils.GetBits(timings14, 8, 5);
                MEMCFG.WRRD = Utils.GetBits(timings14, 0, 4);

                MEMCFG.REFI = Utils.GetBits(timings15, 0, 16);

                MEMCFG.MODPDA = Utils.GetBits(timings16, 24, 6);
                MEMCFG.MRDPDA = Utils.GetBits(timings16, 16, 6);
                MEMCFG.MOD = Utils.GetBits(timings16, 8, 6);
                MEMCFG.MRD = Utils.GetBits(timings16, 0, 6);

                MEMCFG.STAG = Utils.GetBits(timings17, 16, 8);

                MEMCFG.XP = Utils.GetBits(timings18, 0, 6);
                MEMCFG.CKE = Utils.GetBits(timings18, 24, 5);

                MEMCFG.PHYWRL = Utils.GetBits(timings19, 8, 5);
                MEMCFG.PHYRDL = Utils.GetBits(timings19, 16, 6);
                MEMCFG.PHYWRD = Utils.GetBits(timings19, 24, 3);

                MEMCFG.RFC = Utils.GetBits(timings23, 0, 11);
                MEMCFG.RFC2 = Utils.GetBits(timings23, 11, 11);
                MEMCFG.RFC4 = Utils.GetBits(timings23, 22, 11);

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
            try
            {
                cpuTload[_thread] = _value;
                //App.LogInfo($"{_value}");
            }
            catch { }
        }
        public void UpdateLoadThreads()
        {
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
                if (App.thrThreadBooster.ThreadState == System.Threading.ThreadState.Running || App.thrThreadBooster.ThreadState == System.Threading.ThreadState.Background || App.tbtimer.Enabled == true)
                {
                    ThreadBoosterButton = "Stop";
                    ThreadBoosterRunning = true;
                }
                else
                {
                    ThreadBoosterButton = "Start";
                    ThreadBoosterRunning = false;
                }
                OnChange("ThreadBoosterStatus");
                OnChange("ThreadBoosterButton");
            }
            catch { }
        }

        public void SetPSAStatus(bool _status)
        {
            try
            {
                string sleep = "", mode = "";
                if (_status)
                {
                    PSABias = (App.PSABiasCurrent == 0) ? " [Economizer]" : (App.PSABiasCurrent == 1) ? " [Standard]" : " [Booster]";
                    sleep = App.psact_deep_b ? " [Deep Sleep]" : App.psact_light_b ? " [Light Sleep]" : "";
                    mode = ThreadBooster.GameMode ? " [GameMode]" : ThreadBooster.ActiveMode ? " [ActiveMode]" : "";
                }

                if (App.PPImportErrStatus) sleep = "[Error Initialization]";
                PSAStatus = _status ? $"Enabled {PSABias}{sleep}{mode}" : "Disabled";
                TogglePSA = _status ? $"Toggle PowerSaverActive OFF" : "Toggle PowerSaverActive ON";
                PSAStatus = ThreadBoosterRunning || !App.psact_b ? PSAStatus : $"<Inactive> {PSAStatus}";
                OnChange("PSAStatus");
                OnChange("PSABias");
                OnChange("TogglePSA");
            }
            catch { }
        }

        public void SetSleepsAllowed()
        {
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
        public void SetSSHStatus(bool _status)
        {
            try
            {
                SSHStatus = _status == true ? $"Enabled" : "Disabled";
                if (_status == true) SSHStatus = App.systimer.Enabled && (App.pactive.SysSetHack ?? false) ? $"{SSHStatus} {ThreadBooster.CountBits(App.lastSysCpuSetMask)}/{ThreadBooster.CountBits(ThreadBooster.defFullBitMask)}" : $"{SSHStatus} (Inactive)";
                SSHStatus = ThreadBoosterRunning ? SSHStatus : $"<Inactive> {SSHStatus}";
                TogglePSA = _status ? $"Toggle SysSetHack OFF" : "Toggle SysSetHack ON";
                OnChange("SSHStatus");
                OnChange("ToggleSSH");
            }
            catch { }
        }
        public void SetN0Status(bool _status)
        {
            try
            {
                N0Status = _status == true ? $"Enabled" : "Disabled";
                if (_status == true) N0Status = App.numazero_b ? $"{N0Status} Selected: {App.n0enabledT0.Count() + App.n0enabledT1.Count()}T Excluded: {App.n0disabledT0.Count() + App.n0disabledT1.Count()}T" : $"{N0Status} (Inactive)";
                N0Status = ThreadBoosterRunning ? N0Status : $"<Inactive> {N0Status}";
                ToggleN0 = _status ? $"Toggle NumaZero OFF" : "Toggle NumaZero ON";
                OnChange("ToggleN0");
                OnChange("N0Status");
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
