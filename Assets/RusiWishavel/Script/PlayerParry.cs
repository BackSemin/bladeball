using UnityEngine;

public class PlayerParry : MonoBehaviour
{
    [Header("패링 및 쿨타임 설정")]
    public float parryDuration = 3.0f;  // 패링 판정 유효 시간 (3초)
    public float totalCooldown = 5.0f;  // 전체 쿨타임 (5초)
    public float parryReachTime = 3.0f; // 공 도착 예상 시간 조건 (3초 이내)

    [Header("참조 스크립트 및 애니메이터")]
    public BallController ballScript;   // 씬 안의 공 스크립트
    public Animator playerAnimator;     // 플레이어 애니메이터

    [Header("애니메이션 파라미터 및 State 이름")]
    public string parryTriggerName = "doParry"; // 트리거 파라미터 이름
    public string parryStateName = "Parry";     // Animator Controller 상의 실제 State(상태) 이름

    private float cooldownTimer = 0.0f;
    private float activeTimer = 0.0f;
    private bool isParryActive = false;

    void Start()
    {
        // 씬 로드 후 컴포넌트 자동 복구
        FindPlayerAnimator();
        FindBallScript();
    }

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

        // ★ [핵심 1] 스페이스바 누른 즉시 휘두르는 애니메이션 강제 재생!
        PlayParryMotion();

        // 패링 3초 유효 상태 시작
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

        // 공 스크립트 참조가 유실된 경우 자동 재탐색
        if (ballScript == null)
        {
            FindBallScript();
            if (ballScript == null) return;
        }

        // 공의 타겟이 플레이어 본인 혹은 자식 오브젝트인지 검사
        if (ballScript.targetTransform != null &&
           (ballScript.targetTransform == transform || ballScript.targetTransform.IsChildOf(transform)))
        {
            float distanceToBall = Vector3.Distance(transform.position, ballScript.transform.position);
            float ballSpeed = Mathf.Max(ballScript.GetCurrentSpeed(), 0.1f);
            float estimatedTimeToReach = distanceToBall / ballSpeed;

            // 공이 3초 이내 도달 범위 안으로 들어왔을 때 패링 성공!
            if (estimatedTimeToReach <= parryReachTime)
            {
                Debug.Log("Parry SUCCESS! Ball hit back.");

                // ★ [핵심 2] 성공 시 모션 2차 보장
                PlayParryMotion();

                // 2. 공 반사 처리
                ballScript.ParryBall("Player");

                // 3. 성공 시 쿨타임 및 패링 상태 즉시 리셋
                isParryActive = false;
                activeTimer = 0.0f;
                cooldownTimer = 0.0f;
            }
        }
    }

    // 자식 오브젝트(유니짱)의 Animator 탐색
    private void FindPlayerAnimator()
    {
        if (playerAnimator == null)
        {
            playerAnimator = GetComponentInChildren<Animator>(true);
        }
    }

    // 씬 내의 BallController 자동 탐색
    private void FindBallScript()
    {
        if (ballScript == null)
        {
            ballScript = FindFirstObjectByType<BallController>();
        }
    }

    // 모션 즉시/안전 재생
    private void PlayParryMotion()
    {
        FindPlayerAnimator();

        if (playerAnimator != null)
        {
            // 1. 트리거 발생 (기존 방식)
            playerAnimator.SetTrigger(parryTriggerName);

            // 2. [핵심 3] 트리거 전이 조건을 무시하고 Animator의 특정 State를 즉시 강제 실행 (0.0초 시점부터)
            if (!string.IsNullOrEmpty(parryStateName))
            {
                playerAnimator.Play(parryStateName, -1, 0f);
            }
        }
        else
        {
            Debug.LogWarning("[PlayerParry] Animator를 찾을 수 없습니다! Unity-chan 모델링에 Animator가 붙어있는지 확인하세요.");
        }
    }
}