using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE.VX_ACE_TYPES
{
    public class Numeric<T> where T : struct
    {
        public T EngineValue;

        /// <summary>
        /// Actual value of type defined in engine. Resolved at runtime.
        /// Must be numeric.
        /// </summary>
        public T ActualValue => EngineValue / (2 as dynamic);

        public Numeric(T value, bool actualValue = false)
        {
            if(!actualValue)
            this.EngineValue = value;
            else this.EngineValue = (value as dynamic) * 2 + 1;
        }

        public override string ToString()
        {
            string s = "{";

            foreach (var field in GetType().GetFields())
            {
                s += "[" + field.Name + ":" + this.GetType().GetField(field.Name).GetValue(this) + "]";
            }

            foreach (var method in GetType().GetMethods().Where(info => info.IsSpecialName))
            {
                s += "[" + method.Name + ":" + GetType().GetMethod(method.Name)?.Invoke(this, method?.GetParameters()) + "]";
            }


            s += "}";

            return s;
        }
    }
}
