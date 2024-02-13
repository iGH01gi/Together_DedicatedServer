using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.InGame;
        
        //Managers.Sound.Play("Bgm/test_bgm",Define.Sound.Bgm);
    }

    public override void Clear()
    {
        
    }
}