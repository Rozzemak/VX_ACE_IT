using System;
using System.Collections.Generic;
using System.Text;

namespace VX_ACE_IT_CORE.MVC.Model.Interfaces
{
    public static class Stringifiable
    {
        public static string Stringify(this object obj) 
        {
            string s = "\n----[" + obj.GetType().Name + "]----\n";
            s += "{";
            foreach (var field in obj.GetType().GetFields())
            {
                var val = obj.GetType().GetField(field.Name).GetValue(obj);
                s += "[" + field.Name + ":" + StringifyObj(val) + "]\n";
            }

            s += "}";

            return s;
        }

        public static string StringifyObj(dynamic obj)
        {
            var s = "";
            s += "{(Adr:" + (obj).Key.ToString("X") + ")";
            s += "(Val:" + (obj).Value?.ToString() + ")}";
            return s;
        }
    }
}
