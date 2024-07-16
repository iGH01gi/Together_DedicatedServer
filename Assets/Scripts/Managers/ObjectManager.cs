using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

/// <summary>
/// 인게임에 등장하는 모든 게임 오브젝트들을 관리하는 매니저(스폰, 제거, 이동, 상태변경 등)
/// </summary>
public class ObjectManager
{
    public GameObject root;
    
    public ChestController _chestController;
    public CleanseController _cleanseController;
    
    //Managers Init에서 호출
    public void Init()
    {
        root = GameObject.Find("@Object");
        if (root == null)
        {
            root = new GameObject { name = "@Object" };
            Object.DontDestroyOnLoad(root);
        }
        
        _chestController = Util.GetOrAddComponent<ChestController>(root);
        _chestController.Init();
        
        _cleanseController = Util.GetOrAddComponent<CleanseController>(root);
    }
}
    
