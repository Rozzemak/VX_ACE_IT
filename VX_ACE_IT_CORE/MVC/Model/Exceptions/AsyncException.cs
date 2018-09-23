using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VX_ACE_IT_CORE.MVC.Model.Exceptions
{
    [Serializable]
    public class AsyncException : HandledException
    {
        public AsyncException()
        {

        }

        public AsyncException(string message) : base(message)
        {

        }

        public AsyncException(string message, Exception inner) : base(message, inner)
        {

        }

        protected AsyncException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {

        }
    }
}