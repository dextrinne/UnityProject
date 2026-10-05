using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public GameObject winPanel;
    public GameObject topRightPanel;
    public Button btnRestart;
    public Button btnMusic;

    void Start()
    {
        // Привязываем кнопки КОДОМ при старте сцены
        // — так ссылки всегда свежие, потому что Start вызовется заново
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

    void Update()
    {
        if (topRightPanel == null || winPanel == null) return;
        topRightPanel.SetActive(!winPanel.activeSelf);
    }

    void OnRestartClicked()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.RestartLevel();
    }

    void OnMusicClicked()
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.ToggleMusic();
    }
}