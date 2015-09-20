using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackMud
{
    public class Goblin : Living
    {
        public Goblin()
        {
            Name = "a goblin";
        }

        protected override int GetMaxHP() => 50;

        protected override void Alive()
        {

        }

        protected override void Dead()
        {

        }
    }
}
