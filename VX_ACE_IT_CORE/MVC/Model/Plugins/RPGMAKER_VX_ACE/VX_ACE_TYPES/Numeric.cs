using System;
using System.Linq;

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

        // Used for serialisation
        private Numeric()
        {

        }

        public Numeric(T value, bool actualValue = false)
        {
            if (!actualValue)
                EngineValue = value;
            else EngineValue = (value as dynamic) * 2 + 1;
        }

        public override string ToString()
        {
            var s = GetType().GetFields().Aggregate("{", (current, field) => current + ("[" + field.Name + ":" + GetType().GetField(field.Name).GetValue(this) + "]"));

            s = GetType().GetMethods().Where(info => info.IsSpecialName).Aggregate(s, (current, method) => current + ("[" + method.Name + ":" + GetType().GetMethod(method.Name)?.Invoke(this, method?.GetParameters()) + "]"));


            s += "}";

            return s;
        }
    }
}
