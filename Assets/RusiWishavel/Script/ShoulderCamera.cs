using UnityEngine;

public class ShoulderCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0.8f, 1.5f, -3.5f);

    [Header("마우스 Y (카메라 상하 각도만 담당)")]
    public float sensitivityY = 1.5f;
    public float minPitch = -30f;
    public float maxPitch = 80f;

    private float currentPitch = 15f;

    void Update()
    {
        float mouseY = Input.GetAxisRaw("Mouse Y");
        currentPitch -= mouseY * sensitivityY;
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 카메라는 플레이어의 회전(target.rotation)에 그대로 얹혀감 + pitch만 추가로 기울임
        Quaternion camRotation = target.rotation * Quaternion.Euler(currentPitch, 0f, 0f);

        Vector3 pivot = target.position + Vector3.up * offset.y;
        Vector3 armOffset = new Vector3(offset.x, 0f, offset.z);

        transform.position = pivot + camRotation * armOffset;
        transform.LookAt(pivot);
    }
}