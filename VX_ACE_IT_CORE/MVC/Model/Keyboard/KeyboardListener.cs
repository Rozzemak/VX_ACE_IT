using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Open.WinKeyboardHook;
using VX_ACE_IT_CORE.Debug;

namespace VX_ACE_IT_CORE.MVC.Model.Keyboard
{
    public class KeyboardListener
    {
        public readonly IKeyboardInterceptor Interceptor;

        private EventHandler<KeyEventArgs> _debugKeys;


        public KeyboardListener(BaseDebug debug)
        {
            InitEvents(debug);
            Interceptor = new KeyboardInterceptor();
            Interceptor.KeyDown += _debugKeys;
            Interceptor?.StartCapturing();

        }

        private void InitEvents(BaseDebug debug)
        {
            _debugKeys = (sender, args) => debug.AddMessage<object>(
                new Message<object>("Key: [" + args.KeyCode + "] handled:[" + args.Handled.ToString() + "]"));
        }

        public void ResetEvents()
        {
            _debugKeys = null;
        }

        ~KeyboardListener()
        {
            Interceptor?.StopCapturing();
        }
    }
}
