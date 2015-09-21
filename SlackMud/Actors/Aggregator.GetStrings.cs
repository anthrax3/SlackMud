using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using System;
using System.Threading.Tasks;

namespace SlackMud
{
    public partial class Aggregator
    {
        public static Task<string> JoinNames(IEnumerable<IActorRef> targets)
        {
            return JoinStrings(targets, new GetName());
        }

        public static async Task<string> JoinStrings(IEnumerable<IActorRef> targets, object message)
        {
            var res = await Aggregator<string>.Run(targets, message, TimeSpan.FromSeconds(1));
            var strings = res.Select(r => r.Result).ToArray();
            var join = JoinNames(strings);
            return join;
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
