using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum Scene
    {
        //Scene Types that can occur
        Unknown,
        Lobby,
        InGame,
    }

    public enum UIEvent
    {
        //UI Events that can occur
        Click,
        Drag
    }

    public enum PlayerAction
    {
        Idle,
        Run,
        Walk,
        Jump,
    }

    public enum HttpMethod
    {
        Get,
        Post,
        Put,
        Delete,
        Patch,
    }

    public enum InputType
    {
        UIInputHandler
    }

}