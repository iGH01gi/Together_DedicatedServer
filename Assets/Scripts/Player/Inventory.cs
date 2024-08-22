using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    public Dictionary<int,int> _itemCount = new Dictionary<int, int>(); //key: 아이템Id, value: 아이템 개수


    /// <summary>
    /// 아이템을 인벤에 1개 추가함
    /// </summary>
    /// <param name="itemId">아이템 id</param>
    public void AddOneItem(int itemId)
    {
        if(_itemCount.ContainsKey(itemId))
        {
            _itemCount[itemId]++;
        }
        else
        {
            _itemCount.Add(itemId, 1);
        }
    }
    
    /// <summary>
    /// 아이템을 인벤에서 1개 제거함
    /// </summary>
    /// <param name="itemId">제거할 아이템id</param>
    public void RemoveOneItem(int itemId)
    {
        if(_itemCount.ContainsKey(itemId))
        {
            _itemCount[itemId]--;
            
            if(_itemCount[itemId] == 0)
            {
                _itemCount.Remove(itemId);
            }
        }
    }

    /// <summary>
    /// 현재 보유하고 있는 아이템 개수를 반환함
    /// </summary>
    /// <param name="itemId">개수를 확인하려는 아이템id</param>
    /// <returns>없다면 0리턴</returns>
    public int GetItemCount(int itemId)
    {
        if (_itemCount.ContainsKey(itemId))
        {
            return _itemCount[itemId];
        }
        else
        {
            return 0;
        }
    }

    public void Clear()
    {
        _itemCount.Clear();
    }

}