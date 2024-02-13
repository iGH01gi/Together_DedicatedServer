using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimController : MonoBehaviour
{
    private Animator _anim;

    private void Start()
    {
        _anim = GetComponent<Animator>();
    }

    public void PlayAnim(Define.PlayerAction name)
    {
        switch (name)
        {
            case Define.PlayerAction.Idle:
                PlayerAnimClear();
                break;
            case Define.PlayerAction.Run:
                PlayerAnimClear();
                _anim.SetBool("isRunning", true);
                break;
            case Define.PlayerAction.Walk:
                PlayerAnimClear();
                _anim.SetBool("isWalking", true);
                break;
            case Define.PlayerAction.Jump:
                Debug.Log("Jump Jump!");
                _anim.SetTrigger("isJumping");
                break;
            default:
                PlayerAnimClear();
                break;
        }
    }
    
    void PlayerAnimClear()
    {
        _anim.SetBool("isRunning", false);
        _anim.SetBool("isWalking",false);
    }
}