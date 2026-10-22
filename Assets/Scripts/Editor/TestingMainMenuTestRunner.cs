using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class TestingMainMenuTestRunner
{
    private const string ScenePath = "Assets/Scenes/TestingMainMenu.unity";

    [MenuItem("Tools/Validate TestingMainMenu Scene (Static Check)")]
    public static void ValidateSceneStatic()
    {
        Debug.Log("=========================================================");
        Debug.Log("[TestingMainMenuTestRunner] Запуск статической проверки сцены TestingMainMenu...");
        Debug.Log("=========================================================");

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError($"[TestingMainMenuTestRunner] Сцена по пути {ScenePath} не найдена или не валидна!");
            return;
        }

        int errors = 0;

        // 1. Проверяем камеру и MenuCameraController
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("[TestingMainMenuTestRunner] ОШИБКА: Main Camera не найдена!");
            errors++;
        }
        else
        {
            MenuCameraController camCtrl = cam.GetComponent<MenuCameraController>();
            if (camCtrl == null)
            {
                Debug.LogError("[TestingMainMenuTestRunner] ОШИБКА: MenuCameraController не найден на Main Camera!");
                errors++;
            }
            else
            {
                if (camCtrl.PortalAnchor == null)
                {
                    Debug.LogError("[TestingMainMenuTestRunner] ОШИБКА: portalAnchor не назначен!");
                    errors++;
                }
                else
                {
                    Debug.Log($"[TestingMainMenuTestRunner] OK: portalAnchor назначен ({camCtrl.PortalAnchor.position})");
                }

                if (camCtrl.BallAnchor == null)
                {
                    Debug.LogError("[TestingMainMenuTestRunner] ОШИБКА: ballAnchor не назначен!");
                    errors++;
                }
                else
                {
                    Debug.Log($"[TestingMainMenuTestRunner] OK: ballAnchor назначен ({camCtrl.BallAnchor.position})");
                }

                if (camCtrl.ExitAnchor == null)
                {
                    Debug.LogError("[TestingMainMenuTestRunner] ОШИБКА: exitAnchor не назначен!");
                    errors++;
                }
                else
                {
                    Vector3 exitPos = camCtrl.ExitAnchor.position;
                    Vector3 exitRot = camCtrl.ExitAnchor.eulerAngles;
                    Debug.Log($"[TestingMainMenuTestRunner] OK: exitAnchor назначен (pos: {exitPos}, rot: {exitRot})");
                    if (Vector3.Distance(exitPos, new Vector3(0, 22, -15)) > 0.1f)
                    {
                        Debug.LogWarning("[TestingMainMenuTestRunner] ВНИМАНИЕ: exitAnchor pos отличается от (0, 22, -15)!");
                    }
                }
            }
        }

        // 2. Проверяем мяч
        GameObject ball = GameObject.Find("Ball");
        if (ball != null)
        {
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                Debug.LogWarning("[TestingMainMenuTestRunner] ВНИМАНИЕ: Rigidbody на Ball не kinematic (может скатиться во время меню). Исправляем...");
                rb.isKinematic = true;
                rb.useGravity = false;
                EditorUtility.SetDirty(ball);
            }
            else
            {
                Debug.Log("[TestingMainMenuTestRunner] OK: Ball настроен корректно (isKinematic=true)");
            }
        }

        // 3. Проверяем MainMenuController
        MainMenuController menuCtrl = Object.FindAnyObjectByType<MainMenuController>();
        if (menuCtrl == null)
        {
            Debug.LogError("[TestingMainMenuTestRunner] ОШИБКА: MainMenuController отсутствует на сцене!");
            errors++;
        }
        else
        {
            Debug.Log("[TestingMainMenuTestRunner] OK: MainMenuController найден.");
            if (menuCtrl.MainCanvasGroup == null) { Debug.LogError("ОШИБКА: MainCanvasGroup не привязан!"); errors++; }
            if (menuCtrl.SettingsCanvasGroup == null) { Debug.LogError("ОШИБКА: SettingsCanvasGroup не привязан!"); errors++; }
            if (menuCtrl.ScreenFader == null) { Debug.LogError("ОШИБКА: ScreenFader не привязан!"); errors++; }
        }

        // 4. Проверяем Build Settings
        bool inBuildSettings = false;
        foreach (var s in EditorBuildSettings.scenes)
        {
            if (s.path == ScenePath && s.enabled)
            {
                inBuildSettings = true;
                break;
            }
        }

        if (inBuildSettings)
            Debug.Log($"[TestingMainMenuTestRunner] OK: Сцена {ScenePath} зарегистрирована в Build Settings.");
        else
        {
            Debug.LogWarning($"[TestingMainMenuTestRunner] ВНИМАНИЕ: Сцена {ScenePath} не найдена в Build Settings! Регистрируем...");
            TestingMainMenuBuilder.BuildTestingMainMenuScene();
        }

        Debug.Log("=========================================================");
        if (errors == 0)
        {
            Debug.Log("[TestingMainMenuTestRunner] >>> ВСЕ СТАТИЧЕСКИЕ ПРОВЕРКИ ПРОЙДЕНЫ УСПЕШНО! <<<");
        }
        else
        {
            Debug.LogError($"[TestingMainMenuTestRunner] >>> ОБНАРУЖЕНО {errors} ОШИБОК! <<<");
        }
        Debug.Log("=========================================================");
    }

    [MenuItem("Tools/Run Automated PlayMode Test")]
    public static void RunPlayModeTest()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject manager = GameObject.Find("Manager");
        if (manager != null)
        {
            MainMenuTester tester = manager.GetComponent<MainMenuTester>();
            if (tester == null) tester = manager.AddComponent<MainMenuTester>();

            SerializedObject so = new SerializedObject(tester);
            so.FindProperty("runTestOnStart").boolValue = true;
            so.ApplyModifiedProperties();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }

        EditorApplication.isPlaying = true;
    }
}
