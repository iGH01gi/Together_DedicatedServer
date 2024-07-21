public interface IKiller
{
    //킬러 공통 보유 속성
    public int Id { get; set; }
    public string EnglishName { get; set; }
    public string KoreanName { get; set; }
    public string EnglishDescription { get; set; }
    public string KoreanDescription { get; set; }
    public string EnglishAbilityName { get; set; }
    public string KoreanAbilityName { get; set; }
    public string EnglishAbilityDescription { get; set; }
    public string KoreanAbilityDescription { get; set; }
    public float SkillCoolTimeSeconds { get; set; } 
    
    
    /// <summary>
    /// 킬러가 생성될때 설정되어야 하는 것들을 설정함(json 데이터로 값 설정 등...)
    /// </summary>
    void Setting();

    /// <summary>
    /// 스킬 사용시 기능 구현
    /// </summary>
    void Use();
}