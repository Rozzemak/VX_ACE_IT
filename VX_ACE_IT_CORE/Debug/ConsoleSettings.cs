using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VX_ACE_IT_CORE.Debug
{
    class ConsoleSettings
    {
        private readonly int _bufferHeight;
        private readonly int _bufferWidth;
        private readonly Encoding _encoding;

        public ConsoleSettings(Encoding encoding,int bufferWidth= 100, int bufferHeight = Int16.MaxValue-2)
        {
            this._encoding = encoding;
            this._bufferWidth = bufferWidth;
            this._bufferHeight = bufferHeight;
            ApplyConsoleSettings();
        }

        public void ApplyConsoleSettings()
        {
            Console.BufferHeight = _bufferHeight;
            // bug: Throws exception on my main pc. Idk why, maybe win10 1803 build ? 
            //Console.BufferWidth = _bufferWidth;
            Console.OutputEncoding = _encoding;
        }
    }
}
