namespace SlackMud
{
    public class Player : Living
    {
        private string _name;
        public Player(string name)
        {
            _name = name;
        }
        protected override int GetMaxHP() => 100;

        protected override string GetName() => _name;

        protected override void Alive()
        {
            Receive<Inventory>(msg =>
            {
                Context.ActorOf(StringAggregator.Props("You have {0}", Output, Content, new GetName()));
            });
        }

        protected override void Dead()
        {

        }
    }
}
