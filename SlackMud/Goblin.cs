using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackMud
{
    public class Goblin : Living
    {
        protected override int GetMaxHP() => 50;

        protected override string GetName() => "a goblin";

        protected override void Alive()
        {

        }

        protected override void Dead()
        {

        }
    }
}
