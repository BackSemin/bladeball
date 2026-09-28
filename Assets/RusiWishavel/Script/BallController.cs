using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("속도 및 유도 설정")]
    public Transform targetTransform;    // 현재 공이 추적할 타겟
    public float baseSpeed = 15.0f;      // 초기 속도
    public float maxSpeed = 80.0f;       // 최대 제한 속도
    public float speedMultiplier = 1.15f;// 쳐낼 때마다 속도 증가 비율 (15%)

    [Tooltip("패링 직후 직선으로 튕겨 나가는 시간(초)")]
    public float initialStraightTime = 0.25f;

    [Tooltip("타겟을 향해 꺾이는 회전 속도")]
    public float turnSpeed = 15.0f;

    [Header("거리 기반 피격 판정")]
    public float hitDistanceThreshold = 0.8f;

    [Header("이펙트 (선택)")]
    public GameObject destroyEffectPrefab;

    private float currentSpeed;
    private float straightTimer = 0f;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void OnEnable()
    {
        currentSpeed = baseSpeed;
        SelectInitialTarget();
    }

    void Update()
    {
        if (targetTransform == null) return;

        Vector3 targetCenterPos = targetTransform.position + Vector3.up * 1.0f;

        if (straightTimer > 0f)
        {
            straightTimer -= Time.deltaTime;
        }
        else
        {
            Vector3 targetDirection = (targetCenterPos - transform.position).normalized;
            if (targetDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }

        transform.position += transform.forward * currentSpeed * Time.deltaTime;

        // 거리 기반 피격 판정
        float distanceToTarget = Vector3.Distance(transform.position, targetCenterPos);
        if (distanceToTarget <= hitDistanceThreshold)
        {
            ExecuteTargetHit();
        }
    }

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
        }
    }

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
            currentSpeed = Mathf.Min(currentSpeed * speedMultiplier, maxSpeed);
            SetNewTarget(nextTarget);
        }
    }

    private void SetNewTarget(Transform newTarget)
    {
        targetTransform = newTarget;
        straightTimer = initialStraightTime;

        Vector3 targetDirection = (targetTransform.position + Vector3.up * 1.0f - transform.position).normalized;
        if (targetDirection != Vector3.zero)
        {
            Vector3 startDir = Vector3.Lerp(transform.forward, targetDirection, 0.3f);
            transform.rotation = Quaternion.LookRotation(startDir);
        }
    }

    private void ExecuteTargetHit()
    {
        if (destroyEffectPrefab != null)
        {
            Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
        }

        string hitTag = targetTransform.tag;

        // GameManager.GM 으로 충돌/모호성 없이 안전하게 연결!
        if (GameManager.GM != null)
        {
            GameManager.GM.OnCharacterDied(hitTag);
        }

        gameObject.SetActive(false);
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
}