using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackMud
{
    public class NamedThing : Thing
    {
        private string _name;
        protected override string GetName() => _name;

        public NamedThing(string name)
        {
            _name = name;
        }
    }
}
