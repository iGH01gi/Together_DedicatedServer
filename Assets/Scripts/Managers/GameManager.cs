using UnityEngine;

public class GameManager
{
    public GameObject root;
    
    public GaugeController _gaugeController;
    public CleanseController _cleanseController;

    public bool _isGameEnd = false; //게임이 끝났는지 여부

    //Managers Init과 함께 불리는 Init
    public void Init()
    {
        root = GameObject.Find("@Game");
        if (root == null)
        {
            root = new GameObject { name = "@Game" };
            Object.DontDestroyOnLoad(root);
        }
        
        _gaugeController = Util.GetOrAddComponent<GaugeController>(root);
        _cleanseController = Util.GetOrAddComponent<CleanseController>(root);
        _isGameEnd = false;
    }

    /// <summary>
    /// 게임이 종료됐는지 여부를 반환 (최종 승자가 나왔는지)
    /// </summary>
    /// <returns>종료됐다면 true, 진행중이면 false</returns>
    public bool IsGameEnd()
    {
        return _isGameEnd;
    }

    /// <summary>
    /// 게임이 종료됐음을 설정
    /// </summary>
    public void SetGameEnd()
    {
        _isGameEnd = true;
    }

}