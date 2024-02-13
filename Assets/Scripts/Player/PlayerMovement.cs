using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    public static bool _playerIsMoving;
    //probably should be stored elsewhere later
    public float _walkSpeed = 0.02f;
    public float _runSpeed = 0.03f;
    private PlayerAnimController _anim;
    private float timeCount = 0;
    private float slerpFactor = 0.01f;

    private Vector2 moveInput;
    private float runInput;
    private float jumpInput;

    private void Start()
    {
        if (transform.GetComponent<PlayerInput>() != null)
        {
            //
            transform.GetComponent<PlayerInput>().enabled = true;
        }
        _anim = transform.GetChild(1).GetComponent<PlayerAnimController>();
        _playerIsMoving = false;
    }

    private void Update()
    {
        if (jumpInput > 0)
        {
            _anim.PlayAnim(Define.PlayerAction.Jump);
        }

        if (moveInput!= Vector2.zero)
        {
            _playerIsMoving = true;
            if (runInput>0)
            {
                _anim.PlayAnim(Define.PlayerAction.Run);
                Move(_runSpeed);
            }
            else
            {
                _anim.PlayAnim(Define.PlayerAction.Walk);
                Move(_walkSpeed);
            }
        }
        else
        {
            _playerIsMoving = false;
            _anim.PlayAnim(Define.PlayerAction.Idle);
        }
    }

    private void Move(float moveSpeed)
    {
        Vector3 newPos = transform.rotation.normalized * new Vector3(moveSpeed * moveInput.x, 0f, moveSpeed * moveInput.y);
        LookDirection(Mathf.Atan2(moveInput.x,moveInput.y)* Mathf.Rad2Deg);
        transform.position += newPos;
    }

    private void LookDirection(float angle)
    {
        timeCount += (Time.deltaTime * slerpFactor);
        transform.GetChild(1).transform.localRotation = Quaternion.Slerp(transform.GetChild(1).transform.localRotation,Quaternion.AngleAxis(angle, Vector3.up),timeCount);
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnRun(InputValue value)
    {
       runInput = value.Get<float>();
    }
    
    void OnJump(InputValue value)
    {
        jumpInput = value.Get<float>();
        Debug.Log(jumpInput);
    }
}
