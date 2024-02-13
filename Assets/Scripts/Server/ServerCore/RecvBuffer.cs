using System;


public class RecvBuffer
{
    //[r,w][][][][][][][][][]
    private ArraySegment<byte> _buffer;
    private int _readPos;
    private int _writePos;

    public RecvBuffer(int bufferSize)
    {
        _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
    }
    
    public int DataSize { get { return _writePos-_readPos; } } //데이터 써져있는 크기(write 커서 - read커서)
    public int FreeSize { get { return _buffer.Count-_writePos; } } //데이터 뒷쪽 여유 크기(write 할 수 있는)

    public ArraySegment<byte> ReadSegment
    {
        get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
    }

    public ArraySegment<byte> WriteSegment
    {
        get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
    }

    public void Clean()
    {
        int dataSize = DataSize;
        if (dataSize == 0) //모든 데이터를 처리를 한 상태 (r,w겹쳐있음)
        {
            //남은 데이터가 없으면 복사하지 않고 커서 위치만 리셋
            _readPos = _writePos = 0;
        }
        else
        {
            //남은 찌끄레기가 있으면 시작 위치로 복사
            Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, DataSize);
            _readPos = 0;
            _writePos = dataSize;
        }
    }

    //컨텐츠에서 처리 완료하면 OnRead를 호출해서 read 커서를 이동시켜줌
    public bool OnRead(int numOfBytes)
    {
        if (numOfBytes > DataSize) //처리한 바이트가 DataSize보다 크면 문제가 있는것
            return false;

        _readPos += numOfBytes;
        return true;
    }

    //클라에서 쏴준 데이터를 우리가 리시브했을때 write 커서를 이동시키기 위함
    public bool OnWrite(int numOfBytes)
    {
        if(numOfBytes > FreeSize)
            return false;

        _writePos += numOfBytes;
        return true;
    }

}