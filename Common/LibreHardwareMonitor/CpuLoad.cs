﻿// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Copyright (C) LibreHardwareMonitor and Contributors.
// Partial Copyright (C) Michael Möller <mmoeller@openhardwaremonitor.org> and Contributors.
// All Rights Reserved.

using net.r_eg.Conari;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CPUDoc
{
    public class CpuLoad
    {
        private readonly float[] _cpuLoads;
        private readonly int _cpuCount;
        private long[] _idleTimes;
        private float _totalLoad;
        private long[] _totalTimes;
        private static Interop.NtDll.SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION[] information = new Interop.NtDll.SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION[64];
        private static int size = Marshal.SizeOf(typeof(Interop.NtDll.SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION));

        public CpuLoad()
        {
            _cpuCount = Environment.ProcessorCount;
            _cpuLoads = new float[_cpuCount];
            _totalLoad = 0;
            try
            {
                GetTimes(out _idleTimes, out _totalTimes);
            }
            catch (Exception)
            {
                _idleTimes = null;
                _totalTimes = null;
            }

            if (_idleTimes != null)
                IsAvailable = true;
        }

        public bool IsAvailable { get; }

        private static bool GetTimes(out long[] idle, out long[] total)
        {

            idle = null;
            total = null;
            if (Interop.NtDll.NtQuerySystemInformation(Interop.NtDll.SYSTEM_INFORMATION_CLASS.SystemProcessorPerformanceInformation,
                                                       information,
                                                       information.Length * size,
                                                       out IntPtr returnLength) != 0)
            {
                return false;
            }

            idle = new long[(int)returnLength / size];
            total = new long[(int)returnLength / size];

            /*
            using (var lam = new ConariL(@"NtDll.dll"))
            {
                if (lam.DLR.NtQuerySystemInformation<int>(Interop.NtDll.SYSTEM_INFORMATION_CLASS.SystemProcessorPerformanceInformation,
                                                       information,
                                                       information.Length * size,
                                                       out IntPtr returnLength) != 0)
                {
                    return false;
                }
                idle = new long[(int)returnLength / size];
                total = new long[(int)returnLength / size];
            }
            */

            for (int i = 0; i < idle.Length; i++)
            {
                idle[i] = information[i].IdleTime.QuadPart;
                total[i] = information[i].KernelTime.QuadPart + information[i].UserTime.QuadPart;
            }

            return true;
        }

        public float GetTotalLoad()
        {
            return _totalLoad;
        }

        public float GetCpuLoad(int cpu)
        {
            return _cpuLoads[cpu];
        }
        public float GetCpuCount()
        {
            return _cpuLoads.Length;
        }

        public void Update()
        {
            if (_idleTimes == null)
                return;

            if (!GetTimes(out long[] newIdleTimes, out long[] newTotalTimes))
                return;


            for (int i = 0; i < Math.Min(newTotalTimes.Length, _totalTimes.Length); i++)
            {
                if (newTotalTimes[i] - _totalTimes[i] < 100000)
                    return;
            }

            if (newIdleTimes == null)
                return;


            float total = 0;
            int count = 0;
            for (int index = 0; index < _cpuCount; index++)
            {
                float value = 0;
                if (index < newIdleTimes.Length && index < _totalTimes.Length)
                {
                    float idle = (newIdleTimes[index] - _idleTimes[index]) / (float)(newTotalTimes[index] - _totalTimes[index]);
                    value += idle;
                    total += idle;
                    count++;
                }

                value = 1.0f - value;
                value = value < 0 ? 0 : value;
                _cpuLoads[index] = value * 100.0f;
            }

            if (count > 0)
            {
                total = 1.0f - total / count;
                total = total < 0 ? 0 : total;
            }
            else
            {
                total = 0;
            }

            _totalLoad = total * 100;
            _totalTimes = newTotalTimes;
            _idleTimes = newIdleTimes;
        }
    }
}
