using System;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// json 데이터로부터 아이템 데이터를 로드하고, 아이템을 생성하기 위해서 필요한 클래스
/// </summary>
public class ItemManager
{

    public GameObject _root; //여기에 자식으로 아이템 gameobject들이 생성됨.

    private string _jsonPath;
    private string _itemPrefabFolderPath = "Items/"; //아이템 프리팹들이 들어있는 폴더 경로. 아이템id가 해당 폴더에서 프리팹의 이름
    private static string _itemsDataJson; //json이 들어 있게 됨(파싱 해야 함)
    public Dictionary<int, ItemFactory> _itemFactories = new Dictionary<int, ItemFactory>(); //key: 아이템Id, value: 아이템 팩토리 객체

    public void Init()
    {
        _root = GameObject.Find("@Item");
        if (_root == null)
        {
            _root = new GameObject { name = "@Item" };
            Object.DontDestroyOnLoad(_root);
        }

        _jsonPath = Application.streamingAssetsPath + "/Data/Item/Items.json";
        LoadItemData();
    }

    public void Clear()
    {
        //root의 모든 자식을 삭제(=생성된 아이템을 모두 삭제)
        foreach (Transform child in _root.transform)
        {
            Object.Destroy(child.gameObject);
        }

        //모든 플레이어의 인벤토리를 초기화
        foreach (GameObject player in Managers.Player._players.Values)
        {
            player.GetComponent<Player>()._inventory.Clear();
        }
    }

    /// <summary>
    /// 아이템을 구매해서 인벤토리에 추가
    /// </summary>
    /// <param name="playerId">플레이어id</param>
    /// <param name="itemId">구매하려는 아이템id</param>
    /// <returns>구매 성공 여부</returns>
    public bool BuyItem(int playerId, int itemId)
    {
        //게임 종료됐으면 처리X
        if (Managers.Game.IsGameEnd())
        {
            return false;
        }

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
        //게임 종료됐으면 처리X
        if (Managers.Game.IsGameEnd())
        {
            return ;
        }

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
    /// 클라 -> 서버로 아이템 사용 요청 패킷을 받았을때 처리
    /// </summary>
    /// <param name="playerId">아이템 사용 요청을 한 플레이어id</param>
    /// <param name="itemId">아이템id</param>
    /// <param name="packet">아이템 사용요청 패킷</param>
    public void UseItem(int playerId, int itemId, IMessage packet)
    {
        //게임 종료됐으면 처리X
        if (Managers.Game.IsGameEnd())
        {
            return;
        }

        if (Managers.Player.IsPlayerDead(playerId)) //플레이어가 죽었으면 처리X
        {
            return;
        }

        Player dediPlayer = Managers.Player._players[playerId].GetComponent<Player>();
        if (dediPlayer._inventory.GetItemCount(itemId) == 0) //아이템이 없다면 처리X
        {
            Util.PrintLog($"The {playerId}player does not have the {itemId}item.");
            return;
        }

        //킬러라면 처리 x
        if (Managers.Player.IsKiller(playerId))
        {
            return;
        }

        if (_itemFactories.ContainsKey(itemId))
        {
            //인벤토리에서 아이템1개 제거
            dediPlayer._inventory.RemoveOneItem(itemId);

            //아이템 생성
            GameObject itemGameObject = _itemFactories[itemId].CreateItem(playerId);

            //생성한 아이템을 @Item 밑에 넣음
            itemGameObject.transform.SetParent(_root.transform);

            //아이템 사용 효과 발동(IItem 컴포넌트를 가져와서 사용함)
            IItem item = itemGameObject.GetComponent<IItem>();
            item.Use(packet);
        }
        else
        {
            Debug.LogError("해당 아이템이 존재하지 않습니다: " + itemId);
        }
    }


    #region json관련

    /// <summary>
    /// 아이템 데이터 json을 로드후 파싱과정으로 넘김
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
    /// json파일을 이미 받은 상태에서 아이템 데이터를 파싱 + 팩토리 초기화
    /// </summary>
    private void ParseItemData()
    {
        var itemsData = JObject.Parse(_itemsDataJson)["Items"];

        foreach (var itemData in itemsData)
        {
            //아이템 타입에 따라서 아이템 팩토리 생성
            //Dash 아이템 팩토리 생성
            Debug.Log(itemData["EnglishName"]?.ToString());
            if (itemData["EnglishName"]?.ToString() == "Dash")
            {
                DashFactory itemFactory = new DashFactory(itemData["Id"].Value<int>(),
                    itemData["Price"].Value<int>(),
                    itemData["EnglishName"].ToString(),
                    itemData["KoreanName"].ToString(),
                    itemData["EnglishDescription"].ToString(),
                    itemData["KoreanDescription"].ToString(),
                    itemData["DashDistance"].Value<float>());

                _itemFactories.Add(itemFactory.FactoryId, itemFactory);
            }
            //Firework 아이템 팩토리 생성
            else if (itemData["EnglishName"]?.ToString() == "Firework")
            {
                FireworkFactory itemFactory = new FireworkFactory(itemData["Id"].Value<int>(),
                    itemData["Price"].Value<int>(),
                    itemData["EnglishName"].ToString(),
                    itemData["KoreanName"].ToString(),
                    itemData["EnglishDescription"].ToString(),
                    itemData["KoreanDescription"].ToString(),
                    itemData["FlightHeight"].Value<float>());
                _itemFactories.Add(itemFactory.FactoryId, itemFactory);
            }
            //Invisible 아이템 팩토리 생성
            else if (itemData["EnglishName"]?.ToString() == "Invisible")
            {
                InvisibleFactory itemFactory = new InvisibleFactory(itemData["Id"].Value<int>(),
                    itemData["Price"].Value<int>(),
                    itemData["EnglishName"].ToString(),
                    itemData["KoreanName"].ToString(),
                    itemData["EnglishDescription"].ToString(),
                    itemData["KoreanDescription"].ToString(),
                    itemData["InvisibleSeconds"].Value<float>());
                _itemFactories.Add(itemFactory.FactoryId, itemFactory);
            }
            //Flashlight 아이템 팩토리 생성
            else if (itemData["EnglishName"]?.ToString() == "Flashlight")
            {
                FlashlightFactory itemFactory = new FlashlightFactory(itemData["Id"].Value<int>(),
                    itemData["Price"].Value<int>(),
                    itemData["EnglishName"].ToString(),
                    itemData["KoreanName"].ToString(),
                    itemData["EnglishDescription"].ToString(),
                    itemData["KoreanDescription"].ToString(),
                    itemData["BlindDuration"].Value<float>(),
                    itemData["FlashlightDistance"].Value<float>(),
                    itemData["FlashlightAngle"].Value<float>(),
                    itemData["FlashlightAvailableTime"].Value<float>(),
                    itemData["FlashlightTimeRequired"].Value<float>()
                    );
                _itemFactories.Add(itemFactory.FactoryId, itemFactory);
            }
            // Trap 아이템 팩토리 생성
            else if (itemData["EnglishName"]?.ToString() == "Trap")
            {
                TrapFactory itemFactory = new TrapFactory(itemData["Id"].Value<int>(),
                    itemData["Price"].Value<int>(),
                    itemData["EnglishName"].ToString(),
                    itemData["KoreanName"].ToString(),
                    itemData["EnglishDescription"].ToString(),
                    itemData["KoreanDescription"].ToString(),
                    itemData["TrapDuration"].Value<float>(),
                    itemData["TrapRadius"].Value<float>(),
                    itemData["StunDuration"].Value<float>()
                    );
                _itemFactories.Add(itemFactory.FactoryId, itemFactory);
            }

            else
            {
                Debug.LogError("읽을 수 없는 아이템이 입력되었습니다.");
                return;
            }
        }
    }

    /// <summary>
    /// 아이템 데이터 json 반환
    /// </summary>
    /// <returns>string형식의 json 아이템 데이터</returns>
    public string GetItemsDataJson()
    {
        return _itemsDataJson;
    }

    #endregion

    #region GetterMethods

    /// <summary>
    /// 아이템 가격 반환
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public int GetItemPrice(int itemId)
    {
        if (_itemFactories.ContainsKey(itemId))
        {
            return (_itemFactories[itemId].FactoryPrice);
        }
        else
        {
            Debug.LogError("해당 아이템이 존재하지 않습니다: " + itemId);
            return 0;
        }
    }

    /// <summary>
    /// 아이템 영어 이름 반환
    /// </summary>
    /// <param name="itemId">아이템id</param>
    /// <returns>아이템 영어 이름</returns>
    public string GetItemEnglishName(int itemId)
    {
        if (_itemFactories.ContainsKey(itemId))
        {
            return (_itemFactories[itemId].FactoryEnglishName);
        }
        else
        {
            Debug.LogError("해당 아이템이 존재하지 않습니다: " + itemId);
            return null;
        }
    }

    /// <summary>
    /// 아이템 한글 이름 반환
    /// </summary>
    /// <param name="itemId">아이템id</param>
    /// <returns>아이템 한글 이름</returns>
    public string GetItemKoreanName(int itemId)
    {
        if (_itemFactories.ContainsKey(itemId))
        {
            return (_itemFactories[itemId].FactoryKoreanName);
        }
        else
        {
            Debug.LogError("해당 아이템이 존재하지 않습니다: " + itemId);
            return null;
        }
    }

    /// <summary>
    /// 아이템 영어 설명 반환
    /// </summary>
    /// <param name="itemId">아이템id</param>
    /// <returns>아이템 영어 설명</returns>
    public string GetItemEnglishDescription(int itemId)
    {
        if (_itemFactories.ContainsKey(itemId))
        {
            return (_itemFactories[itemId].FactoryEnglishDescription);
        }
        else
        {
            Debug.LogError("해당 아이템이 존재하지 않습니다: " + itemId);
            return null;
        }
    }

    /// <summary>
    /// 아이템 한글 설명 반환
    /// </summary>
    /// <param name="itemId">아이템id</param>
    /// <returns>아이템 한글 설명</returns>
    public string GetItemKoreanDescription(int itemId)
    {
        if (_itemFactories.ContainsKey(itemId))
        {
            return (_itemFactories[itemId].FactoryKoreanDescription);
        }
        else
        {
            Debug.LogError("해당 아이템이 존재하지 않습니다: " + itemId);
            return null;
        }
    }

    #endregion

}