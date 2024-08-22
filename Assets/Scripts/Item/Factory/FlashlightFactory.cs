using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightFactory : ItemFactory
{
    public float BlindDuration { get; set; }
    public float FlashlightDistance { get; set; }
    public float FlashlightAngle { get; set; }
    public float FlashlightAvailableTime { get; set; }
    public float FlashlightTimeRequired { get; set; }

    public FlashlightFactory(int id, int price, string englishName, string koreanName, string englishDescription,
        string koreanDescription, float blindDuration, float flashlightDistance, float flashlightAngle, float flashlightAvailableTime, float flashlightTimeRequired)
    {
        base.FactoryInit(id, price, englishName, koreanName, englishDescription, koreanDescription);
        BlindDuration = blindDuration;
        FlashlightDistance = flashlightDistance;
        FlashlightAngle = flashlightAngle;
        FlashlightAvailableTime = flashlightAvailableTime;
        FlashlightTimeRequired = flashlightTimeRequired;
    }

    public override IItem CreateItem(int playerId)
    {
        //if else를 통해 여기서 맞는 아이템을 생성
        return null;
    }
}