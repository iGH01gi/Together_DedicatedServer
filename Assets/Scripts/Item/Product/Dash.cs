using System;
using Google.Protobuf;
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
    private float _dashTime = 0.35f; //대시 시간(애니메이션 재생 시간) (무적시간이기도 함)
    private float _dashSpeed; //대시 속도
    private bool _isDashing = false; //대시 중인지 여부

    void Update()
    {
        if (_isDashing && !Managers.Player.IsPlayerDead(PlayerID))
        {
            //대시 속도만큼 이동
            _characterController.Move(_player.transform.forward * _dashSpeed * Time.deltaTime);
            //고스트도 같이 이동
            _player._ghost.transform.position = _player.transform.position;
            _player._ghost.transform.rotation = _player.transform.rotation;

            _dashTime -= Time.deltaTime;
            if (_dashTime <= 0)
            {
                _isDashing = false;

                //대시 끝 패킷 브로드캐스트
                DSC_EndDashItem endDashItemPacket = new DSC_EndDashItem()
                {
                    PlayerId = PlayerID,
                    ItemId = ItemID,
                    DashEndTransform = new TransformInfo()
                    {
                        Position = new PositionInfo()
                        {
                            PosX = _player.transform.position.x,
                            PosY = _player.transform.position.y,
                            PosZ = _player.transform.position.z
                        },
                        Rotation = new RotationInfo()
                        {
                            RotX = _player.transform.rotation.x,
                            RotY = _player.transform.rotation.y,
                            RotZ = _player.transform.rotation.z,
                            RotW = _player.transform.rotation.w
                        }
                    }
                };
                Managers.Player.Broadcast(endDashItemPacket);


                //고스트 따라가기 기능 다시 활성화 코드 추가
                _player.ToggleFollowGhost(true);

                //TODO: 대시 무적 해제 코드 추가

                //대시가 끝났으므로 대시오브젝트 삭제
                Destroy(gameObject);
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

    public void Use(IMessage packet)
    {

        //TODO: 대시 동안 무적처리 코드 추가

        //고스트 따라가기 기능 멈춤(대시 사용을 위해서)
        GameObject playerObjet = Managers.Player._players[PlayerID];
        _player = playerObjet.GetComponent<Player>();
        _player.ToggleFollowGhost(false);

        _characterController = playerObjet.GetComponent<CharacterController>();

        //대시 시작 패킷 브로드캐스트
        DSC_UseDashItem useDashItemPacket = new DSC_UseDashItem
        {
            PlayerId = PlayerID,
            ItemId = ItemID,
            DashStartingTransform = new TransformInfo()
            {
                Position = new PositionInfo()
                {
                    PosX = playerObjet.transform.position.x,
                    PosY = playerObjet.transform.position.y,
                    PosZ = playerObjet.transform.position.z
                },
                Rotation = new RotationInfo()
                {
                    RotX = playerObjet.transform.rotation.x,
                    RotY = playerObjet.transform.rotation.y,
                    RotZ = playerObjet.transform.rotation.z,
                    RotW = playerObjet.transform.rotation.w
                }
            }
        };
        Managers.Player.Broadcast(useDashItemPacket);

        //DashDistance만큼의 거리를 dashTime동안 이동하려면 속도가 몇이어야 하는지
        _dashSpeed = DashDistance / _dashTime;

        //시작위치 클라와 맞춤 (고스트도 포함)
        playerObjet.transform.position = new Vector3(useDashItemPacket.DashStartingTransform.Position.PosX, useDashItemPacket.DashStartingTransform.Position.PosY, useDashItemPacket.DashStartingTransform.Position.PosZ);
        playerObjet.transform.rotation = new Quaternion(useDashItemPacket.DashStartingTransform.Rotation.RotX, useDashItemPacket.DashStartingTransform.Rotation.RotY, useDashItemPacket.DashStartingTransform.Rotation.RotZ, useDashItemPacket.DashStartingTransform.Rotation.RotW);
        _player._ghost.transform.position = playerObjet.transform.position;
        _player._ghost.transform.rotation = playerObjet.transform.rotation;


        //대시 시작(update문에서 대시 수행)
        _isDashing = true;

        Util.PrintLog($"Player{PlayerID} Use Item Dash");
    }

    public void OnHit()
    {
    }
}