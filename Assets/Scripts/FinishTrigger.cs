using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    public GameObject winPanel;
    public GameObject ball;
    public TiltPlatform platform; 
    private bool isFinished = false;

    /// <summary>
    /// Обрабатывает достижение финиша игроком: останавливает шар,
    /// отключает управление платформой и показывает панель победы
    /// </summary>
    /// <param name="other">Коллайдер объекта, вошедшего в триггер</param>
    void OnTriggerEnter(Collider other)
    {
        if (isFinished) return;
        if (!other.CompareTag("Player")) return;

        isFinished = true;

        // Останавливаем шар
        if (ball != null)
        {
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }

        // Замораживаем платформу
        if (platform != null)
            platform.SetInputEnabled(false);

        // Показываем панель победы
        if (winPanel != null)
            winPanel.SetActive(true);
    }
}