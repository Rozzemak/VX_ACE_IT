using System;
using System.Collections.Generic;
using System.Text;
using VX_ACE_IT_CORE.Debug;
using VX_ACE_IT_CORE.MVC.Model.Async;

namespace VX_ACE_IT_CORE.MVC.Model.Plugins
{
    public class PluginService : BaseAsync<PluginBase>
    {
         
        // Make service coll public, so we can grab it elsewhere. (Temporary ?)
        public List<PluginBase> Plugins => this.ServiceCollection;

        public PluginService(BaseDebug debug, GameProcess.GameProcess gameProcess, int precision = 33) 
            : base(debug, gameProcess, precision)
        {
           
        }
    }
}
