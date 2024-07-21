public abstract class ItemFactory
{
    public IItem CreateItem()
    {
        IItem item = CreateProduct();
        item.Setting();
        return item;
    }

    protected abstract IItem CreateProduct(); //상속한 팩토리에서 구현
}