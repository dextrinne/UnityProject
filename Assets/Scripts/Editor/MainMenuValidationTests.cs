using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuValidationTests
{
    private const string ScenePath = "Assets/Scenes/TestingMainMenu.unity";

    [Test]
    public void Test01_SceneExistsAndBuildSettingsAreConfigured()
    {
        Assert.IsTrue(System.IO.File.Exists(ScenePath), $"Сцена {ScenePath} должна существовать на диске.");
        
        bool foundInBuild = false;
        foreach (var s in UnityEditor.EditorBuildSettings.scenes)
        {
            if (s.path == ScenePath && s.enabled)
            {
                foundInBuild = true;
                break;
            }
        }
        Assert.IsTrue(foundInBuild, $"Сцена {ScenePath} должна быть добавлена и активна в Build Settings.");
    }

    [Test]
    public void Test02_CameraAnchorsAndControllerAreConfigured()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Assert.IsTrue(scene.IsValid(), "Сцена должна корректно открываться.");

        Camera cam = Camera.main;
        Assert.IsNotNull(cam, "В сцене должна присутствовать Main Camera.");

        MenuCameraController camCtrl = cam.GetComponent<MenuCameraController>();
        Assert.IsNotNull(camCtrl, "На Main Camera должен быть компонент MenuCameraController.");

        // Проверяем точки привязки
        Assert.IsNotNull(camCtrl.PortalAnchor, "portalAnchor (дерево-портал) должен быть назначен.");
        Assert.IsNotNull(camCtrl.BallAnchor, "ballAnchor (шарик) должен быть назначен.");
        Assert.IsNotNull(camCtrl.ExitAnchor, "exitAnchor (выход) должен быть назначен.");

        // Проверяем заданные координаты выхода (0, 22, -15) и вращение (55, 0, 0)
        Vector3 exitPos = camCtrl.ExitAnchor.position;
        Vector3 exitRot = camCtrl.ExitAnchor.eulerAngles;

        Assert.AreEqual(0f, exitPos.x, 0.05f, "ExitAnchor.x должен быть 0");
        Assert.AreEqual(22f, exitPos.y, 0.05f, "ExitAnchor.y должен быть 22");
        Assert.AreEqual(-15f, exitPos.z, 0.05f, "ExitAnchor.z должен быть -15");
        Assert.AreEqual(55f, Mathf.Round(exitRot.x), 1f, "ExitAnchor rot.x должен быть 55");
    }

    [Test]
    public void Test03_MainMenuControllerAndUIElementsConnected()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        MainMenuController menu = Object.FindAnyObjectByType<MainMenuController>();
        Assert.IsNotNull(menu, "MainMenuController должен присутствовать на сцене.");

        Assert.IsNotNull(menu.MainCanvasGroup, "mainCanvasGroup должен быть привязан.");
        Assert.IsNotNull(menu.SettingsCanvasGroup, "settingsCanvasGroup должен быть привязан.");
        Assert.IsNotNull(menu.ScreenFader, "screenFader (затемнение) должен быть привязан.");
        Assert.IsNotNull(menu.CameraController, "cameraController должен быть привязан к MainMenuController.");
    }

    [Test]
    public void Test04_BallAndEnvironmentAreReadyForMenu()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameObject ball = GameObject.Find("Ball");
        if (ball != null)
        {
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Assert.IsTrue(rb.isKinematic, "Ball Rigidbody должен быть kinematic в режиме меню, чтобы не скатываться.");
            }
        }
    }
}
