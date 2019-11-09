using System;
using System.Collections.Generic;
using System.Text;
using VX_ACE_IT_CORE.MVC.Model.Interfaces;

namespace VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE.VX_ACE_TYPES
{
    public class Player
    {
        public KeyValuePair<IntPtr,Numeric<int>> Id, Gold, Hp, Mana, PosX, PosY;
        public KeyValuePair<IntPtr,string> Name;
        public KeyValuePair<IntPtr,List<Item>> Items;
        public KeyValuePair<IntPtr,List<Variable<int>>> Variables;

        public Player()
        {

        }

        public override string ToString()
        {
            return this.Stringify();
        }

       
    }
}
