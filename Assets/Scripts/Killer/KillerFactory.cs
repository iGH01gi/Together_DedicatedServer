public abstract class KillerFactory
{
    public IKiller CreateKiller()
    {
        IKiller killer = CreateProduct();
        killer.Setting();
        return killer;
    }
    
    protected abstract IKiller CreateProduct(); //상속한 팩토리에서 구현
}