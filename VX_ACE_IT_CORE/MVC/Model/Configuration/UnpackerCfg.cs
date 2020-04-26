using System;

namespace VX_ACE_IT_CORE.MVC.Model.Configuration
{
    public class UnpackerCfg
    {
        public string Name { get; set; }
        public Uri Uri { get; set; }
        
        public string[] DefaultParams { get; set; }
    }
}