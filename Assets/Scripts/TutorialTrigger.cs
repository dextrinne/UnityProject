using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [TextArea]
    public string message = "Используй WASD, чтобы наклонять платформу";
    public float duration = 4f;
    public bool showOnce = true;

    private bool shown = false;

    /// <summary>
    /// При входе игрока в триггер показывает подсказку через TutorialManager
    /// </summary>
    /// <param name="other">Коллайдер объекта, вошедшего в триггер</param>
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (showOnce && shown) return;

        shown = true;

        if (TutorialManager.Instance != null)
            TutorialManager.Instance.Show(message, duration);
    }
}