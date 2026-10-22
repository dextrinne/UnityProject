using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class TestingMainMenuBuilder
{
    private const string SourceScenePath = "Assets/Scenes/level1.unity";
    private const string TargetScenePath = "Assets/Scenes/TestingMainMenu.unity";

    [InitializeOnLoadMethod]
    private static void AutoRunOnLoad()
    {
        if (!EditorPrefs.GetBool("TestingMainMenu_AutoBuilt_v1", false))
        {
            EditorPrefs.SetBool("TestingMainMenu_AutoBuilt_v1", true);
            EditorApplication.delayCall += BuildTestingMainMenuScene;
        }
    }

    [MenuItem("Tools/Build TestingMainMenu Scene")]
    public static void BuildTestingMainMenuScene()
    {
        Debug.Log("[TestingMainMenuBuilder] Начинаем создание сцены TestingMainMenu...");

        // 1. Копируем исходную сцену level1, если целевой сцены нет или для чистого билда
        if (!File.Exists(TargetScenePath))
        {
            if (!AssetDatabase.CopyAsset(SourceScenePath, TargetScenePath))
            {
                Debug.LogError($"[TestingMainMenuBuilder] Не удалось скопировать {SourceScenePath} в {TargetScenePath}");
                return;
            }
            AssetDatabase.Refresh();
        }

        // 2. Открываем сцену TestingMainMenu
        var scene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError("[TestingMainMenuBuilder] Не удалось открыть сцену TestingMainMenu!");
            return;
        }

        // 3. Отключаем/удаляем игровые UI и триггеры, не относящиеся к главному меню
        string[] objectsToRemove = { "HUDCanvas", "TutorialCanvas", "TutorialHint", "WinPanel", "PauseMenu_Root", "TutorialManager" };
        foreach (string objName in objectsToRemove)
        {
            GameObject go = GameObject.Find(objName);
            if (go != null)
            {
                Undo.DestroyObjectImmediate(go);
            }
        }

        // 4. Настраиваем мяч (Ball) - делаем кинематическим, чтобы он красиво висел в воздухе/стоял на старте
        GameObject ball = GameObject.Find("Ball");
        if (ball != null)
        {
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // 5. Отключаем управление наклоном платформы в режиме меню
        TiltPlatform[] tiltPlatforms = Object.FindObjectsByType<TiltPlatform>(FindObjectsSortMode.None);
        foreach (var tp in tiltPlatforms)
        {
            tp.enabled = false;
        }

        // 6. Создаем/настраиваем CameraAnchors (точки привязки камеры)
        GameObject anchorsRoot = GameObject.Find("CameraAnchors");
        if (anchorsRoot == null)
        {
            anchorsRoot = new GameObject("CameraAnchors");
            Undo.RegisterCreatedObjectUndo(anchorsRoot, "Create CameraAnchors");
        }

        // Точка 1: Портал (дерево-портал по умолчанию)
        Transform portalAnchor = anchorsRoot.transform.Find("Anchor_Portal");
        if (portalAnchor == null)
        {
            GameObject pGo = new GameObject("Anchor_Portal");
            pGo.transform.SetParent(anchorsRoot.transform);
            portalAnchor = pGo.transform;
        }
        portalAnchor.position = new Vector3(-1.68163f, 5.55238f, -1.03135f);
        portalAnchor.rotation = new Quaternion(0.064943f, 0.396065f, -0.028072f, 0.915493f);

        // Точка 2: Шарик (настройки)
        Transform ballAnchor = anchorsRoot.transform.Find("Anchor_Ball");
        if (ballAnchor == null)
        {
            GameObject bGo = new GameObject("Anchor_Ball");
            bGo.transform.SetParent(anchorsRoot.transform);
            ballAnchor = bGo.transform;
        }
        ballAnchor.position = new Vector3(-3.99229f, 1.93628f, -9.465f);
        ballAnchor.rotation = new Quaternion(0.050085f, -0.439397f, 0.024555f, 0.896559f);

        // Точка 3: Выход (0, 22, -15 / rot: 55, 0, 0)
        Transform exitAnchor = anchorsRoot.transform.Find("Anchor_Exit");
        if (exitAnchor == null)
        {
            GameObject eGo = new GameObject("Anchor_Exit");
            eGo.transform.SetParent(anchorsRoot.transform);
            exitAnchor = eGo.transform;
        }
        exitAnchor.position = new Vector3(0f, 22f, -15f);
        exitAnchor.rotation = Quaternion.Euler(55f, 0f, 0f);

        // 7. Настраиваем Main Camera
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            MenuCameraController camCtrl = mainCam.GetComponent<MenuCameraController>();
            if (camCtrl == null) camCtrl = mainCam.gameObject.AddComponent<MenuCameraController>();

            SerializedObject soCam = new SerializedObject(camCtrl);
            soCam.FindProperty("portalAnchor").objectReferenceValue = portalAnchor;
            soCam.FindProperty("ballAnchor").objectReferenceValue = ballAnchor;
            soCam.FindProperty("exitAnchor").objectReferenceValue = exitAnchor;
            soCam.FindProperty("transitionDuration").floatValue = 1.2f;
            soCam.FindProperty("enableIdleBreathing").boolValue = true;
            soCam.ApplyModifiedProperties();

            mainCam.transform.position = portalAnchor.position;
            mainCam.transform.rotation = portalAnchor.rotation;
        }

        // 8. Создаем/настраиваем Canvas для главного меню
        GameObject canvasGo = GameObject.Find("Canvas");
        if (canvasGo == null)
        {
            canvasGo = new GameObject("Canvas");
            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Canvas");
        }

        Canvas canvas = canvasGo.GetComponent<Canvas>();
        if (canvas == null) canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        if (canvasGo.GetComponent<GraphicRaycaster>() == null)
            canvasGo.AddComponent<GraphicRaycaster>();

        // Очищаем старые дочерние UI элементы внутри Canvas (если пересоздаем)
        for (int i = canvasGo.transform.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(canvasGo.transform.GetChild(i).gameObject);
        }

        // Шрифты
        TMP_FontAsset titleFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Roboto-Bold SDF.asset");
        if (titleFont == null)
            titleFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

        // 8.1 Создаем ScreenFader
        GameObject faderGo = new GameObject("ScreenFader", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        faderGo.transform.SetParent(canvasGo.transform, false);
        RectTransform faderRect = faderGo.GetComponent<RectTransform>();
        StretchFull(faderRect);
        Image faderImg = faderGo.GetComponent<Image>();
        faderImg.color = Color.black;
        CanvasGroup faderGroup = faderGo.GetComponent<CanvasGroup>();
        faderGroup.alpha = 1f;
        faderGroup.blocksRaycasts = true;

        // 8.2 Создаем Panel_Main (Главное меню с кнопками)
        GameObject mainPanelGo = new GameObject("Panel_Main", typeof(RectTransform), typeof(CanvasGroup));
        mainPanelGo.transform.SetParent(canvasGo.transform, false);
        RectTransform mainPanelRect = mainPanelGo.GetComponent<RectTransform>();
        mainPanelRect.anchorMin = new Vector2(0f, 0f);
        mainPanelRect.anchorMax = new Vector2(0.45f, 1f);
        mainPanelRect.offsetMin = new Vector2(80, 60);
        mainPanelRect.offsetMax = new Vector2(0, -60);
        CanvasGroup mainGroup = mainPanelGo.GetComponent<CanvasGroup>();

        // Заголовок игры (Celeste-style)
        GameObject titleGo = new GameObject("GameTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGo.transform.SetParent(mainPanelGo.transform, false);
        RectTransform titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.75f);
        titleRect.anchorMax = new Vector2(1, 0.95f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        TextMeshProUGUI titleText = titleGo.GetComponent<TextMeshProUGUI>();
        if (titleFont != null) titleText.font = titleFont;
        titleText.text = "PORTAL\nJOURNEY";
        titleText.fontSize = 72;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.TopLeft;
        titleText.color = new Color(0.98f, 0.92f, 0.75f); // Золотисто-кремовый

        // Контейнер кнопок главного меню
        GameObject btnContainer = new GameObject("ButtonsContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
        btnContainer.transform.SetParent(mainPanelGo.transform, false);
        RectTransform btnContRect = btnContainer.GetComponent<RectTransform>();
        btnContRect.anchorMin = new Vector2(0, 0.15f);
        btnContRect.anchorMax = new Vector2(1, 0.7f);
        btnContRect.offsetMin = Vector2.zero;
        btnContRect.offsetMax = Vector2.zero;
        VerticalLayoutGroup vlg = btnContainer.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 16;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Создаем кнопки
        Button btnPlay = CreateStyledButton(btnContainer.transform, "Btn_Play", "ИГРАТЬ", new Color(0.18f, 0.65f, 0.42f), new Color(1f, 1f, 1f), titleFont, 64);
        Button btnLevels = CreateStyledButton(btnContainer.transform, "Btn_Levels", "УРОВНИ", new Color(0.15f, 0.2f, 0.3f, 0.85f), new Color(0.9f, 0.95f, 1f), titleFont, 54);
        Button btnSettings = CreateStyledButton(btnContainer.transform, "Btn_Settings", "НАСТРОЙКИ", new Color(0.15f, 0.2f, 0.3f, 0.85f), new Color(0.9f, 0.95f, 1f), titleFont, 54);
        Button btnQuit = CreateStyledButton(btnContainer.transform, "Btn_Quit", "ВЫХОД", new Color(0.5f, 0.15f, 0.2f, 0.85f), new Color(1f, 0.9f, 0.9f), titleFont, 54);

        // 8.3 Создаем Panel_Settings (Экран настроек возле шарика)
        GameObject settingsPanelGo = new GameObject("Panel_Settings", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        settingsPanelGo.transform.SetParent(canvasGo.transform, false);
        RectTransform settingsRect = settingsPanelGo.GetComponent<RectTransform>();
        settingsRect.anchorMin = new Vector2(0.5f, 0.5f);
        settingsRect.anchorMax = new Vector2(0.5f, 0.5f);
        settingsRect.sizeDelta = new Vector2(620, 520);
        settingsRect.anchoredPosition = new Vector2(0, 0);
        Image settingsBg = settingsPanelGo.GetComponent<Image>();
        settingsBg.color = new Color(0.08f, 0.1f, 0.15f, 0.92f); // Стильная темная карточка
        CanvasGroup settingsGroup = settingsPanelGo.GetComponent<CanvasGroup>();
        settingsGroup.alpha = 0f;
        settingsGroup.blocksRaycasts = false;
        settingsGroup.interactable = false;
        settingsPanelGo.SetActive(false);

        // Заголовок настроек
        GameObject sTitleGo = new GameObject("SettingsTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        sTitleGo.transform.SetParent(settingsPanelGo.transform, false);
        RectTransform sTitleRect = sTitleGo.GetComponent<RectTransform>();
        sTitleRect.anchorMin = new Vector2(0, 0.82f);
        sTitleRect.anchorMax = new Vector2(1, 0.96f);
        sTitleRect.offsetMin = Vector2.zero;
        sTitleRect.offsetMax = Vector2.zero;
        TextMeshProUGUI sTitleText = sTitleGo.GetComponent<TextMeshProUGUI>();
        if (titleFont != null) sTitleText.font = titleFont;
        sTitleText.text = "НАСТРОЙКИ";
        sTitleText.fontSize = 42;
        sTitleText.alignment = TextAlignmentOptions.Center;
        sTitleText.color = Color.white;

        // Ползунок звука
        GameObject sliderObj = CreateVolumeSlider(settingsPanelGo.transform, titleFont);
        Slider volSlider = sliderObj.GetComponentInChildren<Slider>();

        // Кнопка Mute
        Button btnMute = CreateStyledButton(settingsPanelGo.transform, "Btn_Mute", "ВКЛ / ВЫКЛ ЗВУК", new Color(0.2f, 0.25f, 0.35f), Color.white, titleFont, 48);
        RectTransform muteRect = btnMute.GetComponent<RectTransform>();
        muteRect.anchoredPosition = new Vector2(0, -60);
        muteRect.sizeDelta = new Vector2(360, 48);

        // Кнопка Назад из настроек
        Button btnBackSettings = CreateStyledButton(settingsPanelGo.transform, "Btn_BackSettings", "НАЗАД К ПОРТАЛУ", new Color(0.2f, 0.5f, 0.4f), Color.white, titleFont, 52);
        RectTransform backSettingsRect = btnBackSettings.GetComponent<RectTransform>();
        backSettingsRect.anchoredPosition = new Vector2(0, -160);
        backSettingsRect.sizeDelta = new Vector2(360, 52);

        // 8.4 Создаем Panel_LevelSelect (Экран выбора уровней)
        GameObject levelsPanelGo = new GameObject("Panel_LevelSelect", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        levelsPanelGo.transform.SetParent(canvasGo.transform, false);
        RectTransform levelsRect = levelsPanelGo.GetComponent<RectTransform>();
        levelsRect.anchorMin = new Vector2(0.5f, 0.5f);
        levelsRect.anchorMax = new Vector2(0.5f, 0.5f);
        levelsRect.sizeDelta = new Vector2(700, 560);
        levelsRect.anchoredPosition = new Vector2(0, 0);
        Image levelsBg = levelsPanelGo.GetComponent<Image>();
        levelsBg.color = new Color(0.08f, 0.1f, 0.15f, 0.92f);
        CanvasGroup levelsGroup = levelsPanelGo.GetComponent<CanvasGroup>();
        levelsGroup.alpha = 0f;
        levelsGroup.blocksRaycasts = false;
        levelsGroup.interactable = false;
        levelsPanelGo.SetActive(false);

        // Заголовок уровней
        GameObject lTitleGo = new GameObject("LevelsTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        lTitleGo.transform.SetParent(levelsPanelGo.transform, false);
        RectTransform lTitleRect = lTitleGo.GetComponent<RectTransform>();
        lTitleRect.anchorMin = new Vector2(0, 0.84f);
        lTitleRect.anchorMax = new Vector2(1, 0.96f);
        lTitleRect.offsetMin = Vector2.zero;
        lTitleRect.offsetMax = Vector2.zero;
        TextMeshProUGUI lTitleText = lTitleGo.GetComponent<TextMeshProUGUI>();
        if (titleFont != null) lTitleText.font = titleFont;
        lTitleText.text = "ВЫБОР УРОВНЯ";
        lTitleText.fontSize = 42;
        lTitleText.alignment = TextAlignmentOptions.Center;
        lTitleText.color = Color.white;

        // Сетка уровней
        GameObject gridGo = new GameObject("Level_Grid", typeof(RectTransform), typeof(GridLayoutGroup));
        gridGo.transform.SetParent(levelsPanelGo.transform, false);
        RectTransform gridRect = gridGo.GetComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0.1f, 0.28f);
        gridRect.anchorMax = new Vector2(0.9f, 0.8f);
        gridRect.offsetMin = Vector2.zero;
        gridRect.offsetMax = Vector2.zero;
        GridLayoutGroup grid = gridGo.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(100, 100);
        grid.spacing = new Vector2(24, 24);
        grid.childAlignment = TextAnchor.MiddleCenter;

        // Кнопка Назад из уровней
        Button btnBackLevels = CreateStyledButton(levelsPanelGo.transform, "Btn_BackLevels", "НАЗАД", new Color(0.2f, 0.5f, 0.4f), Color.white, titleFont, 52);
        RectTransform backLevelsRect = btnBackLevels.GetComponent<RectTransform>();
        backLevelsRect.anchoredPosition = new Vector2(0, -210);
        backLevelsRect.sizeDelta = new Vector2(320, 52);

        // 9. Настраиваем MainMenuController
        GameObject managerGo = GameObject.Find("Manager");
        if (managerGo == null)
        {
            managerGo = new GameObject("Manager");
            Undo.RegisterCreatedObjectUndo(managerGo, "Create Manager");
        }

        LevelManager lvlMgr = managerGo.GetComponent<LevelManager>();
        if (lvlMgr == null) lvlMgr = managerGo.AddComponent<LevelManager>();
        SerializedObject soLvl = new SerializedObject(lvlMgr);
        SerializedProperty scenesProp = soLvl.FindProperty("levelScenes");
        scenesProp.arraySize = 5;
        scenesProp.GetArrayElementAtIndex(0).stringValue = "level1";
        scenesProp.GetArrayElementAtIndex(1).stringValue = "level2";
        scenesProp.GetArrayElementAtIndex(2).stringValue = "level3";
        scenesProp.GetArrayElementAtIndex(3).stringValue = "level4";
        scenesProp.GetArrayElementAtIndex(4).stringValue = "level5";
        soLvl.ApplyModifiedProperties();

        MainMenuController menuCtrl = managerGo.GetComponent<MainMenuController>();
        if (menuCtrl == null) menuCtrl = managerGo.AddComponent<MainMenuController>();

        MainMenuTester tester = managerGo.GetComponent<MainMenuTester>();
        if (tester == null) tester = managerGo.AddComponent<MainMenuTester>();

        GameObject levelBtnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/LevelButtonPrefab.prefab");

        SerializedObject soMenu = new SerializedObject(menuCtrl);
        soMenu.FindProperty("cameraController").objectReferenceValue = mainCam != null ? mainCam.GetComponent<MenuCameraController>() : null;
        soMenu.FindProperty("mainCanvasGroup").objectReferenceValue = mainGroup;
        soMenu.FindProperty("settingsCanvasGroup").objectReferenceValue = settingsGroup;
        soMenu.FindProperty("levelSelectCanvasGroup").objectReferenceValue = levelsGroup;
        soMenu.FindProperty("screenFader").objectReferenceValue = faderGroup;
        soMenu.FindProperty("fadeDuration").floatValue = 0.8f;
        soMenu.FindProperty("btnPlay").objectReferenceValue = btnPlay;
        soMenu.FindProperty("btnOpenLevels").objectReferenceValue = btnLevels;
        soMenu.FindProperty("btnOpenSettings").objectReferenceValue = btnSettings;
        soMenu.FindProperty("btnQuit").objectReferenceValue = btnQuit;
        soMenu.FindProperty("btnBackFromLevels").objectReferenceValue = btnBackLevels;
        soMenu.FindProperty("btnBackFromSettings").objectReferenceValue = btnBackSettings;
        soMenu.FindProperty("levelButtonsContainer").objectReferenceValue = gridGo.transform;
        soMenu.FindProperty("levelButtonPrefab").objectReferenceValue = levelBtnPrefab;
        soMenu.FindProperty("volumeSlider").objectReferenceValue = volSlider;
        soMenu.FindProperty("btnMute").objectReferenceValue = btnMute;
        soMenu.ApplyModifiedProperties();

        // 10. Убеждаемся в наличии EventSystem
        EnsureEventSystem();

        // 11. Сохраняем сцену
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[TestingMainMenuBuilder] Сцена TestingMainMenu успешно настроена и сохранена!");

        // 12. Добавляем TestingMainMenu в Build Settings на 0 место
        AddSceneToBuildSettings(TargetScenePath);
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static Button CreateStyledButton(Transform parent, string name, string text, Color bgColor, Color textColor, TMP_FontAsset font, float height)
    {
        GameObject btnGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(MenuButtonFeedback));
        btnGo.transform.SetParent(parent, false);

        RectTransform rt = btnGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(320, height);

        Image img = btnGo.GetComponent<Image>();
        img.color = bgColor;

        Button btn = btnGo.GetComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = bgColor;
        colors.highlightedColor = bgColor * 1.15f;
        colors.pressedColor = bgColor * 0.85f;
        colors.selectedColor = bgColor * 1.15f;
        btn.colors = colors;

        GameObject txtGo = new GameObject("Text (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(btnGo.transform, false);
        RectTransform txtRt = txtGo.GetComponent<RectTransform>();
        StretchFull(txtRt);

        TextMeshProUGUI tmp = txtGo.GetComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = text;
        tmp.fontSize = Mathf.RoundToInt(height * 0.44f);
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;

        return btn;
    }

    private static GameObject CreateVolumeSlider(Transform parent, TMP_FontAsset font)
    {
        GameObject sliderRoot = new GameObject("VolumeGroup", typeof(RectTransform));
        sliderRoot.transform.SetParent(parent, false);
        RectTransform rootRt = sliderRoot.GetComponent<RectTransform>();
        rootRt.anchoredPosition = new Vector2(0, 40);
        rootRt.sizeDelta = new Vector2(400, 60);

        GameObject labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(sliderRoot.transform, false);
        RectTransform labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.anchoredPosition = new Vector2(0, 24);
        labelRt.sizeDelta = new Vector2(360, 30);
        TextMeshProUGUI lblText = labelGo.GetComponent<TextMeshProUGUI>();
        if (font != null) lblText.font = font;
        lblText.text = "ГРОМКОСТЬ ЗВУКА";
        lblText.fontSize = 20;
        lblText.alignment = TextAlignmentOptions.Center;
        lblText.color = new Color(0.85f, 0.9f, 1f);

        GameObject sliderGo = new GameObject("Slider_Volume", typeof(RectTransform), typeof(Slider));
        sliderGo.transform.SetParent(sliderRoot.transform, false);
        RectTransform sliderRt = sliderGo.GetComponent<RectTransform>();
        sliderRt.anchoredPosition = new Vector2(0, -10);
        sliderRt.sizeDelta = new Vector2(360, 24);

        Slider slider = sliderGo.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.8f;

        GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(sliderGo.transform, false);
        StretchFull(bg.GetComponent<RectTransform>());
        bg.GetComponent<Image>().color = new Color(0.2f, 0.25f, 0.35f, 0.7f);

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderGo.transform, false);
        RectTransform faRt = fillArea.GetComponent<RectTransform>();
        faRt.anchorMin = new Vector2(0, 0.25f);
        faRt.anchorMax = new Vector2(1, 0.75f);
        faRt.offsetMin = new Vector2(5, 0);
        faRt.offsetMax = new Vector2(-5, 0);

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        StretchFull(fill.GetComponent<RectTransform>());
        Image fillImg = fill.GetComponent<Image>();
        fillImg.color = new Color(0.25f, 0.8f, 0.5f);
        slider.fillRect = fill.GetComponent<RectTransform>();

        return sliderGo;
    }

    private static void EnsureEventSystem()
    {
        UnityEngine.EventSystems.EventSystem es = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (es == null)
        {
            GameObject esGo = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            Undo.RegisterCreatedObjectUndo(esGo, "Create EventSystem");
        }
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        bool exists = false;
        foreach (var s in scenes)
        {
            if (s.path == scenePath)
            {
                exists = true;
                break;
            }
        }

        if (!exists)
        {
            EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
            newScenes[0] = new EditorBuildSettingsScene(scenePath, true);
            for (int i = 0; i < scenes.Length; i++)
            {
                newScenes[i + 1] = scenes[i];
            }
            EditorBuildSettings.scenes = newScenes;
            Debug.Log($"[TestingMainMenuBuilder] Сцена {scenePath} добавлена в Build Settings на позицию 0.");
        }
    }
}
