using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VX_ACE_IT_CORE.Debug
{
    [Serializable]
    public class DebugException : Exception
    {
        public DebugException()
        {
        }

        public DebugException(string message) : base(message)
        {
        }

        public DebugException(string message, Exception inner) : base(message, inner)
        {
        }

        protected DebugException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    } 


}
