using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using UnityEngine;

public class Flashlight : MonoBehaviour, IItem
{
    public int ItemID { get; set; }
    public int PlayerID { get; set; }
    public string EnglishName { get; set; }

    public float BlindDuration { get; set; }
    public float FlashlightDistance { get; set; }
    public float FlashlightAngle { get; set; }
    public float FlashlightAvailableTime { get; set; }
    public float FlashlightTimeRequired { get; set; }

    public void Init(int itemId, int playerId, string englishName)
    {
        this.ItemID = itemId;
        this.PlayerID = playerId;
        this.EnglishName = englishName;
    }

    public void Init(int itemId, int playerId, string englishName, float blindDuration, float flashlightDistance, float flashlightAngle, float flashlightAvailableTime, float flashlightTimeRequired)
    {
        Init(itemId, playerId, englishName);
        BlindDuration = blindDuration;
        FlashlightDistance = flashlightDistance;
        FlashlightAngle = flashlightAngle;
        FlashlightAvailableTime = flashlightAvailableTime;
        FlashlightTimeRequired = flashlightTimeRequired;
    }

    public void Use(IMessage packet)
    {
        Debug.Log("Item Flashlight Use");
    }

    public void OnHit()
    {

    }
}