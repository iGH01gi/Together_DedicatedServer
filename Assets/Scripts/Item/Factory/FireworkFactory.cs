public class FireworkFactory : ItemFactory
{
    protected override IItem CreateProduct()
    {
        return new Firework();
    }
}