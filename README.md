﻿![GitHub all releases](https://img.shields.io/github/downloads/mann1x/CPUDoc/total)
![GitHub release (latest by date)](https://img.shields.io/github/v/release/mann1x/CPUDoc)
![GitHub contributors](https://img.shields.io/github/contributors/mann1x/CPUDoc)
![GitHub Repo stars](https://img.shields.io/github/stars/mann1x/CPUDoc?style=social)

# CPUDoc


CPUDoc is a CPU "Little Helper".
It comes with the following features that can be enabled independently.

The ThreadBooster is the main engine director and most dynamic functions are depending on it.

---

## Usage

Just run it and **optionally set it to start at boot via the Settings menu** (Create or remove Auto-start task).
Double click on the system tray icon to open the App and right click for the menu to exit.
With right click you can also toggle some features on or off.

You can start and stop the ThreadBooster in the main menu.
The ThreadBooster pooling rate (how often the governor and related threads are executed) can be manually adjusted.
The default is value is the best fit for Ryzen 3000/5000 and Intel 12th Gen.
Should be the same for Ryzen 7000 and 13th Gen but I didn't test it personally.
You can test different settings if you want, in case you think the allocation/deallocation of T1 threads is too slow.
I recommend to keep it as default.
For testing use any game at very highly CPU bound quality settings and check you get the same performances or better than default value.

[Read more detailed information about usage and features here](USAGE.md)

## SysSetHack (aka SSH)

This feature will dynamically allocate the T1 threads, avoiding any scheduler allocation when not needed.
This will improve the performances substantially when the workload doesn't need the 2nd CPU threads.

You can test with 7-Zip benchmark using half or less the total CPU threads, minus 2. So use 6 threads for a 8C/16T CPU.
The results is similar to disabling SMT/HyperThreading in BIOS but you can still enjoy almost full performances when needed.
More cores are available on the CPU and more benefits can be expected running it.

In general most of the benchmarks are scoring very slightly lower or higher.
Benchmarks are mostly testing single core and full core loads while SSH ideal target for improvement is half load.
The drawback is a little lag when a very high workload needs all the cores; many synthetic MT benchmarks will score a little lower due to that.
The gap it's hardly noticeable in real world usage.

Gaming can be faster by 2-3fps to 10-20fps or more for games that are CPU limited.
SotTR (Shadow of the Tomb Raider), Assassin's Creed Valhalla, and F1 2022 are among the best games, with non GPU bound quality settings, to test for performance improvements.

Best experience should be with enough graphical settings high enough to be GPU bounding and limit the max game CPU usage to the T0 threads without T1 threads enabled.
You can monitor the CPU usage with RivaTuner or CapframeX.

7-Zip compression/decompression benchmark scores increased from 139.3/127.1 to 142.3/131 GIPS.
A modest increase cause there are not many background processes spread by the Windows scheduler all around.
On my main bloated Windows install where I have the world and beyond running in background the picture is different.
The benchmark scores increased from 129.6/122.9 to 137.3/130 GIPS.
On my 3800X with 6 threads from 46.3/44.9 to 49.1/48.3.

It's a 5-7% CPU performance increasing without actually overclocking anything.
Disabling in BIOS the T1 threads is not mandatory, avoiding that Windows allocates workload there is good enough to recover the lost performances in the T0 threads.
Intel seems to manage better with HyperThreading and the performance increase is more in the 2-3% range.

3DMark CPU Profile shows an improvement of almost 15% on my 5950X in the test for 16 core.
There's a gain in performances per core which is about 0.8-1%.
More cores the processor has and better the cumulative gain can be.
When the load is close to the number of physical cores the performance uplift is bigger and goes downward toward full cores, T0 & T1, usage.
This is particularly effective in games which already tend to use only the T0 cores, average performance uplift is about 5-10%.

There are side effects in manipulating the CpuSet System Mask.
One of these is that the background threads gets much less priority than the usual.

CapframeX can be used for benchmarking but it's not lightweight and will reduce the framerate and CPUDoc effectiveness.
It's best to check the in-game benchmark statistics if available.

The adjusted priorities work really well in keeping the gameplay as smooth as possible.
Even if there's a sudden background thread hammering the CPU, eg. Epic Games is starting an update.
In these conditions in F1 22 very often the FPS drop went down to few frames instead of 30-50 less over 130fps.

You can check yourself, both with in-game benches and CapframeX, that usually there's a big improvement in low fps scores, especially percentiles like P95 etc.

---

## PowerSaverActive (aka PSA)

The PowerSaverActive mode will install a custom dynamic power plan.
The plan settings will be adapted on the fly.

There is a **Light and** a **Deep Sleep** mode; Deep mode will have a little more performance penalty but increase a little power consumption savings, depends on the platform.

There are 3 **Performance Bias** which are adjusting in real-time the power and performance settings of the dynamic power plan:
- **Booster** - similar to Ultimate power plan, active when high cpu load is detected
- **Standard** - similar to Balanced/High Performance, active when load is average
- **Economizer** - Very low power consumption, when the load is very low or the system is idling

The Performance Bias will be dynamically set based on CPU Load and User Activity (Active Mode or Game Mode)

**Active mode** is when the System detects user activity; moving the mouse or typing on the keyboard.
This will disable all Sleep modes.

**Game Mode** will be active based on various inputs from the Windows APIs.
Sleep modes will be disabled and the min Performance Bias set to Standard.

Active and Game Modes lowest Power Bias can be customized.

There are the following sources for now to detect User Activity:
- FocusAssist; only Win10 and above, you can set it manually via the notifications panel. Alarms and Priority will enable Game Mode.
- UserNotification; only Win10 (also the older builds) and above, Busy or D3D mode will enable Game Mode.
- ForegroundWindow; this method should work regardless of Windows version. If the foreground window is full-screen it will trigger User Activity.

You can decide to enable or disable Game Mode; many activities (including watching Netflix) will be detected as Gaming.
By default it will be enabled.

There is also a specific setting for ForegroundWindow on secondary screens (Detect Game Mode on secondary monitors).
Normally anything running in full screen will trigger User Activity via FocusAssist or UserNotification and therefore Game Mode but only on the primary screen.
With this setting you can change the default behavior.
It can be useful but as default is disabled; this allows Sleep modes if you eg. keep a TV or movie running on the 2nd screen.

You can decide also the lowest Power Bias to use when in Game Mode and Active Mode.

The new **Power Tweak** setting is available with 3 modes: **LowPower**, **AutoPower**, **HighPower**.
You can use it to skew the settings and behavior toward a more conservative or aggressive power consumption.

The dynamic Power Plan is now available also with two different personalities, Balanced and Ultimate.

The power plan personality will be changed automatically based on the Power Tweak setup; LowPower will always use the Balanced personalty, while AutoPower will use it on Windows 10 and switch to Ultimate for Windows 11.
LowPower will always use the Balanced and HighPower always the Ultimate.

You can also force the personality and override the Power Tweak decision.

The timeouts and thresholds will be fully configurable.
At this time they are static.

The dynamic power plan can be reset by removing it when CPUDoc is not running.
At the next start CPUDoc will reinstall it from its folder.

Please not that if you want setup standby for the monitor and the system you need to do so while CPUDoc is running.
Right now there's no configuration for it in CPUDoc but it's also not manipulating these settings, you can customize them via Windows settings.

---

## ZenControl

This feature is specific to AMD Zen CPUs.
It will allow to set static/dynamic settings via SMU or MSR programming.

Right now it works in conjunction with PSA to adjust in real-time the PBO Limits.
It does provide a, rough for now, basic algorithm to pick PBO Limits based on CPU physical cores.
You can disable the Auto selection and specify manual limits.

---

## NumaZero  (aka N0 or NZ)

This feature can be enabled in Auto mode or manual with a fixed amount of cores specified.
With NumaZero enabled only a subset of the total cores are used and can be used together with SysSetHack.

In Auto mode the cores not in the first CCD or CCX for AMD or the E-Cores for Intel will be disabled.
If the result of the Auto mode is not satisfying, you can select a static value of 2/4/6/8 cores from the first ones.

This could help eg. AMD 5000/7000 with dual CCD and Alder/Raptor Lake with E-Cores when some specific software/game is slower than expected.

You could still see load on the disabled cores/threads. 
This is as expected as processes that are already running there could disregard the "hint" and decide to keep using the disabled cores.
The E-Cores will be parked therefore no load should be seen there once they are disabled.

---

## **USE AT YOUR OWN RISK**

This software is pretty safe to use and it doesn't necessarily require complex configuration.
Almost everything is set in Auto, without user intervention. This is the goal.

But keep into consideration that the use of some features (eg. SSH, N0, ZenControl, IntelControl, etc) could cause games stuttering and system freezing/crashing in extreme cases.

CPU usage is very low, 0.03/0.04% with just over 130MB of memory.
Power consumption savings on AMD with PSA are very substantial, from 10W-30W to 50-60W.

---

## Installation

It's a standalone application; the only software prerequisite is the Desktop Runtime for .NET Core 6.0 Windows Desktop Runtime (https://dotnet.microsoft.com/en-us/download/dotnet/6.0).

Move it into a permanent directory, you can create a shortcut to launch it or use the drop-down menu option to create it on the desktop.

An installer will be available at some point.

If you don't have the .NET Desktop Runtime you will be asked to download and install first. A reboot is not required but may be needed, reboot if it's still asking to install the runtime.

---

***If you have any issue and needs support please enable both the Write Trace Information and Write Debug Trace logs in the settings.
Close and restart the App, do whatever is causing you the issue eg. enable/disable NumaZero, close the App.
Send me the files in the Logs folder.***

****Please use the Issues tab on GitHub if you want to report an issue or have a request****

There are 2 threads on OCN where to discuss about CPUDoc:
- [For AMD platforms discussion](https://www.overclock.net/threads/cpudoc-little-cpu-helper-tool-with-some-exclusive-features.1802082/)
- [For Intel platforms discussion](https://www.overclock.net/threads/cpudoc-little-cpu-helper-tool-with-some-exclusive-features.1802081/)

If for some reason you don't want or can't use PSA, check one of my Custom Power Plans for Windows 10 & 11:
- [ManniX's Custom Power Plans for AMD](https://www.overclock.net/threads/ryzen-custom-power-plans-for-windows-10-balanced-and-ultimate.1776353/)
- [ManniX's Custom Power Plans for Intel](https://www.overclock.net/threads/intel-custom-power-plans-for-windows.1802309/)

---

## Known issues

- Some games may stutter with NumaZero active
    - One example is Assassin's Creed Valhalla with an AMD dual CCD processor
    - Intel with Hybrid architecture (P/E-Cores) should be immune to this issue
- Benchmarks with short all core loads could give a worse score with SSH enabled; this is by design
- Benchmarks with a too friendly affinity could give a worse score with SSH enabled; this is by design
    - Some of the 3DMark benchmarks could exhibit this behavior, if not white-listed (most if not all tests should be currently optimized) 

---

## Compilation

You can compile with Visual Studio 2022 and .NET Core 6.

---

## Road map

More a list to remember what to work on in the future :)

- Create an installer
- Stable and Dev update channels
- Multiple profiles, up to 10
- App detection for automatic switching profiles
- Keyboard HotKeys; change profiles, toggle features, send HotKeys (eg. to switch AfterBurner profile)
- MSR for CPU load detection on AMD/Intel
- IntelCore; change Intel specific settings via MSR
- ZenControl WHEASuppressor, disable FCLK instability errors via MSR MCA control
- ZenControl MCADoctor; granular control of all MCA bits, detect and decode errors

---


## Changelog:

[Full Changelog here](CHANGELOG.md)