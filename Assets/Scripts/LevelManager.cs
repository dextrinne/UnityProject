using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Список уровней (для переходов ◀ ▶)")]
    public string[] levelScenes = { "Level1" };

    public int CurrentLevel { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentLevel = PlayerPrefs.GetInt("CurrentLevel", 0);
    }

    public bool HasPrevious() => CurrentLevel > 0;
    public bool HasNext() => CurrentLevel < levelScenes.Length - 1;

    public void LoadLevel(int index)
    {
        if (index < 0 || index >= levelScenes.Length) return;
        CurrentLevel = index;
        PlayerPrefs.SetInt("CurrentLevel", index);
        PlayerPrefs.Save();
        SceneManager.LoadScene(levelScenes[index]);
    }

    public void NextLevel() => LoadLevel(CurrentLevel + 1);
    public void PrevLevel() => LoadLevel(CurrentLevel - 1);

    // Перезапуск ТЕКУЩЕЙ сцены — надёжно
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}