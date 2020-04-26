using System;

namespace VX_ACE_IT_CORE.MVC.Model.Configuration
{
    public class ExternalAppCfg
    {
        public string Name { get; set; }
        public Uri Uri { get; set; }
        
        public string[] DefaultParams { get; set; }
        
        public bool OnlyGuiAvailable { get; set; }
        
        public bool AutomaticallyDownload { get; set; }
        
        public bool CheckForUpdates { get; set; }
        
        public bool RunInOwnConsole { get; set; }
        
        public bool RedirectConsoleOutput { get; set; }
        
        public bool Debug { get; set; }
    }
}