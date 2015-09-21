using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackMud
{
    public class SetOutput
    {
        public IActorRef Output { get; private set; }
        public SetOutput(IActorRef output)
        {
            Output = output;
        }
    }

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
        public IActorRef ObjectToRemove { get; private set; }
        public ContainerRemove(IActorRef item)
        {
            ObjectToRemove = item;
        }
    }

    public class ContainerLook
    {
        public IActorRef Who { get; private set; }
        public ContainerLook(IActorRef item)
        {
            Who = item;
        }
    }

    public class ContainerAdd
    {
        public IActorRef ObjectToAdd { get; private set; }
        public ContainerAdd(IActorRef item)
        {
            ObjectToAdd = item;
        }
    }

    public class GetName
    {
    }

    public class Where
    {
    }

    public class Look
    {
    }

    public class Inventory
    {
    }

    public class Say
    {
        public Say(string message)
        {
            Message = message;
        }
        public string Message { get; private set; }
    }

    public class ContainerSay
    {
        public ContainerSay(string message, IActorRef who)
        {
            Message = message;
            Who = who;
        }
        public string Message { get; private set; }
        public IActorRef Who { get; private set; }
    }

    public class Said
    {
        public Said(string message, IActorRef who)
        {
            Message = message;
            Who = who;
        }
        public string Message { get; private set; }
        public IActorRef Who { get; private set; }
    }

    public class Notify
    {
        public Notify(string message, IActorRef who = null)
        {
            Message = message;
            Who = who;
        }
        public string Message { get; private set; }
        public IActorRef Who { get; private set; }
    }

    public class ContainerNotify
    {
        public ContainerNotify(string message, params IActorRef[] except)
        {
            Message = message;
            Except = except;
        }
        public string Message { get; private set; }
        public IActorRef[] Except { get; private set; }
    }

    public class FindObjectByName
    {
        public FindObjectByName(string name)
        {
            Name = name;
        }
        public string Name { get; private set; }
    }

    public class FindObjectByNameResult
    {
        public FindObjectByNameResult()
        {
        }

        public FindObjectByNameResult(IActorRef found, string name)
        {
            Item = found;
            Name = name;
        }
        public IActorRef Item { get; private set; }
        public string Name { get; private set; }

        public bool HasValue
        {
            get
            {
                return Item != null;
            }
        }
    }

    public class MatchName
    {
        public MatchName(string name)
        {
            Name = name;
        }
        public string Name { get; private set; }
    }

    public class Take
    {
        public Take(string name)
        {
            NameToFind = name;
        }
        public string NameToFind { get; private set; }
    }

    public class Drop
    {
        public Drop(string name)
        {
            Name = name;
        }
        public string Name { get; private set; }
    }

    public class Put
    {
        public Put(string targetName, string containerName)
        {
            TargetName = targetName;
            ContainerName = containerName;
        }
        public string TargetName { get; private set; }
        public string ContainerName { get; private set; }
    }

    public class StartFight
    {
        public StartFight(string target)
        {
            TargetName = target;
        }
        public string TargetName { get; private set; }
    }

    public class SetTarget
    {
        public SetTarget(IActorRef target)
        {
            Target = target;
        }

        public IActorRef Target { get; private set; }
    }

    public class Attack
    {
    }

    public class TakeDamage
    {
        public TakeDamage(int value)
        {
            Value = value;
        }
        public int Value { get;private set; }
    }

    public class NotifyCombatStatus
    {

    }

    public class Died
    {

    }
}
