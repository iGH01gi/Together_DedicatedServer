using UnityEngine;

public class GameManager
{
    public GameObject root;
    
    public GaugeController _gaugeController;
    public CleanseController _cleanseController;

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
    }

}