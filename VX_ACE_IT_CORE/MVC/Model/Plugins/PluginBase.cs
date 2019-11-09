using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VX_ACE_IT_CORE.Debug;
using VX_ACE_IT_CORE.MVC.Model.Async;
using VX_ACE_IT_CORE.MVC.Model.GameProcess;
using VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE;
using VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE.VX_ACE_TYPES;

namespace VX_ACE_IT_CORE.MVC.Model.Plugins
{
    public class PluginBase : BaseAsync<object>
    {
        private Action _initUpdatablesAction;
        private Task _baseUpdater;

        protected readonly string ModuleName;
        protected readonly ProcessMethods ProcessMethods;
        public List<dynamic> UpdatableTypes = new List<dynamic>(); // Use this for init check ?... like evry 15 sec 
        protected Action InitUpdatablesAction
        {
            get => _initUpdatablesAction;
            set
            {
                if (_initUpdatablesAction == value) return;
                _initUpdatablesAction = value;
                Init(ModuleName, _initUpdatablesAction);
            }
        }

        public IntPtr ModuleBaseAddr;

        public PluginBase(BaseDebug baseDebug, ProcessMethods processMethods, string moduleName, Action initUpdatablesAction, int precision = 29)
        : base(baseDebug, processMethods._gameProcess, precision)
        {
            ProcessMethods = processMethods;
            ModuleName = moduleName;
            UpdateBaseAddress();
            Init(moduleName, initUpdatablesAction);
        }

        protected void Init(string moduleName, Action initUpdatablesAction)
        {
            if (moduleName is null || initUpdatablesAction is null) return;
            InitUpdatablesAction = initUpdatablesAction;
            InitUpdatablesAction?.Invoke();
        }

        public void AddUpdatable<T>(UpdatableType<T> updatableType, bool update = true)
        {
            UpdatableTypes.Add(updatableType);
            if(update) updatableType.BeginUpdatePrimitives(this);
        }

        public void UpdateBaseAddress()
        {
            if (ProcessMethods._gameProcess.GetModuleAddresByName(ModuleName) is null) return;
            _baseUpdater = new Task(() =>
            {
                while (true)
                {
                    Interlocked.Exchange(ref ModuleBaseAddr,
                        ProcessMethods._gameProcess.GetModuleAddresByName(ModuleName).BaseAddress);
                    // There could be problem, with not enough updates for base adress. time/2 should work then.
                    Thread.Sleep(Precision);
                }
            });
            _baseUpdater.Start();
        }
    }
}
