using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers _instance; 
    static Managers Instance {get { Init(); return _instance; } } 
    
    ResourceManager _resource = new ResourceManager();
    PoolManager _pool = new PoolManager();
    SceneManagerEx _scene = new SceneManagerEx();
    PlayerManager _player = new PlayerManager();
    InputManager _input = new InputManager();
    NetworkManager _network = new NetworkManager();
    
    public static  ResourceManager Resource { get { return Instance._resource;} }
    public static PoolManager Pool { get { return Instance._pool; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    public static PlayerManager Player { get { return Instance._player; } }
    public static InputManager Input { get { return Instance._input; } }
    public static NetworkManager Network { get { return Instance._network; } }
    


    void Start()
    {
        Init();
    }
    
    void Update()
    {

    }

    static void Init()
    {
        if (_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            _instance = go.GetComponent<Managers>();
            _instance._pool.Init();
            _instance._input.Init();
            _instance._network.Init();
        }
    }
    
    
    public static void Clear()
    {
        Scene.Clear();
        Pool.Clear();
    }
    
}
