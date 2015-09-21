using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka;
using Akka.Actor.Internal;

namespace SlackMud
{
    public static class Extensions
    {
        public static IEnumerable<T> Yield<T>(this T self)
        {
            yield return self;
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> self, T except)
        {
            return self.Except(except.Yield());
        }

        public static Task<string> GetNames(this IEnumerable<IActorRef> self)
        {
            return Aggregator.JoinNames(self);
        }

        public static Task<string> GetName(this IActorRef self)
        {
            return self.Ask<string>(new GetName());
        }

        public static Task FindContainedObjectByName(this IActorRef self,string name, Action<FindObjectByNameResult> body)
        {
            return self
               .Ask<FindObjectByNameResult>(new FindObjectByName(name))
               .ContinueWith(t => body(t.Result));
        }

        public static void TellAll(this IEnumerable<IActorRef> targets,object message)
        {
            foreach(var target in targets)
            {
                target.Tell(message);
            }
        }
        public static void TellAll(this IEnumerable<IActorRef> targets, object message,IActorRef sender)
        {
            foreach (var target in targets)
            {
                target.Tell(message,sender);
            }
        }

        public static IActorRef ActorOf(this Props self)
        {
            return InternalCurrentActorCellKeeper.Current.ActorOf(self);
        }
    }
}
