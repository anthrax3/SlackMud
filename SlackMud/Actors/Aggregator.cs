using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlackMud
{
    public class AggregatorResult<TMsg>
    {
        public AggregatorResult(TMsg result, IActorRef source)
        {
            Result = result;
            Source = source;
        }

        public IActorRef Source { get;private set; }
        public TMsg Result { get;private set; }
    }
    public class Aggregator<TMsg> : ReceiveActor
    {
        public static Task<AggregatorResult<TMsg>[]> Run(IEnumerable<IActorRef> targets, object message,TimeSpan timeout)
        {
            TaskCompletionSource<AggregatorResult<TMsg>[]> tcs = new TaskCompletionSource<AggregatorResult<TMsg>[]>();
            var props = Props.Create(() => new Aggregator<TMsg>(targets, message, timeout, tcs));
            props.ActorOf();
            return tcs.Task;
        }

        public Aggregator(IEnumerable<IActorRef> targets, object message,TimeSpan timeout, TaskCompletionSource<AggregatorResult<TMsg>[]> tcs)
        {
            var targetArr = targets.ToArray();
            if (targetArr.Length == 0)
            {
                var emptyResult = new AggregatorResult<TMsg>[0];
                tcs.SetResult(emptyResult);
                Context.Stop(Self);
                return;
            }

            targetArr.TellAll(message, Self);

            var replies = new Dictionary<IActorRef, TMsg>();
            SetReceiveTimeout(timeout);
            Receive<ReceiveTimeout>(msg =>
            {
                var emptyResult = new AggregatorResult<TMsg>[0];
                tcs.SetResult(emptyResult);
                Context.Stop(Self);
            });

            Receive<TMsg>(msg =>
            {
                replies.Add(Sender, msg);

                if (replies.Count == targetArr.Length)
                {
                    var result = replies.Select(kvp => new AggregatorResult<TMsg>(kvp.Value, kvp.Key)).ToArray();
                    tcs.SetResult(result);
                    Context.Stop(Self);
                }
            });
        }
    }
}
