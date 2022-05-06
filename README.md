# BenchMaestro


BenchMaestro is a benchmarking and tools uility for CPU & GPU


## **USE AT YOUR OWN RISK**


## Installation

It's a standalone application; the only software pre-requisite is the Desktop Runtime for .NET Core 5.0 (https://versionsof.net/core/5.0/5.0.15/).

Move it into a permanent directory, you can create a shortcut to launch it or use the drop-down menu option to create it on the desktop.

An installer will be available at some point.


## Usage

## **BENCHMARKS WITH MINERS WILL NEED WHITELISTING OF BENCHMAESTRO DIRECTORY IN AV OR MS DEFENDER**

OCN Support thread: https://www.overclock.net/threads/benchmaestro-cpu-gpu-benchmarking-and-tools-utility.1797775/

"CPPC Custom:" You can define your custom CPPC order for thread scheduling by moving with drag & drop the cores in the desired order and checking "Enable"

**Please use the Issues tab on GitHub if you find issues or have a request**


## Compilation

You can compile with Visual Studio 2019 and .NET Core 5.


## Changelog:

- v1.0.35 Alpha
    - New: Proper logging management with NLog
    - New: Align with ZenStates-Core current codebase
    - New: Updated Libre to latest release
    - Fix: Improved compatibility with Hydra 
    - Fix: Memory clocks display issue in Info when VDIMM not available
    - Fix: ZenCore wrong SVI2 planes
- v1.0.34 Alpha
    - New: Reworked ZenCore monitoring
    - Fix: Memory clocks display issue in Info when VDIMM not available
    - Fix: Various streching display issues
    - Fix: Zen Clocks from PowerTable normalized to BCLK for Zen (thx LicSqualo)
    - Fix: Rare unhandled exception when clearing live data display
- v1.0.33 Alpha
    - Fix: Clocks display issues
- v1.0.32 Alpha
    - Add: Bus Clock, vCore and vSOC voltages monitoring for Zen via ZenCore
    - Add: Live vCore display for Zen
    - Add: Add Memory clock for Zen in Info
    - Add: Detection of SHA and VAES Extensions for AMD and Intel
    - Fix: FCLK/UCLK/MCLK and Memory frequency real speeds reported, normalized with Bus Clock for Zen
- v1.0.31 Alpha
    - Fix: Again AutoUpdater not showing at startup
- v1.0.30 Alpha
    - Fix: Wrong build number
- v1.0.29 Alpha
    - Fix: Hopefully for good AutoUpdater not showing at startup
- v1.0.28 Alpha
    - Add: CPU VDD18 (PLL) in CPU Information (thx PJVol)
    - Fix: Threads benchmark columns resized during run
    - Fix: Lost details in screenshot capture, hopefully (thx The_King, Veii, hahagu)
    - Fix: More readable hyperlink font in credits (thx Veii)
    - Fix: Focus main window on load and possibly lost Autoupdater window at boot
- v1.0.27 Alpha
    - New: Benchmark details only captured if expanded
    - Fix: Lost details in screenshot capture
    - Fix: Progress bar margin
    - Fix: Create Screenshots directory at start
    - Fix: Open Screenshots directory goes to ConfigTag folder if exists
- v1.0.26 Alpha
    - Add: Taking a benchmark window screenshot will expand the details and capture them all
    - Add: Read and display memory modules info
    - Add: Read and display memory timings for Zen in details (thx irusanov ZenTimings)
    - Add: Read and display memory voltages info for Zen (thx irusanov ZenTimings)
    - Add: Read and display OS details in Board information
    - Fix: Copy to clipboard screenshot lost after closing SaveScreenshot window
    - Fix: More robust application exit and handling of exceptions
    - Fix: Various layout issues with details expanders
- v1.0.25 Alpha
    - Fix: Logs directory creation issue
- v1.0.24 Alpha
    - Add: Bold and colored live stats
    - Add: Live running and remaining runtime
    - Add: Unload of WinRing DLL at exit
    - Add: Logs moved to Logs subdir
    - Add: Automatic cleanup of leftovers
    - Add: Automatic cleanup of Logs after 30 days
    - Add: Button in settings to open Logs folder
    - Add: Button in take screenshot Camera window to open the Screenshots folder
    - Fix: Lighter load for Zen live display
- v1.0.23 Alpha
    - Fix: Zen1 offset for L3 temperature (thx The_King)
    - Fix: Zen1+ detection
    - Fix: Completely disabled Libre for Zen
- v1.0.22 Alpha
    - Fix: Bug preventing metrics display with CPUMiner
- v1.0.21 Alpha
    - Add: Improved monitoring start detection
    - Add: Performance per watt score
    - Add: Set process to last thread for scheduling instead of last processor thread
    - Add: Proper threads sheduling with support for P/E-cores
    - Add: Display of P/E-cores and threads for Alder Lake
    - Add: Detection of AVX-512 instruction set
    - Add: Scheduler priorities integration, works with all CPUs, supersedes CPPC tags if available as it should match it
    - Add: Testing with 20t for comparisons with Alder Lake
    - Add: Manual check for update button in Settings menu
    - Add: Live display of TDC and EDC for Zen
    - Fix: Offset for Zen1 X for Core and correct L3 Temps (thx The_King)
    - Fix: Correct threads for CPUMiner thread scores
    - Fix: Bug in Cores VIDs and Temps stats 
    - Fix: Improved stop and error handling for bench runs
- v1.0.20 Alpha
    - Add: CpuId debug and tests for P/E Cores
    - Fix: Save and restore custom CPPC (thx Luggage)
    - Fix: Improved handling of Intel P/E Cores with Libre (thx stahlhart)
    - Fix: Improved cleaning at exit (thx stahlhart)
- v1.0.19 Alpha
    - Fix: Moved the extracted bench binary check before idle check (thx hahagu)
    - Fix: Opening links from About (thx The_King)
    - Fix: License text block max width
    - Fix: Reduced timeout for Zen Refresh PowerTable (Riot Vanguard block)
- v1.0.18 Alpha
    - Add: Real time display of few CPU stats while bench running
    - Fix: Tentative fix for Zen/Zen+ PBO limits
- v1.0.17 Alpha
    - Fix: Bug with Score unit at bench start
- v1.0.16 Alpha
    - Add: Zen direct SMU read CPU temperature
    - Add: Threads scores for CPUminer
    - Add: Zen debug info in tracelog 
    - Fix: Bug with Alder Lake with set affinity on last thread
    - Fix: Bug with tracelog writeline
- v1.0.15 Alpha
    - Fix: Layout colors for additional info (CCD, L3, etc)
    - Fix: Missing CCD temp and CCDs average
- v1.0.14 Alpha
    - Add: Threads scores for XMR-Stak-RX miner 
    - Fix: Bench Windows not saving correct last position
    - Fix: Improved Zen Refresh PowerTable
    - Fix: Bug for Zen initialization
    - Fix: Zen sensors
    - Fix: Scores layout improvements
- v1.0.13 Alpha
    - Fix: Missing Bench Details Expanders handling
    - Fix: Initial Window position, now Bench are center top
    - Fix: Windows cannot be restored outside of Workarea, support for taskbar on top
    - Fix: Main Window restore position, cannot be restored outside of Workarea
    - Fix: Dispose of ZenStates-Core DLL
- v1.0.12 Alpha
    - Fix: Zen CO label not initialized causing crash at start with non AMD CPUs
    - Fix: Missing BenchMaestro version in Window title
    - Fix: Improved Bench Details Expanders handling
    - Fix: Improved countdown in seconds for bench run
- v1.0.11 Alpha
    - Add: CCD temperature monitoring for Zen via ZenCore
    - Add: Better clock stretching display
    - Fix: Zen check for CO improved
    - Fix: Fix for Zen1 temperature
    - Fix: Fix for Zen1/2 coremap
    - Fix: ThreadID for Logical cores load in details
- v1.0.10 Alpha
    - New: Fixed version of cpuminer-opt binaries thx to JayDDee, all binaries now are using same algo
    - Fix: Zen restored missing SMU and PT version
    - Fix: Support for Alder Lake
    - Fix: Fix for Zen1 temperature
- v1.0.8 Alpha
    - New: Zen early support for 1000s CPU 
    - New: Zen improved CO counts display with + for positive
    - Fix: Zen CoreMap display fix
    - Fix: Zen CPU details display fix
    - Fix: Submit unknown Zen PowerTable
    - Fix: Tentative fix for Alder Lake
- v1.0.7 Alpha
    - New: Zen monitoring PBO PPT/TDC/EDC with Limits
    - New: Zen CoreMap display, shows if and which cores are burned
    - New: Zen clock stretching detection slightly improved with C0 state
    - New: Better layout display for Effective/Reference/Stretching clocks
    - Fix: Converion decimal issue which resulted in 10x scores
    - Fix: Zen missing clocks and stats for samples with burned cores/ccds
    - Fix: Fixed again switched VDDG CCD/IOD voltages
    - Fix: Score grid row issues
    - Fix: Wrong settings runtime shown
- v1.0.6 Alpha
    - Fix: Switched VDDG CCD/IOD voltages
    - Fix: Reading CPPC tags from localized Windows
- v1.0.5 Alpha
    - Fix: Autoupdater bug
- v1.0.4 Alpha
    - Fix: more retries for Zen PT
    - Fix: Codepage issue with Win11
- v1.0.3 Alpha
    - New: First public Alpha
    - Known issue: details box resize with window is messed up
    - Known issue: fix for logical threads assignment on Alder Lake P/E cores