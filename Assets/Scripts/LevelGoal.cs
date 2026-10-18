using UnityEngine;
using TMPro;
using DG.Tweening;

public class LevelGoal : MonoBehaviour
{
    public static LevelGoal Instance;

    [Header("Настройки цели")]
    public int totalMushrooms = 3;
    private int collected = 0;

    [Header("UI и Портал")]
    public TextMeshProUGUI mushroomText; // UI текст в углу

    [Header("Материалы портала")]
    public GameObject finishPortal;      // Сам объект (для анимации)
    public Collider portalCollider;      // Коллайдер портала (для вкл/выкл физики)
    public Renderer portalRenderer;
    public Material closedMaterial;
    public Material openedMaterial;

    /// <summary>
    /// Инициализирует синглтон и сбрасывает счётчик собранных грибов
    /// </summary>
    void Awake()
    {
        Instance = this;
        collected = 0;
    }

    /// <summary>
    /// Обновляет UI, скрывает портал и назначает ему закрытый материал
    /// </summary>
    void Start()
    {
        UpdateUI();

        // Отключаем коллайдер, чтобы в портал нельзя было заехать
        if (portalCollider != null)
            portalCollider.isTrigger = false;

        // Ставим закрытый материал (портал остается видимым, но серым)
        if (portalRenderer != null && closedMaterial != null)
            portalRenderer.material = closedMaterial;
    }

    /// <summary>
    /// Увеличивает счётчик собранных грибов, обновляет UI и открывает портал при достижении цели
    /// </summary>
    public void CollectMushroom()
    {
        collected++;
        UpdateUI();

        if (collected >= totalMushrooms)
        {
            OpenPortal();
        }
    }

    /// <summary>
    /// Обновляет текст счётчика грибов в формате "Грибы: X/Y"
    /// </summary>
    void UpdateUI()
    {
        if (mushroomText != null)
            mushroomText.text = $"Грибы: {collected}/{totalMushrooms}";
    }

    /// <summary>
    /// Открывает портал: активирует объект, меняет материал на открытый и проигрывает эффект вспышки
    /// </summary>
    void OpenPortal()
    {
        Debug.Log("Все грибы собраны! Портал открыт.");

        // Включаем коллайдер, теперь шар может заехать и сработает триггер победы
        if (portalCollider != null)
            portalCollider.isTrigger = true;

        // Если вы выключали весь объект, включаем его обратно
        if (finishPortal != null)
            finishPortal.SetActive(true);

        // Меняем материал на открытый
        if (portalRenderer != null && openedMaterial != null)
        {
            portalRenderer.material = openedMaterial;

            // Эффект вспышки
            if (DOTween.IsTweening(finishPortal.transform) == false)
                finishPortal.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f);
        }
    }
}