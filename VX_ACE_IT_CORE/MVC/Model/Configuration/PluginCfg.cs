using System.Collections.Generic;

namespace VX_ACE_IT_CORE.MVC.Model.Configuration
{
    public class PluginCfg
    {
        public string Name { get; set; }
        public string DefaultProcessName { get; set; }
        public string Path { get; set; }
        public List<UnpackerCfg> Unpackers { get; set; }
    }
}