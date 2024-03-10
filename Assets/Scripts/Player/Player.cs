using System;
using Google.Protobuf.Protocol;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerInfo Info { get; set; } = new PlayerInfo();
    public int RoomId { get; set; }
    public ClientSession Session { get; set; }

    CharacterController _controller;
    public GameObject _ghost;
    float rotationSpeed = 20f; // 회전 속도를 조절합니다.
    public Vector3 _velocity;
    bool _isRunning = false;
    Quaternion _ghostRotation;

    int _runBit = (1 << 4);
    int _upBit = (1 << 3);
    int _leftBit = (1 << 2);
    int _downBit = (1 << 1);
    int _rightBit = 1;

    static float _walkSpeed = 5f;
    static float _runSpeed = 7.5f;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _velocity = new Vector3(0f, 0f, 0f);
        _isRunning = false;
    }

    private void Update()
    {
        if (_ghost == null)
        {
            _ghost = GameObject.Find("Ghost_" + Info.PlayerId);
        }


        FollowGhost();
    }

    /// <summary>
    /// 자신의 ghost를 따라서 자연스럽게 움직이는 코드
    /// </summary>
    private void FollowGhost()
    {
        if (_ghost != null)
        {
            // 목표 방향을 계산합니다.
            Vector3 directionToGhost = _ghost.transform.position - transform.position;
            directionToGhost.y = 0;

            //목표 위치까지 거리가 0.1보다 작으면 도착한것으로 간주하고 실제 패킷의 회전방향으로 부드럽게 돌려줌
            if (directionToGhost.magnitude < 0.1f)
            {
                Util.PrintLog("도착했음");
                _velocity = Vector3.zero;
                _controller.Move(_velocity);
                transform.rotation = Quaternion.Slerp(transform.rotation,_ghostRotation, Time.deltaTime * rotationSpeed);
                return;
            }

            // 현재 회전에서 목표 회전까지 부드럽게 회전시킵니다.
            Quaternion targetRotation = Quaternion.LookRotation(directionToGhost);
            transform.rotation = targetRotation;

            // 목표 방향으로 이동합니다.
            _velocity = directionToGhost.normalized;
            if (_isRunning)
            {
                _velocity *= _runSpeed;
            }
            else
            {
                _velocity *= _walkSpeed;
            }
            
            
            if (!_controller.isGrounded)
            {
                _velocity.y = -10f;
            }
            
            _controller.Move(_velocity * Time.deltaTime);
        }
    }

    public void SetGhostLastState(int keyboardInput, Quaternion localRotation)
    {
        Vector2 moveInputVector = new Vector2();

        if ((keyboardInput & _runBit) == 1)
        {
            _isRunning = true;
        }
        else
        {
            _isRunning = false;
        }
        
        _ghostRotation = localRotation;
    }

    public void CopyFrom(Player dediPlayer)
    {
        Info.PlayerId = dediPlayer.Info.PlayerId;
        Info.Name = dediPlayer.Info.Name;
        RoomId = dediPlayer.RoomId;
        Session = dediPlayer.Session;
    }
}