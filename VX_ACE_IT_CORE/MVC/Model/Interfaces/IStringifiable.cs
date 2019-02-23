using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading;

namespace VX_ACE_IT_CORE.MVC.Model.Interfaces
{
    public static class Stringifiable
    {
        public static string Stringify(this object obj)
        {
            string s = "\n----[" + obj.GetType().Name + "]----\n";
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
                s += "{";
                foreach (var entry in ((ExpandoObject)objTemp))
                {
                    if (entry.Key == "ToString") continue;
                    var val = dict.GetType().GetField(entry.Key)?.GetValue(obj);
                    s += "[" + entry + ":" + StringifyObj(val) + "]\n";
                }
                s += "}";
            }
            return s;
        }

        public static string StringifyObj(dynamic obj)
        {
            var s = "";
            s += "{(Adr:" + (obj)?.Key.ToString("X") + ")";
            s += "(Val:" + (obj)?.Value?.ToString() + ")}";
            return s;
        }
    }
}
