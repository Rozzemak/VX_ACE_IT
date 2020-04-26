using System.Collections.Generic;

namespace VX_ACE_IT_CORE.MVC.Model.Configuration
{
    public class PluginsCfg
    {
        public string DefaultPath { get; set; }
        public string DefaultProcessName { get; set; }
        public string DefaultPluginName { get; set; }
        public bool CheckForUpdateUnpackers { get; set; }
        public bool CheckForUpdatePatchers { get; set; }
        public List<PluginCfg> Plugins { get; set; }
    }
}