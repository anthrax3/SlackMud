using Akka.Actor;

namespace SlackMud
{
    public class Player : Living
    {
        public Player(string name)
        {
            Name = name;
        }
        protected override int GetMaxHP() => 100;

        protected override void Alive()
        {
            Receive<Inventory>(msg =>
            {
                MyContent.GetNames()
                .ContinueWith(t => new Notify($"You have {t.Result}"))
                .PipeTo(Output);
            });
        }

        protected override void Dead()
        {

        }
    }
}
