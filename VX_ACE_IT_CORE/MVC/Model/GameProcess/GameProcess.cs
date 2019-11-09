using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VX_ACE_IT_CORE.Debug;

namespace VX_ACE_IT_CORE.MVC.Model.GameProcess
{
    public class GameProcess
    {
        private readonly BaseDebug _debug;
        private Process _process { get; set; }

        public event EventHandler OnProcessFound;
        public event EventHandler OnNoProcessFound;
        public event EventHandler OnKill;

        public GameProcess(BaseDebug debug)
        {
            _debug = debug;
        }

        public GameProcess(BaseDebug debug, Process process)
        {
            if (Process == null)
            {
                throw new ArgumentNullException(nameof(Process));
            }

            this._process = process;
        }

        public Process Process
        {
            get
            {
                if (IsProcessFetched()) return _process;
                FetchProcess();
                return _process;
            }
        }

        public void FetchProcess(string name = "game")
        {
            int.TryParse(name.Split('_').Last(), out var pId);
            if (pId == 0)
            {
                if (Process.GetProcessesByName(name).Length > 0)
                    _process = Process.GetProcessesByName(name).First() ??
                               throw new Exception("No process found with name: [" + name + "].");
                else
                {
                    _debug.AddMessage<object>(new Message<object>(
                        "[" + GetType().Name + "] Process: " + "name: [" + name + "]" + " not found.",
                        MessageTypeEnum.Exception));
                    // MessageBox.Show("No process named [" + name + "] found."); // Annoying
                }

                if (IsProcessFetched())
                {
                    OnProcessFound?.Invoke(this, EventArgs.Empty);
                    WatchProcessState();
                    _debug.AddMessage<object>(new Message<object>(
                        "[" + GetType().Name + "] Process: " + "name: [" + name + "]" + " id: [" + Process.Id +
                        "] fetched.", MessageTypeEnum.Event));
                }
                else
                {
                    OnNoProcessFound?.Invoke(this, EventArgs.Empty);
                    _debug.AddMessage<object>(new Message<object>(
                        "[" + GetType().Name + "] Process: " + "name: [" + name + "]" + " NOT fetched.",
                        MessageTypeEnum.Event));
                }
            }
            else
            {
                _process = Process.GetProcessById(pId);
                if (IsProcessFetched())
                {
                    OnProcessFound?.Invoke(this, EventArgs.Empty);
                    WatchProcessState();
                    _debug.AddMessage<object>(new Message<object>(
                        "[" + GetType().Name + "] Process: " + "name: [" + Process.ProcessName + "]" + " id: [" + Process.Id +
                        "] fetched.", MessageTypeEnum.Event));
                }
                else
                {
                    OnNoProcessFound?.Invoke(this, EventArgs.Empty);
                    _debug.AddMessage<object>(new Message<object>(
                        "[" + GetType().Name + "] Process: " + "id: [" + pId + "]" + " NOT fetched.",
                        MessageTypeEnum.Event));
                }
            }
        }

        private void WatchProcessState()
        {
            Process.EnableRaisingEvents = true;
            Process.Exited += ProcessOnExited;
        }

        private void ProcessOnExited(object sender, EventArgs eventArgs)
        {
            OnKill?.Invoke(this, EventArgs.Empty);
        }

        public Module GetModuleAddresByName(string name, bool is32Bit = true)
        {
            return CollectModules(Process)
                 .FirstOrDefault(module => module.ModuleName.ToLower().Equals(name.ToLower()));
        }

        public bool IsProcessFetched()
        {
            return _process != null;
        }

        public List<Module> CollectModules(Process process)
        {
            List<Module> collectedModules = new List<Module>();

            IntPtr[] modulePointers = new IntPtr[0];
            int bytesNeeded = 0;

            // Determine number of modules
            if (!Native.EnumProcessModulesEx(process.Handle, modulePointers, 0, out bytesNeeded, (uint)Native.ModuleFilter.ListModulesAll))
            {
                return collectedModules;
            }

            int totalNumberofModules = bytesNeeded / IntPtr.Size;
            modulePointers = new IntPtr[totalNumberofModules];

            // Collect modules from the process
            if (Native.EnumProcessModulesEx(process.Handle, modulePointers, bytesNeeded, out bytesNeeded, (uint)Native.ModuleFilter.ListModulesAll))
            {
                for (int index = 0; index < totalNumberofModules; index++)
                {
                    StringBuilder moduleFilePath = new StringBuilder(1024);
                    Native.GetModuleFileNameEx(process.Handle, modulePointers[index], moduleFilePath, (uint)(moduleFilePath.Capacity));

                    string moduleName = Path.GetFileName(moduleFilePath.ToString());
                    Native.ModuleInformation moduleInformation = new Native.ModuleInformation();
                    Native.GetModuleInformation(process.Handle, modulePointers[index], out moduleInformation, (uint)(IntPtr.Size * (modulePointers.Length)));

                    // Convert to a normalized module and add it to our list
                    Module module = new Module(moduleName, moduleInformation.lpBaseOfDll, moduleInformation.SizeOfImage);
                    collectedModules.Add(module);
                }
            }

            return collectedModules;
        }
    }

    public class Native
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct ModuleInformation
        {
            public IntPtr lpBaseOfDll;
            public uint SizeOfImage;
            public IntPtr EntryPoint;
        }

        internal enum ModuleFilter
        {
            ListModulesDefault = 0x0,
            ListModules32Bit = 0x01,
            ListModules64Bit = 0x02,
            ListModulesAll = 0x03,
        }

        [DllImport("psapi.dll")]
        public static extern bool EnumProcessModulesEx(IntPtr hProcess, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)] [In][Out] IntPtr[] lphModule, int cb, [MarshalAs(UnmanagedType.U4)] out int lpcbNeeded, uint dwFilterFlag);

        [DllImport("psapi.dll")]
        public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In] [MarshalAs(UnmanagedType.U4)] uint nSize);

        [DllImport("psapi.dll", SetLastError = true)]
        public static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, out ModuleInformation lpmodinfo, uint cb);
    }

    public class Module
    {
        public Module(string moduleName, IntPtr baseAddress, uint size)
        {
            this.ModuleName = moduleName;
            this.BaseAddress = baseAddress;
            this.Size = size;
        }

        public string ModuleName { get; set; }
        public IntPtr BaseAddress { get; set; }
        public uint Size { get; set; }
    }
}
