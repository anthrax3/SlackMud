using System.Collections.Generic;
using System.Linq;
using Akka.Actor;

namespace SlackMud
{
    public class StringAggregator : ReceiveActor
    {
        public static Props Props(string format, IActorRef replyTo, IEnumerable<IActorRef> targets, object message)
        {
            return Akka.Actor.Props.Create(() => new StringAggregator(format, replyTo, targets, message));
        }
        public StringAggregator(string format, IActorRef replyTo, IEnumerable<IActorRef> targets, object message)
        {
            var targetArr = targets.ToArray();
            var replies = new Dictionary<IActorRef, string>();
            foreach (var target in targetArr)
            {
                target.Tell(message, Self);
            }
            if (targetArr.Length == 0)
            {
                var joined = JoinNames(new string[0]);
                replyTo.Tell(string.Format(format, joined));
                Context.Stop(Self);
            }

            Receive<string>(msg =>
            {
                replies.Add(Sender, msg);
                if (replies.Count == targetArr.Length)
                {
                    var names = replies.Values.ToArray();
                    var joined = JoinNames(names);
                    replyTo.Tell(new Notify(string.Format(format, joined)));
                    Context.Stop(Self);
                }
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
