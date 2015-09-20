using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static Task<object> GetName(this IActorRef self,Func<string,object> body)
        {
            return self.Ask<string>(new GetName()).ContinueWith(t => body(t.Result));
        }

        public static void TellAll(this IEnumerable<IActorRef> targets,object message)
        {
            foreach(var target in targets)
            {
                target.Tell(message);
            }
        }
    }
}
