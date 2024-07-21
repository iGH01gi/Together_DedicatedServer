public interface IItem
{
    //아이템 공통 보유 속성
    public int Id { get; set; }
    public int Price { get; set; }
    public string EnglishName { get; set; }
    public string KoreanName { get; set; }
    public string EnglishDescription { get; set; }
    public string KoreanDescription { get; set; }

    /// <summary>
    /// 아이템이 생성될 때 설정되어야 하는 것들을 설정함(json 데이터로 값 설정 등...)
    /// </summary>
    void Setting();

    /// <summary>
    /// 아이템 사용시 기능 구현
    /// </summary>
    void Use();

    /// <summary>
    /// 아이템 선택시 기능 구현(사용은 안하고 들고만 있는 상태)
    /// </summary>
    void OnHold(); //(설치 가능여부 이런것도 여기서 구현 가능)
}