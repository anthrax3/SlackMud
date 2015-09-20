using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackMud
{
    public enum DamageType
    {
        Magic,
        Fire,
        Frost,
        Crushing,
        Cutting,
        Peircing
    }

    public class Weapon : Thing
    {
        public Weapon(string name)
        {
            Name = name;
        }
    }
}
