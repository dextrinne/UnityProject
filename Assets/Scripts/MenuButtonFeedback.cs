using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class MenuButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float hoverScale = 1.06f;
    [SerializeField] private float clickScale = 0.95f;
    [SerializeField] private float smoothSpeed = 16f;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isHovered = false;

    /// <summary>
    /// Запоминает исходный масштаб и устанавливает его целевым
    /// </summary>
    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    /// <summary>
    /// Сбрасывает масштаб и состояние наведения при включении объекта
    /// </summary>
    private void OnEnable()
    {
        transform.localScale = originalScale;
        targetScale = originalScale;
        isHovered = false;
    }

    /// <summary>
    /// Плавно интерполирует масштаб к целевому значению (независимо от timeScale)
    /// </summary>
    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * smoothSpeed);
    }

    /// <summary>
    /// При наведении увеличивает масштаб кнопки
    /// </summary>
    /// <param name="eventData">Данные события указателя</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        targetScale = originalScale * hoverScale;
    }

    /// <summary>
    /// При уходе курсора возвращает исходный масштаб
    /// </summary>
    /// <param name="eventData">Данные события указателя</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        targetScale = originalScale;
    }

    /// <summary>
    /// При нажатии уменьшает масштаб кнопки (эффект нажатия)
    /// </summary>
    /// <param name="eventData">Данные события указателя</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = originalScale * clickScale;
    }

    /// <summary>
    /// При отпускании возвращает масштаб к hover-состоянию или исходному
    /// </summary>
    /// <param name="eventData">Данные события указателя</param>
    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = isHovered ? originalScale * hoverScale : originalScale;
    }
}
