using System;
using Google.Protobuf;
using Google.Protobuf.Protocol;

public class PacketHandler
{
    public static void CDS_PingPongHandler(PacketSession session, IMessage packet)
    {
        CDS_PingPong pingPongPacket = packet as CDS_PingPong;
        ClientSession clientSession = session as ClientSession;
        
        clientSession.PingPong._isPong = true;
    }
}