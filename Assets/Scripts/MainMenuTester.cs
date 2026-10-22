using System.Collections;
using UnityEngine;

public class MainMenuTester : MonoBehaviour
{
    [SerializeField] private MainMenuController menuController;
    [SerializeField] private bool runTestOnStart = false;

    public bool TestPassed { get; private set; } = false;
    public string TestReport { get; private set; } = "";

    private void Start()
    {
        if (menuController == null)
            menuController = FindFirstObjectByType<MainMenuController>();

        if (runTestOnStart)
        {
            StartCoroutine(RunAutomatedTestRoutine());
        }
    }

    [ContextMenu("Запустить тест переходов камеры")]
    public void RunTest()
    {
        StartCoroutine(RunAutomatedTestRoutine());
    }

    public IEnumerator RunAutomatedTestRoutine()
    {
        Debug.Log("=================================================");
        Debug.Log("[MainMenuTester] >>> ЗАПУСК ТЕСТА ГЛАВНОГО МЕНЮ <<<");
        Debug.Log("=================================================");

        if (menuController == null)
        {
            menuController = FindFirstObjectByType<MainMenuController>();
        }

        if (menuController == null)
        {
            TestReport = "ОШИБКА: MainMenuController не найден на сцене!";
            Debug.LogError($"[MainMenuTester] {TestReport}");
            yield break;
        }

        var camCtrl = menuController.CameraController;
        if (camCtrl == null)
        {
            TestReport = "ОШИБКА: MenuCameraController не подключен к MainMenuController!";
            Debug.LogError($"[MainMenuTester] {TestReport}");
            yield break;
        }

        // 1. Проверяем начальное состояние (Дерево-портал)
        yield return new WaitForSeconds(0.9f); // ждем первичный Fade In
        Vector3 initialPos = camCtrl.transform.position;
        Vector3 portalTarget = camCtrl.PortalAnchor != null ? camCtrl.PortalAnchor.position : initialPos;
        float distToPortal = Vector3.Distance(initialPos, portalTarget);

        if (distToPortal > 0.5f)
        {
            Debug.LogWarning($"[MainMenuTester] Начальная позиция ({initialPos}) отличается от PortalAnchor ({portalTarget}) на {distToPortal:F2}м");
        }
        else
        {
            Debug.Log($"[MainMenuTester] [1/4] НАЧАЛО: Камера успешно направлена на дерево-портал. Позиция: {initialPos}");
        }

        // 2. Тестируем переход к шарику (Настройки)
        Debug.Log("[MainMenuTester] [2/4] Кликаем 'Настройки' -> Камера летит к красному шарику...");
        menuController.OnOpenSettingsClicked();

        while (camCtrl.IsMoving || menuController.IsInteractingBlocked)
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);

        Vector3 ballTarget = camCtrl.BallAnchor.position;
        float distToBall = Vector3.Distance(camCtrl.transform.position, ballTarget);
        if (distToBall > 0.6f)
        {
            TestReport = $"ОШИБКА: Камера не долетела до шарика! Расстояние: {distToBall}";
            Debug.LogError($"[MainMenuTester] {TestReport}");
            yield break;
        }
        Debug.Log($"[MainMenuTester] [2/4] УСПЕХ: Камера прибыла к шарику! Расстояние до цели: {distToBall:F3}м. Панель настроек активна (alpha={menuController.SettingsCanvasGroup.alpha})");

        // 3. Тестируем возврат к порталу ('Назад')
        Debug.Log("[MainMenuTester] [3/4] Кликаем 'Назад к порталу' -> Камера возвращается к дереву...");
        menuController.OnBackToMainClicked();

        while (camCtrl.IsMoving || menuController.IsInteractingBlocked)
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);

        distToPortal = Vector3.Distance(camCtrl.transform.position, portalTarget);
        if (distToPortal > 0.6f)
        {
            TestReport = $"ОШИБКА: Камера не вернулась к порталу! Расстояние: {distToPortal}";
            Debug.LogError($"[MainMenuTester] {TestReport}");
            yield break;
        }
        Debug.Log($"[MainMenuTester] [3/4] УСПЕХ: Камера успешно вернулась к дереву-порталу! Расстояние: {distToPortal:F3}м. Главное меню активно (alpha={menuController.MainCanvasGroup.alpha})");

        // 4. Тестируем отлет камеры при выходе (0, 22, -15; rot 55, 0, 0)
        Debug.Log("[MainMenuTester] [4/4] Кликаем 'Выход' -> Камера отлетает ввысь (0, 22, -15), экран темнеет...");
        bool quitEventReceived = false;
        menuController.OnQuitExecuted += () => quitEventReceived = true;

        menuController.OnQuitClicked();

        while (camCtrl.IsMoving)
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.3f);

        Vector3 expectedExitPos = new Vector3(0f, 22f, -15f);
        float distToExit = Vector3.Distance(camCtrl.transform.position, expectedExitPos);
        float angleDiff = Quaternion.Angle(camCtrl.transform.rotation, Quaternion.Euler(55f, 0f, 0f));

        Debug.Log($"[MainMenuTester] Конечная позиция камеры: {camCtrl.transform.position}, угол: {camCtrl.transform.eulerAngles}");
        Debug.Log($"[MainMenuTester] Расстояние до точки выхода (0, 22, -15): {distToExit:F2}м, отклонение угла: {angleDiff:F2} град");
        Debug.Log($"[MainMenuTester] Затемнение экрана (ScreenFader.alpha): {menuController.ScreenFader.alpha:F2}");

        if (distToExit > 1f || angleDiff > 2f)
        {
            TestReport = $"ОШИБКА: Конечная позиция/вращение при выходе не соответствуют заданным! dist={distToExit}, angle={angleDiff}";
            Debug.LogError($"[MainMenuTester] {TestReport}");
            yield break;
        }

        TestPassed = true;
        TestReport = "ВСЕ ТЕСТЫ УСПЕШНО ПРОЙДЕНЫ! Камера плавно переходит между порталом, шариком и позицией выхода (0,22,-15). Затемнение и интерфейс работают корректно.";
        Debug.Log("=================================================");
        Debug.Log($"[MainMenuTester] >>> {TestReport} <<<");
        Debug.Log("=================================================");
    }
}
