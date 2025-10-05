using Microsoft.Win32.SafeHandles;
using net.r_eg.Conari.Mangling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Vanara;
using Windows.UI.Text;
using static CPUDoc.ProcessorInfo;

namespace CPUDoc
{
    public static class ProcessorInfo
    {
        private static IHardwareCore[] cores;
        private static IHardwareCpuSet[] cpuset;
        private static int[] logicalCores;
        private static int LoadMediumThreshold = 40;
        private static int LoadLowThreshold = 20;
        private static int LoadHighThreshold = 95;
        private static PerformanceCounter TotalLoadCounter;
        private static bool? IsHybridCache = null;
        private static int? HybridLevelCache = null;
        private static int InterlockCpuSet = 0;
        public static int InterlockCpuLoadUpdate = 0;
        public static int Clusters { get; private set; }

        public static bool CpuLoadPerfCounter = true;
        public static CpuLoad _cpuLoad = new CpuLoad();
        public static double cpuTotalLoad { get; set; }
        public static double threadsTotalLoad { get; set; }
        public static double TotalEnabledLoad { get; set; }
        public static double TotalEnabledLoadNorm { get; set; }
        public static int SSH_needcores { get; private set; }
        public static int SSH_usedcores { get; private set; }

        /// <summary>
        /// Selection types enum
        /// </summary>
        public enum SelectionType
        {
            None = 0,
            T0 = 1,
            T1 = 2,
            PCores = 4,
            ECores = 8,
            LCores = 16,
            ClusterNumber = 32,
            NumaNode = 64,
            LLCIndex = 128,
            All = 256,
        }
        /// <summary>
        /// Tiered priority selection types enum
        /// </summary>
        public enum TieredType
        {
            None = 0,
            ECoresLast = 1,
            ECoresAsPCores = 2,
            Clusters = 4,
        }
        /// <summary>
        /// Hardware core
        /// </summary>
        public interface IHardwareCore
        {
            /// <summary>
            /// Logical core IDs
            /// </summary>
            int[] LogicalCores { get; }
            /// <summary>
            /// Logical cores count
            /// </summary>
            int LogicalCoresCount { get; }
            /// <summary>
            /// Physical cores count
            /// </summary>
            int PhysicalCoresCount { get; }
            /// <summary>
            /// High usag threshold, logicals count
            /// </summary>
            int SSH_needcores { get; }
            /// <summary>
            /// Used, medium threshold, logicals count
            /// </summary>
            int SSH_usedcores { get; }

        }

        /// <summary>
        /// Hardware CpuSets
        /// </summary>
        public interface IHardwareCpuSet
        {
            /// <summary>
            /// CpuSets ID
            /// </summary>
            int Id { get; }
            /// <summary>
            /// CpuSets EfficiencyClass
            /// </summary>
            int EfficiencyClass { get; }
            /// <summary>
            /// CpuSets LogicalProcessorIndex
            /// </summary>
            int LogicalProcessorIndex { get; }
            /// <summary>
            /// CpuSets CoreIndex
            /// </summary>
            int CoreIndex { get; }
            /// <summary>
            /// CpuSets NumaNodeIndex
            /// </summary>
            int NumaNodeIndex { get; }
            /// <summary>
            /// CpuSets LastLevelCacheIndex
            /// </summary>
            int LastLevelCacheIndex { get; }
            /// <summary>
            /// CpuSets Group
            /// </summary>
            int Group { get; }
            /// <summary>
            /// CpuSets SchedulingClass
            /// </summary>
            int SchedulingClass { get; }
            /// <summary>
            /// CpuSets AllocationTag
            /// </summary>
            long AllocationTag { get; }
            /// <summary>
            /// CpuSets AllFlagsStruct
            /// </summary>
            byte AllFlagsStruct { get; }
            /// <summary>
            /// CpuSets Parked
            /// </summary>
            int Parked
            {
                get
                {
                    return (int)(AllFlagsStruct & 1u);
                }
            }
            /// <summary>
            /// CpuSets Allocated
            /// </summary>
            int Allocated
            {
                get
                {
                    return (int)((AllFlagsStruct & 2u) / 2D);
                }
            }
            /// <summary>
            /// CpuSets AllocatedToTargetProcess
            /// </summary>
            int AllocatedToTargetProcess
            {
                get
                {
                    return (int)((AllFlagsStruct & 4u) / 4D);
                }
            }
            /// <summary>
            /// CpuSets RealTime
            /// </summary>
            int RealTime
            {
                get
                {
                    return (int)((AllFlagsStruct & 8u) / 8D);
                }
            }
            /// <summary>
            /// Cpu Load
            /// </summary>
            int Cluster { get; set; }
            /// <summary>
            /// Cpu Load
            /// </summary>
            float Load { get; set; }
            /// <summary>
            /// Cpu Load Zero times
            /// </summary>
            int LoadZero { get; set; }
            /// <summary>
            /// Cpu Load Medium times
            /// </summary>
            int LoadLow { get; set; }
            /// <summary>
            /// Cpu Load Medium times
            /// </summary>
            int LoadMedium { get; set; }
            /// <summary>
            /// Cpu Load High times
            /// </summary>
            int LoadHigh { get; set; }
            /// <summary>
            /// Forced enable
            /// </summary>
            bool ForcedEnable { get; set; }
            /// <summary>
            /// Forced enable
            /// </summary>
            DateTime ForcedWhen { get; set; }
            /// <summary>
            /// Disabled by SSH
            /// </summary>
            bool Disabled { get; set; }
            /// <summary>
            /// Forced enable
            /// </summary>
            bool Excluded { get; set; }
            /// <summary>
            /// Tiered Priority (follows SchedulingClass)
            /// </summary>
            int TieredPriority { get; set; }
            /// <summary>
            /// THread Number (T0, T1, ...)
            /// </summary>
            int ThreadNumber { get; set; }
            /// <summary>
            /// CPU Load Counter
            /// </summary>
            PerformanceCounter LoadCounter { get; set; }
        }

        /// <summary>
        /// Hardware cores
        /// </summary>
        public static IHardwareCore[] HardwareCores
        {
            get
            {
                return cores ?? (cores = GetLogicalProcessorInformation()
                    .Where(x => x.Relationship == LOGICAL_PROCESSOR_RELATIONSHIP.RelationProcessorCore)
                    .Select(x => new HardwareCore((UInt64) x.ProcessorMask))
                    .ToArray<IHardwareCore>());
            }
        }

        /// <summary>
        /// Init CpuLoad
        /// </summary>
        [SupportedOSPlatform("windows")]
        public static bool CpuLoadInit()
        {
            try
            {
                TotalLoadCounter = new PerformanceCounter(
                "Processor",
                "% Processor Time",
                "_Total",
                true
                );
                TotalLoadCounter.NextValue();
            }
            catch
            {
                App.ExecuteCmd("lodctr", "/R", @"c:\windows\system32");
                App.ExecuteCmd("lodctr", "/R", @"c:\windows\sysWOW64");
                App.ExecuteCmd("WINMGMT", "/RESYNCPERF", @"c:\windows\system32");
                try
                {
                    TotalLoadCounter = new PerformanceCounter(
                    "Processor",
                    "% Processor Time",
                    "_Total",
                    true
                    );
                    TotalLoadCounter.NextValue();
                }
                catch
                {
                    _cpuLoad.Update();
                    CpuLoadPerfCounter = false;
                }
            }
            return CpuLoadPerfCounter;
        }
        /// <summary>
        /// Hardware CpuSets
        /// </summary>
        public static IHardwareCpuSet[] HardwareCpuSets
        {
            get
            {
                cpuset ??= GetSystemCpuSetInformation()
                    .Where(x => x.Type == CPUSET_TYPE.CpuSetInformation && x.CpuSetUnion.CpuSet.Id != 0)
                    .Select(x => new HardwareCpuSet(x))
                    .OrderBy(x => x.LogicalProcessorIndex)
                    .ToArray<IHardwareCpuSet>();
                return cpuset;
            }
        }

        /// <summary>
        /// Update CpuLoad
        /// </summary>
        /*
        public static void CpuLoadUpdate()
        {
            threadsTotalLoad = 0;
            for (var i = 0; i < LogicalCoresCount; ++i)
            {
                float _Load;
                _Load = CpuLoadPerfCounter ? HardwareCpuSets[i].LoadCounter.NextValue() : _cpuLoad.GetCpuLoad(i);
                HardwareCpuSets[i].Load = _Load;
                int _loadhigh = HardwareCpuSets[i].LoadHigh > 40 ? 40 : HardwareCpuSets[i].LoadHigh;
                _loadhigh = _Load >= LoadHighThreshold ? _loadhigh + 2 : _loadhigh <= 1 ? 0 : _loadhigh - 1;
                HardwareCpuSets[i].LoadHigh = _loadhigh;
                int _loadmedium = HardwareCpuSets[i].LoadMedium > 40 ? 40 : HardwareCpuSets[i].LoadMedium;
                _loadmedium = _Load >= LoadMediumThreshold ? _loadmedium + 1 : _loadmedium <= 1 ? 0 : _loadmedium - 1;
                HardwareCpuSets[i].LoadMedium = _loadmedium;
                int _loadlow = HardwareCpuSets[i].LoadLow > 40 ? 40 : HardwareCpuSets[i].LoadLow;
                _loadlow = _Load >= LoadLowThreshold ? _loadlow + 1 : _loadlow <= 1 ? 0 : _loadlow - 1;
                HardwareCpuSets[i].LoadLow = _loadlow;
                int _loadzero = HardwareCpuSets[i].LoadZero > 40 ? 40 : HardwareCpuSets[i].LoadZero;
                _loadzero = _Load == 0 ? _loadzero + 1 : _loadzero <= 2 ? 0 : _loadzero - 1;
                HardwareCpuSets[i].LoadZero = _loadzero;
                App.systemInfo.UpdateLoadThread(i, (int)_Load);
                threadsTotalLoad += (int)_Load;
            }
            App.systemInfo.UpdateLoadThreads();
        }
        */
        //
        /// <summary>
        /// Init extra info for Clusters and Thread numbers
        /// </summary>
        public static void InitExtra()
        {
            var cpuset = HardwareCpuSets.ToArray();
            
            int _cluster = 1;
            int _llcindex = 0;

            for (int i = 0; i < cpuset.Length; i++)
            {

                if (_llcindex != cpuset[i].LastLevelCacheIndex)
                {
                    _cluster++;
                    _llcindex = cpuset[i].LastLevelCacheIndex;
                }
                cpuset[i].Cluster = _cluster;
                cpuset[i].ThreadNumber = GetThreadNumber(cpuset[i].LogicalProcessorIndex);
            }

            if (_cluster == 1 && cpuset.Length > 6)
            {
                bool _intelcluster = false;

                for (int i = 0; i < cpuset.Length; i += 6)
                {
                    if (cpuset[i].EfficiencyClass == 1 && cpuset[i + 1].EfficiencyClass == 1 &&
                        cpuset[i + 2].EfficiencyClass == 0 && cpuset[i + 3].EfficiencyClass == 0 && cpuset[i + 4].EfficiencyClass == 0 &&
                        cpuset[i + 5].EfficiencyClass == 0)
                    {
                        _intelcluster = true;
                    }
                    else if (cpuset[i].EfficiencyClass == 0 && cpuset[i + 1].EfficiencyClass == 0 && cpuset[i + 2].EfficiencyClass == 0 &&
                             cpuset[i + 3].EfficiencyClass == 0 && cpuset[i + 4].EfficiencyClass == 1 && cpuset[i + 5].EfficiencyClass == 1)
                    {
                        _intelcluster = true;
                    }
                    else
                    {
                        _intelcluster = false;
                        break;
                    }
                }

                if (_intelcluster)
                {
                    _cluster = 1;
                    for (int i = 0; i < cpuset.Length; i += 6)
                    {
                        for (int j = 0; j <= 5; j++)
                        {
                            cpuset[i + j].Cluster = _cluster;
                        }
                        _cluster++;
                    }
                }
            }

            ProcessorInfo.Clusters = _cluster;

        }

        /// <summary>
        /// Update CpuLoad
        /// </summary>
        public static void CpuLoadUpdate()
        {
            while (Interlocked.CompareExchange(ref InterlockCpuLoadUpdate, 1, 0) != 0)
                continue;

            threadsTotalLoad = 0;
            TotalEnabledLoad = 0;
            SSH_needcores = 0;
            SSH_usedcores = 0;

            int basecores = 0;
            TimeSpan _deltaStamp = TimeSpan.Zero;

            for (var i = 0; i < HardwareCpuSets.Length; ++i)
            {
                App.systemInfo.UpdateParkedStateCore(ProcessorInfo.PhysicalCore(HardwareCpuSets[i].LogicalProcessorIndex), HardwareCpuSets[i].Parked);
                
                float _Load = CpuLoadPerfCounter ? HardwareCpuSets[i].LoadCounter.NextValue() : _cpuLoad.GetCpuLoad(i);
                HardwareCpuSets[i].Load = _Load;

                int _loadhigh = Math.Min(40, HardwareCpuSets[i].LoadHigh);
                _loadhigh = (_Load >= LoadHighThreshold) ? Math.Min(40, _loadhigh + 2) : (_loadhigh <= 1) ? 0 : Math.Max(0, _loadhigh - 1);
                HardwareCpuSets[i].LoadHigh = _loadhigh;

                int _loadmedium = Math.Min(40, HardwareCpuSets[i].LoadMedium);
                _loadmedium = (_Load >= LoadMediumThreshold) ? Math.Min(40, _loadmedium + 1) : (_loadmedium <= 1) ? 0 : Math.Max(0, _loadmedium - 1);
                HardwareCpuSets[i].LoadMedium = _loadmedium;

                int _loadlow = Math.Min(40, HardwareCpuSets[i].LoadLow);
                _loadlow = (_Load >= LoadLowThreshold) ? Math.Min(40, _loadlow + 1) : (_loadlow <= 1) ? 0 : Math.Max(0, _loadlow - 1);
                HardwareCpuSets[i].LoadLow = _loadlow;

                int _loadzero = Math.Min(40, HardwareCpuSets[i].LoadZero);
                _loadzero = (_Load == 0) ? Math.Min(40, _loadzero + 1) : (_loadzero <= 2) ? 0 : Math.Max(0, _loadzero - 1);
                HardwareCpuSets[i].LoadZero = _loadzero;

                App.systemInfo.UpdateLoadThread(i, (int)_Load);
                threadsTotalLoad += (int)_Load;

                if (HardwareCpuSets[i].Disabled == false && HardwareCpuSets[i].Excluded == false)
                {
                    if (HardwareCpuSets[i].LoadHigh > ThreadBooster.LoadHighThresholdCount)
                        SSH_needcores++;
                    if (HardwareCpuSets[i].LoadMedium > ThreadBooster.LoadMediumThresholdCount)
                        SSH_usedcores++;

                    //App.LogDebug($"CpuLoadUpdate Enabled Logical={i} SSH_needcores {SSH_needcores} SSH_usedcores={SSH_usedcores}");
                    
                    basecores++;
                    TotalEnabledLoad += HardwareCpuSets[i].Load;
                }

                if (HardwareCpuSets[i].Disabled == true && HardwareCpuSets[i].Excluded == false)
                {
                    if (HardwareCpuSets[i].LoadZero > ThreadBooster.LoadZeroThresholdCount)
                    {
                        _deltaStamp = DateTime.Now - HardwareCpuSets[i].ForcedWhen;
                        if (_deltaStamp.TotalSeconds > ThreadBooster.ClearForceThreshold)
                            ClearForceEnabled(i);
                        //App.LogDebug($"CpuLoadUpdate ClearForceEnabled Logical={i} SSH_needcores {SSH_needcores} SSH_usedcores={SSH_usedcores}");
                    } 
                    else
                    {
                        if (HardwareCpuSets[i].LoadHigh > ThreadBooster.LoadHighThresholdCount)
                        {
                            //App.LogDebug($"CpuLoadUpdate ForceEnabled HIGH Logical={i} SSH_needcores {SSH_needcores} SSH_usedcores={SSH_usedcores}");
                            SetForceEnabled(i);
                            SSH_needcores++;
                        }

                        if (HardwareCpuSets[i].LoadMedium > ThreadBooster.LoadMediumThresholdCount)
                        {
                            //App.LogDebug($"CpuLoadUpdate ForceEnabled Logical={i} SSH_needcores {SSH_needcores} SSH_usedcores={SSH_usedcores}");
                            SetForceEnabled(i);
                            SSH_usedcores++;
                        }
                    }
                    if (HardwareCpuSets[i].ForcedEnable)
                    {
                        basecores++;
                        TotalEnabledLoad += HardwareCpuSets[i].Load;
                    }
                    //App.LogDebug($"RTBSSH Disabled Logical={i} SSH_needcores {SSH_needcores} SSH_usedcores={SSH_usedcores}");
                }

            }

            App.systemInfo.UpdateLoadThreads();
            TotalEnabledLoadNorm = TotalEnabledLoad / basecores;
            InterlockCpuLoadUpdate = 0;
        }

        /// <summary>
        /// Reset CpuLoad Threads
        /// </summary>
        public static void ResetLoadThreads()
        {
            for (var i = 0; i < HardwareCpuSets.Length; ++i)
            {
                HardwareCpuSets[i].LoadHigh = 0;
                HardwareCpuSets[i].LoadMedium = 0;
                HardwareCpuSets[i].LoadLow = 0;
                HardwareCpuSets[i].LoadZero = 0;
            }
        }

        /// <summary>
        /// Update Total CpuLoad
        /// </summary>
        public static void CpuTotalLoadUpdate()
        {
            try
            {
                //cpuTotalLoad = CpuLoadPerfCounter ? TotalLoadCounter.NextValue() : _cpuLoad.GetTotalLoad(); 
                cpuTotalLoad = Math.Floor(threadsTotalLoad / App.systemInfo.CPULogicalProcessors);
                // App.LogDebug($"[{cpuTotalLoad:0}] [{Math.Floor(threadsTotalLoad / App.systemInfo.CPULogicalProcessors):0}]");
            }
            catch (InvalidOperationException)
            {
                cpuTotalLoad = Math.Floor(threadsTotalLoad / App.systemInfo.CPULogicalProcessors);
                //cpuTotalLoad = CpuLoadPerfCounter ? TotalLoadCounter.NextValue() : _cpuLoad.GetTotalLoad();
            }
        }

        /// <summary>
        /// Create default BitMask (all enabled and not excluded)   
        /// </summary>
        public static ulong CreateSSHBitMask(ulong defBitMask, int morecores = 0)
        {
            var cpuset = HardwareCpuSets.Where(x => x.Disabled == true && x.Excluded == false).OrderByDescending(x => x.TieredPriority).ToArray();
            int addedcores = 0;
            for (var i = 0; i < cpuset.Length; ++i)
            {
                if (cpuset[i].ForcedEnable == true)
                {
                    //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} ForcedEnable={cpuset[i].LogicalProcessorIndex}");
                    defBitMask |= (uint)1 << (cpuset[i].LogicalProcessorIndex);
                }
                else if (addedcores < morecores)
                {
                    defBitMask |= (uint)1 << (cpuset[i].LogicalProcessorIndex);
                    addedcores++;
                    //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} Adding={cpuset[i].LogicalProcessorIndex}");
                }
                //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} Logical={cpuset[i].LogicalProcessorIndex} Logical={cpuset[i].TieredPriority} morecores={morecores} addedcores={addedcores} defBitMask=0x{defBitMask:X16}");
            }

            return defBitMask;
        }

        /// <summary>
        /// Create default BitMask (all enabled and not excluded)   
        /// </summary>
        public static ulong CreateCustomBitMask(ulong defBitMask, int addcores = 0)
        {

            var cpuset = HardwareCpuSets.Where(x => x.Disabled == true && x.Excluded == false).OrderBy(x => x.TieredPriority).ToArray();
            int addedcores = 0;
            for (var i = 0; i < cpuset.Length; ++i)
            {
                if (cpuset[i].ForcedEnable == true)
                {
                    //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} ForcedEnable={cpuset[i].LogicalProcessorIndex}");
                    defBitMask |= (uint)1 << (cpuset[i].LogicalProcessorIndex);
                }
                else if (addedcores < addcores)
                {
                    //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} Adding={cpuset[i].LogicalProcessorIndex}");
                    defBitMask |= (uint)1 << (cpuset[i].LogicalProcessorIndex);
                    addedcores++;
                }
                //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} {cpuset[i].LogicalProcessorIndex} defBitMask=0x{defBitMask:X16}");
            }

            return defBitMask;
        }
        /// <summary>
        /// Create default BitMask (all enabled and not excluded)   
        /// </summary>
        public static ulong CreateDefaultBitMask()
        {
            ulong _bitMask = 0;

            var cpuset = HardwareCpuSets.Where(x => x.Disabled == false && x.Excluded == false).OrderBy(x => x.LogicalProcessorIndex).ToArray();

            //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} CSLen={cpuset.Length}");

            for (var i = 0; i < cpuset.Length; ++i)
            {
                //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} D={cpuset[i].Disabled} E={cpuset[i].Excluded} Adding={cpuset[i].LogicalProcessorIndex}");
                _bitMask |= (uint)1 << (cpuset[i].LogicalProcessorIndex);
            }

            return _bitMask;
        }

        /// <summary>
        /// Create full BitMask (including disabled but not those excluded)
        /// </summary>
        public static ulong CreateFullBitMask()
        {
            ulong _bitMask = 0;

            var cpuset = HardwareCpuSets.Where(x => x.Excluded == false).OrderBy(x => x.LogicalProcessorIndex).ToArray();

            //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} CSLen={cpuset.Length}");

            for (var i = 0; i < cpuset.Length; ++i)
            {
                //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} D={cpuset[i].Disabled} E={cpuset[i].Excluded} Adding={cpuset[i].LogicalProcessorIndex}");
                _bitMask |= (uint)1 << (cpuset[i].LogicalProcessorIndex);
            }

            return _bitMask;
        }

        /// <summary>
        /// Create a complete BitMask (including disabled and excluded)
        /// </summary>
        public static ulong CreateAllBitMask()
        {
            ulong _bitMask = 0;

            var cpuset = HardwareCpuSets.OrderBy(x => x.LogicalProcessorIndex).ToArray();

            for (var i = 0; i < cpuset.Length; ++i)
            {
                _bitMask |= (uint)1 << (cpuset[i].LogicalProcessorIndex);
            }

            return _bitMask;
        }

        /// <summary>
        /// Get ForceEnabled, return true if at least one core is forced enabled
        /// </summary>
        public static bool IsForceEnableAny()
        {
            try
            {
                var cpuset = HardwareCpuSets
                    .Where(x => x.ForcedEnable == true).FirstOrDefault();

                if (cpuset == default) return false;
                if (cpuset.ForcedEnable == true) return true;
                return false;
            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception {System.Reflection.MethodBase.GetCurrentMethod().Name}:", ex);
                return false;
            }
        }

        /// <summary>
        /// Set ForceEnabled
        /// </summary>
        public static void SetForceEnabled(int logical)
        {
            try
            {
                while (Interlocked.CompareExchange(ref InterlockCpuSet, 1, 0) != 0)
                    continue;

                HardwareCpuSets[logical].ForcedEnable = true;
                HardwareCpuSets[logical].ForcedWhen = DateTime.Now;
            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception {System.Reflection.MethodBase.GetCurrentMethod().Name}:", ex);
            }
            finally
            {
                InterlockCpuSet = 0;
            }
        }

        /// <summary>
        /// Clear ForceEnabled
        /// </summary>
        public static void ClearForceEnabled(int logical)
        {
            try
            {
                while (Interlocked.CompareExchange(ref InterlockCpuSet, 1, 0) != 0)
                    continue;

                HardwareCpuSets[logical].ForcedEnable = false;
                HardwareCpuSets[logical].ForcedWhen = DateTime.MinValue;
            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception {System.Reflection.MethodBase.GetCurrentMethod().Name}:", ex);
            }
            finally
            {
                InterlockCpuSet = 0;
            }
        }
        /// <summary>
        /// Clear ForceEnabled
        /// </summary>
        public static void ClearForceEnabled()
        {
            try
            {
                while (Interlocked.CompareExchange(ref InterlockCpuSet, 1, 0) != 0)
                    continue;

                for (int logical = 0; logical < HardwareCpuSets.Count(); logical++)
                {
                    if (HardwareCpuSets[logical].Disabled == false) continue;
                    HardwareCpuSets[logical].ForcedEnable = false;
                    HardwareCpuSets[logical].ForcedWhen = DateTime.MinValue;
                }
            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception {System.Reflection.MethodBase.GetCurrentMethod().Name}:", ex);
            }
            finally
            {
                InterlockCpuSet = 0;
            }
        }
        /// <summary>
        /// Clear ForceEnabled in logicals list
        /// </summary>
        public static void ClearForceEnabled(List<int> logicals)
        {
            try
            {
                while (Interlocked.CompareExchange(ref InterlockCpuSet, 1, 0) != 0)
                    continue;

                for (int logical = 0; logical < logicals.Count; logical++)
                {
                    HardwareCpuSets[logicals[logical]].ForcedEnable = false;
                    HardwareCpuSets[logicals[logical]].ForcedWhen = DateTime.MinValue;
                }
            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception {System.Reflection.MethodBase.GetCurrentMethod().Name}:", ex);
            }
            finally
            {
                InterlockCpuSet = 0;
            }
        }

        /// <summary>
        /// Set Disabled
        /// </summary>
        public static void SetDisabled(int logical)
        {
            HardwareCpuSets[logical].Disabled = true;
        }

        /// <summary>
        /// Set Disabled in logicals list
        /// </summary>
        public static void SetDisabled(List<int> logicals)
        {
            try
            {
                while (Interlocked.CompareExchange(ref InterlockCpuSet, 1, 0) != 0)
                    continue;

                for (int logical = 0; logical < logicals.Count; logical++)
                {
                    HardwareCpuSets[logicals[logical]].Disabled = true;
                }
            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception {System.Reflection.MethodBase.GetCurrentMethod().Name}:", ex);
            }
            finally
            {
                InterlockCpuSet = 0;
            }
        }

        /// <summary>
        /// Set Disabled by selection type and optional cluster
        /// </summary>
        public static void SetDisabled(SelectionType selectionType, int clustermax = 0)
        {
            try
            {
                while (Interlocked.CompareExchange(ref InterlockCpuSet, 1, 0) != 0)
                    continue;

                for (int logical = 0; logical < HardwareCpuSets.Length; logical++)
                {
                    //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} Logical={logical}");

                    bool _selected = false;

                    //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} thiscore={thiscore} prevcore={prevcore} corecount={corecount}");

                    if (logical != 0)
                    {
                        //if (selectionType.HasFlag(SelectionType.All)) _selected = true;
                        if (selectionType.HasFlag(SelectionType.T1))
                        {
                            if (GetThreadNumber(logical) > 0) _selected = true;
                            //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} T1 Logical={logical} Sel={_selected}");
                        }
                        if (selectionType.HasFlag(SelectionType.ECores))
                        {
                            if (HardwareCpuSets[logical].EfficiencyClass == 0 && IsHybrid) _selected = true;
                            //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} Ecores Logical={logical} Sel={_selected}");
                        }
                        if (selectionType.HasFlag(SelectionType.LCores))
                        {
                            if (HardwareCpuSets[logical].EfficiencyClass == 2 && IsHybrid) _selected = true;
                            //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} Lcores Logical={logical} Sel={_selected}");
                        }
                        if (selectionType.HasFlag(SelectionType.ClusterNumber) && clustermax > 0)
                        {
                            if (HardwareCpuSets[logical].Cluster > clustermax) _selected = true;
                            //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} Cluster Logical={logical} Sel={_selected}");
                        }
                    }
                    //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} Logical={logical} Sel={_selected}");
                    HardwareCpuSets[logical].Disabled = _selected;
                }
            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception {System.Reflection.MethodBase.GetCurrentMethod().Name}:", ex);
            }
            finally
            {
                InterlockCpuSet = 0;
            }
        }

        /// <summary>
        /// Clear Disabled
        /// </summary>
        public static void ClearDisabled(int logical)
        {
            try
            {
                while (Interlocked.CompareExchange(ref InterlockCpuSet, 1, 0) != 0)
                    continue;

                HardwareCpuSets[logical].Disabled = false;
            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception {System.Reflection.MethodBase.GetCurrentMethod().Name}:", ex);
            }
            finally
            {
                InterlockCpuSet = 0;
            }
        }

        /// <summary>
        /// Clear all Disabled
        /// </summary>
        public static void ClearDisabled()
        {
            try
            {
                while (Interlocked.CompareExchange(ref InterlockCpuSet, 1, 0) != 0)
                    continue;

                for (int logical = 0; logical < HardwareCpuSets.Length; logical++)
                {
                    HardwareCpuSets[logical].Disabled = false;
                }
            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception {System.Reflection.MethodBase.GetCurrentMethod().Name}:", ex);
            }
            finally
            {
                InterlockCpuSet = 0;
            }
        }

        /// <summary>
        /// Clear Disabled in logicals list
        /// </summary>
        public static void ClearDisabled(List<int> logicals)
        {
            try
            {
                while (Interlocked.CompareExchange(ref InterlockCpuSet, 1, 0) != 0)
                    continue;

                for (int logical = 0; logical < logicals.Count; logical++)
                {
                    HardwareCpuSets[logicals[logical]].Disabled = false;
                }
            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception {System.Reflection.MethodBase.GetCurrentMethod().Name}:", ex);
            }
            finally
            {
                InterlockCpuSet = 0;
            }
        }

        /// <summary>
        /// Set Excluded
        /// </summary>
        public static void SetExcluded(int logical)
        {
            try
            {
                while (Interlocked.CompareExchange(ref InterlockCpuSet, 1, 0) != 0)
                    continue;

                if (PhysicalCore(logical) != 0) HardwareCpuSets[logical].Excluded = true;
            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception {System.Reflection.MethodBase.GetCurrentMethod().Name}:", ex);
            }
            finally
            {
                InterlockCpuSet = 0;
            }
        }

        /// <summary>
        /// Set Excluded in logicals list
        /// </summary>
        public static void SetExcluded(List<int> logicals)
        {
            try
            {
                while (Interlocked.CompareExchange(ref InterlockCpuSet, 1, 0) != 0)
                    continue;

                for (int logical = 0; logical < logicals.Count; logical++)
                {
                    bool _selection = false;
                    _selection = PhysicalCore(logical) == 0 ? false : true;
                    HardwareCpuSets[logicals[logical]].Excluded = true;
                }
            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception {System.Reflection.MethodBase.GetCurrentMethod().Name}:", ex);
            }
            finally
            {
                InterlockCpuSet = 0;
            }
        }

        /// <summary>
        /// Set Excluded by selection
        /// </summary>
        public static void SetExcluded(SelectionType selectionType = SelectionType.None, int clustermin = 1, int clustermax = int.MaxValue, int limitcores = 0, int numanode = 1, int llcindex = -1)
        {
            try
            {
                while (Interlocked.CompareExchange(ref InterlockCpuSet, 1, 0) != 0)
                    continue;

                int corecount = 1;
                int thiscore = 0;
                int prevcore = 0;

                //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} clustermin={clustermin} clustermax={clustermax} limitcores={limitcores} ishybrid={IsHybrid}");

                for (int logical = 0; logical < HardwareCpuSets.Length; logical++)
                {
                    //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} Logical={logical}");

                    bool _selected = false;
                    thiscore = PhysicalCore(logical);
                    if (thiscore != prevcore)
                    {
                        prevcore = thiscore; 
                        corecount++;
                    }

                    //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} thiscore={thiscore} prevcore={prevcore} corecount={corecount}");

                    if (logical != 0)
                    {
                        if (selectionType.HasFlag(SelectionType.All)) _selected = true;
                        if (selectionType.HasFlag(SelectionType.T0))
                        {
                            if (GetThreadNumber(logical) == 0) _selected = true;
                            //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} T0 Logical={logical} Sel={_selected}");
                        }
                        if (selectionType.HasFlag(SelectionType.T1))
                        {
                            if (GetThreadNumber(logical) > 0) _selected = true;
                            //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} T1 Logical={logical} Sel={_selected}");
                        }
                        if (selectionType.HasFlag(SelectionType.ECores))
                        {
                            if (HardwareCpuSets[logical].EfficiencyClass == 0 && IsHybrid) _selected = true;
                            //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} Ecores Logical={logical} Sel={_selected}");
                        }
                        if (selectionType.HasFlag(SelectionType.PCores))
                        {
                            if (HardwareCpuSets[logical].EfficiencyClass == 1 && IsHybrid) _selected = true;
                            //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} Pcores Logical={logical} Sel={_selected}");
                        }
                        if (selectionType.HasFlag(SelectionType.LCores))
                        {
                            if (HardwareCpuSets[logical].EfficiencyClass == 2 && IsHybrid) _selected = true;
                            //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} Lcores Logical={logical} Sel={_selected}");
                        }
                        if (selectionType.HasFlag(SelectionType.ClusterNumber))
                        {
                            if (HardwareCpuSets[logical].Cluster < clustermin || HardwareCpuSets[logical].Cluster > clustermax) _selected = true;
                            //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} Cluster Logical={logical} Sel={_selected}");
                        }
                        if (selectionType.HasFlag(SelectionType.NumaNode))
                        {
                            if (HardwareCpuSets[logical].NumaNodeIndex == numanode) _selected = true;
                        }
                        if (selectionType.HasFlag(SelectionType.LLCIndex))
                        {
                            if (HardwareCpuSets[logical].LastLevelCacheIndex == llcindex) _selected = true;
                        }
                        if (limitcores > 0 && corecount > limitcores )
                        {
                            _selected = true;
                            //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} Limitcores Logical={logical} Sel={_selected}");
                        }
                    }
                    //App.LogDebug($"{System.Reflection.MethodBase.GetCurrentMethod().Name} Logical={logical} Sel={_selected}");
                    HardwareCpuSets[logical].Excluded = _selected;
                }
            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception {System.Reflection.MethodBase.GetCurrentMethod().Name}:", ex);
            }
            finally 
            {
                InterlockCpuSet = 0;
            }
        }

        /// <summary>
        /// Clear Excluded
        /// </summary>
        public static void ClearExcluded(int logical)
        {
            try
            {
                while (Interlocked.CompareExchange(ref InterlockCpuSet, 1, 0) != 0)
                    continue;

                HardwareCpuSets[logical].Excluded = false;
            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception {System.Reflection.MethodBase.GetCurrentMethod().Name}:", ex);
            }
            finally
            {
                InterlockCpuSet = 0;
            }
        }

        /// <summary>
        /// Clear all Excluded
        /// </summary>
        public static void ClearExcluded()
        {
            try
            {
                while (Interlocked.CompareExchange(ref InterlockCpuSet, 1, 0) != 0)
                    continue;

                for (int logical = 0; logical < HardwareCpuSets.Length; logical++)
                {
                    HardwareCpuSets[logical].Excluded = false;
                }
            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception {System.Reflection.MethodBase.GetCurrentMethod().Name}:", ex);
            }
            finally
            {
                InterlockCpuSet = 0;
            }
        }

        /// <summary>
        /// Clear Excluded in logicals list
        /// </summary>
        public static void ClearExcluded(List<int> logicals)
        {
            try
            {
                while (Interlocked.CompareExchange(ref InterlockCpuSet, 1, 0) != 0)
                    continue;

                for (int logical = 0; logical < logicals.Count; logical++)
                {
                    HardwareCpuSets[logicals[logical]].Excluded = false;
                }
            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception {System.Reflection.MethodBase.GetCurrentMethod().Name}:", ex);
            }
            finally
            {
                InterlockCpuSet = 0;
            }
        }

        /// <summary>
        /// Set Tiered Priority
        /// 
        /// TieredType:
        /// None = 0,
        /// ECoresLast = 1,
        /// ECoresAsPCores = 2,
        /// Clusters = 4,
        /// 
        /// </summary>
        public static void SetTieredPriority(TieredType tieredType = TieredType.None, int clusters = 0)
        {
            try
            {
                while (Interlocked.CompareExchange(ref InterlockCpuSet, 1, 0) != 0)
                    continue;

                int _priority = HardwareCpuSets.Length;
                int _clusters = clusters > 0 ? clusters : Clusters;

                if (!IsHybrid)
                {
                    if (tieredType.HasFlag(TieredType.Clusters))
                    {
                        for (int c = 1; c <= _clusters; c++)
                        {
                            var cpuset = HardwareCpuSets
                                .Where(x => x.ThreadNumber == 0 && x.Cluster == c).OrderByDescending(x => x.SchedulingClass);
                            foreach (var logical in cpuset)
                            {
                                logical.TieredPriority = _priority;
                                _priority--;
                            }

                            cpuset = HardwareCpuSets
                                .Where(x => x.ThreadNumber > 0 && x.Cluster == c).OrderBy(x => x.SchedulingClass);

                            foreach (var logical in cpuset)
                            {
                                logical.TieredPriority = _priority;
                                _priority--;
                            }
                        }
                    }
                    else
                    {
                        var cpuset = HardwareCpuSets
                            .Where(x => x.ThreadNumber == 0).OrderByDescending(x => x.SchedulingClass);

                        foreach (var logical in cpuset)
                        {
                            logical.TieredPriority = _priority;
                            _priority--;
                        }

                        cpuset = HardwareCpuSets
                            .Where(x => x.ThreadNumber > 0).OrderBy(x => x.SchedulingClass);

                        foreach (var logical in cpuset)
                        {
                            logical.TieredPriority = _priority;
                            _priority--;
                        }
                    }
                }
                else // Hybrid Architecture
                {
                    if (tieredType.HasFlag(TieredType.Clusters))
                    {
                        for (int c = 1; c <= _clusters; c++)
                        {
                            var cpusetNormal = HardwareCpuSets
                                .Where(x => x.ThreadNumber == 0 && x.Cluster == c).OrderByDescending(x => x.SchedulingClass);
                            var cpusetHybrid = HardwareCpuSets
                                .Where(x => x.ThreadNumber == 0 && x.Cluster == c && x.EfficiencyClass == 1).OrderByDescending(x => x.SchedulingClass);

                            var cpuset = cpusetHybrid;
                            var together = (cpusetNormal ?? Enumerable.Empty<IHardwareCpuSet>()).Concat(cpusetHybrid ?? Enumerable.Empty<IHardwareCpuSet>());

                            foreach (var logical in cpuset)
                            {
                                logical.TieredPriority = _priority;
                                _priority--;
                            }
                            
                            cpuset = HardwareCpuSets
                                .Where(x => x.ThreadNumber > 0 && x.Cluster == c).OrderBy(x => x.SchedulingClass);

                            foreach (var logical in cpuset)
                            {
                                logical.TieredPriority = _priority;
                                _priority--;
                            }
                        }
                    }
                    else
                    {
                        var cpuset = HardwareCpuSets
                            .Where(x => x.ThreadNumber == 0 && x.EfficiencyClass == 1).OrderByDescending(x => x.SchedulingClass);

                        foreach (var logical in cpuset)
                        {
                            logical.TieredPriority = _priority;
                            _priority--;
                        }

                        cpuset = HardwareCpuSets
                            .Where(x => x.ThreadNumber > 0).OrderBy(x => x.SchedulingClass);

                        foreach (var logical in cpuset)
                        {
                            logical.TieredPriority = _priority;
                            _priority--;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception {System.Reflection.MethodBase.GetCurrentMethod().Name}:", ex);
            }
            finally
            {
                InterlockCpuSet = 0;
            }
        }

        /// <summary>
        /// All logical core IDs
        /// </summary>
        public static int[] LogicalCores
        {
            get
            {
                return logicalCores ?? (logicalCores = HardwareCores
                    .SelectMany(x => x.LogicalCores)
                    .ToArray());
            }
        }
        /// <summary>
        /// Logical cores count
        /// </summary>
        public static int LogicalCoresCount
        {
            get
            {
                logicalCores = HardwareCores
                    .SelectMany(x => x.LogicalCores)
                    .ToArray();
                return logicalCores.Count();
            }
        }
        /// <summary>
        /// Physical cores count
        /// </summary>
        public static int PhysicalCoresCount
        {
            get
            {
                return HardwareCores.Count();
            }
        }

        /// <summary>
        /// Is Hybrid Architecture
        /// </summary>
        public static bool IsHybrid
        {
            get
            {
                if (IsHybridCache.HasValue) return IsHybridCache.Value;
                var effclass = HardwareCpuSets
                    .OrderByDescending(x => x.EfficiencyClass).Select(x => x.EfficiencyClass).First()
                    ;
                IsHybridCache = effclass > 0 ? true : false;
                return IsHybridCache.Value;
            }
        }

        /// <summary>
        /// Hybrid Architecture Level
        /// </summary>
        public static int HybridLevel
        {
            get
            {
                if (HybridLevelCache.HasValue) return HybridLevelCache.Value;
                var _HybridLevel = HardwareCpuSets
                    .OrderByDescending(x => x.EfficiencyClass).Select(x => x.Id).First()
                    ;
                HybridLevelCache = _HybridLevel;
                return HybridLevelCache.Value;
            }
        }

        /// <summary>
        /// Return CPUSets Id
        /// </summary>
        public static int Id
        {
            get
            {
                return HardwareCpuSets
                    .Select(x => x.Id).First()
                    ;
            }
        }

        /// <summary>
        /// Get Core ID from Logical
        /// </summary>
        public static int PhysicalCore(int logicalCore)
        {
            for (var i = 0; i < HardwareCores.Length; ++i)
            {
                if (HardwareCores[i].LogicalCores.Contains(logicalCore)) return i;
            }
            return 0;
        }
        /// <summary>
        /// Get Thread Level from Logical
        /// </summary>
        public static int GetThreadNumber(int logicalCore)
        {
            
            for (var i = 0; i < HardwareCores.Length; ++i)
            {
                if (HardwareCores[i].LogicalCores.Contains(logicalCore))
                {
                    for (int t = 0; t < HardwareCores[i].LogicalCores.Length; ++t)
                    {
                        if (HardwareCores[i].LogicalCores[t] == logicalCore) return t;

                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// Get Cores Sorted by SchedulingClass
        /// </summary>
        public static int[][] CoresByScheduling()
        {
            var coresbysched = new int[HardwareCores.Length][];
            int count = 0;
            var cpusetsbysched = HardwareCpuSets.OrderByDescending(x => x.SchedulingClass);
            int _prev = -1;
            foreach (HardwareCpuSet cpuset in cpusetsbysched)
            {
                int _pcore = PhysicalCore(cpuset.LogicalProcessorIndex);
                //System.Diagnostics.App.LogDebug($"Logical={cpuset.LogicalProcessorIndex} Physical={_pcore} Prev={_prev} ");
                if (_pcore != _prev)
                {
                    //System.Diagnostics.App.LogDebug($"Add Physical={_pcore} Count={count} ");
                    coresbysched[count] = new int[2];
                    coresbysched[count][0] = _pcore;
                    coresbysched[count][1] = cpuset.SchedulingClass;
                    count++;
                }
                _prev = _pcore;
            }
            return coresbysched;
        }

        /// <summary>
        /// Check if a LogicalCore is a Golden Core (first best by default)
        /// </summary>
        public static bool CoresByGoldenCheck(int logicalcore, int goldenlimit = 2)
        {
            var coresbysched = new int[HardwareCores.Length][];
            var cpusetsbysched = HardwareCpuSets.OrderByDescending(x => x.SchedulingClass);
            int coretocheck = PhysicalCore(logicalcore);
            int golden = 1;
            int _prev = -1;
            
            foreach (HardwareCpuSet cpuset in cpusetsbysched)
            {
                int _pcore = PhysicalCore(cpuset.LogicalProcessorIndex);
                //App.LogDebug($"CoresByGoldenCheck Logical={cpuset.LogicalProcessorIndex} Physical={_pcore} Check={coretocheck} Golden={golden}");
                if (_pcore == coretocheck)
                {
                    //App.LogDebug($"CoresByGoldenCheck Logical={cpuset.LogicalProcessorIndex} Physical={_pcore} Check={coretocheck} GOLDEN");
                    return true;
                }
                if (_pcore != _prev)
                {
                    _prev = _pcore;
                    golden++;
                }

                if (golden > goldenlimit) break;
            }
            return false;
        }

        /// <summary>
        /// Get Cores Sorted by EfficiencyClass
        /// </summary>
        public static int[][] CoresByEfficiency()
        {
            var coresbyeff = new int[HardwareCores.Length][];
            int count = 0;
            var cpusetsbyeff = HardwareCpuSets.OrderByDescending(x => x.EfficiencyClass);
            int _prev = -1;
            foreach (HardwareCpuSet cpuset in cpusetsbyeff)
            {
                int _pcore = PhysicalCore(cpuset.LogicalProcessorIndex);
                //System.Diagnostics.App.LogDebug($"Logical={cpuset.LogicalProcessorIndex} Physical={_pcore} Prev={_prev} ");
                if (_pcore != _prev)
                {
                    //System.Diagnostics.App.LogDebug($"Add Physical={_pcore} Count={count} ");
                    coresbyeff[count] = new int[2];
                    coresbyeff[count][0] = _pcore;
                    coresbyeff[count][1] = cpuset.EfficiencyClass;
                    count++;
                }
                _prev = _pcore;
            }
            return coresbyeff;
        }
        /// <summary>
        /// Get Logicals E-Cores only (EfficiencyClass = 0 if EfficiencyClass = 1 > 0)
        /// </summary>
        public static List<int> LogicalsEfficient()
        {
            var coresbyeff = new List<int>();
            var cpusetsbyperf = HardwareCpuSets.Where(x => x.EfficiencyClass == 1);
            var cpusetsbyeff = HardwareCpuSets.Where(x => x.EfficiencyClass == 0);
            if (cpusetsbyperf.Count() > 0)
            {
                foreach (HardwareCpuSet cpuset in cpusetsbyeff)
                {
                    coresbyeff.Add(cpuset.LogicalProcessorIndex);
                }
            }
            return coresbyeff;
        }
        /// <summary>
        /// Get Logicals only LastLevelCacheIndex > 0
        /// </summary>
        public static List<int> LogicalsCache()
        {
            var coresbycache = new List<int>();
            var cpusetsbycache = HardwareCpuSets.Where(x => x.LastLevelCacheIndex > 0);
            foreach (HardwareCpuSet cpuset in cpusetsbycache)
            {
                coresbycache.Add(cpuset.LogicalProcessorIndex);
            }
            return coresbycache;
        }
        /// <summary>
        /// Get Logicals for n clusters
        /// </summary>
        public static List<int> LogicalsClusters(int nclusters)
        {
            var coresbycluster = new List<int>();
            var cpusetsbycluster = HardwareCpuSets.Where(x => x.Cluster <= nclusters);
            foreach (HardwareCpuSet cpuset in cpusetsbycluster)
            {
                coresbycluster.Add(cpuset.LogicalProcessorIndex);
            }
            return coresbycluster;
        }
        /// <summary>
        /// Get Logicals out of n clusters
        /// </summary>
        public static List<int> LogicalsClustersOut(int nclusters)
        {
            var coresbycluster = new List<int>();
            var cpusetsbycluster = HardwareCpuSets.Where(x => x.Cluster > nclusters);
            foreach (HardwareCpuSet cpuset in cpusetsbycluster)
            {
                coresbycluster.Add(cpuset.LogicalProcessorIndex);
            }
            return coresbycluster;
        }
        /// <summary>
        /// Get Logicals only NumaZero
        /// </summary>
        public static List<int> LogicalsNumaZero()
        {
            var coresbyn0 = new List<int>();
            var cpusetsbyperf = HardwareCpuSets.Where(x => x.EfficiencyClass == 1);
            int effclass = (cpusetsbyperf.Count() > 0) ? 1 : 0;
            if (cpusetsbyperf.Count() > 0) effclass = 1;
            var cpusetsbyn0 = HardwareCpuSets.Where(x => x.LastLevelCacheIndex == 0 && x.NumaNodeIndex == 0 && x.EfficiencyClass == effclass).OrderByDescending(x => x.SchedulingClass);
            foreach (HardwareCpuSet cpuset in cpusetsbyn0)
            {
                coresbyn0.Add(cpuset.LogicalProcessorIndex);
            }
            return coresbyn0;
        }
        /// <summary>
        /// Get Logicals only NumaNodeIndex > 0
        /// </summary>
        public static List<int> LogicalsIndex()
        {
            var coresbyindex = new List<int>();
            var cpusetsbyindex = HardwareCpuSets.Where(x => x.NumaNodeIndex > 0);
            foreach (HardwareCpuSet cpuset in cpusetsbyindex)
            {
                    coresbyindex.Add(cpuset.LogicalProcessorIndex);
            }
            return coresbyindex;
        }

        /// <summary>
        /// Return Thread Level of a logical
        /// </summary>
        public static int LogicalThreadLevel(int logical)
        {
            for (int i = 0; i < HardwareCores.Length; i++)
            {
                for (int j = 0; j < HardwareCores[i].LogicalCores.Length; j++)
                {
                    var cpuset = HardwareCpuSets.Where(x => x.LogicalProcessorIndex == HardwareCores[i].LogicalCores[j]).First();
                    //App.LogDebug($"LogicalThreadLevel logical={logical} Thread={j}");
                    return j;
                }
            }
            //App.LogDebug($"LogicalThreadLevel logical={logical} not found");
            return 0;
        }

        /// <summary>
        /// Get Logicals by selection NumaZero
        /// Outputs first List<int> with logicals T0 in selection, second List<int> with logicals T0 not in selection
        /// third List<int> with logicals T1 in selection, fourth List<int> with logicals T1 not in selection
        /// 
        /// ExcludeType:
        /// 1 : T1
        /// 2 : E-Cores
        /// 3 : T1 && E-Cores
        /// 
        /// </summary>
        public static (List<int>, List<int>, List<int>, List<int>) LogicalsBySelection(int cluster = 0, int excludeType = 0, int limitCores = 0, int numa = -1)
        {
            var logicalsT0in = new List<int>();
            var logicalsT0out = new List<int>();
            var logicalsT1in = new List<int>();
            var logicalsT1out = new List<int>();

            //App.LogDebug($"LogicalsBySelection cluster={cluster} excludeType={excludeType} limitCores={limitCores} numa={numa}");
            
            for (int i = 0; i < HardwareCores.Length; i++)
            {
                //App.LogDebug($"LogicalsBySelection core={i}");
                
                for (int j = 0; j < HardwareCores[i].LogicalCores.Length; j++)
                {
                    bool _selected = true;
                    var logical = HardwareCpuSets.Where(x => x.LogicalProcessorIndex == HardwareCores[i].LogicalCores[j]).First();
                    //App.LogDebug($"LogicalsBySelection core={i} logical={logical.LogicalProcessorIndex} Cluster={logical.Cluster} EfficiencyClass={logical.EfficiencyClass} NumaNodeIndex={logical.NumaNodeIndex}");
                    if (logical != null)
                    {
                        if (cluster < logical.Cluster ) _selected = false;
                        //App.LogDebug($"LogicalsBySelection core={i} logical={logical.LogicalProcessorIndex} thread={j} Cluster _selected={_selected}");
                        if (numa >= 0 && logical.NumaNodeIndex > numa) _selected = false;
                        //App.LogDebug($"LogicalsBySelection core={i} logical={logical.LogicalProcessorIndex} thread={j} Numa _selected={_selected}");
                        if ((excludeType == 2 || excludeType == 3) && (logical.EfficiencyClass == 0 && App.systemInfo.IntelHybrid)) _selected = false;
                        //App.LogDebug($"LogicalsBySelection core={i} logical={logical.LogicalProcessorIndex} thread={j} Ecore _selected={_selected}");
                        if (j == 1 && (excludeType == 1 || excludeType == 3)) _selected = false;
                        //App.LogDebug($"LogicalsBySelection core={i} logical={logical.LogicalProcessorIndex} thread={j} T1 _selected={_selected}");
                    }
                    if (i > limitCores - 1 && limitCores > 0) _selected = false;
                    if (_selected && j == 0)
                    {
                        logicalsT0in.Add(logical.LogicalProcessorIndex);
                    }
                    else if (!_selected && j == 0)
                    {
                        logicalsT0out.Add(logical.LogicalProcessorIndex);
                    }
                    else if (_selected && j > 0)
                    {
                        logicalsT1in.Add(logical.LogicalProcessorIndex);
                    }
                    else if (!_selected && j > 0)
                    {
                        logicalsT1out.Add(logical.LogicalProcessorIndex);
                    }
                    //App.LogDebug($"LogicalsBySelection core={i} logical={logical.LogicalProcessorIndex} Disabled={logical.Disabled} Excluded={logical.Excluded} thread={j} _selected={_selected}");
                }
            }

            return (logicalsT0in, logicalsT0out, logicalsT1in, logicalsT1out);
        }
        public static bool IsCoresByEfficiencyAllZeros()
        {
            int zeros = HardwareCpuSets.Where(x => x.EfficiencyClass == 0).Count();
            if (zeros == HardwareCpuSets.Length) return true;
            return false;
        }
        public static bool IsCoresBySchedulingAllZeros()
        {
            int zeros = HardwareCpuSets.Where(x => x.SchedulingClass == 0).Count();
            if (zeros == HardwareCpuSets.Length) return true;
            return false;
        }
        /// <summary>
        /// Get number of Logical processors
        /// </summary>
        public static int TotalLogicalProcessors()
        {
            int count = 0;
            for (var i = 0; i < HardwareCores.Length; ++i)
            {
                count += HardwareCores[i].LogicalCores.Count();
            }
            return count;
        }

        /// <summary>
        /// Get Last Thread ID for Processor
        /// </summary>
        public static int LastThreadID()
        {
            return HardwareCores[HardwareCores.Length - 1].LogicalCores.Last();
        }

        /// <summary>
        /// Current logical core ID
        /// </summary>
        public static int CurrentLogicalCore
        {
            get { return GetCurrentProcessorNumber(); }
        }

        private class HardwareCore : IHardwareCore
        {
            public HardwareCore(UInt64 logicalCoresMask)
            {
                var logicalCores = new List<int>();

                for (var i = 0; i < 64; ++i)
                {
                    if (((logicalCoresMask >> i) & 0x1) == 0) continue;
                    logicalCores.Add(i);
                }

                LogicalCores = logicalCores.ToArray();
            }

            public int[] LogicalCores { get; private set; }
            public int LogicalCoresCount { get; private set; }
            public int PhysicalCoresCount { get; private set; }
            public int SSH_needcores { get; private set; }
            public int SSH_usedcores { get; private set; }
        }

        /// <summary>
        /// CpuSet LogicalProcessorIndex for Logical
        /// </summary>
        public static int? CpuSetLogicalProcessorIndex(int logical)
        {
            if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
            return HardwareCpuSets[logical].LogicalProcessorIndex;
        }

        /// <summary>
        /// Load for Logical
        /// </summary>
        public static int Load(int logical)
        {
            if (logical < 0 || logical > HardwareCpuSets.Count()) return 0;
            return (int)HardwareCpuSets[logical].Load;
        }
        /// <summary>
        /// CpuSet ID for Logical
        /// </summary>
        public static int? CpuSetID(int logical)
        {
            if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
            return HardwareCpuSets[logical].Id;
        }
        /// <summary>
        /// CpuSet EfficiencyClass for Logical
        /// </summary>
        public static int? CpuSetEfficiencyClass(int logical)
        {
            if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
            return HardwareCpuSets[logical].EfficiencyClass;
        }
        /// <summary>
        /// CpuSet CoreIndex for Logical
        /// </summary>
        public static int? CpuSetCoreIndex(int logical)
        {
            if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
            return HardwareCpuSets[logical].CoreIndex;
        }
        /// <summary>
        /// CpuSet NumaNodeIndex for Logical
        /// </summary>
        public static int? CpuSetNumaNodeIndex(int logical)
        {
            if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
            return HardwareCpuSets[logical].NumaNodeIndex;
        }
        /// <summary>
        /// CpuSet LastLevelCacheIndex for Logical
        /// </summary>
        public static int? CpuSetLastLevelCacheIndex(int logical)
        {
            if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
            return HardwareCpuSets[logical].LastLevelCacheIndex;
        }
        /// <summary>
        /// CpuSet Group for Logical
        /// </summary>
        public static int? CpuSetGroup(int logical)
        {
            if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
            return HardwareCpuSets[logical].Group;
        }
        /// <summary>
        /// CpuSet SchedulingClass for Logical
        /// </summary>
        public static int? CpuSetSchedulingClass(int logical)
        {
            if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
            return HardwareCpuSets[logical].SchedulingClass;
        }
        /// <summary>
        /// CpuSet AllocationTag for Logical
        /// </summary>
        public static long? CpuSetAllocationTag(int logical)
        {
            if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
            return HardwareCpuSets[logical].AllocationTag;
        }
        /// <summary>
        /// CpuSet Parked for Logical
        /// </summary>
        public static int? CpuSetParked(int logical)
        {
            if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
            return HardwareCpuSets[logical].Parked;
        }
        /// <summary>
        /// CpuSet Allocated for Logical
        /// </summary>
        public static int? CpuSetAllocated(int logical)
        {
            if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
            return HardwareCpuSets[logical].Allocated;
        }
        /// <summary>
        /// CpuSet RealTime for Logical
        /// </summary>
        public static int? CpuSetRealtime(int logical)
        {
            if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
            return HardwareCpuSets[logical].RealTime;
        }
        /// <summary>
        /// CpuSet AllocatedToTargetProcess for Logical
        /// </summary>
        public static int? CpuSetAllocatedToTargetProcess(int logical)
        {
            if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
            return HardwareCpuSets[logical].AllocatedToTargetProcess;
        }
        /// <summary>
        /// Get Tiered Priority for Logical
        /// </summary>
        public static int? TieredPriority(int logical)
        {
            if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
            return HardwareCpuSets[logical].TieredPriority;
        }
        /// <summary>
        /// Get ThreadNumber for Logical
        /// </summary>
        public static int? ThreadNumber(int logical)
        {
            if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
            return HardwareCpuSets[logical].ThreadNumber;
        }
        /// <summary>
        /// Get Escluded for Logical
        /// </summary>
        public static bool? Excluded(int logical)
        {
            if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
            return HardwareCpuSets[logical].Excluded;
        }
        /// <summary>
        /// Get Disabled for Logical
        /// </summary>
        public static bool? Disabled(int logical)
        {
            if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
            return HardwareCpuSets[logical].Disabled;
        }
        /// <summary>
        /// CpuSet Cluster for Logical
        /// </summary>
        public static int? CpuSetCluster(int logical)
        {
            if (logical < 0 || logical > HardwareCpuSets.Count()) return null;
            return HardwareCpuSets[logical].Cluster;
        }
        private class HardwareCpuSet : IHardwareCpuSet
        {
            [SupportedOSPlatform("windows")]
            public HardwareCpuSet(SYSTEM_CPU_SET_INFORMATION x)
            {
                CpuSet = x;
                Id = (int)x.CpuSetUnion.CpuSet.Id;
                EfficiencyClass = (int)x.CpuSetUnion.CpuSet.EfficiencyClass;
                LogicalProcessorIndex = (int)x.CpuSetUnion.CpuSet.LogicalProcessorIndex;
                CoreIndex = (int)x.CpuSetUnion.CpuSet.CoreIndex;
                NumaNodeIndex = (int)x.CpuSetUnion.CpuSet.NumaNodeIndex;
                LastLevelCacheIndex = (int)x.CpuSetUnion.CpuSet.LastLevelCacheIndex;
                Group = (int)x.CpuSetUnion.CpuSet.Group;
                SchedulingClass = (int)x.CpuSetUnion.CpuSet.CpuSetSchedulingClass.SchedulingClass;
                AllocationTag = (int)x.CpuSetUnion.CpuSet.AllocationTag;
                AllFlagsStruct = x.CpuSetUnion.CpuSet.AllFlagsStruct.AllFlagsStruct;
                TieredPriority = (int)x.CpuSetUnion.CpuSet.CpuSetSchedulingClass.SchedulingClass;
                ThreadNumber = 0;
                Cluster = 1;
                Load = 0;
                LoadZero = 0;
                LoadMedium = 0;
                LoadHigh = 0;
                LoadLow = 0;
                ForcedEnable = false;
                ForcedWhen = DateTime.MinValue;
                Disabled = false;
                Excluded = false;
                
                LoadCounter = CpuLoadPerfCounter ? new PerformanceCounter(
                "Processor",
                "% Processor Time",
                $"{(int)x.CpuSetUnion.CpuSet.LogicalProcessorIndex}",
                true
                ) : new PerformanceCounter();
                LoadCounter.NextValue();
            }
            public SYSTEM_CPU_SET_INFORMATION CpuSet { get; private set; }
            public int Id { get; private set; }
            public int EfficiencyClass { get; private set; }
            public int LogicalProcessorIndex { get; private set; }
            public int CoreIndex { get; private set; }
            public int NumaNodeIndex { get; private set; }
            public int LastLevelCacheIndex { get; private set; }
            public int Group { get; private set; }
            public int SchedulingClass { get; private set; }
            public long AllocationTag { get; private set; }
            public byte AllFlagsStruct { get; private set; }
            public int Cluster { get; set; }
            public float Load { get; set; }
            public int LoadZero { get; set; }
            public int LoadLow { get; set; }
            public int LoadMedium { get; set; }
            public int LoadHigh { get; set; }
            public int TieredPriority { get; set; }
            public int ThreadNumber { get; set; }
            public bool ForcedEnable { get; set; }
            public DateTime ForcedWhen { get; set; }
            public bool Disabled { get; set; }
            public bool Excluded { get; set; }

            public PerformanceCounter LoadCounter { get; set; }
        }
        enum SE_OBJECT_TYPE
        {
            SE_UNKNOWN_OBJECT_TYPE = 0,
            SE_FILE_OBJECT,
            SE_SERVICE,
            SE_PRINTER,
            SE_REGISTRY_KEY,
            SE_LMSHARE,
            SE_KERNEL_OBJECT,
            SE_WINDOW_OBJECT,
            SE_DS_OBJECT,
            SE_DS_OBJECT_ALL,
            SE_PROVIDER_DEFINED_OBJECT,
            SE_WMIGUID_OBJECT,
            SE_REGISTRY_WOW64_32KEY
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESSORCORE
        {
            public byte Flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NUMANODE
        {
            public uint NodeNumber;
        }
        private enum PROCESSOR_CACHE_TYPE
        {
            CacheUnified,
            CacheInstruction,
            CacheData,
            CacheTrace
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CACHE_DESCRIPTOR
        {
            public byte Level;
            public byte Associativity;
            public ushort LineSize;
            public uint Size;
            public PROCESSOR_CACHE_TYPE Type;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct SYSTEM_LOGICAL_PROCESSOR_INFORMATION_UNION
        {
            [FieldOffset(0)]
            public PROCESSORCORE ProcessorCore;
            [FieldOffset(0)]
            public NUMANODE NumaNode;
            [FieldOffset(0)]
            public CACHE_DESCRIPTOR Cache;
            [FieldOffset(0)]
            private UInt64 Reserved1;
            [FieldOffset(8)]
            private UInt64 Reserved2;
        }
        private enum LOGICAL_PROCESSOR_RELATIONSHIP
        {
            RelationProcessorCore,
            RelationNumaNode,
            RelationCache,
            RelationProcessorPackage,
            RelationGroup,
            RelationAll = 0xffff
        }
        private struct SYSTEM_LOGICAL_PROCESSOR_INFORMATION
        {
            public UIntPtr ProcessorMask;
            public LOGICAL_PROCESSOR_RELATIONSHIP Relationship;
            public SYSTEM_LOGICAL_PROCESSOR_INFORMATION_UNION ProcessorInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEM_CPU_SET_INFORMATION_CPUSET
        {
            public int Id;
            public short Group;
            public byte LogicalProcessorIndex;
            public byte CoreIndex;
            public byte LastLevelCacheIndex;
            public byte NumaNodeIndex;
            public byte EfficiencyClass;
            public CPUSET_ALLFLAGS AllFlagsStruct;
            public CPUSET_SCHEDULINGCLASS CpuSetSchedulingClass;
            public long AllocationTag;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct CPUSET_SCHEDULINGCLASS
        {
            [FieldOffset(0)]
            public int Reserved;
            [FieldOffset(0)]
            public byte SchedulingClass;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct CPUSET_ALLFLAGS
        {
            [FieldOffset(0)]
            public byte AllFlags;
            [FieldOffset(0)]
            public byte AllFlagsStruct;
        }
        
        private enum CPUSET_TYPE
        {
            CpuSetInformation
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEM_CPU_SET_INFORMATION
        {
            public int Size;
            public CPUSET_TYPE Type;
            public SYSTEM_CPU_SET_INFORMATION_CPUSET_UNION CpuSetUnion;
        }
        [StructLayout(LayoutKind.Explicit)]
        private struct SYSTEM_CPU_SET_INFORMATION_CPUSET_UNION
        {
            [FieldOffsetAttribute(0)]
            public SYSTEM_CPU_SET_INFORMATION_CPUSET CpuSet;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROC_CPU_SET_INFORMATION_CPUSET
        {
            public int Id;
            public short Group;
            public byte LogicalProcessorIndex;
            public byte CoreIndex;
            public byte LastLevelCacheIndex;
            public byte NumaNodeIndex;
            public byte EfficiencyClass;
            public PROC_CPUSET_ALLFLAGS AllFlagsStruct;
            public PROC_CPUSET_SCHEDULINGCLASS CpuSetSchedulingClass;
            public long AllocationTag;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct PROC_CPUSET_SCHEDULINGCLASS
        {
            [FieldOffset(0)]
            public int Reserved;
            [FieldOffset(0)]
            public byte SchedulingClass;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct PROC_CPUSET_ALLFLAGS
        {
            [FieldOffset(0)]
            public byte AllFlags;
            [FieldOffset(0)]
            public byte AllFlagsStruct;
        }

        private enum PROC_CPUSET_TYPE
        {
            CpuSetInformation
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROC_CPU_SET_INFORMATION
        {
            public int Size;
            public PROC_CPUSET_TYPE Type;
            public PROC_CPU_SET_INFORMATION_CPUSET_UNION CpuSetUnion;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct PROC_CPU_SET_INFORMATION_CPUSET_UNION
        {
            [FieldOffsetAttribute(0)]
            public PROC_CPU_SET_INFORMATION_CPUSET CpuSet;
        }
        private enum _SYSTEM_INFORMATION_CLASS
        {
            SystemAllowedCpuSetsInformation = 168,
            SystemCpuSetInformation = 175,
            SystemCpuSetTagInformation = 176,
        }
        
        [StructLayout(LayoutKind.Explicit)]
        private struct SYSTEM_CPU_SET_MASK
        {
            [FieldOffsetAttribute(0)]
            public ulong Mask;
        }
        
        [ComVisible(true)]
        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_PRIVILEGES
        {
            public int PrivilegeCount;
            public LUID_AND_ATTRIBUTES Privileges;
        }
        
        [ComVisible(true)]
        [StructLayout(LayoutKind.Sequential)]
        private struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;
            public ulong Attributes;
        }
        
        [ComVisible(true)]
        [StructLayout(LayoutKind.Sequential)]
        private struct LUID
        {
            public int LowPart;
            public long HighPart;
        }

        public struct CPC
        {
            public uint HighestPerformance;
            public uint NominalPerformance;
            public uint LowestNonlinearPerformance;
            public uint LowestPerformance;
            public uint GuaranteedPerformanceRegister;
            public uint DesiredPerformanceRegister;
            public uint MinimumPerformanceRegister;
            public uint MaximumPerformanceRegister;
            public uint PerformanceReductionToleranceRegister;
            public uint TimeWindowRegister;
            public ulong CounterWraparoundTime;
            public ulong ReferencePerformanceCounterRegister;
            public ulong DeliveredPerformanceCounterRegister;
            public uint PerformanceLimitedRegister;
            public uint CPPCEnableRegister;
            public uint AutonomousSelectionEnable;
            public uint AutonomousActivityWindowRegister;
            public uint EnergyPerformancePreferenceRegister;
            public uint ReferencePerformance;
        }
        #region Exports
        private const ulong PROCESS_SET_INFORMATION = 0x0200;
        private const ulong PROCESS_SET_LIMITED_INFORMATION = 0x2000;

        private const int ERROR_INSUFFICIENT_BUFFER = 122;

        private const ulong ERROR_NOT_ALL_ASSIGNED = 0x1300L;
        private const ulong SE_PRIVILEGE_ENABLED = 0x00000002L;
        private const ulong TOKEN_ASSIGN_PRIMARY = 0x0001;
        private const ulong TOKEN_DUPLICATE = 0x0002;
        private const ulong TOKEN_IMPERSONATE = 0x0004;
        private const ulong TOKEN_QUERY = 0x0008;
        private const ulong TOKEN_QUERY_SOURCE = 0x0010;
        private const ulong TOKEN_ADJUST_PRIVILEGES = 0x0020;
        private const ulong TOKEN_ADJUST_GROUPS = 0x0040;
        private const ulong TOKEN_ADJUST_DEFAULT = 0x0080;
        private const ulong TOKEN_ADJUST_SESSIONID = 0x0100;
        private const ulong TOKEN_ALL_ACCESS_P = 0x0001;
        private const ulong STANDARD_RIGHTS_REQUIRED = 0x000F0000L;
        private const ulong TOKEN_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED |
                                TOKEN_ASSIGN_PRIMARY |
                                TOKEN_DUPLICATE |
                                TOKEN_IMPERSONATE |
                                TOKEN_QUERY |
                                TOKEN_QUERY_SOURCE |
                                TOKEN_ADJUST_PRIVILEGES |
                                TOKEN_ADJUST_GROUPS |
                                TOKEN_ADJUST_SESSIONID |
                                TOKEN_ADJUST_DEFAULT;

        [DllImport(@"kernel32.dll", SetLastError = true)]
        private static extern bool GetLogicalProcessorInformation(
            IntPtr Buffer,
            ref uint ReturnLength
        );

        private static SYSTEM_LOGICAL_PROCESSOR_INFORMATION[] GetLogicalProcessorInformation()
        {
            uint ReturnLength = 0;
            GetLogicalProcessorInformation(IntPtr.Zero, ref ReturnLength);
            if (Marshal.GetLastWin32Error() == ERROR_INSUFFICIENT_BUFFER)
            {
                IntPtr Ptr = Marshal.AllocHGlobal((int)ReturnLength);
                try
                {
                    if (GetLogicalProcessorInformation(Ptr, ref ReturnLength))
                    {
                        int size = Marshal.SizeOf(typeof(SYSTEM_LOGICAL_PROCESSOR_INFORMATION));
                        int len = (int)ReturnLength / size;
                        SYSTEM_LOGICAL_PROCESSOR_INFORMATION[] Buffer = new SYSTEM_LOGICAL_PROCESSOR_INFORMATION[len];
                        IntPtr Item = Ptr;
                        for (int i = 0; i < len; i++)
                        {
                            Buffer[i] = (SYSTEM_LOGICAL_PROCESSOR_INFORMATION)Marshal.PtrToStructure(Item, typeof(SYSTEM_LOGICAL_PROCESSOR_INFORMATION));
                            Item += size;
                        }
                        return Buffer;
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(Ptr);
                }
            }
            return null;
        }

        [DllImport(@"kernel32.dll", SetLastError = true)]
        private static extern bool GetSystemCpuSetInformation(
            IntPtr Buffer,
            uint BufferLength,
            ref uint ReturnLength,
            IntPtr handle,
            uint Flags
        );

        private static SYSTEM_CPU_SET_INFORMATION[] GetSystemCpuSetInformation()
        {
            uint ReturnLength = 0;
            IntPtr CurProc = Process.GetCurrentProcess().Handle;
            GetSystemCpuSetInformation(IntPtr.Zero, 0, ref ReturnLength, CurProc, 0);
            if (Marshal.GetLastWin32Error() == ERROR_INSUFFICIENT_BUFFER)
            {
                IntPtr Ptr = Marshal.AllocHGlobal((int)ReturnLength);
                try
                {
                    if (GetSystemCpuSetInformation(Ptr, ReturnLength, ref ReturnLength, CurProc, 0))
                    {
                        int size = Marshal.SizeOf(typeof(SYSTEM_CPU_SET_INFORMATION));
                        int len = (int)ReturnLength / size;
                        //System.Diagnostics.App.LogDebug($"CPUSET SIZE={size} len={len} ReturnLength={ReturnLength}");
                        SYSTEM_CPU_SET_INFORMATION[] Buffer = new SYSTEM_CPU_SET_INFORMATION[len];
                        IntPtr Item = Ptr;
                        for (int i = 0; i < len; i++)
                        {
                            Buffer[i] = (SYSTEM_CPU_SET_INFORMATION)Marshal.PtrToStructure(Item, typeof(SYSTEM_CPU_SET_INFORMATION));
                            //App.LogDebug($"PI CpuSet Logical={Buffer[i].CpuSetUnion.CpuSet.LogicalProcessorIndex} Cache={Buffer[i].CpuSetUnion.CpuSet.LastLevelCacheIndex} Sch={Buffer[i].CpuSetUnion.CpuSet.CpuSetSchedulingClass.SchedulingClass} Eff={Buffer[i].CpuSetUnion.CpuSet.EfficiencyClass} Id={Buffer[i].CpuSetUnion.CpuSet.Id}");
                            Item += size;
                        }
                        return Buffer;
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(Ptr);
                }
            }
            return null;
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentThread();

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32")]
        public static extern int GetCurrentThreadId();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetCurrentProcessorNumber();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern UIntPtr SetThreadAffinityMask(IntPtr handle, UIntPtr mask);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern UIntPtr SetProcessAffinityUpdateMode(IntPtr handle, uint flags);

        [DllImport(@"kernel32.dll")]
        private static extern bool SetProcessDefaultCpuSets(
            IntPtr ProcHandle,
            IntPtr ProcCpuSets,
            ulong ProcCpuSetsCount
        );

        [DllImport(@"ntdll.dll")]
        private static extern int NtSetSystemInformation(
            int SIClass,
            IntPtr Data,
            long Size
        );
        [DllImport(@"ntdll.dll")]
        private static extern int NtSetSystemInformation(
            int SIClass,
            long Data,
            long Size
        );
        [DllImport(@"kernel32.dll")]
        private static extern bool OpenProcessToken(
            [In] IntPtr ProcessHandle,
            [In] ulong DesiredAccess,
            [In, Out] ref IntPtr TokenHandle
        );
        [DllImport(@"kernel32.dll")]
        private static extern bool CloseHandle(
            [In] IntPtr Handle
        );
        [DllImport(@"advapi32.dll", SetLastError = true)]
        private static extern bool AdjustTokenPrivileges(
            [In] IntPtr TokenHandle,
            [In] bool DisableAllPrivileges,
            [In] ref TOKEN_PRIVILEGES NewState,
            [In] int BufferLength
        );
        [DllImport(@"advapi32.dll", SetLastError = true)]
        private static extern bool AdjustTokenPrivileges(
            [In] IntPtr TokenHandle,
            [In] bool DisableAllPrivileges,
            [In] TOKEN_PRIVILEGES NewState,
            [In] int BufferLength
        );
        [DllImport(@"advapi32.dll", SetLastError = true)]
        private static extern bool AdjustTokenPrivileges(
            [In] IntPtr TokenHandle,
            [In] bool DisableAllPrivileges,
            [In] ref IntPtr NewState,
            [In] int BufferLength
        );
        [DllImport(@"advapi32.dll")]
        private static extern bool LookupPrivilegeValueA(
            [In, Optional] string lpSystemName,
            [In] string lpName,
            [In] ref LUID lpLuid
        );
        [DllImport(@"advapi32.dll", CharSet = CharSet.Unicode)]
        private static extern bool LookupPrivilegeValueW(
            [In, Optional] string lpSystemName,
            [In] string lpName,
            [In] ref LUID lpLuid
        );

        [DllImport(@"kernel32.dll")]
        public static extern int SetThreadIdealProcessor(
            IntPtr ProcHandle,
            uint IdealLogicalProcessor
        );

        [DllImport(@"kernel32.dll")]
        public static extern IntPtr OpenProcess(
            int dwDesiredAccess,
            bool bInheritHandle,
            IntPtr dwProcessId
        );

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool QueryPerformanceFrequency(ref LARGE_INTEGER lpFrequency);
        [StructLayout(LayoutKind.Explicit, Size = 8)]
        struct LARGE_INTEGER
        {
            [FieldOffset(0)] public long QuadPart;

            [FieldOffset(0)] public uint LowPart;
            [FieldOffset(4)] public int HighPart;

            [FieldOffset(0)] public int LowPartAsInt;
            [FieldOffset(0)] public uint LowPartAsUInt;

            [FieldOffset(4)] public int HighPartAsInt;
            [FieldOffset(4)] public uint HighPartAsUInt;
        }
        public static int GetPerformanceFrequency(ref long freq)
        {

            LARGE_INTEGER pfreq = new LARGE_INTEGER();

            bool _res  = QueryPerformanceFrequency(ref pfreq);

            App.LogDebug($"hi:{pfreq.HighPartAsInt} lo:{pfreq.HighPartAsInt}");
            App.LogDebug($"hi:{pfreq.HighPart} lo:{pfreq.LowPart}");
            App.LogDebug($"hi:{pfreq.HighPartAsUInt} lo:{pfreq.LowPartAsUInt}");

            freq = pfreq.QuadPart;

            if (!_res) return Marshal.GetLastWin32Error();
            return 0;

        }

        [DllImport("kernel32.dll", EntryPoint = "GetSystemFirmwareTable", SetLastError = true)]
        private static extern uint GetSystemFirmwareTable(
            [In] uint FirmwareTableProviderSignature,
            [In] uint FirmwareTableID, 
            [MarshalAs(UnmanagedType.LPArray)] [Out] byte[] FirmwareTableBuffer,
            [In] uint BufferSize
        );

        private const uint FirmwareTableProviderSignature = 0x41435049;
        private const uint FirmwareTableID = 0x54434350;

        public static void GetSystemFirmwareTable()
        {
            byte[] FirmwareTableBuffer = null;
            uint ReturnLength = 0;
            ReturnLength = GetSystemFirmwareTable(FirmwareTableProviderSignature, FirmwareTableID, FirmwareTableBuffer, 0);
            if (Marshal.GetLastWin32Error() == ERROR_INSUFFICIENT_BUFFER)
            {
                IntPtr Item = IntPtr.Zero;
                try
                {
                    FirmwareTableBuffer = new byte[ReturnLength];
                    uint ReturnLengthCheck = GetSystemFirmwareTable(FirmwareTableProviderSignature, FirmwareTableID, FirmwareTableBuffer, ReturnLength);
                    uint length = BitConverter.ToUInt32(FirmwareTableBuffer, 4);
                    uint revision = FirmwareTableBuffer[8];
                    //expor revision
                    App.LogDebug($"GetSystemFirmwareTable ReturnLength {ReturnLength} ReturnLengthCheck {ReturnLengthCheck} Length {length} Revision {revision}");
                    if (ReturnLength == ReturnLengthCheck && FirmwareTableBuffer[48] == 0)
                    {
                        ulong offset = BitConverter.ToUInt64(FirmwareTableBuffer, 56);
                    }
                    else
                    {
                        App.LogDebug($"GetSystemFirmwareTable Error {Marshal.GetLastWin32Error()}");
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(Item);
                }
            }
            else
            {
                App.LogDebug($"GetSystemFirmwareTable lastError {Marshal.GetLastWin32Error()}");
            }
            
        }

        public static object GetObjectFromBytes(byte[] buffer, Type objType)
        {
            object obj = null;
            if ((buffer != null) && (buffer.Length > 0))
            {
                IntPtr ptrObj = IntPtr.Zero;
                try
                {
                    int objSize = Marshal.SizeOf(objType);
                    if (objSize > 0)
                    {
                        if (buffer.Length < objSize)
                            throw new Exception(String.Format("Buffer smaller than needed for creation of object of type {0}", objType));
                        ptrObj = Marshal.AllocHGlobal(objSize);
                        if (ptrObj != IntPtr.Zero)
                        {
                            Marshal.Copy(buffer, 0, ptrObj, objSize);
                            obj = Marshal.PtrToStructure(ptrObj, objType);
                        }
                        else
                            throw new Exception(String.Format("Couldn't allocate memory to create object of type {0}", objType));
                    }
                }
                finally
                {
                    if (ptrObj != IntPtr.Zero)
                        Marshal.FreeHGlobal(ptrObj);
                }
            }
            return obj;
        }

        #endregion
    }
}
