using System;
using VX_ACE_IT_CORE.MVC.Controller;
using VX_ACE_IT_CORE.MVC._Common;

namespace VX_ACE_IT_CORE
{
    public class Core
    {
        public readonly Controller _controller;

        public Core(Config config)
        {
            _controller = new Controller(config);
        }



    }
}
