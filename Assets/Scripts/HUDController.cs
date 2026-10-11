using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public GameObject winPanel;
    public GameObject topRightPanel;
    public Button btnRestart;
    public Button btnMusic;

    /// <summary>
    /// Подписывает кнопки перезапуска и музыки на обработчики
    /// </summary>
    void Start()
    {
        if (btnRestart != null)
        {
            btnRestart.onClick.RemoveAllListeners();
            btnRestart.onClick.AddListener(OnRestartClicked);
        }

        if (btnMusic != null)
        {
            btnMusic.onClick.RemoveAllListeners();
            btnMusic.onClick.AddListener(OnMusicClicked);
        }
    }

    /// <summary>
    /// Скрывает верхнюю правую панель, когда показана панель победы
    /// </summary>
    void Update()
    {
        if (topRightPanel == null || winPanel == null) return;
        topRightPanel.SetActive(!winPanel.activeSelf);
    }

    /// <summary>
    /// Перезапускает текущий уровень через LevelManager
    /// </summary>
    void OnRestartClicked()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.RestartLevel();
    }

    /// <summary>
    /// Переключает музыку через MusicManager
    /// </summary>
    void OnMusicClicked()
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.ToggleMusic();
    }
}