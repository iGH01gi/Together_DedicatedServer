# 데디케이티드 서버
Together게임의 인 게임 로직을 담당하는 데디케이티드 서버 (저작권 문제로 에셋 등 리소스는 제외한 버전)

### 영상 링크 : [Together 유튜브 바로가기](https://www.youtube.com/watch?v=I5oIDU53050)

# Together
![포폴썸네일](https://github.com/user-attachments/assets/a3cad82a-9fe1-49d3-8bb6-cd04d368fa79)
![스크린샷 2024-09-02 110720](https://github.com/user-attachments/assets/18030390-6ff2-4547-881d-fbc4c91579e5)
![image](https://github.com/user-attachments/assets/eb3ce92e-293c-4505-ab11-206c0220028a)
![image](https://github.com/user-attachments/assets/d2dd081d-8025-4dd1-9e3c-f7a1130283d9)
![image](https://github.com/user-attachments/assets/f34ea6eb-d96b-4ebb-bd74-e04ab3e906c2)

- 2인 개발중인 멀티 게임
- 저는 데디서버, 룸서버, 아이템 구현 및 동기화 부분을 담당하였습니다.
- 포톤이나 미러같은 상용 네트워크 서비스를 이용하지 않고 직접 만들었습니다.
- 룸서버의 경우 Jenkins와 Docker를 통해서 자동 배포를 구현하였습니다.
- 레디스를 통해 티어를 나누고, MS SQL을 활용하여 플레이어 정보를 저장할 예정입니다.
- 서버는 AWS을 활용하여 배포 (데디서버는 현재 로컬로 개발중)
- 유니티, C# 룸서버, 데디케이티드서버 를 활용


 # 게임 요약  
- 낮, 밤으로 구성된 라운드  
- 낮 : 상자를 파밍해 코인 획득, 코인으로 밤을 대비한 아이템 구매 가능  
- 밤: 킬러(여러 종류) 및 생존자 선정, 킬러는 생존자를 타격해서 킬러를 넘길 수 있음, 생존자는 아이템 사용을 통해 전략적 플레이, 정화 시스템, 킬러 고유 스킬 활용 가능  
