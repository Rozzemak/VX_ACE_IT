using System;
using System.Collections.Generic;
using System.Text;

namespace VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE.VX_ACE_TYPES
{
    public class Player
    {
        public KeyValuePair<IntPtr,Numeric<int>> Id, Hp, Mana;
        public KeyValuePair<IntPtr,string> Name;
        public KeyValuePair<IntPtr,List<Item>> Items;
        public KeyValuePair<IntPtr,List<Variable<int>>> Variables;

        public Player()
        {

        }

        public override string ToString()
        {
            string s = "\n----[" + GetType().Name +"]----\n";
            s += "{";
            foreach (var field in GetType().GetFields())
            {
                 var val = this.GetType().GetField(field.Name).GetValue(this);
                s += "[" + field.Name + ":" + Stringify(val) + "]\n";
            }

            s += "}";

            return s;
        }

        public string Stringify(dynamic obj)
        {
            string s = "";

            s += "{(Adr:" + (obj).Key.ToString("X")+")";
            s += "(Val:" + (obj).Value?.ToString() + ")}";
            return s;
        }
    }
}
