using System.Collections.Generic;
using Google.Protobuf.Protocol;

public class PlayerManager
{
    object _lock = new object();
    Dictionary<int,Player> _players = new Dictionary<int, Player>(); //key: 데디서버의 playerId, value: 플레이어 정보
    
    /// <summary>
    /// 플레이어를 추가함. 모든이에게 추가된 플레이어의 정보를 알림
    /// </summary>
    /// <param name="playerId">독자적인 id</param>
    public void AddPlayer(ClientSession session,int roomId,string name)
    {
        DSC_AllowEnterGame allowEnterPacket = new DSC_AllowEnterGame();
        DSC_InformNewFaceInDedicatedServer informNewFaceInDedicatedServerPacket = new DSC_InformNewFaceInDedicatedServer();
        
        lock (_lock)
        {
            if (!_players.ContainsKey(session.SessionId))//이미 생서돼있는 플레이어가 아닐때 (재접속이 아닐때)
            {
                //플레이어 생성
                Player newPlayer = new Player();
                newPlayer.Info.PlayerId = session.SessionId;
                newPlayer.Info.Name = name;
                newPlayer.Info.Transform = null;
                newPlayer.RoomId = roomId;
                newPlayer.Session = session;
                session.MyPlayer= newPlayer; //세션에 플레이어 정보 저장
   
                //플레이어 관리목록에 추가
                _players.Add(newPlayer.Info.PlayerId, newPlayer);
                
                //본인한테 입장 허용 패킷 보냄
                allowEnterPacket.MyDedicatedPlayerId = newPlayer.Info.PlayerId;
                List<Player> playerList = new List<Player>(_players.Values);
                allowEnterPacket.Players.AddRange(playerList.ConvertAll(player => player.Info));
                newPlayer.Session.Send(allowEnterPacket);
                
                //다른 클라이언트에게 새로운 플레이어가 게임에 접속했음을 전송함
                informNewFaceInDedicatedServerPacket.NewPlayer = newPlayer.Info;
                foreach (KeyValuePair<int,Player> a in _players)
                {
                    Player player = a.Value;
                    if(player!=newPlayer)
                        player.Session.Send(informNewFaceInDedicatedServerPacket);
                }
            }
            else //재접속일때
            {
                //본인한테 재입장 허용 패킷 보냄
                allowEnterPacket.MyDedicatedPlayerId = session.SessionId;
                List<Player> playerList = new List<Player>(_players.Values);
                allowEnterPacket.Players.AddRange(playerList.ConvertAll(player => player.Info));
                session.Send(allowEnterPacket);
                
                //다른 클라이언트에게 새로운 플레이어가 게임에 재접속했음을 전송함
                informNewFaceInDedicatedServerPacket.NewPlayer = _players[session.SessionId].Info;
                foreach (KeyValuePair<int,Player> a in _players)
                {
                    Player player = a.Value;
                    if(player!=_players[session.SessionId])
                        player.Session.Send(informNewFaceInDedicatedServerPacket);
                }
            }

        }
    }
    
    public bool Remove(int playerId)
    {
        lock (_lock)
        {
            return _players.Remove(playerId);
        }
    }
    
    public Player Find(int playerId)
    {
        lock (_lock)
        {
            Player player = null;
            if (_players.TryGetValue(playerId, out player))
                return player;
            
            return null;
        }
    }

    /// <summary>
    /// 플레이어 매니저에서 플레이어를 제거하고, 모든 클라이언트에게 제거된 플레이어의 정보를 알림
    /// </summary>
    /// <param name="playerId">제거해야할 플레이어id</param>
    public void LeaveGame(int playerId)
    {
        if (_players.ContainsKey(playerId))
        {
            _players.Remove(playerId);
            foreach (Player player in _players.Values)
            {
                DSC_InformLeaveDedicatedServer informLeaveDedicatedServerPacket = new DSC_InformLeaveDedicatedServer();
                informLeaveDedicatedServerPacket.LeavePlayerId = playerId;
                
                if (player.Session != null)
                {
                    ClientSession session = player.Session;
                    session.Send(informLeaveDedicatedServerPacket); 
                }
                
            }

        }
    }
}