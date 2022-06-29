# CPUDoc


CPUDoc is a CPU "Little Helper".

Currently features an experimental ThreadBooster which will disable system scheduling on all the 2nd threads of the CPU.
Except for Core 0 which needs to have both threads active.
This will improve the performances substantially when the workload doesn't need the 2nd CPU threads.

You can test with 7-Zip benchmark using half or less the total CPU threads.
The results is similar to disabling SMT/HyperThreading in BIOS but you can still enjoy almost full performances when needed.
More cores are available on the CPU and more benefits can be expected running it.

In general all the benchmarks are scoring very slightly lower.
The drawback is a little lag when a very high workload needs all the cores; all synthetic MT benchmarks will score a little lower due to that.
It's probably going to be sub-optimal in some specific scenarios but it can be also a massive improvement in others.

From my experience gaming with a huge amount of background applications, while developing it, went from a complete mess of massive stuttering to silk smooth.

Benching on a clean install the results are almost identical, especially in gaming.
7-Zip compression/decompression benchmark scores increased from 139.3/127.1 to 142.3/131 GIPS.
A modest increase cause there are not many background processes spread by the Windows scheduler all around.
On my main bloated Windows install where I have the world and beyond running in background the picture is different.
The benchmark scores increased from 129.6/122.9 to 137.3/130 GIPS.

SOTR shows a small improvement of 10-20 fps in the CPU Renderer.

Testing on your setup is the only way to know if it helps or not.

Later I will add an AMD Ryzen WHEA suppressor (not fixer bacause is misleading lol) for Win10/11 and an E-Cores software disable switch for Alder Lake.

CPU usage is very low, 0.01/0.02% on my 5950X with just over 100MB of memory.


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

- v1.0.2 Alpha
    - Fix: Disabled massive debug logging left enabled
- v1.0.1 Alpha
    - New: Separate threads for Hardware Monitoring, set system mask and ThreadBooster
    - New: Updated Zen-Core DLL to latest codebase with some additions for future work (detecting Max PBO values etc)
    - New: Added Idle detection, PowerManagement class and, Global/Event hook for future work
- v1.0.0 Alpha
    - New: First public Alpha