using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VX_ACE_IT_CORE.Debug;

namespace VX_ACE_IT_CORE.MVC.Model.GameProcess
{
    public class GameProcess
    {
        private readonly BaseDebug _debug;
        private System.Diagnostics.Process _process { get;  set; }

        public event EventHandler OnProcessFound;
        public event EventHandler OnNoProcessFound;
        public event EventHandler OnKill;

        public GameProcess(BaseDebug debug)
        {
            _debug = debug;
        }

        public GameProcess(BaseDebug debug,  System.Diagnostics.Process process)
        {
            if (Process == null)
            {
                throw new ArgumentNullException(nameof(Process));
            }

            this._process = process;
        }

        public System.Diagnostics.Process Process
        {
            get
            {
                if (!IsProcessFetched())
                {
                    FetchProcess();
                    return _process;
                }
                return _process;
            }
        }

        public void FetchProcess(string name = "game")
        {
            int.TryParse(name.Split('_').Last(), out int pId);
            if (pId == 0)
            {
                if (System.Diagnostics.Process.GetProcessesByName(name).Length > 0)
                    _process = System.Diagnostics.Process.GetProcessesByName(name).First() ??
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
                _process = System.Diagnostics.Process.GetProcessById(pId);
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

        public ProcessModule GetModuleAddresByName(string name, bool is32Bit = true)
        {
            if(is32Bit)
            foreach (ProcessModule module in _process.Modules)
            {
                if (module.ModuleName == name) return module;
            }
            return null;
        }

        public bool IsProcessFetched()
        {
            return _process != null;
        }


    }
}
