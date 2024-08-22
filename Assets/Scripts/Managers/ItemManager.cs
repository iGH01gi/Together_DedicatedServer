using System;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf.Protocol;
using Newtonsoft.Json.Linq;
using UnityEngine;

/// <summary>
/// json 데이터로부터 아이템 데이터를 로드하고, 아이템을 생성하기 위해서 필요한 클래스
/// </summary>
public class ItemManager
{
    private string _jsonPath;
    private Dictionary<int, ItemFactory> _itemFactories; //key: 아이템Id, value: 아이템 팩토리 객체
    public Dictionary<int, IItem> _items; //key: 아이템Id, value: 아이템 객체(아이템별 데이터 저장용. 전시품이라고 생각)
    private static string _itemsDataJson; //json이 들어 있게 됨(파싱 해야 함)
    
    public void Init()
    {
        _jsonPath = Application.streamingAssetsPath + "/Data/Item/Items.json";
        InitItemFactories();
        LoadItemData();
    }
    
    /// <summary>
    /// 아이템을 구매해서 인벤토리에 추가
    /// </summary>
    /// <param name="playerId">플레이어id</param>
    /// <param name="itemId">구매하려는 아이템id</param>
    /// <returns>구매 성공 여부</returns>
    public bool BuyItem(int playerId, int itemId)
    {
        //아이템 가격만큼 포인트 차감
        int price = GetItemPrice(itemId);
        Player dediPlayer = Managers.Player._players[playerId].GetComponent<Player>();
        if(dediPlayer._totalPoint < price)
        {
            Util.PrintLog($"not enough money");
            return false;
        }

        dediPlayer._totalPoint -= price;

        //인벤토리에 생성된 아이템 추가
        dediPlayer._inventory.AddOneItem(itemId);

        //구매 성공 패킷 전송
        DSC_ItemBuyResult itemBuySuccessPacket = new DSC_ItemBuyResult();
        itemBuySuccessPacket.PlayerId = playerId;
        itemBuySuccessPacket.ItemId = itemId;
        itemBuySuccessPacket.ItemTotalCount = dediPlayer._inventory.GetItemCount(itemId);
        itemBuySuccessPacket.IsSuccess = true;
        itemBuySuccessPacket.RemainMoney = dediPlayer._totalPoint;
        dediPlayer.Session.Send(itemBuySuccessPacket);

        return true;
    }

    /// <summary>
    /// 아이템 선택시 기능 실행
    /// </summary>
    /// <param name="playerId">플레이어id</param>
    /// <param name="itemId">아이템id</param>
    public void OnHoldItem(int playerId, int itemId)
    {
        //itemId가 -1이면 아이템을 안든것임
        if (itemId == -1)
        {
            //아이템 선택 패킷 브로드캐스트
            DSC_OnHoldItem onHoldItemPacket = new DSC_OnHoldItem();
            onHoldItemPacket.PlayerId = playerId;
            onHoldItemPacket.ItemId = -1;

            Managers.Player.Broadcast(onHoldItemPacket);
        }
        else
        {
            //인벤토리에 해당 아이템이 있는지 확인
            Player dediPlayer = Managers.Player._players[playerId].GetComponent<Player>();
            if (dediPlayer._inventory._itemCount.ContainsKey(itemId))
            {
                //아이템 선택 패킷 브로드캐스트
                DSC_OnHoldItem onHoldItemPacket = new DSC_OnHoldItem();
                onHoldItemPacket.PlayerId = playerId;
                onHoldItemPacket.ItemId = itemId;

                Managers.Player.Broadcast(onHoldItemPacket);
            }
            else
            {
                Util.PrintLog($"플레이어{playerId}의 인벤토리에 {itemId}아이템이 없음.");
            }
        }
    }

    /// <summary>
    /// 폭죽 아이템 썼을때 처리
    /// </summary>
    /// <param name="playerId">폭죽을 사용한 플레이어id</param>
    /// <param name="itemId">폭죽 아이템id</param>
    public void UseFireworkItem(int playerId, int itemId, CDS_UseFireworkItem packet)
    {
        if (Managers.Player.IsPlayerDead(playerId)) //플레이어가 죽었으면 처리X
        {
            return;
        }

        DSC_UseFireworkItem useFireworkItemPacket = new DSC_UseFireworkItem();
        useFireworkItemPacket.PlayerId = playerId;
        useFireworkItemPacket.ItemId = itemId;
        useFireworkItemPacket.FireworkStartingTransform = packet.FireworkStartingTransform;

        Player dediPlayer = Managers.Player._players[playerId].GetComponent<Player>();

        Vector3 playerPosition = dediPlayer.transform.position;
        Vector3 fireworkStartingPosition = new Vector3(packet.FireworkStartingTransform.Position.PosX, packet.FireworkStartingTransform.Position.PosY, packet.FireworkStartingTransform.Position.PosZ);

        //플레이어위치와 폭죽시작 위치가 일정범위 이내여야 함(핵 러프하게 검사)
        if (Vector3.Distance(playerPosition, fireworkStartingPosition) > 10f)
        {
            //핵의심. 무시
            Util.PrintLog($"The distance between the player and the firework start position is too far. Suspected cheat.");
            return;
        }
        else
        {
            //아이템이 없다면 무시
            if (dediPlayer._inventory.GetItemCount(itemId) == 0)
            {
                Util.PrintLog($"The {playerId}player does not have the {itemId}item.");
                return;
            }

            //아이템 사용처리
            dediPlayer._inventory.RemoveOneItem(itemId);

            //아이템 사용 패킷 브로드캐스트
            Managers.Player.Broadcast(useFireworkItemPacket);
        }
    }

    /// <summary>
    /// 투명화 아이템을 썼을때 처리
    /// </summary>
    /// <param name="playerId">투명 아이템을 사용한 플레이어id</param>
    /// <param name="itemId">투명 아이템id</param>
    public void UseInvisibleItem(int playerId, int itemId)
    {
        if (Managers.Player.IsPlayerDead(playerId)) //플레이어가 죽었으면 처리X
        {
            return;
        }

        DSC_UseInvisibleItem useInvisibleItemPacket = new DSC_UseInvisibleItem();
        useInvisibleItemPacket.PlayerId = playerId;
        useInvisibleItemPacket.ItemId = itemId;

        Player dediPlayer = Managers.Player._players[playerId].GetComponent<Player>();

        //아이템이 없다면 무시
        if (dediPlayer._inventory.GetItemCount(itemId) == 0)
        {
            Util.PrintLog($"The {playerId}player does not have the {itemId}item.");
            return;
        }

        //아이템 사용처리
        dediPlayer._inventory.RemoveOneItem(itemId);

        //아이템 사용 패킷 브로드캐스트
        Managers.Player.Broadcast(useInvisibleItemPacket);
    }





    /// <summary>
    /// 아이템 생성
    /// </summary>
    /// <param name="itemId">아이템Id</param>
    /// <returns>초기 데이터 세팅까지 완료된 아이템 </returns>
    public IItem CreateItem(int itemId)
    {
        if (_itemFactories.ContainsKey(itemId))
        {
            return _itemFactories[itemId].CreateItem();
        }
        else
        {
            Util.PrintLog("Cannot find item factory with id: " + itemId);
            return null;
        }
    }
    
    /// <summary>
    /// 아이템 팩토리 초기화
    /// </summary>
    public void InitItemFactories()
    {
        _itemFactories = new Dictionary<int, ItemFactory>();
        _items = new Dictionary<int, IItem>();
        
        //아이템 팩토리 생성
        _itemFactories.Add(1, new DashFactory());
        _itemFactories.Add(2, new FireworkFactory());
    }
    
    /// <summary>
    /// 아이템 데이터를 로드후 파싱
    /// </summary>
    public void LoadItemData()
    {
        if (File.Exists(_jsonPath))
        {
            string dataAsJson = File.ReadAllText(_jsonPath);
            _itemsDataJson = dataAsJson;
        }
        else
        {
            Debug.LogError("Cannot find file at " + _jsonPath);
            return;
        }
        
        //파싱
        ParseItemData();
    }

    /// <summary>
    /// json파일을 이미 받은 상태에서 아이템 데이터를 파싱
    /// </summary>
    private void ParseItemData()
    {
        var itemsData = JObject.Parse(_itemsDataJson)["Items"];
        _items = new Dictionary<int, IItem>();
        
        foreach (var itemData in itemsData)
        {
            IItem item = null;
            string className = itemData["EnglishName"].ToString();
            
            Type type = Type.GetType(className);
            if (type != null)
            {
                item = (IItem)itemData.ToObject(type);
            }
            
            if (item != null)
            {
                _items.Add(item.Id, item);
            }
        }
    }

    /// <summary>
    /// 아이템 가격 반환
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public int GetItemPrice(int itemId)
    {
        if(_items!=null && _items.ContainsKey(itemId))
            return _items[itemId].Price;
        else
            return -1;
    }

    /// <summary>
    /// 아이템 영어 이름 반환
    /// </summary>
    /// <param name="itemId">아이템id</param>
    /// <returns>아이템 영어 이름</returns>
    public string GetItemEnglishName(int itemId)
    {
        if(_items!=null && _items.ContainsKey(itemId))
            return _items[itemId].EnglishName;
        else
            return null;
    }
    
    /// <summary>
    /// 아이템 한글 이름 반환
    /// </summary>
    /// <param name="itemId">아이템id</param>
    /// <returns>아이템 한글 이름</returns>
    public string GetItemKoreanName(int itemId)
    {
        if(_items!=null && _items.ContainsKey(itemId))
            return _items[itemId].KoreanName;
        else
            return null;
    }
    
    /// <summary>
    /// 아이템 영어 설명 반환
    /// </summary>
    /// <param name="itemId">아이템id</param>
    /// <returns>아이템 영어 설명</returns>
    public string GetItemEnglishDescription(int itemId)
    {
        if(_items!=null && _items.ContainsKey(itemId))
            return _items[itemId].EnglishDescription;
        else
            return null;
    }
    
    /// <summary>
    /// 아이템 한글 설명 반환
    /// </summary>
    /// <param name="itemId">아이템id</param>
    /// <returns>아이템 한글 설명</returns>
    public string GetItemKoreanDescription(int itemId)
    {
        if(_items!=null && _items.ContainsKey(itemId))
            return _items[itemId].KoreanDescription;
        else
            return null;
    }
    
    /// <summary>
    /// 아이템 데이터 json 반환
    /// </summary>
    /// <returns>string형식의 json 아이템 데이터</returns>
    public string GetItemsDataJson()
    {
        return _itemsDataJson;
    }
}