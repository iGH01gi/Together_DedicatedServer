using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;

class PacketManager
{
	#region Singleton
	static PacketManager _instance = new PacketManager();
	public static PacketManager Instance { get { return _instance; } }
	#endregion

	PacketManager()
	{
		Register();
	}

	Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
	Dictionary<ushort, Action<PacketSession, IMessage>> _handler = new Dictionary<ushort, Action<PacketSession, IMessage>>();

	public Action<PacketSession, IMessage, ushort> CustomHandler { get; set; }
		
	public void Register()
	{		
		_onRecv.Add((ushort)MsgId.CdsPingPong, MakePacket<CDS_PingPong>);
		_handler.Add((ushort)MsgId.CdsPingPong, PacketHandler.CDS_PingPongHandler);		
		_onRecv.Add((ushort)MsgId.CdsInformRoomInfo, MakePacket<CDS_InformRoomInfo>);
		_handler.Add((ushort)MsgId.CdsInformRoomInfo, PacketHandler.CDS_InformRoomInfoHandler);		
		_onRecv.Add((ushort)MsgId.CdsAllowEnterGame, MakePacket<CDS_AllowEnterGame>);
		_handler.Add((ushort)MsgId.CdsAllowEnterGame, PacketHandler.CDS_AllowEnterGameHandler);		
		_onRecv.Add((ushort)MsgId.CdsMove, MakePacket<CDS_Move>);
		_handler.Add((ushort)MsgId.CdsMove, PacketHandler.CDS_MoveHandler);		
		_onRecv.Add((ushort)MsgId.CdsTryChestOpen, MakePacket<CDS_TryChestOpen>);
		_handler.Add((ushort)MsgId.CdsTryChestOpen, PacketHandler.CDS_TryChestOpenHandler);		
		_onRecv.Add((ushort)MsgId.CdsRequestTimestamp, MakePacket<CDS_RequestTimestamp>);
		_handler.Add((ushort)MsgId.CdsRequestTimestamp, PacketHandler.CDS_RequestTimestampHandler);		
		_onRecv.Add((ushort)MsgId.CdsRequestCleansePermission, MakePacket<CDS_RequestCleansePermission>);
		_handler.Add((ushort)MsgId.CdsRequestCleansePermission, PacketHandler.CDS_RequestCleansePermissionHandler);		
		_onRecv.Add((ushort)MsgId.CdsCleanseQuit, MakePacket<CDS_CleanseQuit>);
		_handler.Add((ushort)MsgId.CdsCleanseQuit, PacketHandler.CDS_CleanseQuitHandler);		
		_onRecv.Add((ushort)MsgId.CdsCleanseSuccess, MakePacket<CDS_CleanseSuccess>);
		_handler.Add((ushort)MsgId.CdsCleanseSuccess, PacketHandler.CDS_CleanseSuccessHandler);		
		_onRecv.Add((ushort)MsgId.CdsItemBuyRequest, MakePacket<CDS_ItemBuyRequest>);
		_handler.Add((ushort)MsgId.CdsItemBuyRequest, PacketHandler.CDS_ItemBuyRequestHandler);		
		_onRecv.Add((ushort)MsgId.CdsOnHoldItem, MakePacket<CDS_OnHoldItem>);
		_handler.Add((ushort)MsgId.CdsOnHoldItem, PacketHandler.CDS_OnHoldItemHandler);		
		_onRecv.Add((ushort)MsgId.CdsUseFireworkItem, MakePacket<CDS_UseFireworkItem>);
		_handler.Add((ushort)MsgId.CdsUseFireworkItem, PacketHandler.CDS_UseFireworkItemHandler);		
		_onRecv.Add((ushort)MsgId.CdsUseInvisibleItem, MakePacket<CDS_UseInvisibleItem>);
		_handler.Add((ushort)MsgId.CdsUseInvisibleItem, PacketHandler.CDS_UseInvisibleItemHandler);
	}

	public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
	{
		ushort count = 0;

		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		Action<PacketSession, ArraySegment<byte>, ushort> action = null;
		if (_onRecv.TryGetValue(id, out action))
			action.Invoke(session, buffer, id);
	}

	void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
	{
		T pkt = new T();
		pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);

		//유니티 메인쓰레드 실행용(OnConnected에서 구현)
		if (CustomHandler != null)
		{
			CustomHandler.Invoke(session, pkt, id);
		}
		else
		{
			Action<PacketSession, IMessage> action = null;
			if (_handler.TryGetValue(id, out action))
				action.Invoke(session, pkt);
		}
	}

	public Action<PacketSession, IMessage> GetPacketHandler(ushort id)
	{
		Action<PacketSession, IMessage> action = null;
		if (_handler.TryGetValue(id, out action))
			return action;
		return null;
	}
}