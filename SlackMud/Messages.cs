using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackMud
{
    public class SetContainer
    {
        public IActorRef Container { get; private set; }
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
}
