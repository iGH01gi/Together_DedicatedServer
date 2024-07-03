using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    CharacterController _controller;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
    }
}