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

    public override GameObject CreateItem(int playerId)
    {
        GameObject invisibleGameObject = new GameObject("Invisible");
        Invisible invisible = invisibleGameObject.AddComponent<Invisible>();
        invisible.Init(FactoryId, playerId, FactoryEnglishName, InvisibleSeconds);
        return invisibleGameObject;
    }
}