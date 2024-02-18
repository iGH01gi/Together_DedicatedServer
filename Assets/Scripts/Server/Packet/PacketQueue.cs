using System.Collections.Generic;
using Google.Protobuf;


public class PacketMessage
{
    public PacketSession Session { get; set; }
    public ushort Id { get; set; }
    public IMessage Message { get; set; }
}

//유니티메인쓰레드와 백그라운드 쓰레드(네트워크를 처리하는) 사이의 소통을 PacketQueue라는 통로를 이용해서 처리
//메인 쓰레드에서는 Pop을 사용해서 처리
public class PacketQueue
{
    public static PacketQueue Instance { get; } = new PacketQueue();

    Queue<PacketMessage> _packetQueue = new Queue<PacketMessage>();
    object _lock = new object();

    public void Push(PacketSession session, ushort id, IMessage packet)
    {
        lock (_lock)
        {
            _packetQueue.Enqueue(new PacketMessage() { Session = session, Id = id, Message = packet });
        }
    }

    public PacketMessage Pop()
    {
        lock (_lock)
        {
            if (_packetQueue.Count == 0)
                return null;

            return _packetQueue.Dequeue();
        }
    }

    public List<PacketMessage> PopAll()
    {
        List<PacketMessage> list = new List<PacketMessage>();

        lock (_lock)
        {
            while (_packetQueue.Count > 0)
                list.Add(_packetQueue.Dequeue());
        }

        return list;
    }
}