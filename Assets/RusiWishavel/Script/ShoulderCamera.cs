using UnityEngine;

public class ShoulderCamera : MonoBehaviour
{
    public Transform target;

    [Header("어깨 오프셋 (캐릭터 기준 우측/위)")]
    public Vector3 shoulderOffset = new Vector3(0.7f, 1.5f, 0f); // 어깨 위치
    public float distance = 5.0f;                                // 카메라와 어깨 사이 거리

    [Header("마우스 상하 각도 및 감도")]
    public float sensitivityY = 2.0f;
    public float minPitch = -15f; // 아래로 내렸을 때 최저 각도
    public float maxPitch = 50f;  // 위로 올렸을 때 최고 각도

    [Header("부드러움 및 충돌")]
    public float followSpeed = 15.0f;      // 위치 추적 속도 (높을수록 딱 붙음)
    public float cameraRadius = 0.25f;     // 카메라 충돌 반지름
    public LayerMask collisionLayers = ~0; // 충돌 검사 레이어

    private float currentPitch = 10f;

    void Update()
    {
        // 마우스 Y축 입력 받기
        float mouseY = Input.GetAxis("Mouse Y");
        currentPitch -= mouseY * sensitivityY;
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. 기준점: 플레이어 어깨 위치 계산
        Vector3 shoulderPos = target.position + target.TransformDirection(shoulderOffset);

        // 2. 어깨 기준 카메라 회전 (플레이어 몸통 회전 + 상하 Pitch)
        Quaternion camRotation = target.rotation * Quaternion.Euler(currentPitch, 0f, 0f);

        // 3. 어깨에서 등 뒤로 distance만큼 떨어진 목표 위치 계산
        Vector3 targetPos = shoulderPos - (camRotation * Vector3.forward) * distance;

        // 4. 장애물/바닥 충돌 감지 (어깨 -> 카메라 목표 지점)
        Vector3 rayDir = targetPos - shoulderPos;
        float maxDist = rayDir.magnitude;
        Vector3 finalPos = targetPos;

        if (Physics.SphereCast(shoulderPos, cameraRadius, rayDir.normalized, out RaycastHit hit, maxDist, collisionLayers))
        {
            // 충돌 시 카메라를 어깨 쪽으로 밀어 넣음
            finalPos = shoulderPos + rayDir.normalized * Mathf.Max(hit.distance, 0.4f);
        }

        // 5. 위치 부드럽게 이동 (붕 뜨는 느낌 제거)
        transform.position = Vector3.Lerp(transform.position, finalPos, Time.deltaTime * followSpeed);
        
        // 6. 회전은 어깨 약간 앞쪽(시선 방향)을 바라보게 고정
        transform.rotation = camRotation;
    }
}