using UnityEngine;
using UnityEngine.UI;

public class WinPanelButtons : MonoBehaviour
{
    public Button btnPrev;
    public Button btnRestart;
    public Button btnNext;

    /// <summary>
    /// При включении панели настраивает доступность кнопок "Назад", "Далее" и "Перезапуск"
    /// в зависимости от текущего уровня
    /// </summary>
    void OnEnable()
    {
        if (LevelManager.Instance == null) return;

        btnPrev.interactable = LevelManager.Instance.HasPrevious();
        btnNext.interactable = LevelManager.Instance.HasNext();
        btnRestart.interactable = true;
    }

    /// <summary>
    /// Переходит на предыдущий уровень через LevelManager
    /// </summary>
    public void OnPrevClicked() => LevelManager.Instance.PrevLevel();

    /// <summary>
    /// Перезапускает текущий уровень через LevelManager
    /// </summary>
    public void OnRestartClicked() => LevelManager.Instance.RestartLevel();

    /// <summary>
    /// Переходит на следующий уровень через LevelManager
    /// </summary>
    public void OnNextClicked() => LevelManager.Instance.NextLevel();
}