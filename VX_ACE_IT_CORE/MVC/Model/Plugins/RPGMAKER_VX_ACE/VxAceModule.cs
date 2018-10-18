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

        public UpdatableType<Player> PlayerUpdatable;

        public VxAceModule(BaseDebug baseDebug, ProcessMethods processMethods, int precision = 17)
        : base(baseDebug, processMethods._gameProcess, precision)
        {
            _processMethods = processMethods;
            UpdateBaseAddress();

            InitUpdatables(baseDebug, processMethods, precision);
        }

        public void InitUpdatables(BaseDebug debug, ProcessMethods processMethods, int precision)
        {
            PlayerUpdatable = new UpdatableType<Player>(debug, processMethods,
                new Player(), new Dictionary<string, List<List<IntPtr>>>()
                {
                    {"Hp", new List<List<IntPtr>>()
                    {
                        new List<IntPtr>(){ new IntPtr(0x25A8B0), new IntPtr(0x30), new IntPtr(0x18), new IntPtr(0x20), new IntPtr((0x38))},
                        new List<IntPtr>(){}, // <- other possible offset value if multipointer rpm fails.
                    }}, // <- Do this for each field. Also, if we happen to have more fields than editable values.. 
                    // 2 solutions, 1a) do not do it -> Just create another type like DrawablePlayer or something.
                    // 1b) Create readable field name rule like 'FieldName_engineStat' 
                    {"Mana", new List<List<IntPtr>>()
                    {
                        new List<IntPtr>(){ new IntPtr(0x25A8B0), new IntPtr(0x30), new IntPtr(0x18), new IntPtr(0x20), new IntPtr((0x38+0x4))},
                    }},
                }, this);
        }

        public void UpdateBaseAddress()
        {
            if (!(_processMethods._gameProcess.GetModuleAddresByName(ModuleName) is null))
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
