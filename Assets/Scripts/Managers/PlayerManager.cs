﻿using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerManager : MonoBehaviour
{
    public PlayerMoveController _playerMoveController;
    
    object _lock = new object();
    public Dictionary<int,GameObject> _players = new Dictionary<int, GameObject>(); //key: 데디서버의 playerId, value: 플레이어 오브젝트
    public Dictionary<int,GameObject> _ghosts = new Dictionary<int, GameObject>()   ; //key: 데디서버의 playerId, value: 고스트 오브젝트
    public string _tempPlayerPrefabPath = "Player/Player";
    public string _tempTargetGhost = "Player/TargetGhost";
    public Transform _spawnPointCenter;
    private bool[] _possibleSpawnPoint; //8개의 스폰포인트 중에 가능한 스폰포인트를 체크하는 배열

    public int _configuredPlayerCount = 8; //방장이 알려준 플레이어 수(대기할때 사용됨, 기본값은 8, 3초까지 기다림)
    public int _roomId = -1; //방장이 알려준 방id(혹시 몰라서 들고있음, 기본값은 -1)

    public void Init()
    {
        _playerMoveController = new PlayerMoveController();
        _spawnPointCenter = GameObject.Find("Map/SpawnPoint").transform;
        _possibleSpawnPoint = new bool[8]{true,true,true,true,true,true,true,true};
    }
    
    /// <summary>
    /// 방장이 보내준 방 정보 저장
    /// </summary>
    /// <param name="roomId">방 번호</param>
    /// <param name="playerCount">데디서버에 접속될 플레이어 수</param>
    public void SetRoomInfo(int roomId,int playerCount)
    {
        _roomId = roomId;
        _configuredPlayerCount = playerCount;
    }
    
    //모든 플레이어에게 패킷 전송
    public void Broadcast(IMessage packet)
    {
        foreach (KeyValuePair<int, GameObject> a in _players)
        {
            /*if (a.Key == movePacket.MyDediplayerId) //본인한테는 보내지 않음
            {
                continue;
            }*/
                    
            Managers.Session._sessions.TryGetValue(a.Key, out ClientSession session);
            if (session != null)
            {
                session.Send(packet);
            }
        }
    }
    
    /// <summary>
    /// 플레이어를 스폰하고 정보를 저장함. 모든이에게 추가된 플레이어의 정보를 알림
    /// </summary>
    /// <param name="playerId">독자적인 id</param>
    public void AddPlayer(ClientSession session,int roomId,string name)
    {
        DSC_AllowEnterGame allowEnterPacket = new DSC_AllowEnterGame();
        DSC_InformNewFaceInDedicatedServer informNewFaceInDedicatedServerPacket = new DSC_InformNewFaceInDedicatedServer();
        
        lock (_lock)
        {
            if (!_players.ContainsKey(session.SessionId))//이미 생성돼있는 플레이어가 아닐때
            {
                //플레이어 정보 생성 + 실체 생성 + 관리목록에 추가
                GameObject newPlayerObj= SpawnPlayer(session, roomId, name);
                _players.Add(session.SessionId, newPlayerObj);
                
                //고스트 실체 생성 + 관리목록에 추가
                GameObject newGhost = SpawnGhost(session.SessionId);
                _ghosts.Add(session.SessionId,newGhost); //고스트목록에 추가

                
                //본인한테 입장 허용 패킷 보냄
                Player newPlayer = newPlayerObj.GetComponent<Player>();
                allowEnterPacket.MyDedicatedPlayerId = newPlayer.Info.PlayerId;
                _players.Values.ToList().ForEach(player => allowEnterPacket.Players.Add(player.GetComponent<Player>().Info));
                Dictionary<int,TransformInfo> playerTransforms = new Dictionary<int, TransformInfo>();
                foreach (KeyValuePair<int,GameObject> a in _players)
                {
                    GameObject playerObj = a.Value;
                    if (playerObj != null)
                    {
                        Player player = playerObj.GetComponent<Player>();
                        TransformInfo transformInfo = new TransformInfo();
                        PositionInfo positionInfo = new PositionInfo();
                        RotationInfo rotationInfo = new RotationInfo();
                        
                        positionInfo.PosX = playerObj.transform.position.x;
                        positionInfo.PosY = playerObj.transform.position.y;
                        positionInfo.PosZ = playerObj.transform.position.z;
                        rotationInfo.RotX = playerObj.transform.rotation.x;
                        rotationInfo.RotY = playerObj.transform.rotation.y;
                        rotationInfo.RotZ = playerObj.transform.rotation.z;
                        rotationInfo.RotW = playerObj.transform.rotation.w;
                        
                        transformInfo.Position = positionInfo;
                        transformInfo.Rotation = rotationInfo;
                        
                        playerTransforms.Add(player.Info.PlayerId,transformInfo);
                    }
                }
                allowEnterPacket.PlayerTransforms.Add(playerTransforms);
                session.Send(allowEnterPacket);
                
                //다른 클라이언트에게 새로운 플레이어가 게임에 접속했음을 전송함
                informNewFaceInDedicatedServerPacket.NewPlayer = newPlayer.Info;
                TransformInfo newPlayerTransformInfo = new TransformInfo();
                PositionInfo newPlayerPositionInfo = new PositionInfo();
                RotationInfo newPlayerRotationInfo = new RotationInfo();
                
                newPlayerPositionInfo.PosX = newPlayerObj.transform.position.x;
                newPlayerPositionInfo.PosY = newPlayerObj.transform.position.y;
                newPlayerPositionInfo.PosZ = newPlayerObj.transform.position.z;
                newPlayerRotationInfo.RotX = newPlayerObj.transform.rotation.x;
                newPlayerRotationInfo.RotY = newPlayerObj.transform.rotation.y;
                newPlayerRotationInfo.RotZ = newPlayerObj.transform.rotation.z;
                newPlayerRotationInfo.RotW = newPlayerObj.transform.rotation.w;
                
                newPlayerTransformInfo.Position = newPlayerPositionInfo;
                newPlayerTransformInfo.Rotation = newPlayerRotationInfo;
                
                informNewFaceInDedicatedServerPacket.SpawnTransform = newPlayerTransformInfo;
                foreach (KeyValuePair<int,GameObject> a in _players)
                {
                    GameObject existingPlayerObj = a.Value;
                    if (existingPlayerObj != null && existingPlayerObj != newPlayerObj)
                    {
                        existingPlayerObj.GetComponent<Player>().Session.Send(informNewFaceInDedicatedServerPacket);
                    }
                }
            }

        }
    }
    
    /// <summary>
    /// 플레이어id에 해당하는 게임오브젝트와 ghost를 destroy하고 플레이어 매니저에서 플레이어,고스트를 제거
    /// </summary>
    /// <param name="playerId"></param>
    /// <returns></returns>
    public bool Remove(int playerId)
    {
        lock (_lock)
        {
            if (_players.TryGetValue(playerId, out GameObject playerObj) && _ghosts.TryGetValue(playerId, out GameObject ghostObj))
            {
                if(playerObj!=null)
                    DespawnPlayer(playerObj);
                if(ghostObj!=null)
                    DespawnGhost(ghostObj);
                
                return _players.Remove(playerId);
            }

            return false;
        }
    }
    /// <summary>
    /// 플레이어를 게임상에서 제거하는 함수 (Destroy처리)
    /// </summary>
    /// <param name="dediPlayer">Destroy할 플레이어 오브젝트</param>
    public void DespawnPlayer(GameObject dediPlayerObj)
    {
        Managers.Resource.Destroy(dediPlayerObj);
    }
    public void DespawnGhost(GameObject ghostObj)
    {
        Managers.Resource.Destroy(ghostObj);
    }
    

    /// <summary>
    /// 플레이어 destroy하고 플레이어 매니저에서 플레이어를 제거하고, 모든 클라이언트에게 제거된 플레이어의 정보를 알림
    /// </summary>
    /// <param name="playerId">제거해야할 플레이어id</param>
    public void LeaveGame(int playerId)
    {
        if (_players.ContainsKey(playerId))
        {
            if (Remove(playerId))
            {
                DSC_InformLeaveDedicatedServer informLeaveDedicatedServerPacket = new DSC_InformLeaveDedicatedServer();
                informLeaveDedicatedServerPacket.LeavePlayerId = playerId;
                foreach (GameObject playerObj in _players.Values)
                {
                    if (playerObj != null)
                    {
                        ClientSession session = playerObj.GetComponent<Player>().Session;
                        session.Send(informLeaveDedicatedServerPacket); 
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 데디서버 플레이어의 정보와 실체를 실제로 생성하는 함수(관리목록 추가는 안함)
    /// </summary>
    /// <param name="dediPlayer">갖고 있어야할 플레이어 정보</param>
    /// <returns></returns>
    public GameObject SpawnPlayer(ClientSession session,int roomId,string name)
    {
        Player newPlayer = new Player();
        newPlayer.Info.PlayerId = session.SessionId;
        newPlayer.Info.Name = name;
        newPlayer.RoomId = roomId;
        newPlayer.Session = session;
        
        GameObject obj =Managers.Resource.Instantiate(_tempPlayerPrefabPath);
        obj.name = $"Player_{newPlayer.Info.PlayerId}"; //플레이어 오브젝트 이름을 "Player_플레이어id"로 설정
        
        //가능한 spawnPoint들 중에 랜덤으로 1개 선택해서 위치 설정. 그리고 해당 spawnPoint는 사용불가로 설정
        List<int> trueIndices = new List<int>();
        
        for (int i = 0; i < _possibleSpawnPoint.Length; i++)
        {
            if (_possibleSpawnPoint[i])
            {
                trueIndices.Add(i);
            }
        }

        if (trueIndices.Count > 0)
        {
            int randomIndex = Random.Range(0, trueIndices.Count); 
            //_spawnPointCenter의 randomIndex번째 자식의 transform을 가져옴
            Transform spawnPoint = _spawnPointCenter.GetChild(trueIndices[randomIndex]);
            //obj의 transform을 spawnPoint와 동일하게 설정
            obj.transform.position = spawnPoint.position;
            obj.transform.rotation = spawnPoint.rotation;
            
            Util.PrintLog($"{newPlayer.Info.PlayerId}번 플레이어는 {trueIndices[randomIndex]}번째 스폰포인트에 스폰되었습니다");
            
            //해당 스폰포인트는 사용불가로 설정
            _possibleSpawnPoint[trueIndices[randomIndex]] = false;
        }
        else
        {
            Util.PrintLog("가능한 스폰포인트가 없습니다");
        }
        
        //플레이어 컴포넌트 추가
        Player dediPlayerComponent = obj.AddComponent<Player>();
        dediPlayerComponent.CopyFrom(newPlayer);

        return obj;
    }
    /// <summary>
    /// 플레이어id(=세션id)에 해당하는 고스트를 생성하는 함수. 위치도 초기 위치로 설정
    /// </summary>
    /// <param name="sessionId">대응되는 플레이어의 id</param>
    /// <returns></returns>
    public GameObject SpawnGhost(int playerId)
    {
        GameObject newGhost = Managers.Resource.Instantiate(_tempTargetGhost);
        newGhost.name = $"Ghost_{playerId}"; //고스트 오브젝트 이름을 "Ghost_플레이어id"로 설정
        //위치는 대응되는 플레이어의 초기 위치로 설정
        
        _players.TryGetValue(playerId, out GameObject playerObj);
        if (playerObj != null)
        {
            newGhost.transform.position = playerObj.transform.position;
            newGhost.transform.rotation = playerObj.transform.rotation;
        }

        return newGhost;
    }
    
}


