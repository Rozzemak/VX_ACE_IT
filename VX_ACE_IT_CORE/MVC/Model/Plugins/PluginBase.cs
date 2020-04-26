using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VX_ACE_IT_CORE.Debug;
using VX_ACE_IT_CORE.MVC.Model.Async;
using VX_ACE_IT_CORE.MVC.Model.GameProcess;
using VX_ACE_IT_CORE.MVC.Model.Plugins.Interfaces;
using VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE;
using VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE.VX_ACE_TYPES;
using VX_ACE_IT_CORE.MVC.Model.TargetApp.Interfaces;

namespace VX_ACE_IT_CORE.MVC.Model.Plugins
{
    public class PluginBase : BaseAsync<object>, IPluginBase
    {
        private Action _initUpdatablesAction;
        private Task _baseUpdater;

        protected readonly string ModuleName;
        protected readonly ProcessMethods ProcessMethods;
        public readonly List<dynamic> UpdatableTypes = new List<dynamic>(); // Use this for init check ?... like evry 15 sec 
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


        #region PublicServices
        public readonly ITargetFileService FileService;
        public readonly ITargetPatchService PatchService;
        public readonly ITargetUnpackerService UnpackerService;
        public readonly ITargetVersionCheckService VersionCheckService;
        #endregion
        
        protected PluginBase(BaseDebug baseDebug, IServiceProvider serviceProvider, ProcessMethods processMethods, string moduleName, Action initUpdatablesAction, int precision = 29)
        : base(baseDebug, processMethods._gameProcess, precision)
        {
            FileService = serviceProvider.GetService<ITargetFileService>();
            PatchService = serviceProvider.GetService<ITargetPatchService>();
            UnpackerService = serviceProvider.GetService<ITargetUnpackerService>();
            VersionCheckService = serviceProvider.GetService<ITargetVersionCheckService>();
            ProcessMethods = processMethods;
            ModuleName = moduleName;
            UpdateBaseAddress();
            Init(moduleName, initUpdatablesAction);
        }

        public void Init(string moduleName, Action initUpdatablesAction)
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
            }, TaskCreationOptions.LongRunning);
            _baseUpdater.Start();
        }
    }
}
