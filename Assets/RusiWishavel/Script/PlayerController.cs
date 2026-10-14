using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float mouseSensitivity = 2f;
    public float gravity = -9.81f;

    [Header("맵 이동 제한 범위")]
    public float minX = -95f;
    public float maxX = 95f;
    public float minZ = -95f;
    public float maxZ = 95f;

    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 1. 마우스 X로 플레이어 회전
        float mouseX = Input.GetAxisRaw("Mouse X");
        transform.Rotate(Vector3.up, mouseX * mouseSensitivity);

        // 2. 이동 처리
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 moveDirection = transform.forward * vertical + transform.right * horizontal;
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        // 3. 중력 적용 및 이동
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // ★ 4. 플레이어 이동 가능 범위 제한 (Clamping)
        ClampPositionWithinBounds();

        // 5. 애니메이션 처리
        if (animator != null)
        {
            animator.SetBool("IsGround", controller.isGrounded);
            float currentSpeed = moveDirection.magnitude * moveSpeed;
            animator.SetFloat("Speed", currentSpeed);
        }

        // 6. 패링 테스트용
        if (Input.GetKeyDown(KeyCode.Space))
        {
            BallController ball = FindFirstObjectByType<BallController>();
            if (ball != null)
            {
                ball.ParryBall("Player");
            }
        }
    }

    // X, Z 위치 범위 제한 함수
    private void ClampPositionWithinBounds()
    {
        Vector3 currentPos = transform.position;

        // X, Z 좌표를 지정한 범위 안으로 보정
        float clampedX = Mathf.Clamp(currentPos.x, minX, maxX);
        float clampedZ = Mathf.Clamp(currentPos.z, minZ, maxZ);

        // 범위 밖으로 나가려고 할 때 위치 강제 고정
        if (currentPos.x != clampedX || currentPos.z != clampedZ)
        {
            // CharacterController는 enabled를 끌 때 위치 이동이 정상 적용됩니다.
            controller.enabled = false;
            transform.position = new Vector3(clampedX, currentPos.y, clampedZ);
            controller.enabled = true;
        }
    }
}