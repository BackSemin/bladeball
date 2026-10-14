using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager GM;

    [Header("씬 오브젝트 및 프리팹 연결")]
    public GameObject playerObj;     // 씬에 있는 Player
    public GameObject ballObj;       // 씬에 있는 Ball
    public GameObject botPrefab;     // ★ 봇 프리팹 (Project 창의 Prefab만 연결)

    [Header("배치 좌표 및 스폰 설정")]
    public Vector3 ballSpawnPos = new Vector3(0f, 10f, 0f); // 공 소환 위치
    public float spawnRadius = 8.0f;                        // 스폰 거리
    public float characterY = 0.5f;                         // 높이

    [Header("웨이브 시스템")]
    public int currentWave = 1;                             // 현재 라운드 (1마리부터 시작)
    private List<GameObject> activeBots = new List<GameObject>(); // 현재 생성된 봇 목록
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
        StartNewGame();
    }

    // ★ 발표/시연용 치트키 입력 감지
    void Update()
    {
        // 1. [F1] 키: 현재 살아있는 모든 봇 처치 후 다음 웨이브로 스킵
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log("<color=yellow>[CHEAT] F1 입력: 모든 봇 처치 및 다음 웨이브로 스킵!</color>");
            CheatClearAllBotsAndNextWave();
        }

        // 2. [F2] 키: 웨이브 1(봇 1마리) 상태로 완전히 리셋 및 초기화
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Debug.Log("<color=cyan>[CHEAT] F2 입력: 1웨이브(봇 1마리)로 초기화!</color>");
            CheatResetToWaveOne();
        }
    }

    // 치트키 1: 즉시 다음 웨이브 진행
    private void CheatClearAllBotsAndNextWave()
    {
        // 예약된 Invoke 지우기 (중복 실행 방지)
        CancelInvoke(nameof(StartNextWave));
        CancelInvoke(nameof(RelocateBallOnly));

        // 모든 봇 파괴
        ClearExistingBots();

        // 웨이브 증가 및 소환
        currentWave++;
        Debug.Log($"[CHEAT] WAVE CLEAR! Wave {currentWave} ({currentWave}마리) 스폰 중...");

        SpawnWaveBots();
        RelocateBallOnly();
    }

    // 치트키 2: Wave 1로 완전 초기화
    private void CheatResetToWaveOne()
    {
        CancelInvoke(nameof(StartNextWave));
        CancelInvoke(nameof(RelocateBallOnly));

        StartNewGame();
    }

    // 게임 시작 (웨이브 1 초기화)
    public void StartNewGame()
    {
        isPlayerDead = false;
        currentWave = 1;

        // 1. 플레이어 제자리 이동
        if (playerObj != null)
        {
            playerObj.transform.position = new Vector3(0f, characterY, -spawnRadius);
            playerObj.transform.rotation = Quaternion.LookRotation(Vector3.forward);
            playerObj.SetActive(true);
        }

        // 2. 웨이브 봇 스폰 (Wave 1 = 1마리)
        SpawnWaveBots();

        // 3. 공 리셋
        RelocateBallOnly();
    }

    // 현재 웨이브 수만큼 봇 스폰
    public void SpawnWaveBots()
    {
        ClearExistingBots();

        if (botPrefab == null)
        {
            Debug.LogError("[GameManager] botPrefab이 Inspector에 연결되지 않았습니다!");
            return;
        }

        float angleStep = 180f / (currentWave + 1);

        for (int i = 0; i < currentWave; i++)
        {
            // 원형 부채꼴 배치 계산 (-90도 ~ 90도 범위)
            float currentAngle = -90f + angleStep * (i + 1);
            float x = Mathf.Sin(currentAngle * Mathf.Deg2Rad) * spawnRadius;
            float z = Mathf.Cos(currentAngle * Mathf.Deg2Rad) * spawnRadius;
            Vector3 spawnPos = new Vector3(x, characterY, z);

            GameObject newBot = Instantiate(botPrefab, spawnPos, Quaternion.identity);

            // 플레이어를 바라보도록 회전
            Vector3 lookDir = (playerObj != null) ? (playerObj.transform.position - spawnPos) : Vector3.back;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
            {
                newBot.transform.rotation = Quaternion.LookRotation(lookDir);
            }

            activeBots.Add(newBot);
        }

        Debug.Log($"[GameManager] Wave {currentWave} 시작! ({currentWave}마리 생성됨)");
    }

    private void ClearExistingBots()
    {
        foreach (GameObject bot in activeBots)
        {
            if (bot != null)
            {
                Destroy(bot);
            }
        }
        activeBots.Clear();
    }

    // 공 위치 초기화
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

    // 태그 기반 사망 처리
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
            HandleEnemyDeath(null);
        }
    }

    // 특정 봇 지정 사망 처리
    public void OnCharacterDied(GameObject deadEnemy)
    {
        HandleEnemyDeath(deadEnemy);
    }

    private void HandleEnemyDeath(GameObject deadEnemy)
    {
        if (deadEnemy != null && activeBots.Contains(deadEnemy))
        {
            activeBots.Remove(deadEnemy);
            Destroy(deadEnemy);
        }
        else if (activeBots.Count > 0)
        {
            GameObject botToRemove = activeBots[0];
            activeBots.RemoveAt(0);
            if (botToRemove != null) Destroy(botToRemove);
        }

        Debug.Log($"Bot 처치 완료! 남은 봇: {activeBots.Count}마리");

        // 라운드의 모든 봇을 처치한 경우
        if (activeBots.Count == 0)
        {
            currentWave++; // 봇 수 증가 (1 -> 2 -> 3 ...)
            Debug.Log($"🎉 WAVE CLEAR! 다음 웨이브({currentWave}마리)를 시작합니다.");
            Invoke(nameof(StartNextWave), 1.5f);
        }
        else
        {
            // 남은 봇이 있으면 공만 리셋하여 공방 재개
            Invoke(nameof(RelocateBallOnly), 1.0f);
        }
    }

    private void StartNextWave()
    {
        SpawnWaveBots();
        RelocateBallOnly();
    }
}