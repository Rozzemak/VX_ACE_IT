using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using VX_ACE_IT_CORE.Debug;
using VX_ACE_IT_CORE.MVC.Model.GameProcess;
using VX_ACE_IT_CORE.MVC.Model.GameWindow;
using VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE;
using VX_ACE_IT_CORE.MVC._Common;

namespace VX_ACE_IT_CORE.MVC.Controller
{
    public class Controller
    {
        private readonly BaseDebug debug;
        private readonly GameWindow _gameWindow;
        private readonly Config _config;

        public readonly GameProcess GameProcess;
        public ProcessMethods ProcessMethods;

        #region Modules
        // Reminder, modules should only use a ProcessMethods,debug, precision.
        public VxAceModule VxAceModule;

        #endregion

        public Controller(BaseDebug debug, Config config)
        {
            this.debug = debug;
            this._config = config;
            GameProcess = new GameProcess(debug);
            _gameWindow = new GameWindow(debug, config, GameProcess);

            GameProcess.OnNoProcessFound+= GameProcessOnOnNoProcessFound;
            GameProcess.OnProcessFound += GameProcessOnOnProcessFound;
        }

        private void GameProcessOnOnProcessFound(object sender, EventArgs eventArgs)
        {
            ProcessMethods = new ProcessMethods(GameProcess);
            InitModules();
        }

        private void GameProcessOnOnNoProcessFound(object sender, EventArgs eventArgs)
        {
            ProcessMethods = null;
        }

        private void InitModules()
        {
            // Impl action ?
            VxAceModule = new VxAceModule(debug, ProcessMethods, null, 17);
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
