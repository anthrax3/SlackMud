using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackMud
{
    public class Thing : ReceiveActor
    {
        public IActorRef Output { get; set; }
        public int MyContainerVolume { get; set; }
        protected IActorRef MyContainer { get; set; }
        protected HashSet<IActorRef> MyContent { get; set; } = new HashSet<IActorRef>();
        public string Name { get; set; } = "unknown";

        public Thing()
        {
            Become(Ambient);
        }

        public Thing(string name) : this()
        {
            Name = name;
        }

        protected virtual void Ambient()
        {
            //api
            Receive<GetName>(msg => Sender.Tell(Name));

            //container commands
            Receive<SetContainer>(msg =>
            {
                MyContainer?.Tell(new ContainerRemove(Self), Sender);
                //TODO: this can be racy
                MyContainer = msg.Container;
                MyContainer.Tell(new ContainerAdd(Self), Sender);
            });

            //add an item to the container, notify others in the same container
            Receive<ContainerAdd>(msg => {
                msg
                .ObjectToAdd
                .GetName(name => new ContainerNotify($"{name} appears", msg.ObjectToAdd))
                .PipeTo(Self);

                MyContent.Add(msg.ObjectToAdd);
            });

            //remove from container, notify others in the same container
            Receive<ContainerRemove>(msg =>
            {
                MyContent.Remove(msg.ObjectToRemove);

                msg
                .ObjectToRemove
                .GetName(name => new ContainerNotify($"{name} disappears", msg.ObjectToRemove))
                .PipeTo(Self);
            });

            //aggregate names of content and notify sender
            Receive<ContainerLook>(msg => {
                StringAggregator
                .Props("You see {0}", msg.Who, MyContent.Except(msg.Who), new GetName())
                .ActorOf();
            });

            //get name of the one that talks, notify all others
            Receive<ContainerSay>(msg => {
                msg
                .Who
                .GetName(name => new ContainerNotify($"{name} says: {msg.Message}", msg.Who))
                .PipeTo(Self);
            });

            //broadcast notification to everyone in this container
            Receive<ContainerNotify>(msg =>
            {
                var targets = msg.Except != null ? MyContent.Except(msg.Except) : MyContent;
                targets.TellAll(new Notify(msg.Message));
            });

            Receive<FindObjectByName>(msg => {
                FindByNameAggregator
                .Props(Sender, MyContent, msg.Name)
                .ActorOf();
            });            

            //actions
            Receive<Say>(msg =>
            {
                //TODO: should say notifications to others and self be handled the same way?
                MyContainer.Tell(new ContainerSay(msg.Message, Self));
                Self.Tell(new Notify($"You say {msg.Message}"));
            });
            Receive<Look>(msg => MyContainer.Forward(new ContainerLook(Self)));
            Receive<Where>(msg => {
                MyContainer
                .GetName(name => new Notify($"You are in {name}"))
                .PipeTo(Self);
            });
            Receive<Take>(msg =>
            {
                var self = Self;
                MyContainer
                .FindContainedObjectByName(msg.NameToFind,findResult =>
                {
                    if (findResult.Item == null)
                    {
                        self.Tell(new Notify($"Could not find {msg.NameToFind}"));
                    }
                    else
                    {
                        self.Tell(new Notify($"You take {findResult.Name}"));
                        //TODO: this is racy, potentially two people could take the same item at the same time
                        //SetContainer should probably even contan the current Container.
                        //if the objects container is different from the passed in value, there is a race condition
                        findResult.Item.Tell(new SetContainer(self));
                    }
                });
            });
            Receive<Drop>(msg =>
            {
                var self = Self;
                var container = MyContainer;
                Self
                .FindContainedObjectByName(msg.Name,findResult =>
                {
                    if (findResult.Item == null)
                    {
                        self.Tell(new Notify($"Could not find {msg.Name}"));
                    }
                    else
                    {
                        self.Tell(new Notify($"You drop {findResult.Name}"));
                        findResult.Item.Tell(new SetContainer(container));
                    }
                });                
            });

            //output stream
            Receive<Notify>(msg => Output?.Tell(msg));            
            Receive<SetOutput>(msg => Output = msg.Output);
        }
    }
}
