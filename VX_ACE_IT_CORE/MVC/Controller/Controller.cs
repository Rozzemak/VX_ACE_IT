using System;
using System.Collections.Generic;
using System.Text;
using VX_ACE_IT_CORE.MVC.Model.GameProcess;
using VX_ACE_IT_CORE.MVC.Model.GameWindow;
using VX_ACE_IT_CORE.MVC._Common;

namespace VX_ACE_IT_CORE.MVC.Controller
{
    public  class Controller
    {
        private readonly GameWindow _gameWindow;

        public Controller(Config config)
        {
            _gameWindow = new GameWindow(new GameProcess(config.ConfigVariables.ProcessName), config);
        }

        public void SetWindowPosFromConfig()
        {
            _gameWindow.SetWindowFromConfig();
        }

        public void SetWindowStyle(WindowStyles windowStyles)
        {
            switch (windowStyles)
            {
                case WindowStyles.Border:
                    _gameWindow.SetWindowStyle();
                    break;
                case WindowStyles.NoBorder:
                    _gameWindow.SetWindowStyleBorder();
                    break;
            }
        }
    }
}
