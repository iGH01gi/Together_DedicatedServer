public class Firework : IItem
{
    //아이템 공통 보유 속성
    public int Id { get; set; }
    public int Price { get; set; }
    public string EnglishName { get; set; }
    public string KoreanName { get; set; }
    public string EnglishDescription { get; set; }
    public string KoreanDescription { get; set; }
    
    //이 아이템만의 속성
    public float FlightHeight { get; set; }
    
    public void Setting()
    {
    }
    
    public void Use()
    {

    }
    
    public void OnHold()
    {
        
    }
}