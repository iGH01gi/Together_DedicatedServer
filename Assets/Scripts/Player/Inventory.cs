using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    public Dictionary<int,int> _itemCount; //key: 아이템Id, value: 아이템 개수
    public Dictionary<int, List<GameObject>> _ownedItems; //key: 아이템Id, value: 아이템 오브젝트
    
    /// <summary>
    /// 아이템을 인벤에 1개 추가함
    /// </summary>
    /// <param name="item">아이템 컴포넌트</param>
    public void AddOneItem(GameObject itemObject)
    {
        IItem item = itemObject.GetComponent<IItem>();
        if(_itemCount.ContainsKey(item.Id) && _ownedItems.ContainsKey(item.Id))
        {
            _itemCount[item.Id]++;
            _ownedItems[item.Id].Add(itemObject);
        }
        else
        {
            _itemCount.Add(item.Id, 1);
            _ownedItems.Add(item.Id, new List<GameObject>(){itemObject});
            _ownedItems[item.Id].Add(itemObject);
        }
    }
    
    /// <summary>
    /// 아이템을 인벤에서 1개 제거함
    /// </summary>
    /// <param name="itemId">제거할 아이템id</param>
    public void RemoveOneItem(int itemId)
    {
        if(_itemCount.ContainsKey(itemId) && _ownedItems.ContainsKey(itemId))
        {
            _itemCount[itemId]--;
            _ownedItems[itemId].RemoveAt(0);
            
            if(_itemCount[itemId] == 0)
            {
                _itemCount.Remove(itemId);
            }
            if(_ownedItems[itemId].Count == 0)
            {
                _ownedItems.Remove(itemId);
            }
        }
    }
    
}