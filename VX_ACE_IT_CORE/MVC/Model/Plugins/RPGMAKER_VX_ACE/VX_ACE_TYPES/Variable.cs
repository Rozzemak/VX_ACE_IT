using System;
using System.Collections.Generic;
using System.Text;

namespace VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE.VX_ACE_TYPES
{
    public class Variable<T> where T : struct 
    {
        public int Id;
        public Numeric<T> Value;
        public string Name;

    }
}
