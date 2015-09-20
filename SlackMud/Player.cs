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
                Context.ActorOf(StringAggregator.Props("You have {0}", Output, MyContent, new GetName()));
            });
        }

        protected override void Dead()
        {

        }
    }
}
