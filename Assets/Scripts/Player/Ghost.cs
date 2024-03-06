using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
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
    private Transform _prefab;
    
    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _prefab = gameObject.transform;
        _velocity = new Vector3(0f,0f,0f);
    }
    
    void Update()
    {
        _controller.Move(_velocity * Time.deltaTime);
    }
    
    public void CalculateVelocity(int keyboardInput)
    {
        Vector3 velocity;
        bool isRunning = false;
        
        //방향키가 아무것도 안눌렀다면
        if ((keyboardInput & (_upBit | _downBit | _leftBit | _rightBit)) == 0)
        {
            velocity = Vector3.zero;
        }
        else
        {
            if ((keyboardInput & _runBit) == 1)
            {
                isRunning = true;
            }
            
            if (isRunning)
            {
                velocity = _runSpeed * transform.forward;
            }
            else
            {
                velocity = _walkSpeed * transform.forward;
            }
        }
        
        if (!_controller.isGrounded)
        {
            velocity.y = -10f;
        }
        _velocity = velocity;
        
    }
    
    
}
