using UnityEngine;

public class PlayerParry : MonoBehaviour
{
    [Header("패링 및 쿨타임 설정")]
    public float parryDuration = 3.0f;  // 패링 판정 유효 시간 (3초)
    public float totalCooldown = 5.0f;  // 전체 쿨타임 (5초 -> 실패 시 3초 활성 + 2초 대기)
    public float parryReachTime = 3.0f; // 공 도착 예상 시간 조건 (3초 이내)

    [Header("참조 스크립트 및 애니메이터")]
    public BallController ballScript;   // 씬 안의 공 스크립트
    public Animator playerAnimator;     // 플레이어 애니메이터

    [Header("애니메이션 파라미터")]
    public string parryTriggerName = "doParry"; // 기존과 동일하게 Trigger 사용

    private float cooldownTimer = 0.0f;
    private float activeTimer = 0.0f;
    private bool isParryActive = false;

    void Update()
    {
        // 1. 쿨타임 감소
        if (cooldownTimer > 0.0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // 2. 패링 유효 시간(3초) 동안 프레임마다 공 접근 감지
        if (isParryActive)
        {
            activeTimer -= Time.deltaTime;

            // 공 감지 시도
            CheckParryCondition();

            // 3초간 공이 안 오고 시간이 끝나면 패링 창 닫힘
            if (activeTimer <= 0.0f)
            {
                isParryActive = false;
                Debug.Log("Parry Window Closed! (Failed)");
            }
        }

        // 3. 스페이스바(Space) 입력 감지
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryStartParry();
        }
    }

    private void TryStartParry()
    {
        // 쿨타임 중이거나 이미 패링 판정 진행 중이면 발동 불가
        if (cooldownTimer > 0.0f || isParryActive)
        {
            Debug.Log("Parry on Cooldown! Remaining time: " + cooldownTimer.ToString("F1") + "s");
            return;
        }

        // 패링 3초 유효 상태 시작 (여기서 애니메이션은 재생하지 않음!)
        isParryActive = true;
        activeTimer = parryDuration;
        cooldownTimer = totalCooldown;

        Debug.Log("Parry Window Activated! Searching for ball for 3 seconds...");

        // 스페이스바 누른 즉시 1차 검사
        CheckParryCondition();
    }

    private void CheckParryCondition()
    {
        if (!isParryActive) return;

        // 공 스크립트 자동 탐색
        if (ballScript == null)
        {
            ballScript = FindFirstObjectByType<BallController>();
            if (ballScript == null) return;
        }

        // 공의 타겟이 나(Player)인지 검사
        if (ballScript.targetTransform == transform)
        {
            float distanceToBall = Vector3.Distance(transform.position, ballScript.transform.position);
            float ballSpeed = Mathf.Max(ballScript.GetCurrentSpeed(), 0.1f);
            float estimatedTimeToReach = distanceToBall / ballSpeed;

            // 공이 3초 이내 도달 범위 안으로 들어왔을 때!
            if (estimatedTimeToReach <= parryReachTime)
            {
                Debug.Log("Parry SUCCESS! Ball hit back.");

                // ★ 1. 패링 성공할 때만 애니메이션 재생!
                if (playerAnimator != null)
                {
                    playerAnimator.SetTrigger(parryTriggerName);
                }

                // 2. 공 반사 처리
                ballScript.ParryBall("Player");

                // ★ 3. 성공 시 쿨타임 및 패링 상태 즉시 리셋!
                isParryActive = false;
                activeTimer = 0.0f;
                cooldownTimer = 0.0f;
            }
        }
    }
}