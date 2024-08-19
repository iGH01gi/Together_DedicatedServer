using UnityEngine;

public class Firework : MonoBehaviour, IItem
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
        //아이템 매니저로부터 아이템 데이터를 받아와서 설정
        Firework fireworkData = Managers.Item._items[1] as Firework;

        Id = fireworkData.Id;
        Price = fireworkData.Price;
        EnglishName = fireworkData.EnglishName;
        KoreanName = fireworkData.KoreanName;
        EnglishDescription = fireworkData.EnglishDescription;
        KoreanDescription = fireworkData.KoreanDescription;
        FlightHeight = fireworkData.FlightHeight;
    }

    public void Use()
    {
        Debug.Log("Item Firework Use");
    }

    public void OnHold()
    {

    }
}