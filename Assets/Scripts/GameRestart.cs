using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRestart : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }
    }

    // вызывается и с клавиши, и с кнопки в UI
    public void Restart()
    {
        // Перезагружаем ТЕКУЩУЮ сцену по её индексу в Build Settings
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (currentSceneIndex < 0)
        {
            Debug.LogError("Сцена не добавлена в Build Settings! Перезапуск невозможен.");
            return;
        }

        SceneManager.LoadScene(currentSceneIndex);
    }
}