using UnityEngine;
using TMPro;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    private AudioSource source;
    private const string MUSIC_KEY = "MusicOn";

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        source = GetComponent<AudioSource>();

        bool musicOn = PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1;
        source.mute = !musicOn;
    }

    void Start()
    {
        // Обновляем иконку после загрузки сцены
        UpdateButtonIcon();
    }

    public TextMeshProUGUI musicButtonText;
    public void ToggleMusic()
    {
        source.mute = !source.mute;
        PlayerPrefs.SetInt(MUSIC_KEY, source.mute ? 0 : 1);
        PlayerPrefs.Save();
        UpdateButtonIcon();

        // Смена иконки при переключении
        if (musicButtonText != null)
            musicButtonText.text = source.mute ? "♪-" : "♪";
    }

    public bool IsMusicOn() => !source.mute;

    private void UpdateButtonIcon()
    {
        // Находим кнопку музыки на сцене
        GameObject btnObj = GameObject.Find("Btn_Music");
        if (btnObj == null) return;

        var tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
            tmp.text = source.mute ? "♪̶" : "♪";
    }
}