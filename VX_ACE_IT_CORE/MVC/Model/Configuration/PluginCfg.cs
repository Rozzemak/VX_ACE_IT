using System.Collections.Generic;

namespace VX_ACE_IT_CORE.MVC.Model.Configuration
{
    public class PluginCfg
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public int DefaultPrecision { get; set; }
        public GameProcessCfg GameProcessCfg { get; set; }
        public MemoryManipulationCfg MemoryManipulationCfg { get; set; }
        public UnpackersCfg UnpackersCfg { get; set; }
    }
}