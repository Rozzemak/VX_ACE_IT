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
    internal class Updatable<T>
    {
        /// <summary>
        /// Type only usable for paramless constructors. Kappa
        /// </summary>
        private readonly dynamic _expando = new ExpandoObject();

        /// <summary>
        /// If type is undefined, then lets create at least props for get / set.
        /// </summary>
        /// <param name="props">Props of undefined object from config file.</param>
        /// <param name="type">If said type is defined, use it.</param>
        public Updatable(IEnumerable<dynamic> props, T type)
        {
            InitType(type);
        }

        /// <summary>
        /// Use action as method for defined types.
        /// </summary>
        /// <param name="propsMethodsPairs">If undefined object has methods, they have to be declared too.</param>
        /// <param name="type">If said type is defined, use it.</param>
        public Updatable(T type)
        {
            InitType(type);
        }

        /// <summary>
        /// Iterates trough type properties, adding them to expando class member.
        /// Seems extremely wonky and i doubt this would work, but let´s see.
        /// </summary>
        /// <param name="type"></param>
        private void InitType<TPType>(TPType type)
        {
            var dictionary = (IDictionary<string, object>)_expando;
            //foreach (var property in type.GetType().GetProperties())
              //  dictionary.Add(property.Name, property.GetValue(type));
            foreach (var field in type.GetType().GetFields())
                dictionary.Add(field.Name, field.GetValue(type));
            //foreach (var method in GetType().GetMethods().Where(info => info.IsSpecialName))
              //  dictionary.Add(method.Name, method);  
            // Force _expepando to use my Stringifiable...
            dictionary.Add("ToString", new Func<string>(() => (_expando as object).Stringify()));
            // Alternative ? Delegate.CreateDelegate(typeof(...), methodInfo)
        }

        public override string ToString()
        {
            return _expando.ToString();
        }
    }
}
