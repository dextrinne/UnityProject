using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonDebug : MonoBehaviour, IPointerClickHandler
{
    public string buttonName = "Button";

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Клик по кнопке: " + buttonName);
    }
}