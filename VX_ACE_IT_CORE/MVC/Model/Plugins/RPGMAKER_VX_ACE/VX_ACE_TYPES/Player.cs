using System;
using System.Collections.Generic;
using System.Text;

namespace VX_ACE_IT_CORE.MVC.Model.Plugins.RPGMAKER_VX_ACE.VX_ACE_TYPES
{
    public class Player
    {
        public Numeric<int> Id, Hp, Mana;
        public string Name;
        public List<Item> Items;
        public List<Variable<int>> Variables;

    }
}
