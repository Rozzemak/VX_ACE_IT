using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using VX_ACE_IT_CORE.Debug;
using VX_ACE_IT_CORE.MVC.Model.GameProcess;
using VX_ACE_IT_CORE.MVC.Model.GameWindow;
using VX_ACE_IT_CORE.MVC._Common;

namespace VX_ACE_IT_CORE.MVC.Controller
{
    public class Controller
    {
        private readonly GameWindow _gameWindow;
        private readonly Config _config;

        public readonly GameProcess GameProcess;

        public Controller(BaseDebug debug, Config config)
        {
            this._config = config;
            GameProcess = new GameProcess(debug);
            _gameWindow = new GameWindow(debug, config, GameProcess);
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
