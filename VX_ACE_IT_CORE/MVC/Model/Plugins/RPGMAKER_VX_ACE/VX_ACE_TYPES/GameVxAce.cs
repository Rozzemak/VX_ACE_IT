using System;
using System.Collections.Generic;
using System.Text;
using VX_ACE_IT_CORE.MVC.Model.Interfaces;

namespace VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE.VX_ACE_TYPES
{
    public class GameVxAce
    {
        /// <summary>
        /// 1 DebugOn
        /// 2 DebugOff
        /// 3 FromBase class
        /// </summary>
        public KeyValuePair<IntPtr, Numeric<int>> Debug;
        public GameVxAce() { }

        public override string ToString()
        {
            return this.Stringify();
        }


    }
}
