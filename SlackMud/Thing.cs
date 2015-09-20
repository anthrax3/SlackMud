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
        public IActorRef Output { get; set; }
        public int ContainerSizeLitres { get; set; }
        protected IActorRef Container { get; set; }
        protected HashSet<IActorRef> Content { get; set; } = new HashSet<IActorRef>();
        protected abstract string GetName();

        public Thing()
        {
            Become(Ambient);
        }

        protected virtual void Ambient()
        {
            //api
            Receive<GetName>(msg => Sender.Tell(GetName()));

            //container commands
            Receive<SetContainer>(msg =>
            {
                Container?.Tell(new ContainerRemove(Self));
                //TODO: this can be racy
                Container = msg.Container;
                Container.Tell(new ContainerAdd(Self));
            });
            Receive<ContainerAdd>(msg => {
                msg
                .Item
                .GetName(name => new ContainerNotify($"{name} appears", msg.Item))
                .PipeTo(Self);

                Content.Add(msg.Item);
            });
            Receive<ContainerRemove>(msg =>
            {
                Content.Remove(msg.Item);

                msg
                .Item
                .GetName(name => new ContainerNotify($"{name} disappears", msg.Item))
                .PipeTo(Self);
            });
            Receive<ContainerDescribe>(msg => AggregateAndNotify("You see {0}", msg.Who, Content.Except(msg.Who), new GetName()));
            Receive<ContainerSay>(msg => {
                msg
                .Who
                .GetName(name => new ContainerNotify($"{name} says: {msg.Message}", msg.Who))
                .PipeTo(Self);
            });
            Receive<ContainerNotify>(msg =>
            {
                var targets = msg.Except != null ? Content.Except(msg.Except) : Content;
                targets.TellAll(new Notify(msg.Message));
            });
            Receive<FindObjectByName>(msg => Context.ActorOf(FindByNameAggregator.Props(Sender, Content, msg.Name)));            

            //actions
            Receive<Say>(msg =>
            {
                //TODO: should say notifications to others and self be handled the same way?
                Container.Tell(new ContainerSay(msg.Message, Self));
                Self.Tell(new Notify($"You say {msg.Message}"));
            });
            Receive<Look>(msg => Container.Forward(new ContainerDescribe(Self)));
            Receive<Where>(msg => {
                Container
                .GetName(name => new Notify($"You are in {name}"))
                .PipeTo(Self);
            });
            Receive<Take>(async msg =>
            {
                var result = await Container.Ask<FoundObjectByName>(new FindObjectByName(msg.Name));
                if (result == null)
                {
                    Self.Tell(new Notify($"Could not find {msg.Name}"));
                }
                else
                {
                    Self.Tell(new Notify($"You take {result.Name}"));
                    result.Item.Tell(new SetContainer(Self));
                }
            });

            //output stream
            Receive<Notify>(msg => Output?.Tell(msg));            
            Receive<SetOutput>(msg => Output = msg.Output);
        }

        private void AggregateAndNotify(string template,IActorRef replyTo,IEnumerable<IActorRef> targets,object message)
        {
            Context.ActorOf(StringAggregator.Props(template,replyTo, targets, message));
        }

    }
}
