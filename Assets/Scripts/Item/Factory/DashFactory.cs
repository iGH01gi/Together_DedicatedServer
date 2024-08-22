public class DashFactory : ItemFactory
{
    public float DashDistance { get; set; }

    public DashFactory(int id, int price, string englishName, string koreanName, string englishDescription,
        string koreanDescription, float dashDistance)
    {
        base.FactoryInit(id, price, englishName, koreanName, englishDescription, koreanDescription);
        DashDistance = dashDistance;
    }

    public override IItem CreateItem(int playerId)
    {
        //if else를 통해 여기서 맞는 아이템을 생성
        return null;
    }
}