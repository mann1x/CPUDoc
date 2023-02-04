﻿![GitHub all releases](https://img.shields.io/github/downloads/mann1x/CPUDoc/total)
![GitHub release (latest by date)](https://img.shields.io/github/v/release/mann1x/CPUDoc)
![GitHub contributors](https://img.shields.io/github/contributors/mann1x/CPUDoc)
![GitHub Repo stars](https://img.shields.io/github/stars/mann1x/CPUDoc?style=social)

# CPUDoc

---

## Changelog:

- v1.2.4 Beta
    - New: Added Process Lasso Performance Mode to detect GameMode
    - New: Added RTC Wake Timers option to PSA
    - New: Optimizations for CS:GO and Battlefield series
- v1.2.3 Beta
    - New: introduced support for Zen AM5 DDR5 display
- v1.2.2 Beta
    - Fix: Fix Zen4 Power Tables and more
    - Fix: Some code optimizations with ChatGPT!
    - Fix: Disabling and enabling SSH in Settings would not update enabled cores display
    - Fix: Zen SMU version not detected display
- v1.2.1 Beta
    - Fix: Fix Zen4 SMU detection
- v1.2.0 Beta
    - New: Added initial support for Zen4 (Raphael) Ryzen 7000
- v1.1.9 Beta
    - New: FocusAssistAPI can be now used also when forcing Alarms or Priority only by querying the status
    - Fix: UserNotificationAPI failure not considered as status to enable GameMode
- v1.1.8 Beta
    - New: PowerSaveActive; introduction of Active Mode, Game Mode, Power Tweak, Plan Personality
    - New: Improved OS detection and logging
    - New: Improved Live CPU Status Display
    - New: Added Splash-screen and startup balloon tool-tip
    - New: Optimizations for GeForce Experience, OBS recording and streaming
    - New: Optimizations for 3DMark CPU tests
    - New: Moved project to VS 2022 and .NET 6
    - New: USB Fix for AMD via "Selective Suspend"
    - New: SSH optimization based on cores number
    - New: Detection of Riot Vanguard - ZenControl is disabled
    - Fix: Live CPU Status Display freezing
    - Fix: Improved application management and optimizations
    - Fix: Improved error and exception handling, startup sequence
    - Fix: Improved fall-back to standard/previous Windows Power Plan
    - Fix: NumaZero T1 disable
    - Fix: Status display, memory clock
    - Fix: Single window open
    - Fix: Improved compatibility with 1usmus's Hydra, CPU-z
    - Fix: NumaZero with Intel E-Cores Parking via HET Policy
    - Fix: PowerSaveActive; improvements with the dynamic power plan for Intel Hybrid
- v1.1.6 Beta
    - Fix: CPU Status Display and NumaZero threads selection
- v1.1.5 Beta
    - Fix: Display and selection of Intel E-Cores
    - Fix: PSA Power Plan optimization for Hybrid
- v1.1.4 Beta
    - Fix: Bug in Zen Initialization
- v1.1.3 Beta
    - New: ZenControl; first implementation of Zen Automatic Control (for now only sets PBO Limits PPT/TDC/EDC)
    - New: PowerSaveActive; improvements with more dynamic settings (HE policy)
    - Fix: Disabled white-list CpuSet mask for processes as can cause rare and random system unresponsive
    - Fix: Display and selection of Intel E-Cores
- v1.1.2 Beta
    - New: Show real time thread load
- v1.1.1 Beta
    - Fix: Correct order with E-Cores
    - Fix: NumaZero initialization
- v1.1.0 Beta
    - Fix: wrong version
- v1.0.9 Beta
    - New: Display System CpuSet Mask; fix initialization
- v1.0.8 Beta
    - New: Display running System CpuSet Mask; green allocated, red deallocated, black disabled.
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
    - Fix: Disable SetSysHack and NumaZero for pre-Win10 OS
    - Fix: Status display in Main tab for the features
- v1.0.3 Beta
    - New: SysSetHack feature; dynamic T1 allocation, increased performances as with SMT/HyperThreading Off
    - New: PowerSaveActive feature; dynamic power plan, ultra low power Light and Deep Sleep modes
    - New: NumaZero feature; only use Cores in same Numa Index, Cache Complex (CCD or CCX) or P-Cores (Intel).
- v1.0.2 Alpha
    - Fix: Disabled massive debug logging left enabled
- v1.0.1 Alpha
    - New: Separate threads for Hardware Monitoring, set system mask and ThreadBooster
    - New: Updated Zen-Core DLL to latest code-base with some additions for future work (detecting Max PBO values etc)
    - New: Added Idle detection, PowerManagement class and, Global/Event hook for future work
- v1.0.0 Alpha
    - New: First public Alpha