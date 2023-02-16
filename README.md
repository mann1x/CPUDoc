![GitHub all releases](https://img.shields.io/github/downloads/mann1x/CPUDoc/total)
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
## Troubleshooting

- Enable the diagnostic logs, close and re-open CPUDoc and attach the LogInfo.txt and LogTrace.txt to the GitHub Issue
- For AMD CPUs: Provide a ZenTimings debug report and if you can a Ryzen Master screen-shot
- Please white-list the following drivers in your AntiVirus software if you have problems
  - inpoutx64.dll
  - WinIo32.dll
  - WinIo32.sys
  - WinRing0x64.dll
  - WinRing0x64.sys

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