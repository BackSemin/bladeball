using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float rotateSpeed = 10f;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // 1. WASD 입력 받기
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        // 2. 이동 처리
        if (moveDirection.magnitude >= 0.1f)
        {
            // 이동하는 방향으로 회전
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

            // 실제 이동
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }
    }
}