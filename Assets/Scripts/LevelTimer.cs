using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelTimer : MonoBehaviour
{
    [Header("Настройки")]
    public float totalTime = 45f;
    public TextMeshProUGUI timerText;

    private float timeLeft;
    private bool isRunning = true;

    /// <summary>
    /// Инициализирует оставшееся время и обновляет текст таймера
    /// </summary>
    void Start()
    {
        // Сбрасываем флаг завершения уровня при старте сцены
        GameState.IsGameFinished = false;

        timeLeft = totalTime;
        UpdateText();
    }

    /// <summary>
    /// Отсчитывает время, обновляет текст и по истечении вызывает OnTimeOut;
    /// останавливается при завершении игры
    /// </summary>
    void Update()
    {
        if (!isRunning) return;
        if (GameState.IsGameFinished)
        {
            isRunning = false; // Останавливаем таймер при победе
            return;
        }

        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0)
        {
            timeLeft = 0;
            isRunning = false;
            UpdateText();
            OnTimeOut();
            return;
        }

        UpdateText();
    }


    /// <summary>
    /// Обновляет текст таймера в формате 00:SS и красит его в красный при <= 10 секунд
    /// </summary>
    void UpdateText()
    {
        if (timerText == null) return;

        int seconds = Mathf.CeilToInt(timeLeft);
        timerText.text = $"00:{seconds:D2}";

        // Красим в красный, когда осталось мало времени
        if (seconds <= 10)
            timerText.color = Color.red;
    }

    /// <summary>
    /// Обрабатывает истечение времени: логирует и перезапускает уровень через LevelManager
    /// </summary>
    void OnTimeOut()
    {
        Debug.Log("Время вышло! Рестарт...");

        // Показываем подсказку или просто рестартим
        if (LevelManager.Instance != null)
            LevelManager.Instance.RestartLevel();
    }
}