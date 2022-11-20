# CPUDoc


CPUDoc is a CPU "Little Helper".
It comes with the following features that can be enabled singuarly.

The ThreadBooster is the main engine director and most dynamic functions are depending on it.

# SysSetHack

This feature will dynamically allocate the T1 threads, avoiding any scheduler allocation when not needed.
This will improve the performances substantially when the workload doesn't need the 2nd CPU threads.

You can test with 7-Zip benchmark using half or less the total CPU threads, minus 2. So use 6 threads for a 8C/16T CPU.
The results is similar to disabling SMT/HyperThreading in BIOS but you can still enjoy almost full performances when needed.
More cores are available on the CPU and more benefits can be expected running it.

In general all the benchmarks are scoring very slightly lower or higher.
The drawback is a little lag when a very high workload needs all the cores; many synthetic MT benchmarks will score a little lower due to that.
The gap it's hardly noticeable in real world usage.

Gaming can be faster by 2-3fps to 5-6fps for games that are CPU limited.
Best experience should be with enough graphical settings high enough to be GPU bounding and limit the max game CPU usage to the T0 threads without T1 threads enabled.
You can monitor the CPU usage with Rivatuner or CapframeX.

Benching on a clean install the results are almost identical, especially in gaming.

7-Zip compression/decompression benchmark scores increased from 139.3/127.1 to 142.3/131 GIPS.
A modest increase cause there are not many background processes spread by the Windows scheduler all around.
On my main bloated Windows install where I have the world and beyond running in background the picture is different.
The benchmark scores increased from 129.6/122.9 to 137.3/130 GIPS.
On my 3800X with 6 threads from 46.3/44.9 to 49.1/48.3.

It's a 5-7% CPU performance increasing without actually overclocking anything.
Disabling in BIOS the T1 threads is not mandatory, avoiding that Windows allocates workload there is good enough to recover the lost performances in the T0 threads.

There are side effects in manipulating the CpuSet System Mask.
One of these is that the background threads gets much less priority than the usual.
This can be seen in CapframeX results where often a big stutter at 40-60ms is recorded but not seen at the screen.

CapframeX can't be used reliably sadly.
You need to check the in-game benchmark statistics if available.

The adjusted priorities work really well in keeping the gameplay as smooth as possible.
Even if there's a sudden background thread hammering the CPU, eg Epic Games is starting an update.
In these conditions in F1 22 very often the FPS drop went down to few frames instead of 30-50 less over 130fps.

# PowerSaverActive

The PowerSaverActive mode will install a custom dznamic power plan.
The plan settings will be adapted on the fly.
There is a Light and a Deep Sleep mode.
The timeouts and threasholds will be configurable.

# NumaZero

This feature can be enabled in Auto mode or manual with a fixed amount of cores specified.
With NumaZero enabled only a subset of the total cores are used and can be used together with SysSetHack.

In Auto mode the cores not in the first CCD or CCX for AMD or the E-Core for AMD will be disabled.
If the result of the Auto mode is not satisfying, you can select a static value of 2/4/6/8 cores from the first ones.

This could help eg. AMD 5000/7000 with dual CCD and Alder/Raptor Lake with E-Cores when some specific software/game is slower than expected.

CPU usage is very low, 0.01/0.02% on my 5950X and 0.03/0.04% on my 3800X with just over 130MB of memory.


## **USE AT YOUR OWN RISK**


## Installation

It's a standalone application; the only software pre-requisite is the Desktop Runtime for .NET Core 5.0 (https://versionsof.net/core/5.0/5.0.15/).

Move it into a permanent directory, you can create a shortcut to launch it or use the drop-down menu option to create it on the desktop.

An installer will be available at some point.


## Usage

Just run it and optionally set it to start at boot.
Double click on the system tray icon to open the App and right click for the menu to exit.

You can start and stop the ThreadBooster in the main menu.

**Please use the Issues tab on GitHub if you find issues or have a request**


## Compilation

You can compile with Visual Studio 2019 and .NET Core 5.


## Changelog:

- v1.1.0 Beta
    - Fix: wrong version
- v1.0.9 Beta
    - New: Display System CpuSet Mask; fix initialization
- v1.0.8 Beta
    - New: Display running System CpuSet Mask; green allocated, red unallocated, black disabled.
- v1.0.7 Beta
    - New: Option to select manual ThreadBooster pooling rate
- v1.0.6 Beta
    - New: Automatic updates notification toggle
    - New: Reset Settings button
    - Fix: Saving settings now works, it was broke
    - Fix: SetSysHack - fix status display when stopped
- v1.0.5 Beta
    - Fix: Start button for ThreadBooster not in sync with the status
    - New: PowerSaveActive - new dynamic settings based on load
- v1.0.4 Beta
    - Fix: Disable SetSysHack and NumaZero for pre Win10 OS
    - Fix: Status display in Main tab for the features
- v1.0.3 Beta
    - New: SysSetHack feature; dynamic T1 allocation, increased performances as with SMT/HyperThreading Off
    - New: PowerSaveActive feature; dynamic power plan, ultra low power Light and Deep Sleep modes
    - New: NumaZero feature; only use Cores in same Numa Index, Cache Complex (CCD or CCX) or P-Cores (Intel).
- v1.0.2 Alpha
    - Fix: Disabled massive debug logging left enabled
- v1.0.1 Alpha
    - New: Separate threads for Hardware Monitoring, set system mask and ThreadBooster
    - New: Updated Zen-Core DLL to latest codebase with some additions for future work (detecting Max PBO values etc)
    - New: Added Idle detection, PowerManagement class and, Global/Event hook for future work
- v1.0.0 Alpha
    - New: First public Alpha