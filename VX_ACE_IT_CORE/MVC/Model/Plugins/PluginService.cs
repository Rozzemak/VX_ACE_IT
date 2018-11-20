﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VX_ACE_IT_CORE.Debug;
using VX_ACE_IT_CORE.MVC.Model.Async;
using VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE;

namespace VX_ACE_IT_CORE.MVC.Model.Plugins
{
    public class PluginService : BaseAsync<PluginBase>
    {
         
        // Make service coll public, so we can grab it elsewhere. (Temporary ?)
        public List<PluginBase> Plugins => this.ServiceCollection;

        public PluginService(BaseDebug debug, GameProcess.GameProcess gameProcess, List<PluginBase> pluginBases, int precision = 33) 
            : base(debug, gameProcess, precision)
        {            
            this.ServiceCollection = pluginBases;
            DebugTest();
        }

        void DebugTest()
        {
            new Task(() =>
            {
                Debug.AddMessage<object>(new Message<object>("Updatables init test began."));
                //Plugins.ForEach(bs => bs.UpdatableTypes.ForEach(o => o.BeginUpdatePrimitives(bs)));
                while (false)
                {
                    Thread.Sleep(Precision*10);
                    Plugins.ForEach(bs => bs.UpdatableTypes.ForEach(o => Debug.AddMessage<object>(new Message<object>(o.Type.ToString()))));
                }

            });
            
        }
    }
}
