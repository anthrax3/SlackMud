using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

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

        private bool IsAlive;
        protected void BecomeAlive()
        {
            IsAlive = true;
            Become(() =>
            {
                Ambient();
                Alive();
            });
        }

        protected void BecomeDead()
        {
            MyContainer.Tell(new ContainerNotify($"{Name} has died!", Self));
            Self.Tell(new Notify($"You have died!"));
            Sender.Tell(new Died());
            HP = 0;
            IsAlive = false;
            var corpse = Context.System.ActorOf(Props.Create(() => new Thing($"Corpse of {Name}")));
            corpse.Tell(new SetContainer(MyContainer));
            MyContent.TellAll(new SetContainer(MyContainer));
            //remove self from container
            MyContainer.Tell(new ContainerRemove(Self));
            Context.Stop(Self);
            //Become(() =>
            //{
            //    Ambient();
            //    Dead();
            //});
        }

        protected virtual void Alive()
        {
            
            
        }
        protected abstract void Dead();

        protected override void ProcessDamage(TakeDamage msg)
        {
            if (!IsAlive)
            {
                return;
            }

            base.ProcessDamage(msg);
            Self.Tell(new NotifyCombatStatus());
            HP -= msg.Value;
            if (HP <= 0)
            {               
                BecomeDead();
            }
        }

        protected override void NotifyCombatStatus()
        {
            base.NotifyCombatStatus();

            MyContainer.Tell(new ContainerNotify($"{Name} - [{HP}]/[{GetMaxHP()}]", Self));
            Self.Tell(new Notify($"You - [{HP}]/[{GetMaxHP()}]"));
        }
    }
}
