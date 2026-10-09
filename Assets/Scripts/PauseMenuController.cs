using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Панель меню")]
    [Tooltip("Сама выезжающая панель/шторка")]
    [SerializeField] private RectTransform menuPanel;

    [Header("Точки перемещения панели")]
    [SerializeField] private RectTransform startClosedPoint;
    [SerializeField] private RectTransform targetOpenedPoint;

    [Header("Элементы для каскадной анимации (по порядку)")]
    [Tooltip("Заголовок, кнопки и блок звука в порядке их появления")]
    [SerializeField] private RectTransform[] menuItems;

    [Header("Фон и кнопка паузы на экране")]
    [SerializeField] private CanvasGroup backgroundOverlay;
    [SerializeField] private Button btnOpenPause;
    [Tooltip("На сколько пикселей вправо смещается кнопка настроек при открытии")]
    [SerializeField] private float btnOpenHideOffsetX = 180f;

    [Header("Кнопки меню")]
    [SerializeField] private Button btnResume;
    [SerializeField] private Button btnRestart;
    [SerializeField] private Button btnMainMenu;

    [Header("Настройки звука")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button btnMute;
    [SerializeField] private Image muteIcon;

    [Header("Параметры анимации DOTween")]
    [SerializeField] private float panelMoveDuration = 0.35f;
    [SerializeField] private float itemAnimDuration = 0.3f;
    [SerializeField] private float itemStaggerDelay = 0.06f; // Пауза между появлением кнопок
    [SerializeField] private float itemStartOffsetX = -80f;   // Смещение кнопок перед вылетом
    [SerializeField] private Ease panelEase = Ease.OutQuad;
    [SerializeField] private Ease itemEase = Ease.OutBack;

    [Header("Сцены")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    // Позиции панели
    private Vector2 closedPos;
    private Vector2 openedPos;

    // Данные кнопки вызова паузы
    private RectTransform btnOpenRect;
    private Vector2 btnOpenOriginalPos;
    private CanvasGroup btnOpenCanvasGroup;

    // Данные каскадных элементов
    private Vector2[] itemOriginalPositions;
    private CanvasGroup[] itemCanvasGroups;

    private bool isPaused = false;
    private Sequence currentSequence;
    private float lastNonZeroVolume = 0.8f;

    void Awake()
    {
        if (menuPanel == null)
        {
            Debug.LogError("[PauseMenu] Поле menuPanel не назначено!");
            return;
        }

        // 1. Координаты шторки
        if (startClosedPoint != null && targetOpenedPoint != null)
        {
            closedPos = startClosedPoint.anchoredPosition;
            openedPos = targetOpenedPoint.anchoredPosition;
        }
        else
        {
            openedPos = menuPanel.anchoredPosition;
            closedPos = new Vector2(-menuPanel.rect.width - 250f, openedPos.y);
        }

        menuPanel.anchoredPosition = closedPos;

        // 2. Инициализация кнопки вызова паузы
        if (btnOpenPause != null)
        {
            btnOpenRect = btnOpenPause.GetComponent<RectTransform>();
            if (btnOpenRect != null)
            {
                btnOpenOriginalPos = btnOpenRect.anchoredPosition;
            }

            btnOpenCanvasGroup = btnOpenPause.GetComponent<CanvasGroup>();
            if (btnOpenCanvasGroup == null)
            {
                btnOpenCanvasGroup = btnOpenPause.gameObject.AddComponent<CanvasGroup>();
            }
        }

        // 3. Фон-затемнение
        if (backgroundOverlay != null)
        {
            backgroundOverlay.alpha = 0f;
            backgroundOverlay.blocksRaycasts = false;
            backgroundOverlay.interactable = false;
        }

        // 4. Запоминаем позиции кнопок внутри меню
        if (menuItems != null && menuItems.Length > 0)
        {
            itemOriginalPositions = new Vector2[menuItems.Length];
            itemCanvasGroups = new CanvasGroup[menuItems.Length];

            for (int i = 0; i < menuItems.Length; i++)
            {
                if (menuItems[i] != null)
                {
                    itemOriginalPositions[i] = menuItems[i].anchoredPosition;

                    CanvasGroup cg = menuItems[i].GetComponent<CanvasGroup>();
                    if (cg == null) cg = menuItems[i].gameObject.AddComponent<CanvasGroup>();
                    itemCanvasGroups[i] = cg;
                }
            }
        }
    }

    void Start()
    {
        if (btnOpenPause != null)
        {
            btnOpenPause.onClick.RemoveAllListeners();
            btnOpenPause.onClick.AddListener(OpenMenu);
        }

        if (btnResume != null) btnResume.onClick.AddListener(CloseMenu);
        if (btnRestart != null) btnRestart.onClick.AddListener(OnRestartClicked);
        if (btnMainMenu != null) btnMainMenu.onClick.AddListener(OnMainMenuClicked);

        // Инициализация звука
        float currentVol = MusicManager.Instance != null ? MusicManager.Instance.GetVolume() : 1f;
        if (currentVol > 0.01f) lastNonZeroVolume = currentVol;

        if (volumeSlider != null)
        {
            volumeSlider.value = currentVol;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        if (btnMute != null)
            btnMute.onClick.AddListener(OnMuteToggleClicked);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) CloseMenu();
            else OpenMenu();
        }
    }

    public void OpenMenu()
    {
        if (isPaused) return;
        isPaused = true;

        currentSequence?.Kill();
        currentSequence = DOTween.Sequence().SetUpdate(true); // Анимация при timeScale = 0

        // 1. Анимация кнопки открытия: уезжает вправо и растворяется
        if (btnOpenRect != null)
        {
            btnOpenPause.interactable = false;
            Vector2 targetPos = btnOpenOriginalPos + new Vector2(btnOpenHideOffsetX, 0f);
            currentSequence.Join(btnOpenRect.DOAnchorPos(targetPos, panelMoveDuration * 0.7f).SetEase(Ease.InQuad));

            if (btnOpenCanvasGroup != null)
                currentSequence.Join(btnOpenCanvasGroup.DOFade(0f, panelMoveDuration * 0.6f));
        }

        // 2. Включаем блокировку кликов и проявляем фон
        if (backgroundOverlay != null)
        {
            backgroundOverlay.blocksRaycasts = true;
            backgroundOverlay.interactable = true;
            currentSequence.Join(backgroundOverlay.DOFade(1f, panelMoveDuration));
        }

        // 3. Выдвигаем саму панель
        currentSequence.Join(menuPanel.DOAnchorPos(openedPos, panelMoveDuration).SetEase(panelEase));

        // 4. Каскадный вылет кнопок меню слева
        if (menuItems != null)
        {
            for (int i = 0; i < menuItems.Length; i++)
            {
                if (menuItems[i] == null) continue;

                int index = i;
                RectTransform item = menuItems[index];
                CanvasGroup cg = itemCanvasGroups[index];

                item.anchoredPosition = itemOriginalPositions[index] + new Vector2(itemStartOffsetX, 0f);
                if (cg != null) cg.alpha = 0f;

                float startTime = 0.1f + (index * itemStaggerDelay);

                currentSequence.Insert(startTime, item.DOAnchorPos(itemOriginalPositions[index], itemAnimDuration).SetEase(itemEase));
                if (cg != null) currentSequence.Insert(startTime, cg.DOFade(1f, itemAnimDuration * 0.8f));
            }
        }

        Time.timeScale = 0f;
    }

    public void CloseMenu()
    {
        if (!isPaused) return;
        isPaused = false;

        Time.timeScale = 1f;
        currentSequence?.Kill();
        currentSequence = DOTween.Sequence().SetUpdate(true);

        // 1. Прячем кнопки меню в обратном порядке
        if (menuItems != null)
        {
            int count = menuItems.Length;
            for (int i = 0; i < count; i++)
            {
                if (menuItems[i] == null) continue;

                int reverseIndex = count - 1 - i;
                RectTransform item = menuItems[reverseIndex];
                CanvasGroup cg = itemCanvasGroups[reverseIndex];

                float startTime = i * (itemStaggerDelay * 0.5f);
                Vector2 targetPos = itemOriginalPositions[reverseIndex] + new Vector2(itemStartOffsetX, 0f);

                currentSequence.Insert(startTime, item.DOAnchorPos(targetPos, 0.2f).SetEase(Ease.InQuad));
                if (cg != null) currentSequence.Insert(startTime, cg.DOFade(0f, 0.15f));
            }
        }

        // 2. Задвигаем шторку и убираем оверлей
        float panelCloseStart = 0.1f;
        currentSequence.Insert(panelCloseStart, menuPanel.DOAnchorPos(closedPos, panelMoveDuration * 0.8f).SetEase(Ease.InQuad));

        if (backgroundOverlay != null)
        {
            backgroundOverlay.blocksRaycasts = false;
            backgroundOverlay.interactable = false;
            currentSequence.Insert(panelCloseStart, backgroundOverlay.DOFade(0f, panelMoveDuration * 0.8f));
        }

        // 3. Возвращаем кнопку открытия на исходную позицию справа с мягким отскоком
        if (btnOpenRect != null)
        {
            float btnReturnStart = panelCloseStart + (panelMoveDuration * 0.3f);

            currentSequence.Insert(btnReturnStart, btnOpenRect.DOAnchorPos(btnOpenOriginalPos, panelMoveDuration).SetEase(Ease.OutBack));
            if (btnOpenCanvasGroup != null)
                currentSequence.Insert(btnReturnStart, btnOpenCanvasGroup.DOFade(1f, panelMoveDuration * 0.8f));

            currentSequence.OnComplete(() =>
            {
                if (btnOpenPause != null)
                    btnOpenPause.interactable = true;
            });
        }
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

    private void OnRestartClicked()
    {
        Time.timeScale = 1f;
        if (LevelManager.Instance != null)
            LevelManager.Instance.RestartLevel();
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void OnDestroy()
    {
        currentSequence?.Kill();
        DOTween.Kill(menuPanel);
        if (btnOpenRect != null) DOTween.Kill(btnOpenRect);
        if (backgroundOverlay != null) DOTween.Kill(backgroundOverlay);
    }
}