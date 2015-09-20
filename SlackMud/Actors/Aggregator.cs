using Akka.Actor;
using System.Collections.Generic;
using System.Linq;

namespace SlackMud
{
    public class Aggregator<TMsg> : ReceiveActor
    {
        public static void Run(IActorRef replyTo, IEnumerable<IActorRef> targets, object message)
        {
            var props = Akka.Actor.Props.Create(() => new Aggregator<TMsg>(replyTo, targets, message));
            props.ActorOf();

        }
        public Aggregator(IActorRef replyTo, IEnumerable<IActorRef> targets, object message)
        {
            var targetArr = targets.ToArray();
            var replies = new Dictionary<IActorRef, TMsg>();
            foreach (var target in targetArr)
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
