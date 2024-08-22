using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisibleFactory : ItemFactory
{
    public float InvisibleSeconds { get; set; }

    public InvisibleFactory(int id, int price, string englishName, string koreanName, string englishDescription,
        string koreanDescription, float invisibleSeconds)
    {
        base.FactoryInit(id, price, englishName, koreanName, englishDescription, koreanDescription);
        InvisibleSeconds = invisibleSeconds;
    }

    public override IItem CreateItem(int playerId)
    {
        //if else를 통해 여기서 맞는 아이템을 생성
        return null;
    }
}