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
        public static int PoolingInterval = 250;
        public static int PoolingIntervalSlow = 1000;
        public static uint defBitMask = 0;
        public static uint newBitMask = 0;
        public static uint defFullBitMask = 0;
        public static uint prevBitMask = 0;
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
        public static bool bInit = false;
        public static int IncreaseHysteresis = 6;
        public static bool SetHysteresis = true;
        public static int FullLoadHystSecs = 10;
        public static double HighTotalLoadThreshold = 90;
        public static double HighTotalLoadFactor = 0.90;
        public static double TotalT0Load = 0;
        public static double TotalT0LoadNorm = 0;
        private static int LoadZeroThresholdCount = 3;
        private static int LoadMediumThresholdCount = 3;
        private static int LoadHighThresholdCount = 1;
        private static int ClearForceThreshold = 5;
        
        public static int SetSysCpuSet(uint bitMask)
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
            foreach (int logical in App.logicalsT0)
            {
                defBitMask |= (uint)1 << (logical);
                basecores++;
                App.LogDebug($"Build defBitMask 0x{defBitMask:X8} {logical}");
            }
            defFullBitMask = defBitMask;
            foreach (int logical in App.logicalsT1)
            {
                defFullBitMask |= (uint)1 << (logical);
                addcores++;
                App.LogDebug($"Build defFullBitMask 0x{defBitMask:X8} {logical}");
            }
            prevBitMask = defBitMask;
            setcores = basecores;
            HighTotalLoadThreshold = (basecores + addcores) * 100 / ProcessorInfo.LogicalCoresCount * HighTotalLoadFactor;
        }

        public static void OnThreadBooster(object sender, ElapsedEventArgs args)
        {
            try
            {
                tbtoken = new CancellationToken();
                tbtoken = (CancellationToken)App.tbcts.Token;

                Process.GetCurrentProcess().PriorityBoostEnabled = true;

                if (tbtoken.IsCancellationRequested)
                {
                    App.LogDebug("TB CANCELLATION REQUESTED");
                    tbtoken.ThrowIfCancellationRequested();
                }

                if (!bInit)
                {
                    BuildDefaultMask();
                    SetSysCpuSet(defBitMask);
                    bInit = true;
                }

                ProcessorInfo.CpuTotalLoadUpdate();

                _deltaStamp = DateTime.Now - prevFullcoresStamp;
                
                if (ProcessorInfo.cpuTotalLoad > HighTotalLoadThreshold || _deltaStamp.TotalSeconds < FullLoadHystSecs)
                {
                    if (ProcessorInfo.cpuTotalLoad > HighTotalLoadThreshold) prevFullcoresStamp = DateTime.Now;
                    //App.LogDebug($"CpuLoad: {ProcessorInfo.cpuTotalLoad:0} {_deltaStamp.TotalSeconds}");
                    if (defFullBitMask != prevBitMask)
                    {
                        App.LogDebug($"defFullBitMask 0x{defFullBitMask:X8}");
                        try
                        {
                            if (SetSysCpuSet(defFullBitMask) == 0)
                            {
                                prevBitMask = defFullBitMask;
                                setcores = basecores + addcores;
                            }
                        }
                        catch (Exception ex)
                        {
                            App.LogDebug($"Failed SetSystemCpuSet: {ex}");
                        }
                    }
                    App.tbtimer.Interval = PoolingIntervalSlow;
                }
                else
                {
                    ProcessorInfo.CpuLoadUpdate();
                    App.tbtimer.Interval = PoolingInterval;
                    //App.LogDebug($"CpuLoad: {ProcessorInfo.cpuTotalLoad:0}");
                    //App.LogDebug($"\tLoad0={ProcessorInfo.HardwareCpuSets[0].Load:0} \tLoad1={ProcessorInfo.HardwareCpuSets[1].Load:0}");

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

                        //App.LogDebug($"L:{logical} Load:{ProcessorInfo.HardwareCpuSets[logical].Load:0} {ProcessorInfo.HardwareCpuSets[logical].LoadHigh}");
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
                        if (i < morecores || ProcessorInfo.HardwareCpuSets[App.logicalsT1[i]].ForcedEnable)
                            newBitMask |= (uint)1 << (App.logicalsT1[i]);
                        //App.LogDebug($"calc newbitMask 0x{newBitMask:X8} {morecores}");
                    }

                    if (newBitMask != prevBitMask && !SetHysteresis)
                    {
                        //App.LogDebug($"set newbitMask 0x{newBitMask:X8} prevBitMask 0x{prevBitMask:X8} {morecores}");
                        //App.LogDebug($"setcores {setcores} newsetcores {newsetcores} basecores {basecores} morecores {morecores}");
                        prevNeedcores = needcores;
                        prevMorecores = morecores;
                        if (newsetcores > setcores) prevIncreaseStamp = DateTime.Now;
                        setcores = newsetcores;
                        try
                        {
                            if (SetSysCpuSet(newBitMask) == 0)
                                prevBitMask = newBitMask;
                        }
                        catch (Exception ex)
                        {
                            App.LogInfo($"Failed SetSystemCpuSet: {ex}");
                        }
                    }
                    //if (needcores > 0 || morecores > 0 || usedcores > basecores)
                    //    App.LogDebug($"needcores {needcores} morecores {morecores} usedcores {usedcores} setcores {setcores} newsetcores {newsetcores} T0Load {TotalT0LoadNorm}");
                }
            }
            catch (OperationCanceledException)
            {
                SetSysCpuSet(0);
                App.LogDebug("HWM Monitoring cycle exiting due to OperationCanceled");
                throw;
            }
            catch (Exception ex)
            {
                SetSysCpuSet(0);
                App.LogExError($"HWM Monitoring cycle Exception: {ex.Message}", ex); 
            }
            finally
            {
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

        }
    }
}
