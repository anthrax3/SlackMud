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
            nameToFind = SimplifyName(nameToFind);
            var task = Aggregator<string>.Run(targets, new GetName(), TimeSpan.FromSeconds(1));
            task.ContinueWith(t =>
            {
                if (t.Result.Length == 0)
                {
                    replyTo.Tell(new FindObjectByNameResult());
                    return;
                }

                foreach(var res in t.Result)
                {
                    if (res.Result.ToLowerInvariant().Contains(nameToFind))
                    {
                        replyTo.Tell(new FindObjectByNameResult(res.Source, res.Result));
                        return;
                    }
                }
                replyTo.Tell(new FindObjectByNameResult());
            });
        }

        private static string SimplifyName(string nameToFind)
        {
            var toFind = nameToFind.ToLowerInvariant();
            if (toFind.StartsWith("the "))
                toFind = toFind.Substring(4);
            else if (toFind.StartsWith("a "))
                toFind = toFind.Substring(2);
            else if (toFind.StartsWith("an "))
                toFind = toFind.Substring(3);
            return toFind;
        }
    }
}
