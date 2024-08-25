using Google.Protobuf;

public interface IItem
{
    public int ItemID { get; set; }
    public int PlayerID { get; set; }
    public string EnglishName { get; set; }

    /// <summary>
    /// 아이템이 생성될 때 필수로 설정되어야 하는 것들을 설정함
    /// </summary>
    public abstract void Init(int itemId, int playerId, string englishName);

    /// <summary>
    /// 아이템 사용시 기능 구현
    /// </summary>
    /// <param name="packet">아이템 사용에 필요한 패킷정보</param>
    public abstract void Use(IMessage packet);

    /// <summary>
    /// 아이템 맞았을 때의 기능
    /// </summary>
    public abstract void OnHit();
}