using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using LibreHardwareMonitor.Hardware;
using System.Timers;
using System.Diagnostics;
using System.IO;
using ZenStates.Core;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace CPUDoc
{
    public class ThreadBooster
    {
        public static CancellationToken tbtoken = new CancellationToken();
        public static CancellationToken systoken = new CancellationToken();
        public static int PoolingTick = 0;
        public static int PoolingInterval = 250;
        public static int PoolingIntervalSlow = 1000;
        public static int PoolingIntervalDefault = 250;
        public static int PoolingIntervalSlowDefault = 1000;
        public static uint defBitMask = 0;
        public static uint newBitMask = 0;
        public static uint defFullBitMask = 0;
        //public static uint prevBitMask = 0;
        public static int prevNeedcores = 0;
        public static int prevMorecores = 0;
        public static int basecores = 0;
        public static int addcores = 0;
        public static int needcores = 0;
        public static int usedcores = 0;
        public static int morecores = 0;
        public static int setcores = 0;
        public static int newsetcores = 0;
        public static DateTime prevIncreaseStamp = DateTime.MinValue;
        public static DateTime prevFullcoresStamp = DateTime.MinValue;
        public static TimeSpan _deltaStamp;
        public static DateTime tbpoolingStamp = DateTime.MinValue;
        public static TimeSpan _deltaPooling;
        public static TimeSpan _deltaUA;
        public static TimeSpan _deltaHPX;
        public static TimeSpan _deltaBAL;
        public static DateTime _stampHPX = DateTime.MinValue;
        public static DateTime _stampBAL = DateTime.MinValue;

        public static bool bInit = false;
        public static int IncreaseHysteresis = 6;
        public static bool SetHysteresis = true;
        public static int FullLoadHystSecs = 10;
        public static double HighTotalLoadThreshold = 90;
        public static double HighTotalLoadLowThreshold = 80;
        public static double HighTotalLoadFactor = 0.90;
        public static double TotalT0Load = 0;
        public static double TotalT0LoadNorm = 0;
        public static double _cpuTotalLoad;
        private static int LoadZeroThresholdCount = 3;
        private static int LoadMediumThresholdCount = 3;
        private static int LoadHighThresholdCount = 1;
        private static int ClearForceThreshold = 5;

        public static int hepfg = 5;
        public static int hepbg = 5;
        public static int hepfgdeep = 5;
        public static int hepbgdeep = 5;

        public static List<ZenControlMode> zenControlModes = new List<ZenControlMode>();
        public static bool zencontrol_b = false;
        public static bool zencontrol_b_ppt = false;
        public static bool zencontrol_b_tdc = false;
        public static bool zencontrol_b_edc = false;
        public static bool zencontrol_reapply = false;
        public static int zencontrol_mode = 0;

        public static int _SetSysCpuSet(uint bitMask)
        {
            int _ret = -1;
            if (bitMask == 0)
            {
                App.SetSysCpuSet();
                _ret = App.SetSysCpuSet();
            }
            else
            {
                App.SetSysCpuSet(bitMask);
                _ret = App.SetSysCpuSet(bitMask);
            }

            App.LogDebug($"Exec SetSystemCpuSet 0x{bitMask:X8} Return:{_ret}");

            return _ret;

        }
        public static void BuildDefaultMask()
        {
            defBitMask = 0;
            basecores = 0;
            addcores = 0;

            List<int> _t0;
            List<int> _t1;

            if (App.numazero_b && App.pactive.NumaZero)
            {
                _t0 = App.n0enabledT0;
                _t1 = App.n0enabledT1;
            } else
            {
                _t0 = App.logicalsT0;
                _t1 = App.logicalsT1;
            }


            foreach (int logical in _t0)
            {
                defBitMask |= (uint)1 << (logical);
                basecores++;
                //App.LogDebug($"Build defBitMask 0x{defBitMask:X8} {logical}");
            }
            defFullBitMask = defBitMask;
            foreach (int logical in _t1)
            {
                defFullBitMask |= (uint)1 << (logical);
                addcores++;
                //App.LogDebug($"Build defFullBitMask 0x{defBitMask:X8} {logical}");
            }
            setcores = basecores;
            HighTotalLoadThreshold = (basecores + addcores) * 100 / ProcessorInfo.LogicalCoresCount * HighTotalLoadFactor;
            App.LogDebug($"defBitMask {CountBits(defBitMask)} defFullBitMask {CountBits(defFullBitMask)} N0={App.numazero_b}");
        }

        public static void OnThreadBooster(object sender, ElapsedEventArgs args)
        {
            try
            {
                tbtoken = new CancellationToken();
                tbtoken = (CancellationToken)App.tbcts.Token;
                _cpuTotalLoad = ProcessorInfo.cpuTotalLoad;

                if (tbpoolingStamp != DateTime.MinValue)
                {
                    _deltaPooling = DateTime.Now - tbpoolingStamp;
                    int _pooling = (int)_deltaPooling.TotalMilliseconds;
                    App.TBPoolingAverage.Push(_pooling);
                    //App.LogDebug($"POOLING={_pooling}ms UTILITY={App.TBPoolingAverage.Current-ThreadBooster.PoolingInterval:0}ms MAX={App.TBPoolingAverage.GetMax}ms");
                }
                tbpoolingStamp = DateTime.Now;

                //Process.GetCurrentProcess().PriorityBoostEnabled = true;

                if (tbtoken.IsCancellationRequested)
                {
                    App.LogDebug("TB CANCELLATION REQUESTED");
                    tbtoken.ThrowIfCancellationRequested();
                }

                if (!bInit)
                {
                    BuildDefaultMask();
                    if (!App.pactive.SysSetHack && App.pactive.NumaZero)
                    {
                        App.SysCpuSetMask = defFullBitMask;
                    } else
                    {
                        App.SysCpuSetMask = defBitMask;
                    }
                    App.lastSysCpuSetMask = 0;
                    App.systemInfo.PSABias = "";
                    ZenControlInit();
                    bInit = true;
                }

                if (App.reapplyProfile)
                {
                    App.lastSysCpuSetMask = 0;
                    App.SysCpuSetMask = defBitMask;
                    App.reapplyProfile = false;
                }

                if (App.pactive.PowerSaverActive && !App.psact_plan) 
                {
                    App.PSAEnable();
                    if (App.psact_plan)
                    {
                        PSAct_Light(false);
                        PSAct_Deep(false);
                    }
                }
                
                _deltaStamp = DateTime.Now - prevFullcoresStamp;

                if (App.IsForegroundWwindowFullScreen()) App.UAStamp = DateTime.Now;
                if (App.GetIdleTime() < App.pactive.PSALightSleepSeconds * 1000) App.UAStamp = DateTime.Now;
                
                _deltaUA = DateTime.Now - App.UAStamp;

                if (_deltaUA.TotalSeconds > ClearForceThreshold && App.cpuTotalLoad.Current <= App.pactive.PSALightSleepThreshold)
                {
                    foreach (int logical in App.logicalsT1)
                    {
                        ProcessorInfo.ClearForceEnabled(logical);
                    }
                }

                if (App.pactive.PowerSaverActive) PowerSaverActive();

                if (zencontrol_b) ZenControl();

                if (App.pactive.SysSetHack || App.pactive.PowerSaverActive)
                {
                    int _computedhlt = (int)(CountBits(defFullBitMask) * 100 / ProcessorInfo.LogicalCoresCount * HighTotalLoadFactor);
                    if (_cpuTotalLoad > _computedhlt || _deltaStamp.TotalSeconds < FullLoadHystSecs)
                    {
                        App.tbtimer.Interval = PoolingIntervalSlow;

                        if (_cpuTotalLoad > _computedhlt) prevFullcoresStamp = DateTime.Now;
                        //App.LogDebug($"Full CpuLoad: {ProcessorInfo.cpuTotalLoad:0} {_deltaStamp.TotalSeconds}");

                        if (defFullBitMask != App.lastSysCpuSetMask && App.pactive.SysSetHack)
                        {
                            App.LogDebug($"SysCpuSetMask defFullBitMask 0x{defFullBitMask:X8}");
                            setcores = basecores + addcores;
                            App.SysCpuSetMask = defFullBitMask;
                        }
                    }
                    else
                    {
                        App.tbtimer.Interval = PoolingInterval;
                        //App.LogDebug($"CpuLoad: {ProcessorInfo.cpuTotalLoad:0}");
                        //App.LogDebug($"\tLoad-0-T0={ProcessorInfo.HardwareCpuSets[0].Load:0} \tLoad-0-T1={ProcessorInfo.HardwareCpuSets[1].Load:0}");

                        if (App.pactive.SysSetHack)
                        {
                            needcores = 0;
                            usedcores = 0;
                            newsetcores = 0;
                            morecores = 0;
                            TotalT0Load = 0;

                            newBitMask = defBitMask;

                            foreach (int logical in App.logicalsT0)
                            {
                                if (ProcessorInfo.HardwareCpuSets[logical].LoadHigh > LoadHighThresholdCount)
                                    needcores++;
                                if (ProcessorInfo.HardwareCpuSets[logical].LoadMedium > LoadMediumThresholdCount)
                                    usedcores++;
                                TotalT0Load += ProcessorInfo.HardwareCpuSets[logical].Load;

                                //App.LogDebug($"L:{logical} Load:{ProcessorInfo.HardwareCpuSets[logical].Load:0} High:{ProcessorInfo.HardwareCpuSets[logical].LoadHigh} Zero:{ProcessorInfo.HardwareCpuSets[logical].LoadZero}");
                            }

                            foreach (int logical in App.logicalsT1)
                            {
                                if (ProcessorInfo.HardwareCpuSets[logical].LoadHigh > LoadHighThresholdCount)
                                    needcores++;
                                if (ProcessorInfo.HardwareCpuSets[logical].LoadMedium > LoadMediumThresholdCount)
                                {
                                    ProcessorInfo.SetForceEnabled(logical);
                                    usedcores++;
                                }
                                else if (ProcessorInfo.HardwareCpuSets[logical].LoadZero > LoadZeroThresholdCount)
                                {
                                    _deltaStamp = DateTime.Now - ProcessorInfo.HardwareCpuSets[logical].ForcedWhen;
                                    if (_deltaStamp.TotalSeconds > ClearForceThreshold)
                                        ProcessorInfo.ClearForceEnabled(logical);
                                }
                            }

                            morecores = needcores - basecores > 0 ? needcores - basecores : 0;

                            TotalT0LoadNorm = TotalT0Load / basecores;

                            if (needcores >= basecores && TotalT0LoadNorm > 98)
                                morecores += (usedcores - needcores) / 2 >= 1 ? (usedcores - needcores) / 2 : 0;

                            if (morecores > addcores) morecores = addcores;

                            newsetcores = basecores + morecores;

                            _deltaStamp = DateTime.Now - prevIncreaseStamp;

                            SetHysteresis = (_deltaStamp.TotalSeconds <= IncreaseHysteresis && newsetcores <= setcores) ? true : false;

                            //App.LogDebug($"SetHysteresis {SetHysteresis} {_deltaStamp.TotalSeconds}");

                            if (newsetcores < setcores && _deltaStamp.TotalSeconds <= IncreaseHysteresis)
                            {
                                //App.LogDebug($"Slow decreasing");
                                morecores = prevMorecores - 1;
                            }
                            else if (newsetcores >= basecores && setcores <= basecores && needcores >= basecores && TotalT0LoadNorm > 98)
                            {
                                morecores = addcores;
                            }

                            newsetcores = basecores + morecores;

                            for (int i = 0; i < addcores; ++i)
                            {
                                if (i < morecores || 
                                    (ProcessorInfo.HardwareCpuSets[App.logicalsT1[i]].ForcedEnable && !App.numazero_b) ||
                                    (ProcessorInfo.HardwareCpuSets[App.logicalsT1[i]].ForcedEnable && App.numazero_b && App.n0enabledT1.Contains(i))) {
                                    {
                                        newBitMask |= (uint)1 << (App.logicalsT1[i]);
                                    }
                                }
                                //App.LogDebug($"i={i} T1Forced={ProcessorInfo.HardwareCpuSets[App.logicalsT1[i]].ForcedEnable} calc newbitMask 0x{newBitMask:X8} Morecores:{morecores}");
                            }

                            if (newBitMask != App.SysCpuSetMask && !SetHysteresis)
                            {
                                //App.LogDebug($"set newbitMask 0x{newBitMask:X8} prevBitMask 0x{App.SysCpuSetMask:X8} {morecores}");
                                //App.LogDebug($"setcores {setcores} newsetcores {newsetcores} basecores {basecores} morecores {morecores}");
                                prevNeedcores = needcores;
                                prevMorecores = morecores;
                                if (newsetcores > setcores) prevIncreaseStamp = DateTime.Now;
                                setcores = newsetcores;
                                App.SysCpuSetMask = newBitMask;
                            }
                            //if (needcores > 0 || morecores > 0 || usedcores > basecores)
                            //    App.LogDebug($"needcores {needcores} morecores {morecores} usedcores {usedcores} setcores {setcores} newsetcores {newsetcores} T0Load {TotalT0LoadNorm}");
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                App.SetSysCpuSet(0);
                App.PSAPlanDisable();
                App.systemInfo.PSABias = "";
                App.LogDebug("ThreadBooster cycle exiting due to OperationCanceled");
                throw;
            }
            catch (Exception ex)
            {
                App.SetSysCpuSet(0);
                App.PSAPlanDisable();
                App.systemInfo.PSABias = "";
                App.LogExError($"ThreadBooster cycle Exception: {ex.Message}", ex); 
            }
            finally
            {
                PoolingTick++;
                PoolingTick = PoolingTick > 4 ? 0 : PoolingTick;
                //App.LogDebug($"TB MONITOR TICK {App.tbtimer.Interval}ms");
            }
        }
        public static void SetPSAActive(int? id)
        {
            if (id == null) id = 1;

            uint _value;
            string _label = (id == 0) ? " [LowPower]" : (id == 1) ? " [Balanced]" : " [HighPerformance]";
            App.systemInfo.PSABias = _label;
            App.systemInfo.SetPSAStatus(true);
            
            //Processor performance time check interval
            _value = (uint)((id == 0) ? 15 : (id == 1) ? 30: 30);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("4d2b0152-7d5c-498b-88e2-34345392a2c5"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance increase threshold
            _value = (uint)((id == 0) ? 85 : (id == 1) ? 60 : 30);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("06cadf0e-64ed-448a-8927-ce7bf90eb35d"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance increase threshold for Processor Power Efficiency Class 1
            _value = (uint)((id == 0) ? 95 : (id == 1) ? 60 : 60);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("06cadf0e-64ed-448a-8927-ce7bf90eb35e"), PowerManagerAPI.PowerMode.AC, _value);

            //Minimum processor state
            _value = (uint)((id == 0) ? 80 : (id == 1) ? 100 : 100);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("893dee8e-2bef-41e0-89c6-b55d0929964c"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor idle promote threshold
            _value = (uint)((id == 0) ? 45 : (id == 1) ? 60 : 60);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("7b224883-b3cc-4d79-819f-8374152cbe7c"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance boost policy
            _value = (uint)((id == 0) ? 85 : (id == 1) ? 100 : 100);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("45bcc044-d885-43e2-8605-ee0ec6e96b59"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance decrease time
            _value = (uint)((id == 0) ? 1 : (id == 1) ? 5 : 5);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("d8edeb9b-95cf-4f95-a73c-b061973693c8"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor idle time check
            _value = (uint)((id == 0) ? 20000 : (id == 1) ? 30000 : 50000);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("c4581c31-89ab-4597-8e2b-9c9cab440e6b"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor idle demote threshold
            _value = (uint)((id == 0) ? 25 : (id == 1) ? 35 : 40);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("4b92d758-5a24-4851-a470-815d78aee119"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor duty cycling
            _value = (uint)((id == 0) ? 1 : (id == 1) ? 0 : 0);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("4e4450b3-6179-4e91-b8f1-5bb9938f81a1"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance decrease threshold
            _value = (uint)((id == 0) ? 25 : (id == 1) ? 10 : 5);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("12a0ab44-fe28-4fa9-b3bd-4b64f44960a6"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance decrease threshold for Processor Power Efficiency Class 1
            _value = (uint)((id == 0) ? 60 : (id == 1) ? 60 : 5);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("12a0ab44-fe28-4fa9-b3bd-4b64f44960a7"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor idle threshold scaling
            _value = (uint)((id == 0) ? 1 : (id == 1) ? 0 : 0);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("6c2993b0-8f48-481f-bcc6-00dd2742aa06"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance boost mode
            _value = (uint)((id == 0) ? 3 : (id == 1) ? 2 : 2);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("be337238-0d82-4146-a960-4f3749d470c7"), PowerManagerAPI.PowerMode.AC | PowerManagerAPI.PowerMode.DC, _value);

            //Processor performance increase policy
            _value = (uint)((id == 0) ? 0 : (id == 1) ? 2 : 2);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("465e1f50-b610-473a-ab58-00d1077dc418"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance decrease policy
            _value = (uint)((id == 0) ? 0 : (id == 1) ? 1 : 1);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("40fbefc7-2e9d-4d25-a185-0cfd8574bac6"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance autonomous mode
            _value = (uint)((id == 0) ? 0 : (id == 1) ? 1 : 1);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("8baa4a8a-14c6-4451-8e8b-14bdbd197537"), PowerManagerAPI.PowerMode.AC, _value);

            App.powerManager.SetActiveGuid(App.PPGuid);
        
        }
        public static string ZenControlMode()
        {
            if (App.psact_deep_b) return "deep";
            if (App.psact_light_b) return "light";
            if (App.PSABiasCurrent != null)
            {
                if (App.PSABiasCurrent == 0) return "low";
                if (App.PSABiasCurrent == 1) return "bal";
                if (App.PSABiasCurrent == 2) return "hpx";
            }
            return "hpx";
        }
        public static void ZenControl()
        {
            if (zencontrol_b)
            {
                if (zencontrol_reapply)
                {
                    string _mode = ZenControlMode();

                    ZenControlPBO(_mode);
                }
            }
        }

        public static void ZenControlInit()
        {
            if (App.systemInfo.ZenStates && App.pactive.ZenControl)
            {
                //CHECK IF SMU OP Available
                if (App.systemInfo.ZenPPT > 0) zencontrol_b_ppt = true;
                if (App.systemInfo.ZenTDC > 0) zencontrol_b_tdc = true;
                if (App.systemInfo.ZenEDC > 0) zencontrol_b_edc = true;

                zenControlModes.Clear();

                int _ppt, _tdc, _edc;
                int _autoppt, _autotdc, _autoedc, _ccds;
                string _mode;

                _ccds = App.systemInfo.ZenCCDTotal > 0 ? App.systemInfo.ZenCCDTotal : 1;

                //HIGH PERFORMANCE
                _mode = "hpx";

                _autoppt = (ProcessorInfo.PhysicalCoresCount * 15) + (_ccds * 15);
                _autotdc = (ProcessorInfo.PhysicalCoresCount * 9) + (_ccds * 10);
                _autoedc = (ProcessorInfo.PhysicalCoresCount * 12) + (_ccds * 10);

                _autoppt = App.pactive.ZenControlPPTAuto ? _autoppt : App.pactive.ZenControlPPThpx;
                _autotdc = App.pactive.ZenControlTDCAuto ? _autotdc : App.pactive.ZenControlTDChpx;
                _autoedc = App.pactive.ZenControlEDCAuto ? _autoedc : App.pactive.ZenControlEDChpx;

                _ppt = App.pactive.ZenControlPPTAuto && !zencontrol_b_ppt ? 0 : _autoppt;
                _tdc = App.pactive.ZenControlTDCAuto && !zencontrol_b_tdc ? 0 : _autotdc;
                _edc = App.pactive.ZenControlEDCAuto && !zencontrol_b_edc ? 0 : _autoedc;

                zenControlModes.Add(new ZenControlMode(_mode, _ppt, _tdc, _edc));

                //BALANCED
                _mode = "bal";

                _ppt = App.pactive.ZenControlPPTAuto && !zencontrol_b_ppt ? 0 : (_autoppt - (int)((double)25 * (_autoppt / 100)));
                _tdc = App.pactive.ZenControlTDCAuto && !zencontrol_b_tdc ? 0 : (_autotdc - (int)((double)25 * (_autotdc / 100)));
                _edc = App.pactive.ZenControlEDCAuto && !zencontrol_b_edc ? 0 : (_autoedc - (int)((double)25 * (_autoedc / 100)));

                zenControlModes.Add(new ZenControlMode(_mode, _ppt, _tdc, _edc));

                //LOWPOWER
                _mode = "low";

                _ppt = App.pactive.ZenControlPPTAuto && !zencontrol_b_ppt ? 0 : (_autoppt - (int)((double)50 * (_autoppt / 100)));
                _tdc = App.pactive.ZenControlTDCAuto && !zencontrol_b_tdc ? 0 : (_autotdc - (int)((double)50 * (_autotdc / 100)));
                _edc = App.pactive.ZenControlEDCAuto && !zencontrol_b_edc ? 0 : (_autoedc - (int)((double)50 * (_autoedc / 100)));

                zenControlModes.Add(new ZenControlMode(_mode, _ppt, _tdc, _edc));
                
                //LIGHT SLEEP
                _mode = "light";
                _ppt = App.pactive.ZenControlPPTAuto && !zencontrol_b_ppt ? 0 : 60 > _autoppt ? _autoppt : 60;
                _tdc = App.pactive.ZenControlTDCAuto && !zencontrol_b_tdc ? 0 : 45 > _autotdc ? _autotdc : 45;
                _edc = App.pactive.ZenControlEDCAuto && !zencontrol_b_edc ? 0 : 65 > _autoedc ? _autoedc : 65;

                zenControlModes.Add(new ZenControlMode(_mode, _ppt, _tdc, _edc));

                //DEEP SLEEP
                _mode = "deep";
                _ppt = App.pactive.ZenControlPPTAuto && !zencontrol_b_ppt ? 0 : 60 > _autoppt ? _autoppt : 45;
                _tdc = App.pactive.ZenControlTDCAuto && !zencontrol_b_tdc ? 0 : 45 > _autotdc ? _autotdc : 25;
                _edc = App.pactive.ZenControlEDCAuto && !zencontrol_b_edc ? 0 : 65 > _autoedc ? _autoedc : 45;

                zenControlModes.Add(new ZenControlMode(_mode, _ppt, _tdc, _edc));

                zencontrol_b = true;
            }
        }
        public static void ZenControlPBO(string _mode)
        {
            try
            {
                ZenControlMode zcmode = GetZenControlMode(_mode);
                if (zcmode != null)
                {
                    if (zencontrol_b_ppt) App.systemInfo.Zen.SetPPTLimit((uint)zcmode.PPT);
                    if (zencontrol_b_tdc) App.systemInfo.Zen.SetTDCVDDLimit((uint)zcmode.TDC);
                    if (zencontrol_b_edc) App.systemInfo.Zen.SetEDCVDDLimit((uint)zcmode.EDC);
                }
            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception ZenControlPBO:", ex);
            }
        }
        public static ZenControlMode GetZenControlMode(string _mode)
        {
            try
            {
                foreach(ZenControlMode zc in zenControlModes)
                {
                    if (zc.Mode == _mode) return zc;
                }
                return null;
            }
            catch (Exception ex)
            {
                App.LogExInfo($"Exception GetZenControlMode:", ex);
                return null;
            }
        }
        public static void PowerSaverActive()
        {
            int _cpuTotalLoad = (int)App.cpuTotalLoad.Current;
            int _cpuTotalLoadLong = (int)App.cpuTotalLoadLong.Current;

            _deltaHPX = DateTime.Now - _stampHPX;
            _deltaBAL = DateTime.Now - _stampBAL;

            //App.LogDebug($"PTick {PoolingTick}");
            if (PoolingTick == 4)
            {
                if (App.powerManager.GetActiveGuid() != App.PPGuid)
                {
                    App.powerManager.SetActiveGuid(App.PPGuid);
                    App.LogDebug("Power Plan fix: back to Dynamic");
                }
            }

            //App.LogDebug($"{_deltaHPX.TotalSeconds}>{App.pactive.PSABiasHpxHysteresis} {_deltaBAL.TotalSeconds}>{App.pactive.PSABiasBalHysteresis}");

            if (_cpuTotalLoadLong > App.pactive.PSABiasHpxThreshold)
            {
                App.PSABiasCurrent = 2;
            }
            else if (_cpuTotalLoadLong > App.pactive.PSABiasBalThreshold)
            {
                if (_deltaHPX.TotalSeconds > App.pactive.PSABiasHpxHysteresis) App.PSABiasCurrent = 1;

            }
            else
            {
                if (_deltaHPX.TotalSeconds > App.pactive.PSABiasHpxHysteresis && _deltaBAL.TotalSeconds > App.pactive.PSABiasBalHysteresis) App.PSABiasCurrent = 0;
            }

            if (App.PSABiasCurrent != App.lastPSABiasCurrent || App.lastPSABiasCurrent == null)
            {
                SetPSAActive(App.PSABiasCurrent);
                App.lastPSABiasCurrent = App.PSABiasCurrent;
                App.LogDebug($"New PSA Bias:{App.systemInfo.PSABias}");
                if (App.PSABiasCurrent == 2) _stampHPX = DateTime.Now;
                if (App.PSABiasCurrent == 1) _stampBAL = DateTime.Now;
                zencontrol_reapply = true;
            }

            if (_deltaUA.TotalSeconds > App.pactive.PSALightSleepSeconds && !App.psact_light_b && _cpuTotalLoad <= App.pactive.PSALightSleepThreshold)
            {
                App.psact_light_b = true;
                PSAct_Light(App.psact_light_b);
                zencontrol_reapply = true;
                //App.LogDebug($"IN LIGHT SLEEP AVGLOAD={_cpuTotalLoad}");
            }

            if (_deltaUA.TotalSeconds > App.pactive.PSADeepSleepSeconds && !App.psact_deep_b && _cpuTotalLoad <= App.pactive.PSADeepSleepThreshold)
            {
                App.psact_deep_b = true;
                PSAct_Deep(App.psact_deep_b);
                zencontrol_reapply = true;
                //App.LogDebug($"IN DEEP SLEEP AVGLOAD={_cpuTotalLoad}");
            }

            if (App.psact_deep_b && (_deltaUA.TotalSeconds <= App.pactive.PSALightSleepSeconds || App.cpuTotalLoad.Current > App.pactive.PSADeepSleepThreshold * 2))
            {
                App.psact_deep_b = false;
                PSAct_Deep(App.psact_deep_b);
                zencontrol_reapply = true;
                //App.LogDebug($"OUT DEEP SLEEP {_cpuTotalLoad}>{App.pactive.PSADeepSleepThreshold * 2} {_deltaUA.TotalSeconds}<={App.pactive.PSALightSleepSeconds}");
            }

            if (App.psact_light_b && (_deltaUA.TotalSeconds <= App.pactive.PSALightSleepSeconds || App.cpuTotalLoad.Current > App.pactive.PSALightSleepThreshold * 2))
            {
                App.psact_light_b = false;
                PSAct_Light(App.psact_light_b);
                SetPSAActive(App.PSABiasCurrent);
                zencontrol_reapply = true;
                App.UAStamp = DateTime.Now;
                //App.LogDebug($"OUT LIGHT SLEEP {_cpuTotalLoad}>{App.pactive.PSALightSleepThreshold * 2} {_deltaUA.TotalSeconds}<={App.pactive.PSALightSleepSeconds}");
            }

        }
        public static void PSAct_Light(bool enable)
        {
            uint _value;

            using (Process thisprocess = Process.GetCurrentProcess())
            {
                if (enable) Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
                if (!enable) Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
            }

            //AHCI Link Power Management - HIPM/DIPM
            _value = (uint)(enable ? 1 : 0);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("0b2d69d7-a2a1-449c-9680-f91c70521c60"), PowerManagerAPI.PowerMode.AC, _value);

            //Primary NVMe Idle Timeout
            _value = (uint)(enable ? 100 : 500);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("d639518a-e56d-4345-8af2-b9f32fb26109"), PowerManagerAPI.PowerMode.AC, _value);

            //Secondary NVMe Idle Timeout
            _value = (uint)(enable ? 100 : 4000);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("d3d55efd-c1ff-424e-9dc3-441be7833010"), PowerManagerAPI.PowerMode.AC, _value);

            //AHCI Link Power Management - Adaptive
            _value = (uint)(enable ? 100 : 0);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("dab60367-53fe-4fbc-825e-521d069d2456"), PowerManagerAPI.PowerMode.AC, _value);

            //Secondary NVMe Power State Transition Latency Tolerance
            _value = (uint)(enable ? 100 : 0);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("dbc9e238-6de9-49e3-92cd-8c2b4946b472"), PowerManagerAPI.PowerMode.AC, _value);

            //Primary NVMe Power State Transition Latency Tolerance
            _value = (uint)(enable ? 50 : 0);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.DISK_SUBGROUP, new Guid("fc95af4d-40e7-4b6d-835a-56d131dbc80e"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance autonomous mode
            _value = (uint)(enable ? 0 : 1);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("8baa4a8a-14c6-4451-8e8b-14bdbd197537"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor energy performance preference policy
            _value = (uint)(enable ? 25 : 0);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("36687f9e-e3a5-4dbf-b1dc-15eb381c6863"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor energy performance preference policy for Processor Power Efficiency Class 1
            _value = (uint)(enable ? 25 : 0);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("36687f9e-e3a5-4dbf-b1dc-15eb381c6864"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance increase policy
            _value = (uint)(enable ? 1 : 2);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("465e1f50-b610-473a-ab58-00d1077dc418"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance decrease policy
            _value = (uint)(enable ? 0 : 1);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("40fbefc7-2e9d-4d25-a185-0cfd8574bac6"), PowerManagerAPI.PowerMode.AC, _value);

            //Minimum processor state
            _value = (uint)(enable ? 17 : 100);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("893dee8e-2bef-41e0-89c6-b55d0929964c"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor duty cycling
            _value = (uint)(enable ? 1 : 0);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("4e4450b3-6179-4e91-b8f1-5bb9938f81a1"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance decrease threshold
            _value = (uint)(enable ? 60 : 5);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("12a0ab44-fe28-4fa9-b3bd-4b64f44960a6"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance increase threshold for Processor Power Efficiency Class 1
            _value = (uint)(enable ? 95 : 60);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("06cadf0e-64ed-448a-8927-ce7bf90eb35e"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance decrease threshold for Processor Power Efficiency Class 1
            _value = (uint)(enable ? 60 : 5);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("12a0ab44-fe28-4fa9-b3bd-4b64f44960a7"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor idle promote threshold
            _value = (uint)(enable ? 20 : 60);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("7b224883-b3cc-4d79-819f-8374152cbe7c"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance boost policy
            _value = (uint)(enable ? 60 : 85);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("45bcc044-d885-43e2-8605-ee0ec6e96b59"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance decrease time
            _value = (uint)(enable ? 1 : 5);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("d8edeb9b-95cf-4f95-a73c-b061973693c8"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor idle time check
            _value = (uint)(enable ? 5000 : 50000);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("c4581c31-89ab-4597-8e2b-9c9cab440e6b"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor idle threshold scaling
            _value = (uint)(enable ? 1 : 0);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("6c2993b0-8f48-481f-bcc6-00dd2742aa06"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor idle demote threshold
            _value = (uint)(enable ? 25 : 40);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("4b92d758-5a24-4851-a470-815d78aee119"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance boost mode
            _value = (uint)(enable ? 3 : 2);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("be337238-0d82-4146-a960-4f3749d470c7"), PowerManagerAPI.PowerMode.AC|PowerManagerAPI.PowerMode.DC, _value);

            //Latency sensitivity hint min unparked cores/packages
            _value = (uint)(enable ? 50 : 90);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("616cdaa5-695e-4545-97ad-97dc2d1bdd88"), PowerManagerAPI.PowerMode.AC | PowerManagerAPI.PowerMode.DC, _value);

            App.powerManager.SetActiveGuid(App.PPGuid);
        }
        public static void PSAct_Deep(bool enable)
        {
            uint _value;

            //App.SysCpuSetMask = enable ? 0 : defBitMask;

            //Heterogeneous thread scheduling policy
            _value = (uint)(enable ? hepfgdeep : hepfg);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("93b8b6dc-0698-4d1c-9ee4-0644e900c85d"), PowerManagerAPI.PowerMode.AC, _value);

            //Heterogeneous short running thread scheduling policy
            _value = (uint)(enable ? hepbgdeep : hepbg);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("bae08b81-2d5e-4688-ad6a-13243356654b"), PowerManagerAPI.PowerMode.AC, _value);

            //Link State Power Management
            _value = (uint)(enable ? 2 : 0);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PCIEXPRESS_SETTINGS_SUBGROUP, new Guid("ee12f906-d277-404b-b6da-e5fa1a576df5"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor energy performance preference policy
            _value = (uint)(enable ? 90 : 25);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("36687f9e-e3a5-4dbf-b1dc-15eb381c6863"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance boost policy
            _value = (uint)(enable ? 40 : 60);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("45bcc044-d885-43e2-8605-ee0ec6e96b59"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor energy performance preference policy for Processor Power Efficiency Class 1
            _value = (uint)(enable ? 95 : 25);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("36687f9e-e3a5-4dbf-b1dc-15eb381c6864"), PowerManagerAPI.PowerMode.AC, _value);

            //Maximum processor state
            _value = (uint)(enable ? 99 : 100);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("bc5038f7-23e0-4960-96da-33abaf5935ec"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance increase time
            _value = (uint)(enable ? 3 : 1);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("984cf492-3bed-4488-a8f9-4286c97bf5aa"), PowerManagerAPI.PowerMode.AC, _value);

            //Latency sensitivity hint processor performance
            _value = (uint)(enable ? 85 : 99);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("619b7505-003b-4e82-b7a6-4dd29c300971"), PowerManagerAPI.PowerMode.AC, _value);

            //Core parking

            //Processor performance core parking min cores
            _value = (uint)(enable ? 10 : 100);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("0cc5b647-c1df-4637-891a-dec35c318583"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance core parking min cores for Processor Power Efficiency Class 1
            //_value = (uint)(enable ? 0 : 100);
            //App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("0cc5b647-c1df-4637-891a-dec35c318584"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance core parking concurrency threshold
            _value = (uint)(enable ? 80 : 97);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("2430ab6f-a520-44a2-9601-f7f23b5134b1"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance core parking increase time
            _value = (uint)(enable ? 3 : 100);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("619b7505-003b-4e82-b7a6-4dd29c300971"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance core parking decrease policy
            _value = (uint)(enable ? 2 : 0);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("71021b41-c749-4d21-be74-a00f335d582b"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance core parking decrease time
            _value = (uint)(enable ? 2 : 100);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("dfd10d17-d5eb-45dd-877a-9a34ddd15c82"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance core parking decrease policy
            _value = (uint)(enable ? 2 : 0);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("71021b41-c749-4d21-be74-a00f335d582b"), PowerManagerAPI.PowerMode.AC, _value);

            //Processor performance core parking overutilization threshold
            _value = (uint)(enable ? 85 : 60);
            App.powerManager.SetDynamic(PowerManagerAPI.SettingSubgroup.PROCESSOR_SETTINGS_SUBGROUP, new Guid("943c8cb6-6f93-4227-ad87-e9a3feec08d1"), PowerManagerAPI.PowerMode.AC, _value);

            App.powerManager.SetActiveGuid(App.PPGuid);
        }
        public static int CountBits(uint? n = 0)
        {
            n = n == null ? 0 : n;
            int count = 0;
            do
            {
                int has = (int)(n & 1);
                if (has == 1)
                {
                    count++;
                }

            } while ((n >>= 1) != 0);

            return count;
        }
        public static void OnSysCpuSet(object sender, ElapsedEventArgs args)
        {
            try
            {
                systoken = new CancellationToken();
                systoken = (CancellationToken)App.syscts.Token;

                if (systoken.IsCancellationRequested)
                {
                    App.LogDebug("SYSCPUSET CANCELLATION REQUESTED");
                    systoken.ThrowIfCancellationRequested();
                }

                //App.LogDebug($"SYSCPUSET ON SetSystemCpuSet: 0x{App.SysCpuSetMask:X8} Last: 0x{App.lastSysCpuSetMask:X8}");

                try
                {
                    if ((App.lastSysCpuSetMask != App.SysCpuSetMask && !App.psact_deep_b) || (App.psact_deep_b && App.SysCpuSetMask == 0))
                    {
                        App.LogInfo($"OnSysCpuSet: 0x{App.lastSysCpuSetMask:X8} -> 0x{App.SysCpuSetMask:X8} - {CountBits(App.lastSysCpuSetMask)} -> {CountBits(App.SysCpuSetMask)} Max: {CountBits(defFullBitMask)}");
                        if (App.SetSysCpuSet(App.SysCpuSetMask) == 0)
                        {
                            App.lastSysCpuSetMask = App.SysCpuSetMask;

                            if (App.SysCpuSetMask != 0)
                            {
                                Processes.MaskParse();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    App.LogInfo($"Failed SetSystemCpuSet: {ex}");
                    App.SetSysCpuSet(0);
                }

            }
            catch (ObjectDisposedException)
            {
                App.LogDebug("OnSysCpuSet cycle exiting due to ObjectDisposed");
                App.SetSysCpuSet(0);
                throw;
            }
            catch (OperationCanceledException)
            {
                App.LogDebug("OnSysCpuSet cycle exiting due to OperationCanceled");
                App.SetSysCpuSet(0);
                throw;
            }
            catch (Exception ex)
            {
                App.LogExError($"OnSysCpuSet cycle Exception: {ex.Message}", ex);
                App.SetSysCpuSet(0);
            }
        }

        public static void ProcMask(string processname, bool sysm, bool bitm)
        {
            //if (sysm && App.pactive.SysSetHack) ProcDefSysMask(processname);
            if (bitm && App.pactive.SysSetHack) ProcDefMask(processname);
        }

        public static void ProcDefMask(string processname)
        {
            try
            {
                Process[] whitelist = Process.GetProcessesByName(processname);
                if (whitelist.Length > 0)
                {
                    ProcessThreadCollection threads;
                    for (int ins = 0; ins < whitelist.Length; ++ins)
                    {
                        threads = whitelist[ins].Threads;
                        //App.LogDebug($"ProcDefMask: {processname}");
                        for (int t = 0; t < threads.Count; ++t)
                        {
                            threads[t].IdealProcessor = 0;
                            threads[t].ProcessorAffinity = (IntPtr)defFullBitMask;
                        }
                    }
                }
                else {
                    App.LogInfo($"Failed not found ProcDefMask: {processname}");
                }
            }
            catch 
            {
                App.LogInfo($"Failed ProcDefMask: {processname}");
            }
        }
        public static void ProcDefSysMask(string processname)
        {
            try
            {
                Process[] whitelist = Process.GetProcessesByName(processname);
                if (whitelist.Length > 0)
                {
                    int pid = whitelist[0].Id;
                    int ret = App.ResetSysCpuSetProc(pid, defFullBitMask);
                    //App.LogDebug($"ProcSysDefMask: {processname}");
                    if (ret != 0) App.LogInfo($"Failed ProcSysDefMask: {pid} - {processname} = {ret}");
                }
                else
                {
                    App.LogInfo($"Failed not found ProcSysDefMask: {processname}");
                }
            }
            catch
            {
                App.LogInfo($"Failed ProcSysDefMask: {processname}");
            }
        }

        public static void Close()
        {

            App.tbtimer.Enabled = false;

            App.tbcts.Cancel();

            try
            {
                App.mrestb.Wait(App.tbcts.Token);
            }
            catch (OperationCanceledException)
            {
                App.LogDebug($"ThreadBooster Closed");
            }
            finally
            {
                App.PSAPlanDisable();
            }

        }
    }
    public class ZenControlMode
    {
        public string Mode { get; set; }
        public int PPT { get; set; }
        public int TDC { get; set; }
        public int EDC { get; set; }
        public ZenControlMode(string _mode, int _ppt, int _tdc, int _edc)
        {
            Mode = _mode;
            PPT = _ppt;
            TDC = _tdc;
            EDC = _edc;
            App.LogDebug($"Added ZenControlMode: {_mode} {_ppt}/{_tdc}/{_edc}");
        }
    }

}
