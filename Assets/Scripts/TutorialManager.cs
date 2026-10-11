using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("UI")]
    public CanvasGroup hintGroup;      // CanvasGroup на TutorialHint
    public RectTransform hintRect;     // RectTransform TutorialHint
    public TextMeshProUGUI hintText;   // Текст подсказки

    [Header("Настройки")]
    public float fadeDuration = 0.3f;
    public float defaultShowTime = 4f;

    private Coroutine currentRoutine;

    /// <summary>
    /// Реализует синглтон и изначально скрывает подсказку
    /// </summary>
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Изначально скрыто
        hintGroup.alpha = 0;
        hintRect.localScale = Vector3.one * 0.8f;
    }

    /// <summary>
    /// Сбрасываем ссылку, чтобы не осталось мёртвого Instance
    /// </summary>
    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Показывает подсказку с указанным текстом на заданное время
    /// </summary>
    /// <param name="message">Текст подсказки</param>
    /// <param name="duration">Время показа</param>
    public void Show(string message, float duration = -1f)
    {
        if (duration < 0) duration = defaultShowTime;

        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(ShowRoutine(message, duration));
    }

    /// <summary>
    /// Скрывает подсказку с анимацией
    /// </summary>
    public void Hide()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(HideRoutine());
    }

    /// <summary>
    /// Корутина появления подсказки через DOTween с последующим автоскрытием
    /// </summary>
    /// <param name="message">Текст подсказки</param>
    /// <param name="duration">Время показа</param>
    /// <returns>Корутина</returns>
    private IEnumerator ShowRoutine(string message, float duration)
    {
        hintText.text = message;

        // Появление — DOTween
        hintGroup.DOKill();
        hintRect.DOKill();

        hintGroup.DOFade(1f, fadeDuration);
        hintRect.DOScale(1f, fadeDuration).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(duration);
        yield return HideRoutine();
    }

    /// <summary>
    /// Корутина скрытия подсказки через DOTween
    /// </summary>
    /// <returns>Корутина</returns>
    private IEnumerator HideRoutine()
    {
        hintGroup.DOKill();
        hintRect.DOKill();

        hintGroup.DOFade(0f, fadeDuration);
        hintRect.DOScale(0.8f, fadeDuration).SetEase(Ease.InBack);

        yield return new WaitForSeconds(fadeDuration);
    }
}