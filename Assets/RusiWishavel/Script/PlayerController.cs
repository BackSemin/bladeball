using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float mouseSensitivity = 2f;
    public float gravity = -9.81f;

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
        // 마우스 X로 플레이어 자체를 회전시킴 (카메라는 이걸 그냥 따라올 뿐)
        float mouseX = Input.GetAxisRaw("Mouse X");
        transform.Rotate(Vector3.up, mouseX * mouseSensitivity);

        // 이동은 항상 플레이어가 보는 방향(transform.forward/right) 기준
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 moveDirection = transform.forward * vertical + transform.right * horizontal;
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        // 중력 적용
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // 애니메이션 (기존 그대로)
        if (animator != null)
        {
            animator.SetBool("IsGround", controller.isGrounded);
            float currentSpeed = moveDirection.magnitude * moveSpeed;
            animator.SetFloat("Speed", currentSpeed);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            BallController ball = FindFirstObjectByType<BallController>();
            if (ball != null)
            {
                // "Player" 태그를 넘겨주어 무작위 Enemy를 타겟팅하도록 함
                ball.ParryBall("Player");
            }
        }
    }
}