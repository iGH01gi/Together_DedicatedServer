using System;
using Google.Protobuf.Protocol;
using UnityEngine;

public class Dash : MonoBehaviour, IItem
{
    //IItem 인터페이스 구현
    public int ItemID { get; set; }
    public int PlayerID { get; set; }
    public string EnglishName { get; set; }


    //이 아이템만의 속성
    public float DashDistance { get; set; }

    private Player _player;
    private CharacterController _characterController;
    private float _dashTime = 0.24f; //대시 시간(애니메이션 재생 시간) (무적시간이기도 함)
    private float _dashSpeed; //대시 속도
    private bool _isDashing = false; //대시 중인지 여부

    void Update()
    {
        if (_isDashing && !Managers.Player.IsPlayerDead(PlayerID))
        {
            //대시 속도만큼 이동
            _characterController.Move(_player.transform.forward * _dashSpeed * Time.deltaTime);
            //고스트도 같이 이동
            _player._ghost.transform.position = transform.position;
            _player._ghost.transform.rotation = transform.rotation;

            _dashTime -= Time.deltaTime;
            if (_dashTime <= 0)
            {
                //대시 끝 패킷 브로드캐스트
                DSC_EndDashItem endDashItemPacket = new DSC_EndDashItem()
                {
                    PlayerId = PlayerID,
                    ItemId = ItemID,
                    DashEndTransform = new TransformInfo()
                    {
                        Position = new PositionInfo()
                        {
                            PosX = transform.position.x,
                            PosY = transform.position.y,
                            PosZ = transform.position.z
                        },
                        Rotation = new RotationInfo()
                        {
                            RotX = transform.rotation.x,
                            RotY = transform.rotation.y,
                            RotZ = transform.rotation.z,
                            RotW = transform.rotation.w
                        }
                    }

                };

                _isDashing = false;

                //고스트 따라가기 기능 다시 활성화 코드 추가
                _player.ToggleFollowGhost(true);

                //TODO: 대시 무적 해제 코드 추가
            }
        }
    }

    public void Init(int itemId, int playerId, string englishName)
    {
        this.ItemID = itemId;
        this.PlayerID = playerId;
        this.EnglishName = englishName;
    }

    public void Init(int itemId, int playerId, string englishName, float dashDistance)
    {
        Init(itemId, playerId, englishName);
        DashDistance = dashDistance;
    }

    public void Use()
    {
        //대시 시간(애니메이션 재생 시간) (무적시간이기도 함)
        float dashTime = 0.24f;

        //TODO: 대시 동안 무적처리 코드 추가

        //고스트 따라가기 기능 멈춤(대시 사용을 위해서)
        GameObject playerObjet = Managers.Player._players[PlayerID];
        _player = playerObjet.GetComponent<Player>();
        _player.ToggleFollowGhost(false);

        _characterController = playerObjet.GetComponent<CharacterController>();

        //대시 시작 패킷 브로드캐스트
        DSC_UseDashItem useDashItemPacket = new DSC_UseDashItem();
        useDashItemPacket.PlayerId = PlayerID;
        useDashItemPacket.ItemId = ItemID;
        useDashItemPacket.DashStartingTransform.Position.PosX = playerObjet.transform.position.x;
        useDashItemPacket.DashStartingTransform.Position.PosY = playerObjet.transform.position.y;
        useDashItemPacket.DashStartingTransform.Position.PosZ = playerObjet.transform.position.z;
        useDashItemPacket.DashStartingTransform.Rotation.RotX = playerObjet.transform.rotation.x;
        useDashItemPacket.DashStartingTransform.Rotation.RotY = playerObjet.transform.rotation.y;
        useDashItemPacket.DashStartingTransform.Rotation.RotZ = playerObjet.transform.rotation.z;
        useDashItemPacket.DashStartingTransform.Rotation.RotW = playerObjet.transform.rotation.w;
        Managers.Player.Broadcast(useDashItemPacket);

        //DashDistance만큼의 거리를 dashTime동안 이동하려면 속도가 몇이어야 하는지
        _dashSpeed = DashDistance / _dashTime;

        //대시 시작(update문에서 대시 수행)
        _isDashing = true;
    }

    public void OnHit()
    {
    }
}