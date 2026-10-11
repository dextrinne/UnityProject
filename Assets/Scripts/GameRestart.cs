using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRestart : MonoBehaviour
{
    /// <summary>
    /// Проверяет нажатие клавиши R и при необходимости перезапускает игру
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }
    }

    /// <summary>
    /// Перезагружает текущую сцену по её индексу в Build Settings
    /// </summary>
    public void Restart()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (currentSceneIndex < 0)
        {
            Debug.LogError("Сцена не добавлена в Build Settings! Перезапуск невозможен.");
            return;
        }

        SceneManager.LoadScene(currentSceneIndex);
    }
}