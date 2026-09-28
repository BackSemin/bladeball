using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Instance 대신 GM이라는 명확한 이름 사용으로 네임스페이스 충돌 방지
    public static GameManager GM;

    [Header("씬에 배치된 실제 오브젝트 연결")]
    public GameObject playerObj;     // 씬에 있는 Player
    public GameObject botObj;        // 씬에 있는 Bot
    public GameObject ballObj;       // 씬에 있는 Ball

    [Header("배치 좌표 설정")]
    public Vector3 ballSpawnPos = new Vector3(0f, 10f, 0f); // 공 소환 위치 (0, 10, 0)
    public float spawnRadius = 8.0f;                        // (0,0) 중심에서 캐릭터 스폰 거리
    public float characterY = 0.5f;                         // 캐릭터 Y축 높이 (0.5)

    private bool isPlayerDead = false;

    void Awake()
    {
        if (GM == null)
        {
            GM = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        RelocateAllObjects();
    }

    // 씬에 있는 오브젝트들의 위치만 초기화
    public void RelocateAllObjects()
    {
        isPlayerDead = false;

        // 1. 플레이어 위치 이동 (0, 0.5, -8) 및 정면 조준
        if (playerObj != null)
        {
            playerObj.transform.position = new Vector3(0f, characterY, -spawnRadius);
            playerObj.transform.rotation = Quaternion.LookRotation(Vector3.forward);
            playerObj.SetActive(true);
        }

        // 2. 봇 위치 이동 (0, 0.5, 8) 및 정면 조준
        if (botObj != null)
        {
            botObj.transform.position = new Vector3(0f, characterY, spawnRadius);
            botObj.transform.rotation = Quaternion.LookRotation(Vector3.back);
            botObj.SetActive(true);
        }

        // 3. 공 위치 이동 (0, 10, 0)
        RelocateBallOnly();
    }

    // 공만 (0, 10, 0)으로 이동시키는 함수
    public void RelocateBallOnly()
    {
        if (isPlayerDead)
        {
            Debug.Log("Player is dead. Ball will not be relocated.");
            return;
        }

        if (ballObj != null)
        {
            ballObj.transform.position = ballSpawnPos;
            ballObj.transform.rotation = Quaternion.identity;
            ballObj.SetActive(true);

            BallController ballScript = ballObj.GetComponent<BallController>();
            if (ballScript != null)
            {
                ballScript.enabled = false;
                ballScript.enabled = true; // 스크립트 리셋
            }

            Debug.Log("Ball Relocated to (0, 10, 0)");
        }
    }

    // 캐릭터 피격 시 판정
    public void OnCharacterDied(string deadTag)
    {
        if (deadTag == "Player")
        {
            isPlayerDead = true;
            if (playerObj != null) playerObj.SetActive(false);
            if (ballObj != null) ballObj.SetActive(false);
            Debug.Log("GAME OVER: Player Died!");
        }
        else if (deadTag == "Enemy")
        {
            if (botObj != null) botObj.SetActive(false);
            if (ballObj != null) ballObj.SetActive(false);

            Debug.Log("ROUND CLEAR: Bot Died! Relocating Ball...");

            // 1초 뒤 공만 (0, 10, 0)으로 다시 배치
            Invoke(nameof(RelocateBallOnly), 1.0f);
        }
    }
}