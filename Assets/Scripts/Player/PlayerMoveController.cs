using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerMoveController : MonoBehaviour
{
    //키보드 인풋 판별용 비트
    int _runBit = (1 << 4);
    int _upBit = (1 << 3);
    int _leftBit = (1 << 2);
    int _downBit = (1 << 1);
    int _rightBit = 1;

    //플레이어 이동속도
    public float _walkSpeed = 2f;
    public float _runSpeed = 3f;  //최대 뛰기속도 8f까지 정상작동확인 완료.

    /// <summary>
    /// 핵 검사한 후 팔로워가 따라갈 targetGhost를 설정함. (추측항법)
    /// 핵 아닐때만 다른 클라이언트들에게 동기화 패킷을 보냄
    /// </summary>
    /// <param name="movePacket"></param>
    public void ProcessingCDSMove(int playerId, CDS_Move movePacket)
    {
        if (Managers.Player.IsPlayerDead(playerId)) //플레이어가 죽었으면 처리X
        {
            return;
        }

        if (Managers.Player._ghosts.TryGetValue(playerId, out GameObject ghostObj))
        {
            {
                //패킷정보를 꺼내옴
                float posX = movePacket.TransformInfo.Position.PosX;
                float posY = movePacket.TransformInfo.Position.PosY;
                float posZ = movePacket.TransformInfo.Position.PosZ;
                Vector3 pastPosition = new Vector3(posX, posY, posZ);
            }
            float rotX = movePacket.TransformInfo.Rotation.RotX;
            float rotY = movePacket.TransformInfo.Rotation.RotY;
            float rotZ = movePacket.TransformInfo.Rotation.RotZ;
            float rotW = movePacket.TransformInfo.Rotation.RotW;
            Quaternion pastLocalRotation = new Quaternion(rotX, rotY, rotZ, rotW);

            int keyboardInput = movePacket.KeyboardInput;

            Vector3 velocity = new Vector3();
            velocity.x = movePacket.Velocity.X;
            velocity.y = movePacket.Velocity.Y;
            velocity.z = movePacket.Velocity.Z;

            DateTime pastDateTime = movePacket.Timestamp.ToDateTime();


            //TODO : 데디서버의 고스트의 위치와 받은 패킷의 정보를 대조해서 해킹인지 아닌지 판별하는 코드가 필요 (해킹같다면 return해서 무시)


            //추측항법을 이용해서 위치 예측
            TransformInfo predictedTransformInfo = DeadReckoning(pastDateTime, movePacket.TransformInfo, velocity);
            Vector3 predictedPosition = new Vector3(predictedTransformInfo.Position.PosX, predictedTransformInfo.Position.PosY, predictedTransformInfo.Position.PosZ);

            ghostObj.transform.position = predictedPosition; //고스트 위치 갱신

            SetPlayerRunState(playerId, keyboardInput); //플레이어 뛰는 상태 동기화

            if (Managers.Player._players.TryGetValue(playerId, out GameObject playerObj))
            {
                //회전해야하는 값 세팅해주기
                playerObj.GetComponent<Player>()._targetRotation = pastLocalRotation;

                //카메라 월드 회전값 저장(시야 정보 얻는 것)
                float cameraWorldRotX = movePacket.CameraWorldRotation.RotX;
                float cameraWorldRotY = movePacket.CameraWorldRotation.RotY;
                float cameraWorldRotZ = movePacket.CameraWorldRotation.RotZ;
                float cameraWorldRotW = movePacket.CameraWorldRotation.RotW;
                playerObj.GetComponent<Player>()._cameraWorldRotation = new Quaternion(cameraWorldRotX, cameraWorldRotY, cameraWorldRotZ, cameraWorldRotW);
            }

            //다른 클라이언트들에게 동기화 패킷 보냄 (클라 입장에선 고스트 정보임)
            DSC_Move dscMovePacket = new DSC_Move();
            dscMovePacket.DediplayerId = movePacket.DediplayerId;
            //dscMovePacket.TransformInfo = movePacket.TransformInfo; (고스트 위치가 아닌, 데디서버의 실 플레이어 위치를 보냄)
            dscMovePacket.TransformInfo = new TransformInfo()
            {
                Position = new PositionInfo()
                {
                    PosX = playerObj.transform.position.x,
                    PosY = playerObj.transform.position.y,
                    PosZ = playerObj.transform.position.z
                },
                Rotation = new RotationInfo()
                {
                    RotX = playerObj.transform.rotation.x,
                    RotY = playerObj.transform.rotation.y,
                    RotZ = playerObj.transform.rotation.z,
                    RotW = playerObj.transform.rotation.w
                }
            };
            dscMovePacket.KeyboardInput = movePacket.KeyboardInput;
            dscMovePacket.Velocity = movePacket.Velocity;
            dscMovePacket.Timestamp = movePacket.Timestamp;
            dscMovePacket.CameraWorldRotation = movePacket.CameraWorldRotation;
            Managers.Player.Broadcast(dscMovePacket);
        }

    }



    /// <summary>
    /// 과거의 정보를 갖고 데드레커닝을 통해 현재 위치를 예측하는 함수
    /// </summary>
    /// <param name="pastDateTime">과거 시간</param>
    /// <param name="pastTransform">과거 트랜스폼</param>
    /// <param name="pastVelocity">과거 속도</param>
    /// <returns>예측된 위치(회전은 고려 x)</returns>
    public TransformInfo DeadReckoning(DateTime pastDateTime, TransformInfo pastTransform, Vector3 pastVelocity)
    {
        //현재 DateTime과 과거 DateTime의 차이를 구함
        TimeSpan timeSpan = DateTime.UtcNow - pastDateTime;
        
        //단순 시간계산만으로 위치를 예측하면 끊기듯이 이동하기 때문에 보정을 이용해 더 이동해줘야 함
        float alpha = 1.3f;

        //과거 위치를 기준으로 과거 속도를 이용해 예측 위치를 구함 (y축방향은 기존 값 이용해야 지터링 안생김)
        float posX = pastTransform.Position.PosX + pastVelocity.x * (float)timeSpan.TotalSeconds * alpha;
        float posZ = pastTransform.Position.PosZ + pastVelocity.z * (float)timeSpan.TotalSeconds * alpha;

        //그 결과를 리턴
        TransformInfo transformInfo = new TransformInfo();
        PositionInfo positionInfo = new PositionInfo();
        positionInfo.PosX = posX;
        positionInfo.PosY = pastTransform.Position.PosY;
        positionInfo.PosZ = posZ;
        transformInfo.Position = positionInfo;
        transformInfo.Rotation = pastTransform.Rotation;

        return transformInfo;
        
    }
    
    //이거 전에 고스트에서 계속 이동할 속도 계산할때 썻던 것
    public void CalculateVelocity(int keyboardInput, Quaternion localRotation)
    {
        /*Vector3 velocity;
        bool isRunning = false;
        Vector2 moveInputVector = new Vector2();
        moveInputVector.x =
            (keyboardInput & (_leftBit | _rightBit)) == 0 ? 0 : (keyboardInput & _leftBit) == 0 ? 1 : -1;
        moveInputVector.y = (keyboardInput & (_downBit | _upBit)) == 0 ? 0 : (keyboardInput & _downBit) == 0 ? 1 : -1;

        //방향키가 아무것도 안눌렀다면
        if ((keyboardInput & (_upBit | _downBit | _leftBit | _rightBit)) == 0)
        {
            velocity = Vector3.zero;
        }
        else
        {
            if ((keyboardInput & _runBit) != 0)
            {
                isRunning = true;
            }

            if (isRunning)
            {
                velocity = localRotation.normalized * new Vector3( moveInputVector.x, 0,  moveInputVector.y).normalized * _runSpeed;
            }
            else
            {
                velocity = localRotation.normalized * new Vector3( moveInputVector.x, 0,  moveInputVector.y).normalized * _walkSpeed;
            }
        }
        _velocity = velocity;*/
    }

    public void SetPlayerRunState(int dediPlayerId,int keyboardInput)
    {
        if (Managers.Player._players.TryGetValue(dediPlayerId, out GameObject playerObj))
        {
            if ((keyboardInput & _runBit) != _runBit)
            {
                playerObj.GetComponent<Player>()._isRunning = false;
            }
            else
            {
                playerObj.GetComponent<Player>()._isRunning = true;
            }
        }
    }
}
