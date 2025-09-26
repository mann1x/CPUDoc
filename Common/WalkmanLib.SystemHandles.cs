// Get all system open handles method - uses NTQuerySystemInformation and NTQueryObject
//https://gist.github.com/i-e-b/2290426
//https://stackoverflow.com/a/13735033/2999220
//https://stackoverflow.com/a/6351168/2999220


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace WalkmanLib
{
    class SystemHandles
    {
        #region Native Methods

        #region Enums

        //https://pinvoke.net/default.aspx/Enums.NtStatus
        //https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-erref/596a1078-e883-4972-9bbc-49e60bebca55
        protected enum NTSTATUS : uint
        {
            STATUS_SUCCESS = 0x00000000,
            STATUS_BUFFER_OVERFLOW = 0x80000005,
            STATUS_INFO_LENGTH_MISMATCH = 0xC0000004
        }

        //https://www.pinvoke.net/default.aspx/ntdll/SYSTEM_INFORMATION_CLASS.html
        protected enum SYSTEM_INFORMATION_CLASS
        {
            SystemBasicInformation = 0x00,
            SystemProcessorInformation = 0x01,
            SystemPerformanceInformation = 0x02,
            SystemTimeOfDayInformation = 0x03,
            SystemPathInformation = 0x04,
            SystemProcessInformation = 0x05,
            SystemCallCountInformation = 0x06,
            SystemDeviceInformation = 0x07,
            SystemProcessorPerformanceInformation = 0x08,
            SystemFlagsInformation = 0x09,
            SystemCallTimeInformation = 0x0A,
            SystemModuleInformation = 0x0B,
            SystemLocksInformation = 0x0C,
            SystemStackTraceInformation = 0x0D,
            SystemPagedPoolInformation = 0x0E,
            SystemNonPagedPoolInformation = 0x0F,
            SystemHandleInformation = 0x10,
            SystemObjectInformation = 0x11,
            SystemPageFileInformation = 0x12,
            SystemVdmInstemulInformation = 0x13,
            SystemVdmBopInformation = 0x14,
            SystemFileCacheInformation = 0x15,
            SystemPoolTagInformation = 0x16,
            SystemInterruptInformation = 0x17,
            SystemDpcBehaviorInformation = 0x18,
            SystemFullMemoryInformation = 0x19,
            SystemLoadGdiDriverInformation = 0x1A,
            SystemUnloadGdiDriverInformation = 0x1B,
            SystemTimeAdjustmentInformation = 0x1C,
            SystemSummaryMemoryInformation = 0x1D,
            SystemMirrorMemoryInformation = 0x1E,
            SystemPerformanceTraceInformation = 0x1F,
            SystemObsolete0 = 0x20,
            SystemExceptionInformation = 0x21,
            SystemCrashDumpStateInformation = 0x22,
            SystemKernelDebuggerInformation = 0x23,
            SystemContextSwitchInformation = 0x24,
            SystemRegistryQuotaInformation = 0x25,
            SystemExtendServiceTableInformation = 0x26,
            SystemPrioritySeperation = 0x27,
            SystemVerifierAddDriverInformation = 0x28,
            SystemVerifierRemoveDriverInformation = 0x29,
            SystemProcessorIdleInformation = 0x2A,
            SystemLegacyDriverInformation = 0x2B,
            SystemCurrentTimeZoneInformation = 0x2C,
            SystemLookasideInformation = 0x2D,
            SystemTimeSlipNotification = 0x2E,
            SystemSessionCreate = 0x2F,
            SystemSessionDetach = 0x30,
            SystemSessionInformation = 0x31,
            SystemRangeStartInformation = 0x32,
            SystemVerifierInformation = 0x33,
            SystemVerifierThunkExtend = 0x34,
            SystemSessionProcessInformation = 0x35,
            SystemLoadGdiDriverInSystemSpace = 0x36,
            SystemNumaProcessorMap = 0x37,
            SystemPrefetcherInformation = 0x38,
            SystemExtendedProcessInformation = 0x39,
            SystemRecommendedSharedDataAlignment = 0x3A,
            SystemComPlusPackage = 0x3B,
            SystemNumaAvailableMemory = 0x3C,
            SystemProcessorPowerInformation = 0x3D,
            SystemEmulationBasicInformation = 0x3E,
            SystemEmulationProcessorInformation = 0x3F,
            SystemExtendedHandleInformation = 0x40,
            SystemLostDelayedWriteInformation = 0x41,
            SystemBigPoolInformation = 0x42,
            SystemSessionPoolTagInformation = 0x43,
            SystemSessionMappedViewInformation = 0x44,
            SystemHotpatchInformation = 0x45,
            SystemObjectSecurityMode = 0x46,
            SystemWatchdogTimerHandler = 0x47,
            SystemWatchdogTimerInformation = 0x48,
            SystemLogicalProcessorInformation = 0x49,
            SystemWow64SharedInformationObsolete = 0x4A,
            SystemRegisterFirmwareTableInformationHandler = 0x4B,
            SystemFirmwareTableInformation = 0x4C,
            SystemModuleInformationEx = 0x4D,
            SystemVerifierTriageInformation = 0x4E,
            SystemSuperfetchInformation = 0x4F,
            SystemMemoryListInformation = 0x50,
            SystemFileCacheInformationEx = 0x51,
            SystemThreadPriorityClientIdInformation = 0x52,
            SystemProcessorIdleCycleTimeInformation = 0x53,
            SystemVerifierCancellationInformation = 0x54,
            SystemProcessorPowerInformationEx = 0x55,
            SystemRefTraceInformation = 0x56,
            SystemSpecialPoolInformation = 0x57,
            SystemProcessIdInformation = 0x58,
            SystemErrorPortInformation = 0x59,
            SystemBootEnvironmentInformation = 0x5A,
            SystemHypervisorInformation = 0x5B,
            SystemVerifierInformationEx = 0x5C,
            SystemTimeZoneInformation = 0x5D,
            SystemImageFileExecutionOptionsInformation = 0x5E,
            SystemCoverageInformation = 0x5F,
            SystemPrefetchPatchInformation = 0x60,
            SystemVerifierFaultsInformation = 0x61,
            SystemSystemPartitionInformation = 0x62,
            SystemSystemDiskInformation = 0x63,
            SystemProcessorPerformanceDistribution = 0x64,
            SystemNumaProximityNodeInformation = 0x65,
            SystemDynamicTimeZoneInformation = 0x66,
            SystemCodeIntegrityInformation = 0x67,
            SystemProcessorMicrocodeUpdateInformation = 0x68,
            SystemProcessorBrandString = 0x69,
            SystemVirtualAddressInformation = 0x6A,
            SystemLogicalProcessorAndGroupInformation = 0x6B,
            SystemProcessorCycleTimeInformation = 0x6C,
            SystemStoreInformation = 0x6D,
            SystemRegistryAppendString = 0x6E,
            SystemAitSamplingValue = 0x6F,
            SystemVhdBootInformation = 0x70,
            SystemCpuQuotaInformation = 0x71,
            SystemNativeBasicInformation = 0x72,
            SystemErrorPortTimeouts = 0x73,
            SystemLowPriorityIoInformation = 0x74,
            SystemBootEntropyInformation = 0x75,
            SystemVerifierCountersInformation = 0x76,
            SystemPagedPoolInformationEx = 0x77,
            SystemSystemPtesInformationEx = 0x78,
            SystemNodeDistanceInformation = 0x79,
            SystemAcpiAuditInformation = 0x7A,
            SystemBasicPerformanceInformation = 0x7B,
            SystemQueryPerformanceCounterInformation = 0x7C,
            SystemSessionBigPoolInformation = 0x7D,
            SystemBootGraphicsInformation = 0x7E,
            SystemScrubPhysicalMemoryInformation = 0x7F,
            SystemBadPageInformation = 0x80,
            SystemProcessorProfileControlArea = 0x81,
            SystemCombinePhysicalMemoryInformation = 0x82,
            SystemEntropyInterruptTimingInformation = 0x83,
            SystemConsoleInformation = 0x84,
            SystemPlatformBinaryInformation = 0x85,
            SystemPolicyInformation = 0x86,
            SystemHypervisorProcessorCountInformation = 0x87,
            SystemDeviceDataInformation = 0x88,
            SystemDeviceDataEnumerationInformation = 0x89,
            SystemMemoryTopologyInformation = 0x8A,
            SystemMemoryChannelInformation = 0x8B,
            SystemBootLogoInformation = 0x8C,
            SystemProcessorPerformanceInformationEx = 0x8D,
            SystemCriticalProcessErrorLogInformation = 0x8E,
            SystemSecureBootPolicyInformation = 0x8F,
            SystemPageFileInformationEx = 0x90,
            SystemSecureBootInformation = 0x91,
            SystemEntropyInterruptTimingRawInformation = 0x92,
            SystemPortableWorkspaceEfiLauncherInformation = 0x93,
            SystemFullProcessInformation = 0x94,
            SystemKernelDebuggerInformationEx = 0x95,
            SystemBootMetadataInformation = 0x96,
            SystemSoftRebootInformation = 0x97,
            SystemElamCertificateInformation = 0x98,
            SystemOfflineDumpConfigInformation = 0x99,
            SystemProcessorFeaturesInformation = 0x9A,
            SystemRegistryReconciliationInformation = 0x9B,
            SystemEdidInformation = 0x9C,
            SystemManufacturingInformation = 0x9D,
            SystemEnergyEstimationConfigInformation = 0x9E,
            SystemHypervisorDetailInformation = 0x9F,
            SystemProcessorCycleStatsInformation = 0xA0,
            SystemVmGenerationCountInformation = 0xA1,
            SystemTrustedPlatformModuleInformation = 0xA2,
            SystemKernelDebuggerFlags = 0xA3,
            SystemCodeIntegrityPolicyInformation = 0xA4,
            SystemIsolatedUserModeInformation = 0xA5,
            SystemHardwareSecurityTestInterfaceResultsInformation = 0xA6,
            SystemSingleModuleInformation = 0xA7,
            SystemAllowedCpuSetsInformation = 0xA8,
            SystemDmaProtectionInformation = 0xA9,
            SystemInterruptCpuSetsInformation = 0xAA,
            SystemSecureBootPolicyFullInformation = 0xAB,
            SystemCodeIntegrityPolicyFullInformation = 0xAC,
            SystemAffinitizedInterruptProcessorInformation = 0xAD,
            SystemRootSiloInformation = 0xAE,
            SystemCpuSetInformation = 0xAF,
            SystemCpuSetTagInformation = 0xB0,
            SystemWin32WerStartCallout = 0xB1,
            SystemSecureKernelProfileInformation = 0xB2,
            SystemCodeIntegrityPlatformManifestInformation = 0xB3,
            SystemInterruptSteeringInformation = 0xB4,
            SystemSuppportedProcessorArchitectures = 0xB5,
            SystemMemoryUsageInformation = 0xB6,
            SystemCodeIntegrityCertificateInformation = 0xB7,
            SystemPhysicalMemoryInformation = 0xB8,
            SystemControlFlowTransition = 0xB9,
            SystemKernelDebuggingAllowed = 0xBA,
            SystemActivityModerationExeState = 0xBB,
            SystemActivityModerationUserSettings = 0xBC,
            SystemCodeIntegrityPoliciesFullInformation = 0xBD,
            SystemCodeIntegrityUnlockInformation = 0xBE,
            SystemIntegrityQuotaInformation = 0xBF,
            SystemFlushInformation = 0xC0,
            SystemProcessorIdleMaskInformation = 0xC1,
            SystemSecureDumpEncryptionInformation = 0xC2,
            SystemWriteConstraintInformation = 0xC3,
            SystemKernelVaShadowInformation = 0xC4,
            SystemHypervisorSharedPageInformation = 0xC5,
            SystemFirmwareBootPerformanceInformation = 0xC6,
            SystemCodeIntegrityVerificationInformation = 0xC7,
            SystemFirmwarePartitionInformation = 0xC8,
            SystemSpeculationControlInformation = 0xC9,
            SystemDmaGuardPolicyInformation = 0xCA,
            SystemEnclaveLaunchControlInformation = 0xCB,
            SystemWorkloadAllowedCpuSetsInformation = 0xCC,
            SystemCodeIntegrityUnlockModeInformation = 0xCD,
            SystemLeapSecondInformation = 0xCE,
            SystemFlags2Information = 0xCF,
            SystemSecurityModelInformation = 0xD0,
            SystemCodeIntegritySyntheticCacheInformation = 0xD1,
            MaxSystemInfoClass = 0xD2
        }

        //https://www.pinvoke.net/default.aspx/Enums.OBJECT_INFORMATION_CLASS
        protected enum OBJECT_INFORMATION_CLASS
        {
            ObjectBasicInformation = 0,
            ObjectNameInformation = 1,
            ObjectTypeInformation = 2,
            ObjectAllTypesInformation = 3,
            ObjectHandleInformation = 4
        }

        //https://docs.microsoft.com/en-za/windows/win32/procthread/process-security-and-access-rights
        //https://www.pinvoke.net/default.aspx/Enums.ProcessAccess
        protected enum PROCESS_ACCESS_RIGHTS
        {
            PROCESS_TERMINATE = 0x00000001,
            PROCESS_CREATE_THREAD = 0x00000002,
            PROCESS_SET_SESSION_ID = 0x00000004,
            PROCESS_VM_OPERATION = 0x00000008,
            PROCESS_VM_READ = 0x00000010,
            PROCESS_VM_WRITE = 0x00000020,
            PROCESS_DUP_HANDLE = 0x00000040,
            PROCESS_CREATE_PROCESS = 0x00000080,
            PROCESS_SET_QUOTA = 0x00000100,
            PROCESS_SET_INFORMATION = 0x00000200,
            PROCESS_QUERY_INFORMATION = 0x00000400,
            PROCESS_SUSPEND_RESUME = 0x00000800,
            PROCESS_QUERY_LIMITED_INFORMATION = 0x00001000,
            DELETE = 0x00010000,
            READ_CONTROL = 0x00020000,
            WRITE_DAC = 0x00040000,
            WRITE_OWNER = 0x00080000,
            STANDARD_RIGHTS_REQUIRED = 0x000F0000,
            SYNCHRONIZE = 0x00100000,

            PROCESS_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFFF
        }

        //https://docs.microsoft.com/en-us/windows/win32/api/handleapi/nf-handleapi-duplicatehandle#DUPLICATE_CLOSE_SOURCE
        protected enum DUPLICATE_HANDLE_OPTIONS
        {
            DUPLICATE_CLOSE_SOURCE = 0x00000001,
            DUPLICATE_SAME_ACCESS = 0x00000002
        }

        //http://www.jasinskionline.com/TechnicalWiki/SYSTEM_HANDLE_INFORMATION-WinApi-Struct.ashx
        internal enum SYSTEM_HANDLE_FLAGS : byte
        {
            PROTECT_FROM_CLOSE = 0x01,
            INHERIT = 0x02
        }

        //https://www.winehq.org/pipermail/wine-patches/2005-October/021642.html
        //https://github.com/olimsaidov/autorun-remover/blob/b558df6487ae1cb4cb998fab3330c07bb7de0f21/NativeAPI.pas#L108
        internal enum SYSTEM_HANDLE_TYPE
        {
            UNKNOWN = 00,
            TYPE = 01,
            DIRECTORY = 02,
            SYMBOLIC_LINK = 03,
            TOKEN = 04,
            PROCESS = 05,
            THREAD = 06,
            JOB = 07,
            EVENT = 08,
            EVENT_PAIR = 09,
            MUTANT = 10,
            UNKNOWN_11 = 11,
            SEMAPHORE = 12,
            TIMER = 13,
            PROFILE = 14,
            WINDOW_STATION = 15,
            DESKTOP = 16,
            SECTION = 17,
            KEY = 18,
            PORT = 19,
            WAITABLE_PORT = 20,
            ADAPTER = 21,
            CONTROLLER = 22,
            DEVICE = 23,
            DRIVER = 24,
            IO_COMPLETION = 25,
            FILE = 28,

            // From my own research
            TP_WORKER_FACTORY,
            ALPC_PORT,
            KEYED_EVENT,
            SESSION,
            IO_COMPLETION_RESERVE,
            WMI_GUID,
            USER_APC_RESERVE,
            IR_TIMER,
            COMPOSITION,
            WAIT_COMPLETION_PACKET,
            DXGK_SHARED_RESOURCE,
            DXGK_SHARED_SYNC_OBJECT,
            DXGK_DISPLAY_MANAGER_OBJECT,
            DXGK_COMPOSITION_OBJECT,
            OTHER
        }

        #endregion

        #region Structs

        //https://www.codeproject.com/script/Articles/ViewDownloads.aspx?aid=18975&zep=OpenedFileFinder%2fUtils.h&rzp=%2fKB%2fshell%2fOpenedFileFinder%2f%2fopenedfilefinder_src.zip
        [StructLayout(LayoutKind.Sequential)]
        protected struct SYSTEM_HANDLE_INFORMATION
        {
            //public IntPtr dwCount;
            public uint dwCount;

            // see https://stackoverflow.com/a/38884095/2999220 - MarshalAs doesn't allow variable sized arrays
            //[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct)]
            //public SYSTEM_HANDLE[] Handles;
            public IntPtr Handles;
        }

        //https://stackoverflow.com/a/5163277/2999220
        //http://www.jasinskionline.com/TechnicalWiki/SYSTEM_HANDLE_INFORMATION-WinApi-Struct.ashx
        [StructLayout(LayoutKind.Sequential)]
        internal struct SYSTEM_HANDLE
        {
            /// <summary>Handle Owner Process ID</summary>
            public uint dwProcessId;
            /// <summary>Object Type</summary>
            public byte bObjectType;
            /// <summary>Handle Flags</summary>
            public SYSTEM_HANDLE_FLAGS bFlags;
            /// <summary>Handle Value</summary>
            public ushort wValue;
            /// <summary>Object Pointer</summary>
            IntPtr pAddress;
            /// <summary>Access Mask</summary>
            public uint dwGrantedAccess;
        }

        //https://docs.microsoft.com/en-us/windows/win32/api/ntdef/ns-ntdef-_unicode_string
        //https://www.pinvoke.net/default.aspx/Structures/UNICODE_STRING.html
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        protected struct UNICODE_STRING
        {
            public readonly ushort Length;
            public readonly ushort MaximumLength;
            [MarshalAs(UnmanagedType.LPWStr)]
            public readonly string Buffer;

            public UNICODE_STRING(string s)
            {
                Length = (ushort)(s.Length * 2);
                MaximumLength = (ushort)(Length + 2);
                Buffer = s;
            }
        }

        //https://www.pinvoke.net/default.aspx/Structures.GENERIC_MAPPING
        //http://www.jasinskionline.com/technicalwiki/GENERIC_MAPPING-WinApi-Struct.ashx
        [StructLayout(LayoutKind.Sequential)]
        protected struct GENERIC_MAPPING
        {
            public uint GenericRead;
            public uint GenericWrite;
            public uint GenericExecute;
            public uint GenericAll;
        }

        //http://www.jasinskionline.com/technicalwiki/OBJECT_NAME_INFORMATION-WinApi-Struct.ashx
        [StructLayout(LayoutKind.Sequential)]
        protected struct OBJECT_NAME_INFORMATION
        {
            public UNICODE_STRING Name;
        }

        //https://docs.microsoft.com/en-za/windows-hardware/drivers/ddi/ntifs/ns-ntifs-__public_object_type_information
        //http://www.jasinskionline.com/technicalwiki/OBJECT_TYPE_INFORMATION-WinApi-Struct.ashx
        [StructLayout(LayoutKind.Sequential)]
        protected struct OBJECT_TYPE_INFORMATION
        {
            public UNICODE_STRING TypeName;
            public int ObjectCount;
            public int HandleCount;
            int Reserved1;
            int Reserved2;
            int Reserved3;
            int Reserved4;
            public int PeakObjectCount;
            public int PeakHandleCount;
            int Reserved5;
            int Reserved6;
            int Reserved7;
            int Reserved8;
            public int InvalidAttributes;
            public GENERIC_MAPPING GenericMapping;
            public int ValidAccess;
            byte Unknown;
            public byte MaintainHandleDatabase;
            public int PoolType;
            public int PagedPoolUsage;
            public int NonPagedPoolUsage;
        }

        #endregion

        #region Methods

        //https://docs.microsoft.com/en-us/windows/win32/api/winternl/nf-winternl-ntquerysysteminformation
        [DllImport("ntdll.dll")]
        protected static extern NTSTATUS NtQuerySystemInformation(
            [In] SYSTEM_INFORMATION_CLASS SystemInformationClass,
            [Out] IntPtr SystemInformation,
            [In] uint SystemInformationLength,
            [Out] out uint ReturnLength
        );

        //https://docs.microsoft.com/en-us/windows/win32/api/winternl/nf-winternl-ntqueryobject
        [DllImport("ntdll.dll")]
        protected static extern NTSTATUS NtQueryObject(
            [In] IntPtr Handle,
            [In] OBJECT_INFORMATION_CLASS ObjectInformationClass,
            [In] IntPtr ObjectInformation,
            [In] uint ObjectInformationLength,
            [Out] out uint ReturnLength
        );

        //https://docs.microsoft.com/en-za/windows/win32/api/processthreadsapi/nf-processthreadsapi-openprocess
        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern IntPtr OpenProcess(
            [In] PROCESS_ACCESS_RIGHTS dwDesiredAccess,
            [In, MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            [In] uint dwProcessId
        );

        //https://docs.microsoft.com/en-us/windows/win32/api/handleapi/nf-handleapi-duplicatehandle
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        protected static extern bool DuplicateHandle(
            [In] IntPtr hSourceProcessHandle,
            [In] IntPtr hSourceHandle,
            [In] IntPtr hTargetProcessHandle,
            [Out] out IntPtr lpTargetHandle,
            [In] uint dwDesiredAccess,
            [In, MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            [In] DUPLICATE_HANDLE_OPTIONS dwOptions
        );

        //https://docs.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getcurrentprocess
        [DllImport("kernel32.dll")]
        protected static extern IntPtr GetCurrentProcess();

        //https://docs.microsoft.com/en-us/windows/win32/api/handleapi/nf-handleapi-closehandle
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        protected static extern bool CloseHandle(
            [In] IntPtr hObject
        );

        //https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-querydosdevicea
        //https://docs.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-querydosdevicew
        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern uint QueryDosDevice(
            [In] string lpDeviceName,
            [Out] StringBuilder lpTargetPath,
            [In] uint ucchMax
        );

        #endregion

        #endregion

        #region Public Methods

        #region GetSystemHandles

        /// <summary>Gets all the open handles on the system. Use GetHandleInfo to retrieve proper type and name information.</summary>
        /// <returns>Enumerable list of system handles</returns>
        internal static IEnumerable<SYSTEM_HANDLE> GetSystemHandles()
        {
            uint length = 0x1000;
            IntPtr ptr = IntPtr.Zero;
            bool done = false;
            try
            {
                while (!done)
                {
                    ptr = Marshal.AllocHGlobal((int)length);
                    uint wantedLength;
                    switch (NtQuerySystemInformation(
                        SYSTEM_INFORMATION_CLASS.SystemHandleInformation,
                        ptr, length, out wantedLength))
                    {
                        case NTSTATUS.STATUS_SUCCESS:
                            done = true; // can't double-break in C#
                            break;
                        case NTSTATUS.STATUS_INFO_LENGTH_MISMATCH:
                            length = Math.Max(length, wantedLength);
                            Marshal.FreeHGlobal(ptr);
                            ptr = IntPtr.Zero;
                            break;
                        default:
                            throw new Exception("Failed to retrieve system handle information.", new Win32Exception());
                    }
                }

                long handleCount = IntPtr.Size == 4 ? Marshal.ReadInt32(ptr) : Marshal.ReadInt64(ptr);
                long offset = IntPtr.Size;
                long size = Marshal.SizeOf(typeof(SYSTEM_HANDLE));

                for (long i = 0; i < handleCount; i++)
                {
                    SYSTEM_HANDLE struc = Marshal.PtrToStructure<SYSTEM_HANDLE>((IntPtr)((long)ptr + offset));
                    yield return struc;

                    offset += size;
                }
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }

        #endregion

        #region GetHandleInfo

        internal struct HandleInfo
        {
            public uint ProcessID;
            public ushort HandleID;
            public uint GrantedAccess;
            public byte RawType;
            public SYSTEM_HANDLE_FLAGS Flags;
            public string Name;
            public string TypeString;
            public SYSTEM_HANDLE_TYPE Type;
        }

        private static ConcurrentDictionary<byte, string> rawTypeMap = new ConcurrentDictionary<byte, string>();

        private static SYSTEM_HANDLE_TYPE HandleTypeFromString(string typeString)
        {
            switch (typeString)
            {
                case null:
                    return SYSTEM_HANDLE_TYPE.UNKNOWN;
                case "Directory":
                    return SYSTEM_HANDLE_TYPE.DIRECTORY;
                case "SymbolicLink":
                    return SYSTEM_HANDLE_TYPE.SYMBOLIC_LINK;
                case "Token":
                    return SYSTEM_HANDLE_TYPE.TOKEN;
                case "Process":
                    return SYSTEM_HANDLE_TYPE.PROCESS;
                case "Thread":
                    return SYSTEM_HANDLE_TYPE.THREAD;
                case "Job":
                    return SYSTEM_HANDLE_TYPE.JOB;
                case "Event":
                    return SYSTEM_HANDLE_TYPE.EVENT;
                case "Mutant":
                    return SYSTEM_HANDLE_TYPE.MUTANT;
                case "Semaphore":
                    return SYSTEM_HANDLE_TYPE.SEMAPHORE;
                case "Timer":
                    return SYSTEM_HANDLE_TYPE.TIMER;
                case "WindowStation":
                    return SYSTEM_HANDLE_TYPE.WINDOW_STATION;
                case "Desktop":
                    return SYSTEM_HANDLE_TYPE.DESKTOP;
                case "Section":
                    return SYSTEM_HANDLE_TYPE.SECTION;
                case "Key":
                    return SYSTEM_HANDLE_TYPE.KEY;
                case "IoCompletion":
                    return SYSTEM_HANDLE_TYPE.IO_COMPLETION;
                case "File":
                    return SYSTEM_HANDLE_TYPE.FILE;
                case "TpWorkerFactory":
                    return SYSTEM_HANDLE_TYPE.TP_WORKER_FACTORY;
                case "ALPC Port":
                    return SYSTEM_HANDLE_TYPE.ALPC_PORT;
                case "KeyedEvent":
                    return SYSTEM_HANDLE_TYPE.KEYED_EVENT;
                case "Session":
                    return SYSTEM_HANDLE_TYPE.SESSION;
                case "IoCompletionReserve":
                    return SYSTEM_HANDLE_TYPE.IO_COMPLETION_RESERVE;
                case "WmiGuid":
                    return SYSTEM_HANDLE_TYPE.WMI_GUID;
                case "UserApcReserve":
                    return SYSTEM_HANDLE_TYPE.USER_APC_RESERVE;
                case "IRTimer":
                    return SYSTEM_HANDLE_TYPE.IR_TIMER;
                case "Composition":
                    return SYSTEM_HANDLE_TYPE.COMPOSITION;
                case "WaitCompletionPacket":
                    return SYSTEM_HANDLE_TYPE.WAIT_COMPLETION_PACKET;
                case "DxgkSharedResource":
                    return SYSTEM_HANDLE_TYPE.DXGK_SHARED_RESOURCE;
                case "DxgkSharedSyncObject":
                    return SYSTEM_HANDLE_TYPE.DXGK_SHARED_SYNC_OBJECT;
                case "DxgkDisplayManagerObject":
                    return SYSTEM_HANDLE_TYPE.DXGK_DISPLAY_MANAGER_OBJECT;
                case "DxgkCompositionObject":
                    return SYSTEM_HANDLE_TYPE.DXGK_COMPOSITION_OBJECT;
                default:
                    return SYSTEM_HANDLE_TYPE.OTHER;
            }
        }

        /// <summary>
        /// Gets the handle type and name, and puts the other properties into more user-friendly fields.
        /// 
        /// This function gets typeInfo from an internal type map (rawType to typeString) that is built as types are retrieved.
        /// To get full type information of handle types that could not be retrieved,
        /// either put the handles into a list, build a second map and apply them retroactively,
        /// or call this function on all System Handles beforehand with getting names Disabled.
        /// </summary>
        /// <param name="handle">Handle struct returned by GetSystemHandles</param>
        /// <param name="getAllNames">False (default) to ignore certain names that cause the system query to hang. Only set to true in a thread that can be killed.</param>
        /// <param name="onlyGetNameFor">Set this to only attempt to get Handle names for a specific handle type. Set to int.MaxValue to disable getting file names.</param>
        /// <returns>HandleInfo struct with retrievable information populated.</returns>
        internal static HandleInfo GetHandleInfo(SYSTEM_HANDLE handle, bool getAllNames = false, SYSTEM_HANDLE_TYPE onlyGetNameFor = SYSTEM_HANDLE_TYPE.UNKNOWN)
        {
            HandleInfo handleInfo = new HandleInfo
            {
                ProcessID = handle.dwProcessId,
                HandleID = handle.wValue,
                GrantedAccess = handle.dwGrantedAccess,
                RawType = handle.bObjectType,
                Flags = handle.bFlags,
                Name = null,
                TypeString = null,
                Type = SYSTEM_HANDLE_TYPE.UNKNOWN
            };

            // get type from cached map if it exists
            if (rawTypeMap.ContainsKey(handleInfo.RawType))
            {
                handleInfo.TypeString = rawTypeMap[handleInfo.RawType];
                handleInfo.Type = HandleTypeFromString(handleInfo.TypeString);
            }

            IntPtr sourceProcessHandle = IntPtr.Zero;
            IntPtr handleDuplicate = IntPtr.Zero;
            try
            {
                sourceProcessHandle = OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_DUP_HANDLE, true, handleInfo.ProcessID);

                // To read info about a handle owned by another process we must duplicate it into ours
                // For simplicity, current process handles will also get duplicated; remember that process handles cannot be compared for equality
                if (!DuplicateHandle(sourceProcessHandle, (IntPtr)handleInfo.HandleID, GetCurrentProcess(), out handleDuplicate, 0, false, DUPLICATE_HANDLE_OPTIONS.DUPLICATE_SAME_ACCESS))
                    return handleInfo;

                // Get the object type if it hasn't been retrieved from cache map above
                if (!rawTypeMap.ContainsKey(handleInfo.RawType))
                {
                    uint length;
                    NtQueryObject(handleDuplicate, OBJECT_INFORMATION_CLASS.ObjectTypeInformation, IntPtr.Zero, 0, out length);

                    IntPtr ptr = IntPtr.Zero;
                    try
                    {
                        ptr = Marshal.AllocHGlobal((int)length);
                        if (NtQueryObject(handleDuplicate, OBJECT_INFORMATION_CLASS.ObjectTypeInformation, ptr, length, out length) != NTSTATUS.STATUS_SUCCESS)
                            return handleInfo;

                        OBJECT_TYPE_INFORMATION typeInfo = Marshal.PtrToStructure<OBJECT_TYPE_INFORMATION>(ptr);
                        handleInfo.TypeString = typeInfo.TypeName.Buffer;
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(ptr);
                    }

                    rawTypeMap.TryAdd(handleInfo.RawType, handleInfo.TypeString);
                    handleInfo.Type = HandleTypeFromString(handleInfo.TypeString);
                }

                // Get the object name
                if (handleInfo.TypeString != null &&
                    // only check onlyGetNameFor if it isn't UNKNOWN
                    (onlyGetNameFor == SYSTEM_HANDLE_TYPE.UNKNOWN || handleInfo.Type == onlyGetNameFor) &&
                    (getAllNames == true || (
                        // this type can hang for ~15mins, but excluding it cuts a lot of results, and it does eventually resolve...
                        //!(handleInfo.Type == SYSTEM_HANDLE_TYPE.FILE && handleInfo.GrantedAccess == 0x120089 && handleInfo.Flags == 0x00                       ) &&
                        !(handleInfo.Type == SYSTEM_HANDLE_TYPE.FILE && handleInfo.GrantedAccess == 0x120089 && handleInfo.Flags == SYSTEM_HANDLE_FLAGS.INHERIT) &&
                        !(handleInfo.Type == SYSTEM_HANDLE_TYPE.FILE && handleInfo.GrantedAccess == 0x120189 && handleInfo.Flags == 0x00) &&
                        !(handleInfo.Type == SYSTEM_HANDLE_TYPE.FILE && handleInfo.GrantedAccess == 0x120189 && handleInfo.Flags == SYSTEM_HANDLE_FLAGS.INHERIT) &&
                        !(handleInfo.Type == SYSTEM_HANDLE_TYPE.FILE && handleInfo.GrantedAccess == 0x12019f && handleInfo.Flags == 0x00) &&
                        !(handleInfo.Type == SYSTEM_HANDLE_TYPE.FILE && handleInfo.GrantedAccess == 0x12019f && handleInfo.Flags == SYSTEM_HANDLE_FLAGS.INHERIT) &&
                        !(handleInfo.Type == SYSTEM_HANDLE_TYPE.FILE && handleInfo.GrantedAccess == 0x1a019f && handleInfo.Flags == 0x00) &&
                        !(handleInfo.Type == SYSTEM_HANDLE_TYPE.FILE && handleInfo.GrantedAccess == 0x1a019f && handleInfo.Flags == SYSTEM_HANDLE_FLAGS.INHERIT)
                    )))// don't query some objects that get stuck (NtQueryObject hangs on NamedPipes)
                {
                    uint length;
                    NtQueryObject(handleDuplicate, OBJECT_INFORMATION_CLASS.ObjectNameInformation, IntPtr.Zero, 0, out length);

                    IntPtr ptr = IntPtr.Zero;
                    try
                    {
                        ptr = Marshal.AllocHGlobal((int)length);
                        if (NtQueryObject(handleDuplicate, OBJECT_INFORMATION_CLASS.ObjectNameInformation, ptr, length, out length) != NTSTATUS.STATUS_SUCCESS)
                            return handleInfo;

                        if (ptr != IntPtr.Zero)
                        {
                            OBJECT_NAME_INFORMATION nameInfo = Marshal.PtrToStructure<OBJECT_NAME_INFORMATION>(ptr);
                            handleInfo.Name = nameInfo.Name.Buffer;
                        }
                    }
                    catch { }
                    finally
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                }
            }
            catch { }
            finally
            {
                CloseHandle(sourceProcessHandle);
                if (handleDuplicate != IntPtr.Zero)
                    CloseHandle(handleDuplicate);
            }

            return handleInfo;
        }

        #endregion

        #region CloseSystemHandle

        // https://www.codeproject.com/Articles/18975/Listing-Used-Files
        /// <summary>Attempts to close a handle in a different process. Fails silently if the handle exists but could not be closed.</summary>
        /// <param name="ProcessID">Process ID of the process containing the handle to close</param>
        /// <param name="HandleID">Handle value in the target process to close</param>
        internal static void CloseSystemHandle(uint ProcessID, ushort HandleID)
        {
            IntPtr sourceProcessHandle = IntPtr.Zero;
            IntPtr handleDuplicate = IntPtr.Zero;
            try
            {
                sourceProcessHandle = OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_DUP_HANDLE, true, ProcessID);
                if ((int)sourceProcessHandle < 1)
                    throw new ArgumentException("Process ID Not Found!", "ProcessID", new Win32Exception());

                // always returns false, no point in checking
                DuplicateHandle(sourceProcessHandle, (IntPtr)HandleID, GetCurrentProcess(), out handleDuplicate, 0, false, DUPLICATE_HANDLE_OPTIONS.DUPLICATE_CLOSE_SOURCE);
                if ((int)handleDuplicate < 1 && Marshal.GetLastWin32Error() == 6) // ERROR_INVALID_HANDLE: The handle is invalid.
                    throw new ArgumentException("Handle ID Not Found!", "HandleID", new Win32Exception(6));
            }
            catch { }
            finally
            {
                CloseHandle(sourceProcessHandle);
                if (handleDuplicate != IntPtr.Zero)
                    CloseHandle(handleDuplicate);
            }
        }

        #endregion

        #region ConvertDevicePathToDosPath

        private static Dictionary<string, string> deviceMap;
        private const string networkDeviceQueryDosDevicePrefix = "\\Device\\LanmanRedirector\\";
        private const string networkDeviceSystemHandlePrefix = "\\Device\\Mup\\";
        private const int MAX_PATH = 260;

        private static string NormalizeDeviceName(string deviceName)
        {
            try 
            {
                if (string.Compare( // if deviceName.StartsWith(networkDeviceQueryDosDevicePrefix)
                    deviceName, 0,
                    networkDeviceQueryDosDevicePrefix, 0,
                    networkDeviceQueryDosDevicePrefix.Length, StringComparison.InvariantCulture) == 0)
                {
                    string shareName = deviceName.Substring(deviceName.IndexOf('\\', networkDeviceQueryDosDevicePrefix.Length) + 1);
                    return string.Concat(networkDeviceSystemHandlePrefix, shareName);
                }
            }
            catch { }
            return deviceName;
        }

        private static Dictionary<string, string> BuildDeviceMap()
        {
            string[] logicalDrives = Environment.GetLogicalDrives();
            Dictionary<string, string> localDeviceMap = new Dictionary<string, string>(logicalDrives.Length);

            try
            {
                StringBuilder lpTargetPath = new StringBuilder(MAX_PATH);
                foreach (string drive in logicalDrives)
                {
                    string lpDeviceName = drive.Substring(0, 2);

                    QueryDosDevice(lpDeviceName, lpTargetPath, MAX_PATH);

                    localDeviceMap.Add(
                        NormalizeDeviceName(lpTargetPath.ToString()),
                        lpDeviceName
                    );
                }
                // add a map so \\COMPUTER\ shares get picked up correctly - these will come as \Device\Mup\COMPUTER\share
                localDeviceMap.Add(
                    // remove the last slash from networkDeviceSystemHandlePrefix:
                    networkDeviceSystemHandlePrefix.Substring(0, networkDeviceSystemHandlePrefix.Length - 1),
                    "\\");
            }
            catch { }
            return localDeviceMap;
        }

        private static void EnsureDeviceMap()
        {
            if (deviceMap == null)
            {
                Dictionary<string, string> localDeviceMap = BuildDeviceMap();
                Interlocked.CompareExchange(ref deviceMap, localDeviceMap, null);
            }
        }

        /// <summary>
        /// Converts a device path to a DOS path. Requires a trailing slash if just the device path is passed.
        /// Returns string.Empty if no device is found.
        /// </summary>
        /// <param name="devicePath">Full path including a device. Device paths usually start with \Device\HarddiskVolume[n]\</param>
        /// <returns>DOS Path or string.Empty if none found</returns>
        public static string ConvertDevicePathToDosPath(string devicePath)
        {
            try 
            {
                EnsureDeviceMap();
                int i = devicePath.Length;

                // search in reverse, to catch network shares that are mapped before returning general network path
                while (i > 0 && (i = devicePath.LastIndexOf('\\', i - 1)) != -1)
                {
                    string drive;
                    if (deviceMap.TryGetValue(devicePath.Remove(i), out drive))
                        return string.Concat(drive, devicePath.Substring(i));
                }
            }
            catch { }
            return devicePath;
        }

        #endregion

        #region GetFileHandles / GetLockingProcesses

        /// <summary>
        /// Searches through all the open handles on the system, and returns handles with a path containing <paramref name="filePath"/>.
        /// If on a network share, <paramref name="filePath"/> should refer to the deepest mapped drive.
        /// </summary>
        /// <param name="filePath">Path to look for handles to.</param>
        /// <returns>Enumerable list of handles matching <paramref name="filePath"/></returns>
        internal static IEnumerable<HandleInfo> GetFileHandles(string filePath)
        {            
            if (File.Exists(filePath))
                filePath = new FileInfo(filePath).FullName;
            else if (Directory.Exists(filePath))
                filePath = new DirectoryInfo(filePath).FullName;

            foreach (SYSTEM_HANDLE systemHandle in GetSystemHandles())
            {
                HandleInfo handleInfo = GetHandleInfo(systemHandle, onlyGetNameFor: SYSTEM_HANDLE_TYPE.FILE);
                if (handleInfo.Type == SYSTEM_HANDLE_TYPE.FILE && !String.IsNullOrEmpty(handleInfo.Name))
                {
                    handleInfo.Name = ConvertDevicePathToDosPath(handleInfo.Name);
                    if (handleInfo.Name.Contains(filePath))
                        yield return handleInfo;
                }
            }
        }

        /// <summary>
        /// Gets a list of processes locking <paramref name="filePath"/>.
        /// Processes that can't be retrieved by PID (if they have exited) will be excluded.
        /// If on a network share, <paramref name="filePath"/> should refer to the deepest mapped drive.
        /// </summary>
        /// <param name="filePath">Path to look for locking processes.</param>
        /// <returns>List of processes locking <paramref name="filePath"/>.</returns>
        public static List<Process> GetLockingProcesses(string filePath)
        {
            List<Process> processes = new List<Process>();
            try
            {
                foreach (HandleInfo handleInfo in GetFileHandles(filePath))
                {
                    try
                    {
                        Process process = Process.GetProcessById((int)handleInfo.ProcessID);
                        processes.Add(process);
                    } // process has exited
                    catch (ArgumentException) { }
                }
            }
            catch { }

            return processes;
        }

        #endregion

        #endregion
    }
}