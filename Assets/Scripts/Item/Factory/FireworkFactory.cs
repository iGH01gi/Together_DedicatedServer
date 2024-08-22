public class FireworkFactory : ItemFactory
{
    //이 아이템만의 속성
    public float FlightHeight { get; set; }

    public FireworkFactory(int id, int price, string englishName, string koreanName, string englishDescription,
        string koreanDescription, float flightHeight)
    {
        base.FactoryInit(id, price, englishName, koreanName, englishDescription, koreanDescription);
        FlightHeight = flightHeight;
    }
    public override IItem CreateItem(int playerId)
    {
        return null;
    }
}