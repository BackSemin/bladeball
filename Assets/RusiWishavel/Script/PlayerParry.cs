using UnityEngine;

public class PlayerParry : MonoBehaviour
{
    [Header("패링 조건 및 쿨타임")]
    public float parryReachTime = 3.0f; // 공이 플레이어에게 도착하기까지 남은 예상 시간 (3초 이내여야 인정)
    public float cooldownTime = 5.0f;   // 패링 실패/성공 후 쿨타임 (5초)

    [Header("참조 스크립트")]
    public BallController ballScript;   // 씬 안의 공 스크립트

    private float cooldownTimer = 0.0f;

    void Update()
    {
        // 쿨타임 타이머 감소
        if (cooldownTimer > 0.0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // 스페이스바(Space) 입력 감지
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryParry();
        }
    }

    private void TryParry()
    {
        // 1. 쿨타임 중이면 패링 불가
        if (cooldownTimer > 0.0f)
        {
            Debug.Log("Parry on Cooldown! Remaining time: " + cooldownTimer.ToString("F1") + "s");
            return;
        }

        // 공 스크립트가 연결되지 않은 경우 자동으로 씬에서 탐색
        if (ballScript == null)
        {
            ballScript = FindFirstObjectByType<BallController>();
        }

        if (ballScript == null)
        {
            Debug.LogWarning("No BallController found in the scene!");
            return;
        }

        // 2. 공의 타겟이 나(Player)인지 검사
        if (ballScript.targetTransform == transform)
        {
            // 공과의 거리 계산
            float distanceToBall = Vector3.Distance(transform.position, ballScript.transform.position);

            // 공의 현재 속도 가져오기 (0 방지)
            float ballSpeed = Mathf.Max(ballScript.GetCurrentSpeed(), 0.1f);

            // 공이 나에게 닿기까지 걸리는 예상 시간 = 거리 / 속도
            float estimatedTimeToReach = distanceToBall / ballSpeed;

            // 3. 공이 3초 이내에 도착할 수 있는 거리일 때만 패링 성공
            if (estimatedTimeToReach <= parryReachTime)
            {
                Debug.Log("Parry SUCCESS! Ball hit back.");

                // 공을 쳐내어 Enemy 타겟으로 전환
                ballScript.ParryBall("Player");

                // 패링 사용했으므로 5초 쿨타임 시작
                cooldownTimer = cooldownTime;
            }
            else
            {
                // 공이 3초보다 멀리 있어서 패링 실패 (헛스윙)
                Debug.Log("Parry FAILED! Ball too far (Will reach in " + estimatedTimeToReach.ToString("F1") + "s > " + parryReachTime + "s)");

                // 헛스윙 실패 시에도 5초 쿨타임 적용
                cooldownTimer = cooldownTime;
            }
        }
        else
        {
            // 내가 타겟이 아닌데 누른 경우에도 실패 처리 및 쿨타임 적용
            Debug.Log("Parry FAILED! Ball is not targeting you.");
            cooldownTimer = cooldownTime;
        }
    }
}