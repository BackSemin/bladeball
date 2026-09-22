using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("속도 및 유도 설정")]
    public Transform targetTransform;    // 현재 공이 추적할 타겟
    public float baseSpeed = 15.0f;      // 초기 속도
    public float maxSpeed = 80.0f;       // 최대 제한 속도
    public float speedMultiplier = 1.15f;// 쳐낼 때마다 속도 증가 비율 (15%)

    [Tooltip("유도 추적 속도")]
    public float homingSpeed = 25.0f;    // 타겟이 움직일 때 따라가는 유도력

    [Header("이펙트 (선택)")]
    public GameObject destroyEffectPrefab; // 충돌 파괴 이펙트

    private float currentSpeed;
    private Rigidbody rb;

    void Awake()
    {
        // 공이 다른 캐릭터를 밀어내지 못하도록 물리 연산 끄기
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;  // 물리적 힘/충돌을 받지 않음
            rb.useGravity = false;  // 중력 무시
        }

        // 콜라이더 트리거 강제 설정
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;  // 밀지 않고 그냥 통과하도록 설정
        }
    }

    void Start()
    {
        currentSpeed = baseSpeed;

        // 게임 시작 시 무작위 타겟 선정
        SelectInitialTarget();
    }

    void Update()
    {
        if (targetTransform == null) return;

        // 1. 실시간으로 타겟의 현재 위치 방향 계산 (타겟 높이 +1m)
        Vector3 targetDirection = (targetTransform.position + Vector3.up * 1.0f - transform.position).normalized;

        if (targetDirection != Vector3.zero)
        {
            // 2. 타겟을 향해 매 프레임 실시간으로 빠르게 회전
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, homingSpeed * 10f * Time.deltaTime);
        }

        // 3. 현재 바라보고 있는 방향으로 전진
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

    // 새로운 타겟을 정하고 방향 전환
    private void SetNewTarget(Transform newTarget)
    {
        targetTransform = newTarget;

        // 패링하자마자 즉시 새 타겟 방향을 조준
        Vector3 targetDirection = (targetTransform.position + Vector3.up * 1.0f - transform.position).normalized;
        if (targetDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(targetDirection);
        }
    }

    // 충돌 시 판정: 오직 지정된 타겟에게 부딪혔을 때만 파괴!
    private void OnTriggerEnter(Collider other)
    {
        // 부딪힌 대상이 현재 지목된 targetTransform과 일치하는지 검사
        if (targetTransform != null && other.transform == targetTransform)
        {
            Debug.Log("Target Hit! Destroying: " + other.name + " and Ball.");

            if (destroyEffectPrefab != null)
            {
                Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
            }

            // 1. 피격당한 타겟 파괴
            Destroy(other.gameObject);

            // 2. 공 파괴
            Destroy(gameObject);
        }
    }
}