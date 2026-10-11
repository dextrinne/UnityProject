using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    private AudioSource source;
    private const string VOLUME_KEY = "MusicVolume";
    private const string MUSIC_KEY = "MusicOn";

    /// <summary>
    /// Реализует синглтон, создаёт AudioSource при отсутствии и загружает сохранённые настройки музыки
    /// </summary>
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
            source = gameObject.AddComponent<AudioSource>();
        }

        // Загружаем сохранённые настройки
        float savedVolume = PlayerPrefs.GetFloat(VOLUME_KEY, 1f);
        bool musicOn = PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1;

        source.volume = savedVolume;
        source.mute = !musicOn;
    }

    /// <summary>
    /// Устанавливает громкость музыки, обновляет mute и сохраняет настройки в PlayerPrefs
    /// </summary>
    /// <param name="volume">Громкость</param>
    public void SetVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (source != null)
        {
            source.volume = volume;
            source.mute = volume <= 0.001f;
        }
        PlayerPrefs.SetFloat(VOLUME_KEY, volume);
        PlayerPrefs.SetInt(MUSIC_KEY, (source != null && !source.mute) ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Возвращает текущую громкость музыки
    /// </summary>
    /// <returns></returns>
    public float GetVolume()
    {
        return source != null ? source.volume : PlayerPrefs.GetFloat(VOLUME_KEY, 1f);
    }

    /// <summary>
    /// Переключает музыку вкл/выкл и сохраняет состояние в PlayerPrefs
    /// </summary>
    public void ToggleMusic()
    {
        if (source == null) return;

        source.mute = !source.mute;
        PlayerPrefs.SetInt(MUSIC_KEY, source.mute ? 0 : 1);
        PlayerPrefs.Save();
    }

    public bool IsMusicOn() => source != null && !source.mute;
}