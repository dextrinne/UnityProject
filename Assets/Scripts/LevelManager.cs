using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Список уровней (для переходов ◀ ▶)")]
    public string[] levelScenes = { "Level1", "Level2", "Level3", "Level4", "Level5" };

    public int CurrentLevel { get; private set; }

    /// <summary>
    /// Реализует синглтон, загружает сохранённый индекс уровня и не уничтожается при смене сцен
    /// </summary>
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentLevel = PlayerPrefs.GetInt("CurrentLevel", 0);
    }

    public bool HasPrevious() => CurrentLevel > 0;
    public bool HasNext() => CurrentLevel < levelScenes.Length - 1;

    /// <summary>
    /// Загружает уровень по индексу, сохраняет его в PlayerPrefs и переходит на сцену
    /// </summary>
    /// <param name="index"></param>
    public void LoadLevel(int index)
    {
        if (index < 0 || index >= levelScenes.Length) return;
        CurrentLevel = index;
        PlayerPrefs.SetInt("CurrentLevel", index);
        PlayerPrefs.Save();

        // Сбрасываем флаг победы перед загрузкой нового уровня
        GameState.IsGameFinished = false;

        SceneManager.LoadScene(levelScenes[index]);
    }

    public void NextLevel() => LoadLevel(CurrentLevel + 1);
    public void PrevLevel() => LoadLevel(CurrentLevel - 1);

    /// <summary>
    /// Перезапускает текущую активную сцену
    /// </summary>
    public void RestartLevel()
    {
        // Сбрасываем флаг победы, чтобы игра не думала, что мы всё ещё на экране победы
        GameState.IsGameFinished = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}