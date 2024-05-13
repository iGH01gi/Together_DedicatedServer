using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public int _chestId = 0; // 상자의 고유 ID (0부터 시작)
    public int _chestLevel; //상자의 레벨(1,2,3중에 하나)
    public int _point = 0; // 상자가 가지고 있는 포인트 (1렙:꽝or1, 2렙:꽝or2, 3렙:3)
    public bool _isOpened = false; // 상자가 열렸는지 여부

    public void InitChest(int chestId, int chestLevel, int point)
    {
        _chestId = chestId;
        _chestLevel = chestLevel;
        _point = point;
        _isOpened = false;
    }
}