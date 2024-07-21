public class TheHeartlessFactory : KillerFactory
{
    protected override IKiller CreateProduct()
    {
        return new TheHeartless();
    }
}