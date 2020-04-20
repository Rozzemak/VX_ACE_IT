using System.Collections.Generic;

namespace VX_ACE_IT_CORE.MVC.Model.Configuration
{
    public class GameWindowCfg
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsWindowBorderVisible { get; set; }
        public bool ForceRes { get; set; }
        public bool KeepAspectRatio { get; set; }
        public IEnumerable<string>? DefaultResolutions { get; set; }
    }
}