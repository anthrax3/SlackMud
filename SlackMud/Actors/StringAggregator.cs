using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using System;

namespace SlackMud
{
    public class StringAggregator : ReceiveActor
    {
        public static void Run(string format, IActorRef replyTo, IEnumerable<IActorRef> targets, object message)
        {
            Aggregator<string>.Run(targets, message, TimeSpan.FromSeconds(1))
            .ContinueWith(t =>
            {
                var strings = t.Result.Select(r => r.Result).ToArray();
                var join = JoinNames(strings);
                var result = new Notify(string.Format(format, join));
                replyTo.Tell(result);                
            });
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
}
