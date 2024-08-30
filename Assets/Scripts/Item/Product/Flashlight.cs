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
    private GameObject _flashLightSource;
    private Light _light;
    private Coroutine _currentPlayingCoroutine;
    private float _angleLimit = 50; //플레이어 눈 forward와 빛 과

    public void LateUpdate()
    {
        if (_isLightOn)
        {
            //빛과 동일한 길이의 레이 표시
            Debug.DrawRay(_flashLightSource.transform.position, _flashLightSource.transform.forward * FlashlightDistance, Color.red, 0.1f);

            //회전 목표 카메라 위치를 가져옴
            Quaternion targetRotation = Managers.Player._players[PlayerID].GetComponent<Player>()._cameraWorldRotation;

            // 현재 회전을 가져옵니다.
            Quaternion currentRotation = _flashLightSource.transform.rotation;

            // 현재 회전의 Euler 각도를 가져옵니다.
            Vector3 eulerAngles = currentRotation.eulerAngles;

            // X축 회전값을 _movementInput._rotationX로 설정합니다.
            float newXRotation = targetRotation.eulerAngles.x;

            // 새로운 회전값을 적용합니다.
            Quaternion newRotation = Quaternion.Euler(newXRotation, eulerAngles.y, eulerAngles.z);
            _flashLightSource.transform.rotation = newRotation;

            //손전등에서 빛 길이만큼 ray를 쏴서 SurvivorTrigger든 KillerTrigger든 첫번째로 감지된 콜라이더를 구함
            Ray ray = new Ray(_flashLightSource.transform.position, _flashLightSource.transform.forward);

            // 모든 충돌된 콜라이더를 배열로 반환
            RaycastHit[] hits = Physics.RaycastAll(ray, FlashlightDistance);

            // 배열을 순회하며 "Eye" 태그를 가진 콜라이더를 찾음
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Eye"))
                {
                    Debug.Log("Raycast hit: " + hit.collider.name);

                    //감지된 콜라이더의 부모 오브젝트를 가져옴
                    GameObject eyeGameObject = hit.collider.gameObject;

                    //빛 시작점과, Eye의 거리가 FlashlightDistance이하인지 계산
                    if (Vector3.Distance(_flashLightSource.transform.position, eyeGameObject.transform.position) <=
                        FlashlightDistance)
                    {

                        //_flashLightSource의 backward 방향과 eyeGameObject의 forward 방향이 이루는 각도가 50도 이하인지 계산
                        if (Vector3.Angle(-_flashLightSource.transform.forward, eyeGameObject.transform.forward) <=
                            _angleLimit)
                        {
                            Debug.Log("Flashlight hit Eye with angle limit");

                            DSC_OnHitFlashlightItem onHitFlashlightItemPacket = new DSC_OnHitFlashlightItem()
                            {
                                PlayerId = PlayerID,
                                ItemId = ItemID
                            };
                            Managers.Player.Broadcast(onHitFlashlightItemPacket);
                        }
                    }

                    // 원하는 콜라이더를 찾았으므로, 더 이상 순회하지 않음
                    break;
                }
            }

            /*RaycastHit hit;
            if (Physics.Raycast(_flashLightSource.transform.position, _flashLightSource.transform.forward, out hit, FlashlightDistance))
            {
                //감지된 콜라이더의 태그가 Eye이면
                if (hit.collider.CompareTag("Eye"))
                {
                    //감지된 콜라이더의 부모 오브젝트를 가져옴
                    GameObject eyeGameObject = hit.collider.gameObject;

                    //빛 시작점과, Eye의 거리가 FlashlightDistance이하인지 계산
                    if (Vector3.Distance(_flashLightSource.transform.position, eyeGameObject.transform.position) <=
                        FlashlightDistance)
                    {
                        Debug.Log("Flashlight hit Eye");
                        //_light의 backward 방향과 eyeGameObject의 forward 방향이
                    }
                }
            }*/
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
        _flashLightSource = Util.FindChild(playerGameObject, "FlashLightSource", true);

        if (_flashLightSource != null)
        {
            _light = _flashLightSource.GetComponent<Light>();

            //불 킴
            _isLightOn = true;

            //일정 시간 후 불 끔
            _currentPlayingCoroutine = StartCoroutine(LightOffAfterSeconds(FlashlightAvailableTime));
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

        //파괴
        Destroy(gameObject);
    }

    public void OnHit()
    {

    }
}