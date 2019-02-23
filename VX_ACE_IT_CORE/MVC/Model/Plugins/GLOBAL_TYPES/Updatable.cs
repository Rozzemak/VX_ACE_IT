using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using VX_ACE_IT_CORE.MVC.Model.Interfaces;

namespace VX_ACE_IT_CORE.MVC.Model.Plugins.GLOBAL_TYPES
{

    /// <summary>
    /// Updatable decorator, has to use OffsetLoader to load 
    /// undefined object params, then creates said object at runtime.
    /// If type is defined, lets use this too, but in defined manner.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Updatable<T>
    {
        /// <summary>
        /// Type only usable for paramless constructors. Kappa
        /// </summary>
        private readonly dynamic _expando = new ExpandoObject();

        public T GetUpdatable => (T)_expando;

        /// <summary>
        /// If type is undefined, then lets create at least props for get / set.
        /// </summary>
        /// <param name="props">Props of undefined object from config file.</param>
        public Updatable(IEnumerable<dynamic> props)
        {
            InitType<T>(default(T), props);
        }

        /// <summary>
        /// Use action as method for defined types.
        /// </summary>
        /// <param name="type">If said type is defined, use it.</param>
        public Updatable(T type)
        {
            InitType(type);
        }

        /// <summary>
        /// Iterates trough type properties, adding them to expando class member.
        /// Seems extremely wonky and i doubt this would work, but let´s see.
        /// </summary>
        /// <param name="type">Defined type</param>
        /// <param name="props">Props of expandoObject</param>
        private void InitType<TPType>(TPType type = default (TPType), IEnumerable<dynamic> props = null)
        {
            var dictionary = (IDictionary<string, object>)_expando;
            //foreach (var property in type.GetType().GetProperties())
              //  dictionary.Add(property.Name, property.GetValue(type));
            if (props == null)
            {
                foreach (var field in type.GetType().GetFields())
                    dictionary.Add(field.Name, field.GetValue(type));
                //foreach (var method in GetType().GetMethods().Where(info => info.IsSpecialName))
                //  dictionary.Add(method.Name, method);  
                // Force _expepando to use my Stringifiable...
                // Alternative ? Delegate.CreateDelegate(typeof(...), methodInfo)
            }
            else
            {
                foreach (var field in props)
                    dictionary.Add(field, new object());
            }
            dictionary.Add("ToString", new Func<string>(() => (_expando as object).Stringify()));
        }

        public override string ToString()
        {
            return _expando.ToString();
        }
    }
}
