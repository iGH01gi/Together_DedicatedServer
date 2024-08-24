using UnityEngine;

public class DashFactory : ItemFactory
{
    public float DashDistance { get; set; }

    public DashFactory(int id, int price, string englishName, string koreanName, string englishDescription,
        string koreanDescription, float dashDistance)
    {
        base.FactoryInit(id, price, englishName, koreanName, englishDescription, koreanDescription);
        DashDistance = dashDistance;
    }

    public override GameObject CreateItem(int playerId)
    {
        GameObject dashGameObject = new GameObject("Dash");
        Dash dash = dashGameObject.AddComponent<Dash>();
        dash.Init(FactoryId, playerId, FactoryEnglishName, DashDistance);
        return dashGameObject;
    }
}