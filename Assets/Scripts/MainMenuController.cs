using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Управление камерой")]
    [SerializeField] private MenuCameraController cameraController;

    [Header("Панели экранов")]
    [SerializeField] private CanvasGroup mainCanvasGroup;
    [SerializeField] private CanvasGroup settingsCanvasGroup;
    [SerializeField] private CanvasGroup levelSelectCanvasGroup;

    [Header("Затемнение экрана (Screen Fader)")]
    [Tooltip("Полноэкранная темная панель с CanvasGroup поверх всех UI")]
    [SerializeField] private CanvasGroup screenFader;
    [SerializeField] private float fadeDuration = 0.8f;

    [Header("Кнопки главного экрана")]
    [SerializeField] private Button btnPlay;
    [SerializeField] private Button btnOpenLevels;
    [SerializeField] private Button btnOpenSettings;
    [SerializeField] private Button btnQuit;

    [Header("Кнопки возврата ('Назад')")]
    [SerializeField] private Button btnBackFromLevels;
    [SerializeField] private Button btnBackFromSettings;

    [Header("Выбор уровня")]
    [SerializeField] private Transform levelButtonsContainer;
    [SerializeField] private GameObject levelButtonPrefab;

    [Header("Настройки звука")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button btnMute;
    [SerializeField] private Image muteIcon;

    private float lastNonZeroVolume = 0.8f;
    private bool isInteractingBlocked = false;

    // Публичные свойства для тестов и внешнего контроля
    public MenuCameraController CameraController => cameraController;
    public CanvasGroup MainCanvasGroup => mainCanvasGroup;
    public CanvasGroup SettingsCanvasGroup => settingsCanvasGroup;
    public CanvasGroup LevelSelectCanvasGroup => levelSelectCanvasGroup;
    public CanvasGroup ScreenFader => screenFader;
    public bool IsInteractingBlocked => isInteractingBlocked;

    public event System.Action OnQuitExecuted;

    private void Start()
    {
        // 1. Плавный Fade In при открытии меню
        if (screenFader != null)
        {
            screenFader.gameObject.SetActive(true);
            screenFader.alpha = 1f;
            StartCoroutine(FadeRoutine(screenFader, 1f, 0f, fadeDuration, () => screenFader.blocksRaycasts = false));
        }

        // 2. Исходные состояния экранов
        SetCanvasGroupState(mainCanvasGroup, true);
        SetCanvasGroupState(settingsCanvasGroup, false);
        SetCanvasGroupState(levelSelectCanvasGroup, false);

        // 3. Подписка на кнопки
        if (btnPlay != null) btnPlay.onClick.AddListener(OnPlayClicked);
        if (btnOpenLevels != null) btnOpenLevels.onClick.AddListener(OnOpenLevelsClicked);
        if (btnOpenSettings != null) btnOpenSettings.onClick.AddListener(OnOpenSettingsClicked);
        if (btnQuit != null) btnQuit.onClick.AddListener(OnQuitClicked);

        if (btnBackFromLevels != null) btnBackFromLevels.onClick.AddListener(OnBackToMainClicked);
        if (btnBackFromSettings != null) btnBackFromSettings.onClick.AddListener(OnBackToMainClicked);

        // 4. Инициализация звука и уровней
        InitAudioSettings();
        BuildLevelSelectGrid();
    }

    // --- Логика навигации с камерой ---

    public void OnOpenSettingsClicked()
    {
        if (isInteractingBlocked) return;
        isInteractingBlocked = true;

        // Прячем главное меню
        StartCoroutine(FadeRoutine(mainCanvasGroup, 1f, 0f, 0.3f, () =>
        {
            SetCanvasGroupState(mainCanvasGroup, false);
            // Камера плавно летит к красному шарику
            if (cameraController != null)
            {
                cameraController.MoveToBall(() =>
                {
                    // Показываем настройки у шарика
                    SetCanvasGroupState(settingsCanvasGroup, true);
                    StartCoroutine(FadeRoutine(settingsCanvasGroup, 0f, 1f, 0.3f, () => isInteractingBlocked = false));
                });
            }
            else
            {
                SetCanvasGroupState(settingsCanvasGroup, true);
                StartCoroutine(FadeRoutine(settingsCanvasGroup, 0f, 1f, 0.3f, () => isInteractingBlocked = false));
            }
        }));
    }

    public void OnBackToMainClicked()
    {
        if (isInteractingBlocked) return;
        isInteractingBlocked = true;

        CanvasGroup activeSubPanel = (settingsCanvasGroup != null && settingsCanvasGroup.alpha > 0.01f) 
            ? settingsCanvasGroup 
            : levelSelectCanvasGroup;

        StartCoroutine(FadeRoutine(activeSubPanel, 1f, 0f, 0.3f, () =>
        {
            SetCanvasGroupState(activeSubPanel, false);
            // Камера возвращается к дереву-порталу
            if (cameraController != null)
            {
                cameraController.MoveToPortal(() =>
                {
                    SetCanvasGroupState(mainCanvasGroup, true);
                    StartCoroutine(FadeRoutine(mainCanvasGroup, 0f, 1f, 0.3f, () => isInteractingBlocked = false));
                });
            }
            else
            {
                SetCanvasGroupState(mainCanvasGroup, true);
                StartCoroutine(FadeRoutine(mainCanvasGroup, 0f, 1f, 0.3f, () => isInteractingBlocked = false));
            }
        }));
    }

    public void OnOpenLevelsClicked()
    {
        if (isInteractingBlocked) return;
        StartCoroutine(FadeRoutine(mainCanvasGroup, 1f, 0f, 0.25f, () =>
        {
            SetCanvasGroupState(mainCanvasGroup, false);
            SetCanvasGroupState(levelSelectCanvasGroup, true);
            StartCoroutine(FadeRoutine(levelSelectCanvasGroup, 0f, 1f, 0.25f));
        }));
    }

    // --- Выход из игры (с отлетом камеры наверх и затемнением) ---

    public void OnQuitClicked()
    {
        if (isInteractingBlocked) return;
        isInteractingBlocked = true;

        // 1. Скрываем UI
        StartCoroutine(FadeRoutine(mainCanvasGroup, 1f, 0f, 0.3f, null));

        // 2. Камера уходит на (0, 22, -15), экран плавно темнеет
        float exitTime = 1.4f;
        if (cameraController != null)
        {
            cameraController.MoveToExit(exitTime);
        }

        if (screenFader != null)
        {
            screenFader.blocksRaycasts = true;
            StartCoroutine(FadeRoutine(screenFader, 0f, 1f, exitTime, () =>
            {
                DoQuit();
            }));
        }
        else
        {
            StartCoroutine(WaitAndExecute(exitTime, DoQuit));
        }
    }

    private void DoQuit()
    {
        Debug.Log("[MainMenu] Выход из игры");
        OnQuitExecuted?.Invoke();
#if UNITY_EDITOR
        // В редакторе не останавливаем PlayMode автоматически при тесте, если есть подписчики
        if (OnQuitExecuted == null)
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // --- Запуск игры ---

    public void OnPlayClicked()
    {
        if (isInteractingBlocked) return;
        isInteractingBlocked = true;

        int savedLevel = PlayerPrefs.GetInt("CurrentLevel", 0);

        // Плавный Fade Out экрана перед загрузкой сцены
        if (screenFader != null)
        {
            screenFader.blocksRaycasts = true;
            StartCoroutine(FadeRoutine(screenFader, 0f, 1f, 0.6f, () =>
            {
                LoadLevel(savedLevel);
            }));
        }
        else
        {
            LoadLevel(savedLevel);
        }
    }

    private void LoadLevel(int levelIndex)
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadLevel(levelIndex);
        }
        else
        {
            Debug.LogWarning("[MainMenu] LevelManager отсутствует на сцене, загружаем уровень по индексу.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(levelIndex);
        }
    }

    // --- Утилиты плавного затухания ---

    private void SetCanvasGroupState(CanvasGroup group, bool active)
    {
        if (group == null) return;
        group.alpha = active ? 1f : 0f;
        group.interactable = active;
        group.blocksRaycasts = active;
        group.gameObject.SetActive(active);
    }

    private IEnumerator FadeRoutine(CanvasGroup group, float from, float to, float duration, System.Action onComplete = null)
    {
        if (group == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        group.gameObject.SetActive(true);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        group.alpha = to;
        onComplete?.Invoke();
    }

    private IEnumerator WaitAndExecute(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    // --- Сетка уровней & Звук ---

    private void BuildLevelSelectGrid()
    {
        if (levelButtonsContainer == null || levelButtonPrefab == null) return;
        if (LevelManager.Instance == null) return;

        foreach (Transform child in levelButtonsContainer)
            Destroy(child.gameObject);

        string[] scenes = LevelManager.Instance.levelScenes;
        if (scenes == null) return;

        for (int i = 0; i < scenes.Length; i++)
        {
            int levelIndex = i;
            GameObject btnObj = Instantiate(levelButtonPrefab, levelButtonsContainer);
            TextMeshProUGUI label = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = (levelIndex + 1).ToString();

            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() =>
                {
                    if (isInteractingBlocked) return;
                    isInteractingBlocked = true;
                    if (screenFader != null)
                    {
                        screenFader.blocksRaycasts = true;
                        StartCoroutine(FadeRoutine(screenFader, 0f, 1f, 0.5f, () => LoadLevel(levelIndex)));
                    }
                    else LoadLevel(levelIndex);
                });
            }
        }
    }

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
            btnMute.onClick.AddListener(OnMuteToggleClicked);

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
            muteIcon.color = isSoundOn ? Color.white : new Color(1f, 1f, 1f, 0.35f);
    }
}
