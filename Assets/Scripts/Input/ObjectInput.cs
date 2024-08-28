using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

/// <summary>
/// 트리거 관련한 처리하는 함수가 모여있는 클래스
/// </summary>
public class ObjectInput : MonoBehaviour
{
    /// <summary>
    /// 트랩에 걸렸을때 처리하는 함수
    /// </summary>
    /// <param name="dediPlayerId"></param>
    public void ProcessTrapped(int dediPlayerId, string trapId)
    {
        StartCoroutine(Trapped(dediPlayerId, trapId));
    }

    /// <summary>
    /// 트랩에 걸렸을때
    /// </summary>
    /// <param name="dediPlayerId">트랩에 걸린 데디플레이어id</param>
    private IEnumerator Trapped(int dediPlayerId, string trapId)
    {
        float stunDuration = (Managers.Item._itemFactories[4] as TrapFactory).StunDuration;

        GameObject playerGameObject = Managers.Player._players[dediPlayerId];
        Player dediPlayer = playerGameObject.GetComponent<Player>();
        if (dediPlayer._playerStatus._isCurrentTrapped) //이미 트랩에 걸렸으면 리턴
        {
            yield break;
        }

        //트랩 걸렸다고 브로드캐스트
        DSC_OnHitTrapItem onHitTrapItem = new DSC_OnHitTrapItem();
        onHitTrapItem.PlayerId = dediPlayerId;
        onHitTrapItem.ItemId = 4;
        onHitTrapItem.TrapId = trapId;
        Managers.Player.Broadcast(onHitTrapItem);


        //현재 트랩에 걸렸다고 표시
        dediPlayer._playerStatus._isCurrentTrapped = true; 
        //고스트 따라가기 기능 멈춤
        dediPlayer.ToggleFollowGhost(false);

        //스턴시간만큼 기다림
        yield return new WaitForSeconds(stunDuration);

        //트랩에 걸린 상태 해제
        dediPlayer._playerStatus._isCurrentTrapped = false;
        //고스트 따라가기 기능 재개
        dediPlayer.ToggleFollowGhost(true);
    }
}
