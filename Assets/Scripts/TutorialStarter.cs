using UnityEngine;
using System.Collections;

public class TutorialStarter : MonoBehaviour
{
    [TextArea]
    public string startMessage = "Используй WASD, чтобы наклонять платформу и доехать до финиша";
    public float delay = 0.1f; 
    public float duration = 5f;

    /// <summary>
    /// Запускает корутину показа стартовой подсказки с небольшой задержкой
    /// </summary>
    void Start()
    {
        // Задержка, чтобы TutorialManager успел инициализироваться
        StartCoroutine(ShowStartHint());
    }

    /// <summary>
    /// Ждёт задержку и показывает стартовую подсказку через TutorialManager
    /// </summary>
    /// <returns>Корутина</returns>
    IEnumerator ShowStartHint()
    {
        yield return new WaitForSeconds(delay);

        if (TutorialManager.Instance != null)
            TutorialManager.Instance.Show(startMessage, duration);
    }
}