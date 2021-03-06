﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using VX_ACE_IT_CORE.Debug;
using VX_ACE_IT_CORE.MVC.Model.GameProcess;
using VX_ACE_IT_CORE.MVC.Model.GameWindow;
using VX_ACE_IT_CORE.MVC.Model.Keyboard;
using VX_ACE_IT_CORE.MVC.Model.Offsets;
using VX_ACE_IT_CORE.MVC.Model.Plugins;
using VX_ACE_IT_CORE.MVC.Model.Plugins.GLOBAL_TYPES;
using VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE;
using VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE.VX_ACE_TYPES;
using VX_ACE_IT_CORE.MVC._Common;
using VX_ACE_IT_CORE.MVC.Model.Plugins.Interfaces;
using PluginBase = VX_ACE_IT_CORE.MVC.Model.Plugins.PluginBase;

namespace VX_ACE_IT_CORE.MVC.Controller
{
    public class Controller : BaseController
    {
        private readonly BaseDebug _debug;
        private readonly GameWindow _gameWindow;
        private readonly Config _config;
        private readonly Model.Notifications.Notifications _notifications;

        public readonly GameProcess GameProcess;
        public ProcessMethods? ProcessMethods;

        public PluginService? PluginService;

        //public GameOverlayPlugin GameOverlayPlugin;

        public KeyboardListener Keyboard;

        public Controller(IServiceProvider serviceProvider, BaseDebug debug, Config config) : base(serviceProvider)
        {
            this._debug = debug;
            this._notifications = new Model.Notifications.Notifications(debug);
            this._config = config;
            GameProcess = new GameProcess(debug);
            _gameWindow = new GameWindow(debug, config, GameProcess);
            GameProcess.OnNoProcessFound += GameProcessOnOnNoProcessFound;
            GameProcess.OnProcessFound += GameProcessOnOnProcessFound;
        }

        private void GameProcessOnOnProcessFound(object sender, EventArgs eventArgs)
        {
            ProcessMethods = new ProcessMethods(GameProcess);
            InitPlugins();
            InitOverlay();
            Keyboard = new KeyboardListener(_debug);
        }

        private void GameProcessOnOnNoProcessFound(object sender, EventArgs eventArgs)
        {
            ProcessMethods = null;
            PluginService = null;
        }

        private void InitPlugins()
        {
            this.PluginService = new PluginService(this._debug, this.GameProcess, new List<IPluginBase>() { new VxAceModule(this._debug,  ServiceProvider, ProcessMethods, null) }, 33);
        }

        private void InitOverlay()
        {
            //GameOverlayPlugin = new GameOverlayPlugin();
            //GameOverlayPlugin.StartDemo(this.GameProcess.Process, Dispatcher.CurrentDispatcher);
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
