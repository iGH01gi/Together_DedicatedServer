using UnityEngine;

public abstract class ItemFactory
{
    //공통 속성
    public int FactoryId { get; set; }
    public int FactoryPrice { get; set; }
    public string FactoryEnglishName { get; set; }
    public string FactoryKoreanName { get; set; }
    public string FactoryEnglishDescription { get; set; }
    public string FactoryKoreanDescription { get; set; }

    //필수 설정되어야 하는 것들 설정
    public virtual void FactoryInit(int id, int price, string englishName, string koreanName, string englishDescription,
        string koreanDescription)
    {
        FactoryId = id;
        FactoryPrice = price;
        FactoryEnglishName = englishName;
        FactoryKoreanName = koreanName;
        FactoryEnglishDescription = englishDescription;
        FactoryKoreanDescription = koreanDescription;
    }

    public abstract GameObject CreateItem(int playerId);
}