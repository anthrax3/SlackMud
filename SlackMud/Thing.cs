using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackMud
{
    public abstract class Thing : ReceiveActor
    {
        protected IActorRef Container { get; set; } = ActorRefs.Nobody;
        protected HashSet<IActorRef> Content { get; set; } = new HashSet<IActorRef>();
        protected abstract string GetName();

        public Thing()
        {
            Become(Ambient);
        }

        protected virtual void Ambient()
        {
            Receive<SetContainer>(msg =>
            {
                Container.Tell(new ContainerRemove(Self));
                //TODO: this can be racy
                Container = msg.Container;
                Container.Tell(new ContainerAdd(Self));
            });
            Receive<ContainerDescribe>(msg =>
            {
                //get all items except the observer
                var items = Content.Except(Enumerable.Repeat(msg.Item, 1));
                Context.ActorOf(StringAggregator.Props("You see {0}", Sender, items, new GetName()));
            });
            Receive<Look>(msg => Container.Forward(new ContainerDescribe(Self)));
            Receive<GetName>(msg => Sender.Tell(GetName()));
            Receive<GetContainerName>(msg => Container.Forward(new GetName()));
            Receive<ContainerAdd>(msg => Content.Add(msg.Item));
            Receive<ContainerRemove>(msg => Content.Remove(msg.Item));
        }
    }
}
