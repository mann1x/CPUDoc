using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Reflection;
using Microsoft.Win32.SafeHandles;
using System.Threading;

namespace CPUDoc
{
    public class GroupPolicy
    {
        [ComImport, Guid("EA502722-A23D-11d1-A7D3-0000F87571E3")]
        internal class GPClass
        {
        }

        [ComImport, Guid("EA502723-A23D-11d1-A7D3-0000F87571E3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IGroupPolicyObject
        {
            uint New([MarshalAs(UnmanagedType.LPWStr)] string domainName, [MarshalAs(UnmanagedType.LPWStr)] string displayName, uint flags);

            uint OpenDSGPO([MarshalAs(UnmanagedType.LPWStr)] string path, uint flags);

            uint OpenLocalMachineGPO(uint flags);

            uint OpenRemoteMachineGPO([MarshalAs(UnmanagedType.LPWStr)] string computerName, uint flags);

            uint Save([MarshalAs(UnmanagedType.Bool)] bool machine, [MarshalAs(UnmanagedType.Bool)] bool add, [MarshalAs(UnmanagedType.LPStruct)] Guid extension, [MarshalAs(UnmanagedType.LPStruct)] Guid app);

            uint Delete();

            uint GetName([MarshalAs(UnmanagedType.LPWStr)] StringBuilder name, int maxLength);

            uint GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] StringBuilder name, int maxLength);

            uint SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string name);

            uint GetPath([MarshalAs(UnmanagedType.LPWStr)] StringBuilder path, int maxPath);

            uint GetDSPath(uint section, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder path, int maxPath);

            uint GetFileSysPath(uint section, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder path, int maxPath);

            uint GetRegistryKey(uint section, out IntPtr key);

            uint GetOptions(out uint options);

            uint SetOptions(uint options, uint mask);

            uint GetType(out IntPtr gpoType);

            uint GetMachineName([MarshalAs(UnmanagedType.LPWStr)] StringBuilder name, int maxLength);

            uint GetPropertySheetPages(out IntPtr pages);
        }


        public enum GroupPolicySection
        {
            Root = 0,
            User = 1,
            Machine = 2,
        }

        public abstract class GroupPolicyObject
        {
            protected const int MaxLength = 1024;

            /// <summary>
            /// The snap-in that processes .pol files
            /// </summary>
            private static readonly Guid RegistryExtension = new Guid(0x35378EAC, 0x683F, 0x11D2, 0xA8, 0x9A, 0x00, 0xC0, 0x4F, 0xBB, 0xCF, 0xA2);

            /// <summary>
            /// This application
            /// </summary>
            private static readonly Guid LocalGuid = new Guid(GetAssemblyAttribute<GuidAttribute>(Assembly.GetExecutingAssembly()).Value);

            protected IGroupPolicyObject Instance = null;

            static T GetAssemblyAttribute<T>(ICustomAttributeProvider assembly) where T : Attribute
            {
                object[] attributes = assembly.GetCustomAttributes(typeof(T), true);
                if (attributes.Length == 0)
                    return null;

                return (T)attributes[0];
            }

            internal GroupPolicyObject()
            {
                Instance = GetInstance();
            }

            public void Save()
            {
                var result = Instance.Save(true, true, RegistryExtension, LocalGuid);
                if (result != 0)
                {
                    throw new Exception("Error saving machine settings");
                }

                result = Instance.Save(false, true, RegistryExtension, LocalGuid);
                if (result != 0)
                {
                    throw new Exception("Error saving user settings");
                }
            }

            public void Delete()
            {
                var result = Instance.Delete();
                if (result != 0)
                {
                    throw new Exception("Error deleting the GPO");
                }
                Instance = null;
            }

            public RegistryKey GetRootRegistryKey(GroupPolicySection section)
            {
                IntPtr key;
                var result = Instance.GetRegistryKey((uint)section, out key);
                if (result != 0)
                {
                    throw new Exception(string.Format("Unable to get section '{0}'", Enum.GetName(typeof(GroupPolicySection), section)));
                }

                var handle = new SafeRegistryHandle(key, true);
                return RegistryKey.FromHandle(handle, RegistryView.Default);
            }

            public abstract string GetPathTo(GroupPolicySection section);

            protected static IGroupPolicyObject GetInstance()
            {
                var concrete = new GPClass();
                return (IGroupPolicyObject)concrete;
            }
        }

        public class GroupPolicyObjectSettings
        {
            public readonly bool LoadRegistryInformation;
            public readonly bool Readonly;

            public GroupPolicyObjectSettings(bool loadRegistryInfo = true, bool readOnly = false)
            {
                LoadRegistryInformation = loadRegistryInfo;
                Readonly = readOnly;
            }

            private const uint RegistryFlag = 0x00000001;
            private const uint ReadonlyFlag = 0x00000002;

            internal uint Flag
            {
                get
                {
                    uint flag = 0x00000000;
                    if (LoadRegistryInformation)
                    {
                        flag |= RegistryFlag;
                    }

                    if (Readonly)
                    {
                        flag |= ReadonlyFlag;
                    }

                    return flag;
                }
            }
        }

        public class ComputerGroupPolicyObject : GroupPolicyObject
        {
            public readonly bool IsLocal;

            public ComputerGroupPolicyObject(GroupPolicyObjectSettings options = null)
            {
                options = options ?? new GroupPolicyObjectSettings();
                var result = Instance.OpenLocalMachineGPO(options.Flag);
                if (result != 0)
                {
                    throw new Exception("Unable to open local machine GPO");
                }
                IsLocal = true;
            }

            public ComputerGroupPolicyObject(string computerName, GroupPolicyObjectSettings options = null)
            {
                options = options ?? new GroupPolicyObjectSettings();
                var result = Instance.OpenRemoteMachineGPO(computerName, options.Flag);
                if (result != 0)
                {
                    throw new Exception(string.Format("Unable to open GPO on remote machine '{0}'", computerName));
                }
                IsLocal = false;
            }

            public static void SetPolicySetting(string registryInformation, string settingValue, RegistryValueKind registryValueKind)
            {
                string valueName;
                GroupPolicySection section;
                string key = Key(registryInformation, out valueName, out section);

                // Thread must be STA
                Exception exception = null;
                var t = new Thread(() =>
                {
                    try
                    {
                        var gpo = new ComputerGroupPolicyObject();
                        using (RegistryKey rootRegistryKey = gpo.GetRootRegistryKey(section))
                        {
                            // Data can't be null so we can use this value to indicate key must be delete
                            if (settingValue == null)
                            {
                                using (RegistryKey subKey = rootRegistryKey.OpenSubKey(key, true))
                                {
                                    if (subKey != null)
                                    {
                                        subKey.DeleteValue(valueName);
                                    }
                                }
                            }
                            else
                            {
                                using (RegistryKey subKey = rootRegistryKey.CreateSubKey(key))
                                {
                                    subKey.SetValue(valueName, settingValue, registryValueKind);
                                }
                            }
                        }

                        gpo.Save();
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }
                });
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                t.Join();

                if (exception != null)
                    throw exception;
            }

            public static object GetPolicySetting(string registryInformation)
            {
                string valueName;
                GroupPolicySection section;
                string key = Key(registryInformation, out valueName, out section);

                // Thread must be STA
                object result = null;
                var t = new Thread(() =>
                {
                    var gpo = new ComputerGroupPolicyObject();
                    using (RegistryKey rootRegistryKey = gpo.GetRootRegistryKey(section))
                    {
                        // Data can't be null so we can use this value to indicate key must be delete
                        using (RegistryKey subKey = rootRegistryKey.OpenSubKey(key, true))
                        {
                            if (subKey == null)
                            {
                                result = null;
                            }
                            else
                            {
                                result = subKey.GetValue(valueName);
                            }
                        }
                    }
                });
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                t.Join();

                return result;
            }

            private static string Key(string registryInformation, out string value, out GroupPolicySection section)
            {
                // Parse parameter of format HKCU\Software\Policies\Microsoft\Windows\Personalization!NoChangingSoundScheme
                string[] split = registryInformation.Split('!');
                string key = split[0];
                string hive = key.Substring(0, key.IndexOf('\\'));
                key = key.Substring(key.IndexOf('\\') + 1);

                value = split[1];

                if (hive.Equals(@"HKLM", StringComparison.OrdinalIgnoreCase)
                    || hive.Equals(@"HKEY_LOCAL_MACHINE", StringComparison.OrdinalIgnoreCase))
                {
                    section = GroupPolicySection.Machine;
                }
                else
                {
                    section = GroupPolicySection.User;
                }
                return key;
            }

            /// <summary>
            ///     Retrieves the file system path to the root of the specified GPO section.
            ///     The path is in UNC format.
            /// </summary>
            public override string GetPathTo(GroupPolicySection section)
            {
                var sb = new StringBuilder(MaxLength);
                var result = Instance.GetFileSysPath((uint)section, sb, MaxLength);
                if (result != 0)
                {
                    throw new Exception(string.Format("Unable to retrieve path to section '{0}'", Enum.GetName(typeof(GroupPolicySection), section)));
                }

                return sb.ToString();
            }
        }
    }
}
