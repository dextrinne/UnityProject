using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonDebug : MonoBehaviour, IPointerClickHandler
{
    public string buttonName = "Button";

    /// <summary>
    /// Обрабатывает клик по UI-кнопке и выводит в консоль её имя
    /// </summary>
    /// <param name="eventData">Данные события клика</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Клик по кнопке: " + buttonName);
    }
}