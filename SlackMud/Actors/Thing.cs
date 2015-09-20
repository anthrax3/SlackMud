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
            Receive<ContainerAdd>(msg =>
            {
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
            Receive<ContainerLook>(msg =>
            {
                StringAggregator.Run("You see {0}", msg.Who, MyContent.Except(msg.Who), new GetName());
            });

            //get name of the one that talks, notify all others
            Receive<ContainerSay>(msg =>
            {
                msg
                .Who
                .GetName(name => new ContainerNotify($"{name} says: {msg.Message}", msg.Who))
                .PipeTo(Self);
            });

            //broadcast notification to everyone in this container
            Receive<ContainerNotify>(msg => ContainerNotify(msg, MyContent));
            Receive<FindObjectByName>(msg => FindObjectByName(msg, Sender, MyContent));
            //actions
            Receive<Say>(msg => Say(msg, Self, MyContainer));
            Receive<Look>(msg => Look(Self, MyContainer));
            Receive<Where>(msg => Where(Self, MyContainer));
            Receive<Take>(msg => Take(msg, Self, MyContainer));
            Receive<Drop>(msg => Drop(msg, Self, MyContainer));
            Receive<Put>(msg => Put(msg, Self, MyContainer));

            //output stream
            Receive<Notify>(msg => Output?.Tell(msg));
            Receive<SetOutput>(msg => Output = msg.Output);
        }

        //Why static handlers? this prevents any state mutations of the actor inside any async workflows.
        //the actor is the entrypoint which then delegates to various async workflows
        private static void ContainerNotify(ContainerNotify msg, IEnumerable<IActorRef> MyContent)
        {
            var targets = msg.Except != null ? MyContent.Except(msg.Except) : MyContent;
            targets.TellAll(new Notify(msg.Message));
        }

        private static void FindObjectByName(FindObjectByName msg, IActorRef Sender, IEnumerable<IActorRef> MyContent)
        {

            FindByNameAggregator.Run(Sender, MyContent, msg.Name);
        }

        private static void Say(Say msg, IActorRef Self, IActorRef MyContainer)
        {
            //TODO: should say notifications to others and self be handled the same way?
            MyContainer.Tell(new ContainerSay(msg.Message, Self));
            Self.Tell(new Notify($"You say {msg.Message}"));
        }

        private static void Look(IActorRef Self, IActorRef MyContainer)
        {
            MyContainer.Forward(new ContainerLook(Self));
        }

        private static void Where(IActorRef Self, IActorRef MyContainer)
        {
            MyContainer
            .GetName(name => new Notify($"You are in {name}"))
            .PipeTo(Self);
        }

        private static void Take(Take msg, IActorRef Self, IActorRef MyContainer)
        {
            MyContainer
            .FindContainedObjectByName(msg.NameToFind, findResult =>
            {
                if (findResult.Item == null)
                {
                    Self.Tell(new Notify($"Could not find {msg.NameToFind}"));
                }
                else
                {
                    Self.Tell(new Notify($"You take {findResult.Name}"));
                    //TODO: this is racy, potentially two people could take the same item at the same time
                    //SetContainer should probably even contan the current Container.
                    //if the objects container is different from the passed in value, there is a race condition
                    findResult.Item.Tell(new SetContainer(Self));
                }
            });
        }

        private static void Drop(Drop msg, IActorRef Self, IActorRef MyContainer)
        {
            Self
            .FindContainedObjectByName(msg.Name, findResult =>
            {
                if (findResult.Item == null)
                {
                    Self.Tell(new Notify($"Could not find {msg.Name}"));
                }
                else
                {
                    Self.Tell(new Notify($"You drop {findResult.Name}"));
                    findResult.Item.Tell(new SetContainer(MyContainer));
                }
            });
        }

        private static void Put(Put msg, IActorRef Self, IActorRef MyContainer)
        {
            var self = Self;
            var t1 = MyContainer.Ask<FoundObjectByName>(new FindObjectByName(msg.TargetName));
            var c1 = MyContainer.Ask<FoundObjectByName>(new FindObjectByName(msg.ContainerName));
            var t2 = Self.Ask<FoundObjectByName>(new FindObjectByName(msg.TargetName));
            var c2 = Self.Ask<FoundObjectByName>(new FindObjectByName(msg.ContainerName));

            Task.WhenAll(t1, t2, c1, c2).ContinueWith(tasks =>
            {
                //prioritize items in inventory first
                var t = t2?.Result?.Item != null ? t2 : t1;
                var c = c2?.Result?.Item != null ? c2 : c1;

                if (t.Result.Item == null)
                {
                    self.Tell(new Notify($"Could not find {msg.TargetName}"));
                    return;
                }

                if (c.Result.Item == null)
                {
                    self.Tell(new Notify($"Could not find {msg.ContainerName}"));
                    return;
                }

                self.Tell(new Notify($"You put {t.Result.Name} in {c.Result.Name}"));
                t.Result.Item.Tell(new SetContainer(c.Result.Item));
            });
        }
    }
}