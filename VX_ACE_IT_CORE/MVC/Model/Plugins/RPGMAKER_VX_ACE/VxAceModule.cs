using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VX_ACE_IT_CORE.Debug;
using VX_ACE_IT_CORE.MVC.Model.Async;
using VX_ACE_IT_CORE.MVC.Model.GameProcess;
using VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE.VX_ACE_TYPES;

namespace VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE
{
    public class VxAceModule : BaseAsync<object>
    {
        private const string ModuleName = "RGSS301.dll";
        private readonly ProcessMethods _processMethods;

        public IntPtr RgssBase;

        public VxAceModule(BaseDebug baseDebug, ProcessMethods processMethods, int precision = 17)
        : base(baseDebug, processMethods._gameProcess, precision)
        {
            _processMethods = processMethods;
            UpdateBaseAddress();
        }

        public void UpdateBaseAddress()
        {
            new Task(() =>
            {
                while (true)
                {
                    // if there is any problem, exchange will return previous value. useful ?
                    Interlocked.Exchange(ref RgssBase, _processMethods._gameProcess.GetModuleAddresByName(ModuleName).BaseAddress);
                    // NOOOOO RgssBase = _processMethods._gameProcess.GetModuleAddresByName(ModuleName).BaseAddress; 
                    // There could be problem, with not enough updates for base adress. time/2 should work then.
                    Thread.Sleep(Precision);
                }
            }).Start();
        }

        // This is useless, can´t think of usefull case.
        public T GetVxNumber<T>(T type) where T : struct
        {
            return new Numeric<T>(type).ActualValue;
        }

    }
}
