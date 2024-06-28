using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public int _chestId = 0; // 상자의 고유 ID (0부터 시작)
    public int _chestLevel; //상자의 레벨(1,2,3중에 하나)
    public int _point = 0; // 상자가 가지고 있는 포인트 (1렙:꽝or1, 2렙:꽝or2, 3렙:3)
    private int _isOpened = 0; // 상자가 열렸는지 여부(0: false, 1: true)

    public void InitChest(int chestId, int chestLevel, int point)
    {
        _chestId = chestId;
        _chestLevel = chestLevel;
        _point = point;
        _isOpened = 0;
    }
    
    /// <summary>
    /// atomic하게 상자 열기를 시도하고 가능하면 열기까지 함 (동시성 문제 해결을 위해 사용)
    /// </summary>
    /// <returns>열렸다면 true반환. 아니라면 false</returns>
    public bool TryOpenChestAtomic()
    {
        // _isOpened의 현재 값이 0(아직 안열린 상태)이면 상자 연 처리를 하고 true를 반환하고, 그렇지 않으면 false를 반환합니다.
        return System.Threading.Interlocked.CompareExchange(ref _isOpened, 1, 0) == 0;
    }

    
}