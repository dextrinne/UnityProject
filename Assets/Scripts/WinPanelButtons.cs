using UnityEngine;
using UnityEngine.UI;

public class WinPanelButtons : MonoBehaviour
{
    public Button btnPrev;
    public Button btnRestart;
    public Button btnNext;

    void OnEnable()
    {
        if (LevelManager.Instance == null) return;

        btnPrev.interactable = LevelManager.Instance.HasPrevious();
        btnNext.interactable = LevelManager.Instance.HasNext();
        btnRestart.interactable = true;
    }

    public void OnPrevClicked() => LevelManager.Instance.PrevLevel();
    public void OnRestartClicked() => LevelManager.Instance.RestartLevel();
    public void OnNextClicked() => LevelManager.Instance.NextLevel();
}