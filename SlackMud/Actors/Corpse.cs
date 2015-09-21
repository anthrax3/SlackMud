using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
namespace SlackMud.Actors
{
    public class Corpse : Thing
    {
        public class Decompose
        {
        }

        public Corpse(string name)
        {
            Name = name;                   
        }

        protected override void Ambient()
        {
            var timer = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), Self, new Decompose(), Self);
            var times = 0;
            base.Ambient();
            Receive<Decompose>(msg =>
            {
                MyContainer.Tell(new ContainerNotify($"{Name} is decomposing", Self));
                times++;
                if (times == 3)
                {
                    timer.Cancel();
                    MyContainer.Tell(new ContainerNotify($"{Name} has turned to dust", Self));
                    MyContainer.Tell(new ContainerRemove(Self));
                    Context.Stop(Self);
                }
            });
        }

        protected override void ProcessDamage(TakeDamage msg)
        {            
            base.ProcessDamage(msg);
            MyContainer.Tell(new ContainerNotify($"{Name} drops a chunk of flesh", Self));
            var meat = Context.System.ActorOf(Props.Create(() => new Corpse("a chunk of flesh")));
            meat.Tell(new SetContainer(MyContainer));
        }
    }
}
