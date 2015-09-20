using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackMud
{
    public abstract class Living : Thing
    {
        protected int HP { get; set; }
        protected abstract int GetMaxHP();

        public Living() 
        {
            HP = GetMaxHP();
            BecomeAlive();
        }

        protected void BecomeAlive()
        {
            Become(() =>
            {
                Ambient();
                Alive();
            });
        }

        protected void BecomeDead()
        {
            Become(() =>
            {
                Ambient();
                Dead();
            });
        }

        protected abstract void Alive();
        protected abstract void Dead();
    }
}
