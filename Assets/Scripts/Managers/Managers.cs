using System;
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
    SessionManager _session = new SessionManager();
    
    public static  ResourceManager Resource { get { return Instance._resource;} }
    public static PoolManager Pool { get { return Instance._pool; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    public static PlayerManager Player { get { return Instance._player; } }
    public static InputManager Input { get { return Instance._input; } }
    public static NetworkManager Network { get { return Instance._network; } }
    public static SessionManager Session { get { return Instance._session; } }


    void Start()
    {
        Init();
    }

    //private float _logicalInterval = 0.1f; //0.1초마다 게임 로직 시뮬레이션 (클라와 맞춰야함 파일 불러오기 등으로)
    //private float _passedTime=0.0f; //마지막 로직처리후 흐른 시간
    
    void Update()
    {
        _network.Update();
        JobTimer.Instance.Flush();
        
        /*_passedTime += Time.deltaTime;
        if (_passedTime >= _logicalInterval)
        {
            //실제 시뮬레이션 게임로직 처리
            
            //로직처리후 흐른시간 초기화
            _passedTime = 0;
        }*/
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
            _instance._session.Init();
        }
    }
    
    
    public static void Clear()
    {
        Scene.Clear();
        Pool.Clear();
    }
    

    
}
