using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VX_ACE_IT_CORE.MVC.Model.Exceptions
{
    [Serializable]
    public class HandledException : Exception
    {
        public HandledException()
        {

        }

        public HandledException(string message) : base(message)
        {

        }

        public HandledException(string message, Exception inner) : base(message, inner)
        {

        }

        protected HandledException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {

        }
    }
}