using System;
using System.Collections.Generic;
using System.Text;
using Notifications.Wpf;
using VX_ACE_IT_CORE.Debug;
using VX_ACE_IT_CORE.MVC.Model.Plugins;

namespace VX_ACE_IT_CORE.MVC.Model.Notifications
{
    public class Notifications
    {
        private BaseDebug _baseDebug;

        public NotificationManager NotificationManager;
        public Notifications(BaseDebug baseDebug)
        {
            NotificationManager = new NotificationManager(){};
            _baseDebug = baseDebug;
            baseDebug.OnMessageAdded += OnMessageAdded;
        }

        private void OnMessageAdded(object sender, EventArgs e)
        {
            var msg = sender as Message<object>;
            if (msg?.MessageType == MessageTypeEnum.Exception)
            {
                NotificationManager.Show(new NotificationContent
                {
                    Title = "",
                    Message = msg.MessageContent.ToString(),
                    Type = NotificationType.Error
                }, "", TimeSpan.FromSeconds(5), () => {}, () => {});
            }
            else if (msg?.MessageType == MessageTypeEnum.Event)
            {
                NotificationManager.Show(new NotificationContent
                {
                    Title = "",
                    Message = msg.MessageContent.ToString(),
                    Type = NotificationType.Success
                }, "", TimeSpan.FromSeconds(5), () => { }, () => { });
            }
        }
    }
}
