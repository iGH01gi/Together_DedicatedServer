using System;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.InputSystem;

public class InGameInputTemp : MonoBehaviour
{
    int _runBit = (1 << 4);
    int _upBit = (1 << 3);
    int _leftBit = (1 << 2);
    int _downBit = (1 << 1);
    int _rightBit = 1;
    
    
    public static Vector2 _moveInput;
    static int sensitivityAdjuster = 3;
    static float _walkSpeed = 5f;
    static float _runSpeed = 7.5f;
    public static float _minViewDistance = 15f;
    private float _rotationX = 0f;
    public Vector3 _velocity;
    

    CharacterController _controller;
    private Transform _camera;
    private Transform _player;
    private Transform _prefab;

    public static bool _isRunning = false;
    private void ChangeAnim()
    {
        //_player.GetComponent<PlayerAnimController>().PlayAnim(_moveInput,_isRunning);
    }
    
    void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
        ChangeAnim();
    }

    void OnRun(InputValue value)
    {
        _isRunning = value.isPressed;
        ChangeAnim();
    }

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _prefab = gameObject.transform;
        _camera = _prefab.transform.GetChild(0);
        _player = _prefab.transform.GetChild(1);
        _velocity = new Vector3(0f,0f,0f);
        
        /*Managers.Logic.SendPlayerMoveEvent-= SendMove;
        Managers.Logic.SendPlayerMoveEvent += SendMove;*/
    }
    
    void Update()
    {
        /*_camera.localRotation = Quaternion.Euler(_rotationX,0f,0f);
        _prefab.Rotate(3f * mouseX * Vector3.up);
        if (_moveInput.magnitude<=0)
        {
            _player.transform.Rotate(3f * -mouseX * Vector3.up);
        }
        if (_moveInput.magnitude > 0)
        {
            _player.transform.localRotation =
                Quaternion.AngleAxis(Mathf.Atan2(_moveInput.x, _moveInput.y) * Mathf.Rad2Deg, Vector3.up);
        }
        _velocity= CalculateVelocity(_moveInput, _prefab.localRotation);
        _controller.Move(_velocity * Time.deltaTime);*/
    }
    

    /*void SendMove()
    {
        //서버로 현재위치,쿼터니언의 4개의 부동소수점 값, 누른 키, utc타임 정보를 보냄
        CDS_Move packet = new CDS_Move();
        
        packet.MyDediplayerId = Managers.Player._myDediPlayerId;
        
        TransformInfo transformInfo = new TransformInfo();
        Vector3 position = _prefab.position;
        Quaternion rotation = _prefab.rotation;
        transformInfo.PosX = position.x;
        transformInfo.PosY = position.y;
        transformInfo.PosZ = position.z;
        transformInfo.RotX = rotation.x;
        transformInfo.RotY = rotation.y;
        transformInfo.RotZ = rotation.z;
        transformInfo.RotW = rotation.w;
        packet.Transform = transformInfo;
        
        int moveBit = 0;
        if (_isRunning)
        {
            moveBit |= _runBit;
        }
        if (_moveInput.y > 0.5f) //윗키눌림
        {
            moveBit |= _upBit;
        }
        if(_moveInput.y < -0.5f) //아래키눌림
        {
            moveBit |= _downBit;
        }
        if(_moveInput.x < -0.5f) //왼쪽키눌림
        {
            moveBit |= _leftBit;
        }
        if(_moveInput.x > 0.5f) //오른쪽키눌림
        {
            moveBit |= _rightBit;
        }
        packet.KeyboardInput = moveBit;
        
        packet.UtcTimeStamp = DateTime.UtcNow.ToBinary();
        
        Managers.Network._dedicatedServerSession.Send(packet);
    }*/

    private Vector3 CalculateVelocity(Vector2 moveInputVector, Quaternion prefabRotation)
    {
        Vector3 velocity;
        if (_isRunning)
        {
            velocity = prefabRotation.normalized * new Vector3(_runSpeed * moveInputVector.x, 0, _runSpeed * moveInputVector.y);
        }
        else
        {
            velocity = prefabRotation.normalized * new Vector3(_walkSpeed * moveInputVector.x, 0, _walkSpeed * moveInputVector.y);
        }

        if (!_controller.isGrounded)
        {
            velocity.y = -10f;
        }

        return velocity;
    }

}
