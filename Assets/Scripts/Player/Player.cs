using System;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    public PlayerInfo Info { get; set; } = new PlayerInfo();
    public int RoomId { get; set; }
    public ClientSession Session { get; set; }
    
    public bool _isKiller = false; //폭탄마 여부

    CharacterController _controller;
    public GameObject _ghost;
    public Vector3 _velocity;
    public bool _isRunning = false;
    public Quaternion _targetRotation; //서버에서 받은 목표 회전값. 이 값으로 update문에서 회전시킴
    
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
    /// 자신의 ghost를 따라서 자연스럽게 움직이는 코드 (회전은 고스트 따라할필요 x)
    /// </summary>
    private void FollowGhost()
    {
        if (_ghost != null)
        {
            //목표 방향으로 회전합니다.
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * 30f);
            
            // 목표 방향을 계산합니다. _ghost.transform.position과 transform.position의 높이차는 고려하지 않고 x,z만 고려
            Vector3 directionToGhost = _ghost.transform.position - transform.position;
            directionToGhost.y = 0; // Y축을 고려하지 않음

            //목표 위치까지 거리가 beta보다 작으면 도착한것으로 간주하고 멈춤
            float beta = 0.05f;
            if (directionToGhost.magnitude < beta)
            {
                _velocity = Vector3.zero;
                _controller.Move(_velocity);
                return;
            }
            
            // 목표 방향으로 이동합니다.
            _velocity = directionToGhost.normalized;
            if (_isRunning)
            {
                _velocity *= Managers.Player._playerMoveController._runSpeed;
            }
            else
            {
                _velocity *= Managers.Player._playerMoveController._walkSpeed;
            }
            
            _velocity.y = -10f; //중력 같은 효과
            
            _controller.Move(_velocity * Time.deltaTime);
        }
    }
    
    public void CopyFrom(Player dediPlayer)
    {
        Info.PlayerId = dediPlayer.Info.PlayerId;
        Info.Name = dediPlayer.Info.Name;
        RoomId = dediPlayer.RoomId;
        Session = dediPlayer.Session;
    }
}