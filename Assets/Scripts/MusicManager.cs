using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    private AudioSource source;
    private const string MUSIC_KEY = "MusicOn";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        source = GetComponent<AudioSource>();
        if (source == null)
        {
            Debug.LogError("MusicManager: нет AudioSource!");
            return;
        }

        bool musicOn = PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1;
        source.mute = !musicOn;
    }

    void OnEnable()
    {
        // Подписываемся на загрузку сцены — обновляем иконку каждый раз
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Сцена загрузилась — обновляем иконку на новой кнопке
        UpdateButtonIcon();
    }

    public void ToggleMusic()
    {
        if (source == null) return;

        source.mute = !source.mute;
        PlayerPrefs.SetInt(MUSIC_KEY, source.mute ? 0 : 1);
        PlayerPrefs.Save();
        UpdateButtonIcon();
    }

    public bool IsMusicOn() => source != null && !source.mute;

    private void UpdateButtonIcon()
    {
        // Ищем кнопку в ТЕКУЩЕЙ сцене каждый раз
        GameObject btnObj = GameObject.Find("Btn_Music");
        if (btnObj == null) return;

        var tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
            tmp.text = source.mute ? "♪-" : "♪";
    }
}