using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Protocol;
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



    private bool _isLightOn = false;
    private GameObject _lightGameObject;
    private Coroutine _currentPlayingCoroutine;
    private Quaternion _originalLightRotation;

    public void LateUpdate()
    {
        if (_isLightOn)
        {
            
        }
    }


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
        //이미 사용중인데 또 사용하려고 하면, 기존 코루틴 종료하고 코루틴 다시시작
        if (_isLightOn)
        {
            StopCoroutine(_currentPlayingCoroutine);
            _currentPlayingCoroutine = StartCoroutine(LightOffAfterSeconds(FlashlightAvailableTime));
            return;
        }

        GameObject playerGameObject = Managers.Player._players[PlayerID];
        GameObject flashlightGameObject = Util.FindChild(playerGameObject, "3", true);

        if (flashlightGameObject != null)
        {
            _lightGameObject = Util.FindChild(flashlightGameObject, "Light", true);
            if (_lightGameObject != null)
            {
                //회전 원복을 위한 값 저장
                _originalLightRotation = _lightGameObject.transform.rotation;

                //불 킴
                _isLightOn = true;

                //일정 시간 후 불 끔
                _currentPlayingCoroutine = StartCoroutine(LightOffAfterSeconds(FlashlightAvailableTime));
            }
        }
    }

    IEnumerator LightOffAfterSeconds(float seconds)
    {
        //손전등 켰다고 브로드캐스트
        DSC_UseFlashlightItem useFlashlightItemPacket = new DSC_UseFlashlightItem()
        {
            PlayerId = PlayerID,
            ItemId = ItemID,
        };
        Managers.Player.Broadcast(useFlashlightItemPacket);

        yield return new WaitForSeconds(seconds);
        _isLightOn = false;

        //회전 원복
        _lightGameObject.transform.rotation = _originalLightRotation;

        //파괴
        Destroy(gameObject);
    }

    public void OnHit()
    {

    }
}