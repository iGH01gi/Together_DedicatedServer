using UnityEngine;

public class TheHeartless : MonoBehaviour, IKiller
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
    public float SkillCoolTimeSeconds { get; set; } //스킬 쿨타임 초
    
    
    //이 킬러만의 속성
    public float HeartlessSeconds { get; set; } //심장소리 안들리게 할 시간
    
    public void Setting()
    {

    }

    public void Use()
    {

    }
}