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
    public class VxAceModule : PluginBase
    {
        public VxAceModule(BaseDebug baseDebug, ProcessMethods processMethods, Action updatables, int precision = 17)
        : base(baseDebug, processMethods, "RGSS301.dll", updatables, precision)
        {
            if (updatables is null)
            {
                InitUpdatables();
            }
        }

        public void InitUpdatables()
        {
            var action = new Action(() =>
            {
                List<List<IntPtr>> lists = new List<List<IntPtr>>();
                for (int i = -10; i > 10; i++)
                {
                    lists.Add(new List<IntPtr>()
                    {
                        new IntPtr(0x25A8B0),
                        new IntPtr(0x30),
                        new IntPtr(0x18),
                        new IntPtr(0x20),
                        new IntPtr((0x38) + ((0x4)*i))
                    });
                }
                lists.Add(new List<IntPtr>()
                {
                    new IntPtr(0x25A8B0),
                    new IntPtr(0x30),
                    new IntPtr(0x18),
                    new IntPtr(0x20),
                    new IntPtr((0x38))
                });

                var playerUpdatable = new UpdatableType<Player>(this.Debug, this.ProcessMethods,
                    new Player(), new Dictionary<string, List<List<IntPtr>>>()
                    {
                        {
                            "Hp", lists
                        }, // <- Do this for each field. Also, if we happen to have more fields than editable values.. 
                        // 2 solutions, 1a) do not do it -> Just create another type like DrawablePlayer or something.
                        // 1b) Create readable field name rule like 'FieldName_engineStat' 
                        {
                            "Mana", new List<List<IntPtr>>()
                            {
                                new List<IntPtr>()
                                {
                                    new IntPtr(0x25A8B0),
                                    new IntPtr(0x30),
                                    new IntPtr(0x18),
                                    new IntPtr(0x20),
                                    new IntPtr((0x38 - (0x4)))
                                },
                            }
                        },
                    }, this);
                //playerUpdatable.BeginUpdatePrimitives(this);
                this.UpdatableTypes.Add(playerUpdatable);
            });

            InitUpdatablesAction = action;
        }

        // This is useless, can´t think of usefull case.
        public T GetVxNumber<T>(T type) where T : struct
        {
            return new Numeric<T>(type).ActualValue;
        }

    }
}
