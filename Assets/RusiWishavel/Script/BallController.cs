using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("속도 및 유도 설정")]
    public Transform targetTransform;    // 현재 공이 추적할 타겟
    public float baseSpeed = 15.0f;      // 초기 속도
    public float maxSpeed = 80.0f;       // 최대 제한 속도
    public float speedMultiplier = 1.15f;// 쳐낼 때마다 속도 증가 비율 (15%)

    [Tooltip("패링 직후 직선으로 튕겨 나가는 시간(초)")]
    public float initialStraightTime = 0.25f; // 이 시간 동안은 앞으로 튕겨나감

    [Tooltip("타겟을 향해 꺾이는 회전 속도")]
    public float turnSpeed = 15.0f;       // 높을수록 타겟으로 빠르게 꺾임

    [Header("이펙트 (선택)")]
    public GameObject destroyEffectPrefab; // 충돌 파괴 이펙트

    private float currentSpeed;
    private float straightTimer = 0f; // 직진 타이머

    void Start()
    {
        currentSpeed = baseSpeed;

        // 게임 시작 시 무작위 타겟 선정
        SelectInitialTarget();
    }

    void Update()
    {
        if (targetTransform == null) return;

        // 1. 패링 직후 일정 시간(initialStraightTime)이 지난 후에만 타겟 방향으로 회전(유도) 시작
        if (straightTimer > 0f)
        {
            straightTimer -= Time.deltaTime;
        }
        else
        {
            // 타겟 위치 조준 (높이 +1m)
            Vector3 targetDirection = (targetTransform.position + Vector3.up * 1.0f - transform.position).normalized;

            if (targetDirection != Vector3.zero)
            {
                // 타겟을 향해 부드럽게 꺾이면서 회전
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }

        // 2. 바라보는 방향으로 계속 전진
        transform.position += transform.forward * currentSpeed * Time.deltaTime;
    }

    // 게임 시작 시 Player 또는 Enemy 중 무작위 첫 타겟 지정
    private void SelectInitialTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        System.Collections.Generic.List<GameObject> allTargets = new System.Collections.Generic.List<GameObject>();
        allTargets.AddRange(players);
        allTargets.AddRange(enemies);

        if (allTargets.Count > 0)
        {
            int randomIndex = Random.Range(0, allTargets.Count);
            SetNewTarget(allTargets[randomIndex].transform);
            Debug.Log("Game Start! First Target: " + targetTransform.name + " (" + targetTransform.tag + ")");
        }
        else
        {
            Debug.LogWarning("No Player or Enemy tag found in the scene!");
        }
    }

    // 공을 쳐냈을 때 반대편 타겟으로 전환 (패링)
    public void ParryBall(string hitterTag)
    {
        Transform nextTarget = null;

        if (hitterTag == "Player")
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (enemies.Length > 0)
            {
                int randomIndex = Random.Range(0, enemies.Length);
                nextTarget = enemies[randomIndex].transform;
            }
        }
        else if (hitterTag == "Enemy")
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                nextTarget = playerObj.transform;
            }
        }

        if (nextTarget != null)
        {
            // 속도 증가
            currentSpeed = Mathf.Min(currentSpeed * speedMultiplier, maxSpeed);

            // 새 타겟 설정
            SetNewTarget(nextTarget);

            Debug.Log("[" + hitterTag + "] Parry Success! Next Target: " + targetTransform.name + " (Speed: " + currentSpeed.ToString("F1") + ")");
        }
    }

    // 새로운 타겟을 정하고 튕겨나가는 타이머 리셋
    private void SetNewTarget(Transform newTarget)
    {
        targetTransform = newTarget;

        // 쳐낸 순간 정면으로 튕겨 나가도록 타이머 설정
        straightTimer = initialStraightTime;

        // 쳐낸 주체의 정면(또는 공의 반사 방향)으로 회전 살짝 변경
        Vector3 targetDirection = (targetTransform.position + Vector3.up * 1.0f - transform.position).normalized;
        if (targetDirection != Vector3.zero)
        {
            // 완벽히 타겟을 안 바라보고, 정면과 타겟의 중간 지점으로 시작 각도를 부여해 자연스러운 궤적 연출
            Vector3 startDir = Vector3.Lerp(transform.forward, targetDirection, 0.3f);
            transform.rotation = Quaternion.LookRotation(startDir);
        }
    }

    // 충돌 시 부딪힌 대상과 공 둘 다 파괴
    private void OnTriggerEnter(Collider other)
    {
        // Player 또는 Enemy 태그를 가진 캐릭터에 부딪혔을 때
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            Debug.Log("Collision Detected! Destroying: " + other.name + " and Ball.");

            if (destroyEffectPrefab != null)
            {
                Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
            }

            // 1. 부딪힌 캐릭터 파괴
            Destroy(other.gameObject);

            // 2. 공 오브젝트 파괴
            Destroy(gameObject);
        }
    }
}