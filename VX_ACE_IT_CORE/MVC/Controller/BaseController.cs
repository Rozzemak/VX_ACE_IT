using System;

namespace VX_ACE_IT_CORE.MVC.Controller
{
    public class BaseController
    {
        public readonly IServiceProvider ServiceProvider;

        public BaseController(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }
        
    }
}