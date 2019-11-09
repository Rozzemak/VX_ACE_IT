using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;

namespace VX_ACE_IT_CORE.MVC.Model.Interfaces
{
    public static class Stringifiable
    {
        public static string Stringify(this object obj)
        {
            var s = "\n----[" + obj.GetType().Name + "]----\n";
            if (!(obj is ExpandoObject))
            {
                lock (obj)
                {
                    s += "{";
                    foreach (var field in obj.GetType().GetFields())
                    {
                        var val = obj.GetType().GetField(field.Name).GetValue(obj);
                        s += "[" + field.Name + ":" + StringifyObj(val) + "]\n";
                    }
                }
                s += "}";
            }
            else
            {
                // This is nearly the same procedure as above, but we have 
                // to think about destructuring of dyn. types and how to handle them without 
                // changing the existing procedural code functionality. 
                var dict = (IDictionary<string, object>)obj;
                object objTemp = new ExpandoObject();
                Interlocked.Exchange(ref objTemp, obj);
                s += "{\n";
                s = ((ExpandoObject) objTemp).Where(entry => !entry.Key.ToLower().Contains("ToString".ToLower())).Aggregate(s, (current, entry) => current + ("[" + StringifyObj(entry) + "]\n"));
                s += "}";
            }
            return s;
        }

        public static string StringifyObj(dynamic obj)
        {
            var s = "";
            try
            {
                if(obj?.Key is string)
                    s += "{("+ obj?.Key + ":0x" + ((obj)?.Value)?.Key?.ToString("X") + ")";
                else
                    s += "{(0x" + obj?.Key.ToString("X") + ")";
            }
            catch
            {
                // ignored
            }

            if (s == "")
                s += "{("+ ((obj)?.Key)?.ToString() +")";
            s += "(Val:" + (obj)?.Value?.ToString() + ")}";
            return s;
        }
    }
}
