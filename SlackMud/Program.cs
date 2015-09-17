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
                backpack.Tell(new SetContainer(player1));
                Run(player1).Wait();
            }
        }

        private static async Task Run(IActorRef player1)
        {
            Console.WriteLine("type; look, where, name, inventory");
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
                            var description = await player1.Ask<string>(new Look());
                            Console.WriteLine(description);
                            break;
                        }
                    case "inventory":
                        {
                            var description = await player1.Ask<string>(new Inventory());
                            Console.WriteLine(description);
                            break;
                        }

                }
            }
        }       
    }
}
