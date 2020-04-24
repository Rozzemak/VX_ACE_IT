using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VX_ACE_IT_CORE.Debug
{
    public enum MessageTypeEnum
    {
        Standard,
        Warning,
        Error,
        Exception,
        Indifferent,
        DefaultWriteAll,
        Event,
        Rest,
        HttpClient,
        ________
    }

    public class Message<T>
    {
        public T MessageContent;
        public MessageTypeEnum MessageType;
        public bool shown;

        public Message(T messageContent, MessageTypeEnum messageType = MessageTypeEnum.Standard)
        {
            MessageContent = messageContent;
            MessageType = messageType;
        }

    }
}