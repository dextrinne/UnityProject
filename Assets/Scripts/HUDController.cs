using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public GameObject winPanel;        // ссылка на панель победы
    public Button btnRestart;          // кнопка рестарта в углу
    public Button btnMusic;            // кнопка музыки в углу
    public GameObject topRightPanel;   // весь блок в углу

    void Update()
    {
        // Скрываем HUD, если открыто любое меню
        bool menuOpen = winPanel != null && winPanel.activeSelf;
        topRightPanel.SetActive(!menuOpen);
    }
}