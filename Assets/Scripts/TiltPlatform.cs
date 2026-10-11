using UnityEngine;

public class TiltPlatform : MonoBehaviour
{
    [Header("Настройки наклона")]
    public float tiltSpeed = 4f;
    public float maxAngle = 15f;

    private Quaternion startRotation;
    private bool inputEnabled = true;

    /// <summary>
    /// Сохраняет начальное вращение платформы
    /// </summary>
    void Start()
    {
        startRotation = transform.rotation;
    }

    /// <summary>
    /// Наклоняет платформу по вводу игрока или плавно возвращает в исходное положение при отключённом управлении
    /// </summary>
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

    /// <summary>
    /// Включает или отключает управление наклоном платформы
    /// </summary>
    /// <param name="enabled">Новое состояние управления</param>
    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
    }

    /// <summary>
    /// Мгновенно возвращает платформу в начальное вращение
    /// </summary>
    public void ResetRotation()
    {
        transform.rotation = startRotation;
    }
}