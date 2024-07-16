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
    InputManager _input = new InputManager();
    NetworkManager _network = new NetworkManager();
    SessionManager _session = new SessionManager();
    PlayerManager _player;
    ObjectManager _object = new ObjectManager();
    LogicManager _logic = new LogicManager();
    TimeManager _time;
    GameManager _game = new GameManager();
    
    
    public static  ResourceManager Resource { get { return Instance._resource;} }
    public static PoolManager Pool { get { return Instance._pool; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    public static InputManager Input { get { return Instance._input; } }
    public static NetworkManager Network { get { return Instance._network; } }
    public static SessionManager Session { get { return Instance._session; } }
    public static PlayerManager Player { get { return Instance._player; } }
    public static ObjectManager Object { get { return Instance._object; } }
    public static LogicManager Logic { get { return Instance._logic; } }
    public static TimeManager Time { get { return Instance._time; } }
    public static GameManager Game { get { return Instance._game; } }


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
        MainThreadJobQueue.Instance.Flush(); //메인쓰레드에서 처리하도록 일감만 밀어넣고 직접 실행X하기 위한 잡큐 (discoonect처리하고 있음)
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
                go.AddComponent<PlayerManager>(); //특별처리 (모노비헤비어)
                go.AddComponent<TimeManager>(); //특별처리 (모노비헤비어)
            }

            DontDestroyOnLoad(go);
            _instance = go.GetComponent<Managers>();
            _instance._player = go.GetComponent<PlayerManager>(); //특별처리 (모노비헤비어)
            _instance._object.Init();
            _instance._pool.Init();
            _instance._input.Init();
            _instance._network.Init();
            _instance._session.Init();
            _instance._player.Init();
            _instance._time = go.GetComponent<TimeManager>(); //특별처리 (모노비헤비어)
            _instance._game.Init();
        }
    }
    
    
    public static void Clear()
    {
        Scene.Clear();
        Pool.Clear();
    }
    

    
}
