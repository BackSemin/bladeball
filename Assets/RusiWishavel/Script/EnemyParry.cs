using UnityEngine;

public class EnemyParry : MonoBehaviour
{
    [Header("패링 속도 및 반응 조건")]
    public float maxParryableSpeed = 15.0f; // 이 속도 이상이면 헛스윙 및 실패

    // ★ 핵심: 플레이어(3.0s)보다 짧게 설정하여 적의 패링 사거리/반응 시간을 줄임!
    [Tooltip("공 도달 남은 시간 (값이 작을수록 적이 공에 바짝 붙었을 때만 패링)")]
    public float enemyParryReachTime = 1.0f; // 추천값: 0.8 ~ 1.2

    [Header("참조 컴포넌트")]
    public BallController ballScript;
    public Animator enemyAnimator;

    [Header("애니메이터 파라미터 이름")]
    public string parryTriggerName = "doParry";

    private bool hasAttemptedParry = false;

    void Update()
    {
        CheckAndTryParry();
    }

    private void CheckAndTryParry()
    {
        if (ballScript == null)
        {
            ballScript = FindFirstObjectByType<BallController>();
            if (ballScript == null) return;
        }

        // 공의 타겟이 '적(Enemy)' 본인인지 확인
        if (ballScript.targetTransform == transform)
        {
            float currentBallSpeed = ballScript.GetCurrentSpeed();
            float distanceToBall = Vector3.Distance(transform.position, ballScript.transform.position);
            float estimatedTimeToReach = distanceToBall / Mathf.Max(currentBallSpeed, 0.1f);

            // ★ 적의 반응 시간에 도달했을 때만 동작 (1초 이내 접근 시)
            if (estimatedTimeToReach <= enemyParryReachTime && !hasAttemptedParry)
            {
                hasAttemptedParry = true;

                // 애니메이션(헛스윙 또는 성공) 재생
                if (enemyAnimator != null)
                {
                    enemyAnimator.SetTrigger(parryTriggerName);
                }

                // 속도 검사: 제한 속도 미만일 때만 성공
                if (currentBallSpeed < maxParryableSpeed)
                {
                    Debug.Log("Enemy Parry SUCCESS!");
                    ballScript.ParryBall("Enemy");
                }
                else
                {
                    Debug.Log("Enemy Parry FAILED! (Too Fast)");
                }
            }
        }
        else
        {
            hasAttemptedParry = false;
        }
    }
}