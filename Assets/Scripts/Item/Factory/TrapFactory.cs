using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapFactory : ItemFactory
{
    public float TrapDuration { get; set; }
    public float TrapRadius { get; set; }
    public float StunDuration { get; set; }

    public TrapFactory(int id, int price, string englishName, string koreanName, string englishDescription,
        string koreanDescription, float trapDuration, float trapRadius, float stunDuration)
    {
        base.FactoryInit(id, price, englishName, koreanName, englishDescription, koreanDescription);
        TrapDuration = trapDuration;
        TrapRadius = trapRadius;
        StunDuration = stunDuration;
    }

    public override GameObject CreateItem(int playerId)
    {
        //if else를 통해 여기서 맞는 아이템을 생성
        return null;
    }
}