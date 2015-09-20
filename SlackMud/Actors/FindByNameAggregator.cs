using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlackMud
{
    public class FindByNameAggregator : ReceiveActor
    {
        public static void Run(IActorRef replyTo, IEnumerable<IActorRef> targets, string nameToFind)
        {
            var props = Akka.Actor.Props.Create(() => new FindByNameAggregator(replyTo, targets, nameToFind));
            props.ActorOf();
        }
        public FindByNameAggregator(IActorRef replyTo, IEnumerable<IActorRef> targets, string nameToFind)
        {
            var targetArr = targets.ToArray();
            var replies = new Dictionary<IActorRef, string>();

            if (targetArr.Length == 0)
            {
                replyTo.Tell(new FoundObjectByName(null,null));
                Context.Stop(Self);
            }

            foreach (var target in targetArr)
            {
                target.Tell(new GetName(), Self);
            }

            SetReceiveTimeout(TimeSpan.FromSeconds(1));

            //in case we get no results, timeout
            Receive<ReceiveTimeout>(msg =>
            {
                replyTo.Tell(new FoundObjectByName(null, null));
                Context.Stop(Self);
            });

            Receive<string>(msg =>
            {
                var toFind = nameToFind.ToLowerInvariant();
                if (toFind.StartsWith("the "))
                    toFind = toFind.Substring(4);
                else if (toFind.StartsWith("a "))
                    toFind = toFind.Substring(2);
                else if (toFind.StartsWith("an "))
                    toFind = toFind.Substring(3);

                if (msg.ToLowerInvariant().Contains(toFind))
                {
                    replyTo.Tell(new FoundObjectByName(Sender,msg));
                    Context.Stop(Self);
                    return;
                }

                replies.Add(Sender, msg);
                if (replies.Count == targetArr.Length)
                {
                    replyTo.Tell(new FoundObjectByName(null,null));
                    Context.Stop(Self);
                }
            });
        }
    }
}
