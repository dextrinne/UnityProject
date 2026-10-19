using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Панели экранов")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Кнопки главного экрана")]
    [SerializeField] private Button btnPlay;
    [SerializeField] private Button btnOpenLevels;
    [SerializeField] private Button btnOpenSettings;

    [Header("Кнопки возврата ('Назад')")]
    [SerializeField] private Button btnBackFromLevels;
    [SerializeField] private Button btnBackFromSettings;

    [Header("Выбор уровня")]
    [Tooltip("Контейнер с компонентом GridLayoutGroup, куда будут спавниться кнопки уровней")]
    [SerializeField] private Transform levelButtonsContainer;
    [Tooltip("Префаб кнопки уровня (должен содержать Button и TextMeshProUGUI)")]
    [SerializeField] private GameObject levelButtonPrefab;

    [Header("Настройки звука")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button btnMute;
    [SerializeField] private Image muteIcon;

    private float lastNonZeroVolume = 0.8f;

    void Start()
    {
        // 1. Показываем только главный экран
        ShowMainPanel();

        // 2. Назначаем слушатели навигации
        btnPlay.onClick.AddListener(OnPlayClicked);
        btnOpenLevels.onClick.AddListener(ShowLevelSelectPanel);
        btnOpenSettings.onClick.AddListener(ShowSettingsPanel);

        btnBackFromLevels.onClick.AddListener(ShowMainPanel);
        btnBackFromSettings.onClick.AddListener(ShowMainPanel);

        // 3. Инициализируем настройки звука
        InitAudioSettings();

        // 4. Генерируем кнопки выбора уровней
        BuildLevelSelectGrid();
    }

    // --- Навигация по экранам ---

    public void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        levelSelectPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    public void ShowLevelSelectPanel()
    {
        mainPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void ShowSettingsPanel()
    {
        mainPanel.SetActive(false);
        levelSelectPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // --- Логика запуска игры ---

    private void OnPlayClicked()
    {
        // Загружаем сохраненный уровень, либо 0-й по умолчанию
        int savedLevel = PlayerPrefs.GetInt("CurrentLevel", 0);

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadLevel(savedLevel);
        }
        else
        {
            Debug.LogError("[MainMenu] LevelManager отсутствует на сцене!");
        }
    }

    // --- Генерация сетки уровней ---

    private void BuildLevelSelectGrid()
    {
        if (levelButtonsContainer == null || levelButtonPrefab == null) return;
        if (LevelManager.Instance == null) return;

        // Очищаем старые кнопки, если были
        foreach (Transform child in levelButtonsContainer)
        {
            Destroy(child.gameObject);
        }

        string[] scenes = LevelManager.Instance.levelScenes;
        int currentSavedLevel = PlayerPrefs.GetInt("CurrentLevel", 0);

        for (int i = 0; i < scenes.Length; i++)
        {
            int levelIndex = i; // Локальная копия индекса для замыкания в лямбде
            GameObject btnObj = Instantiate(levelButtonPrefab, levelButtonsContainer);

            // Меняем надпись кнопки на номер уровня (1, 2, 3...)
            TextMeshProUGUI label = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = (levelIndex + 1).ToString();
            }

            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() =>
                {
                    LevelManager.Instance.LoadLevel(levelIndex);
                });
            }
        }
    }

    // --- Логика звука ---

    private void InitAudioSettings()
    {
        float currentVol = MusicManager.Instance != null ? MusicManager.Instance.GetVolume() : 1f;
        if (currentVol > 0.01f) lastNonZeroVolume = currentVol;

        if (volumeSlider != null)
        {
            volumeSlider.value = currentVol;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        if (btnMute != null)
        {
            btnMute.onClick.AddListener(OnMuteToggleClicked);
        }

        UpdateMuteVisual(currentVol > 0.01f);
    }

    private void OnVolumeChanged(float val)
    {
        if (val > 0.01f) lastNonZeroVolume = val;
        if (MusicManager.Instance != null) MusicManager.Instance.SetVolume(val);
        UpdateMuteVisual(val > 0.01f);
    }

    private void OnMuteToggleClicked()
    {
        if (volumeSlider == null) return;

        if (volumeSlider.value > 0.01f)
        {
            lastNonZeroVolume = volumeSlider.value;
            volumeSlider.value = 0f;
        }
        else
        {
            volumeSlider.value = lastNonZeroVolume > 0.05f ? lastNonZeroVolume : 0.8f;
        }
    }

    private void UpdateMuteVisual(bool isSoundOn)
    {
        if (muteIcon != null)
        {
            muteIcon.color = isSoundOn ? Color.white : new Color(1f, 1f, 1f, 0.35f);
        }
    }
}