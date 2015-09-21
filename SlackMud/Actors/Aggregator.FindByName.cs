using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlackMud
{
    public partial class Aggregator
    {
        public static async Task<FindObjectByNameResult> FindByName(IEnumerable<IActorRef> targets, string nameToFind)
        {
            nameToFind = SimplifyName(nameToFind);
            var result = await Aggregator<string>.Run(targets, new GetName(), TimeSpan.FromSeconds(1));
            if (result.Length == 0)
            {
                return new FindObjectByNameResult();
            }
            foreach (var res in result)
            {
                if (res.Result.ToLowerInvariant().Contains(nameToFind))
                {
                    return new FindObjectByNameResult(res.Source, res.Result);
                }
            }
            return new FindObjectByNameResult();
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
