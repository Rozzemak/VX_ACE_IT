using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace VX_ACE_IT_CORE.MVC.Model.GameProcess
{
    class GameProcess
    {
        public Process Process { get; }

        public GameProcess(string name = "game")
        {
            Process = Process.GetProcessesByName(name).First() ?? throw new Exception("No process found with name: " + name + ".");
        }


    }
}
