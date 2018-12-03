using System;
using System.Collections.Generic;
using System.Text;
using KeyboardInterceptor;
using VX_ACE_IT_CORE.Debug;

namespace VX_ACE_IT_CORE.MVC.Model.Keyboard
{
    public class KeyboardListener
    {

        public KeyboardListener(BaseDebug debug)
        {
            var interceptor = new Interceptor(key => {
                debug.AddMessage<object>(new Message<object>(nameof(key)));
            });
            interceptor.Start();
        }

    }
}
