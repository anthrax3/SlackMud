using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackMud
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var system = ActorSystem.Create("Mud"))
            {
                var room1 = system.ActorOf(Props.Create(() => new NamedThing("the kitchen")));
                var backpack = system.ActorOf(Props.Create(() => new NamedThing("a backpack")));
                var player1 = system.ActorOf(Props.Create(() => new Player("Allan")));
                var player2 = system.ActorOf(Props.Create(() => new Player("Åke")));
                var goblin = system.ActorOf(Props.Create(() => new Goblin()));
                player1.Tell(new SetContainer(room1));
                player2.Tell(new SetContainer(room1));
                goblin.Tell(new SetContainer(room1));
            //    backpack.Tell(new SetContainer(player1));
                Run(player1).Wait();
            }
        }

        private static async Task Run(IActorRef player1)
        {
            while (true)
            {
                var command = Console.ReadLine();
                switch (command)
                {
                    case "where":
                        var containerName = await player1.Ask<string>(new GetContainerName());
                        Console.WriteLine($"You are in {containerName}");
                        break;
                    case "name":
                        var name = await player1.Ask<string>(new GetName());
                        Console.WriteLine($"Your name is {name}.");
                        break;
                    case "look":
                        {
                            var names = await player1.Ask<string[]>(new Look());
                            var joinNames = JoinNames(names);
                            Console.WriteLine("You see " + joinNames);
                            break;
                        }
                    case "inventory":
                        {
                            var names = await player1.Ask<string[]>(new Inventory());
                            var joinNames = JoinNames(names);
                            Console.WriteLine("You have " + joinNames);
                            break;
                        }

                }
            }
        }

        protected static string JoinNames(string[] items)
        {
            if (items.Length == 0)
                return "nothing";

            if (items.Length == 1)
                return items.First();

            return string.Join(", ", items, 0, items.Length - 1) + " and " + items.LastOrDefault();
        }
    }

    public class SetContainer
    {
        public IActorRef Container { get;private set; }
        public SetContainer(IActorRef container)
        {
            Container = container;
        }
    }

    public class ContainerRemove
    {
        public IActorRef Item { get; private set; }
        public ContainerRemove(IActorRef item)
        {
            Item = item;
        }
    }

    public class ContainerDescribe
    {
        public IActorRef Item { get; private set; }
        public ContainerDescribe(IActorRef item)
        {
            Item = item;
        }
    }

    public class ContainerAdd
    {
        public IActorRef Item { get; private set; }
        public ContainerAdd(IActorRef item)
        {
            Item = item;
        }
    }

    public class GetName
    {
    }

    public class GetContainerName
    {
    }

    public class Look
    {
    }

    public class Inventory
    {
    }


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
                var aggregator = Context.ActorOf(Props.Create(() => new Aggregator<string>(Sender, items, new GetName())));              
            });
            Receive<Look>(msg => Container.Forward(new ContainerDescribe(Self)));
            Receive<GetName>(msg => Sender.Tell(GetName()));
            Receive<GetContainerName>(msg => Container.Forward(new GetName()));
            Receive<ContainerAdd>(msg => Content.Add(msg.Item));
            Receive<ContainerRemove>(msg => Content.Remove(msg.Item));
        }      
    }



    public class NamedThing : Thing
    {
        private string _name;
        protected override string GetName() => _name;

        public NamedThing(string name)
        {
            _name = name;
        }
    }

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
            Become(()=>
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

    public class Player : Living
    {
        private string _name;
        public Player(string name)
        {
            _name = name;
        }
        protected override int GetMaxHP() => 100;

        protected override string GetName() => _name;

        protected override void Alive()
        {
            Receive<Inventory>(msg =>
            {
                var aggregator = Context.ActorOf(Props.Create(() => new Aggregator<string>(Sender, Content, new GetName())));              
            });
        }

        protected override void Dead()
        {
        
        }
    }

    public class Aggregator<TMsg> : ReceiveActor
    {
        public Aggregator(IActorRef replyTo, IEnumerable<IActorRef> targets,object message)
        {
            var targetArr = targets.ToArray();
            var replies = new Dictionary<IActorRef, TMsg>();
            foreach(var target in targetArr)
            {
                target.Tell(message, Self);
            }
            if (targetArr.Length == 0)
            {
                var result = new TMsg[0];
                replyTo.Tell(result);
                Context.Stop(Self);
            }

            Receive<TMsg>(msg =>
            {
                replies.Add(Sender, msg);
                if (replies.Count == targetArr.Length)
                {
                    var result = replies.Values.ToArray();
                    replyTo.Tell(result);
                    Context.Stop(Self);
                }
            });
        }           
    }
}
