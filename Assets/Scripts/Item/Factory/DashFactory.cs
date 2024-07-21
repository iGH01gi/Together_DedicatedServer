public class DashFactory : ItemFactory
{
    protected override IItem CreateProduct()
    {
        return new Dash();
    }
}