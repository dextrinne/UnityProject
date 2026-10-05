using UnityEngine;

public class TiltPlatform : MonoBehaviour
{
    [Header("Настройки наклона")]
    public float tiltSpeed = 4f;
    public float maxAngle = 15f;

    private Quaternion startRotation;
    private bool inputEnabled = true;

    void Start()
    {
        startRotation = transform.rotation;
    }

    void Update()
    {
        if (!inputEnabled)
        {
            // Плавно возвращаемся в исходное положение
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                startRotation,
                Time.deltaTime * tiltSpeed
            );
            return;
        }

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

    // Публичные методы для внешнего управления
    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
    }

    public void ResetRotation()
    {
        transform.rotation = startRotation;
    }
}