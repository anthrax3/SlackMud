using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackMud
{
    public class OutputActor : ReceiveActor
    {
        public OutputActor()
        {
            Receive<Notify>(msg =>
            {
                Console.WriteLine(msg.Message);
            });
        }
    }
}
