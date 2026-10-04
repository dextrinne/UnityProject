using UnityEngine;

public class TiltPlatform : MonoBehaviour
{
    [Header("Настройки наклона")]
    public float tiltSpeed = 4f;
    public float maxAngle = 15f;

    private Quaternion startRotation;

    void Start()
    {
        // Запоминаем изначальный поворот платформы
        startRotation = transform.rotation;
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Quaternion targetRotation = Quaternion.Euler(
            vertical * maxAngle,
            0f,
            -horizontal * maxAngle
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * tiltSpeed
        );
    }

    // Метод для сброса платформы 
    public void ResetRotation()
    {
        transform.rotation = startRotation;
    }
}