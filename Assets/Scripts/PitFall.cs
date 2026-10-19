using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class PitFall : MonoBehaviour
{
    private bool isFalling = false;


    /// <summary>
    /// При падении игрока в яму останавливает шар, делает его кинематическим
    /// и запускает перезапуск уровня с задержкой 
    /// </summary>
    /// <param name="other">Коллайдер объекта, вошедшего в триггер</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isFalling)
        {
            isFalling = true;
            Debug.Log("Шарик упал в яму! Перезапуск...");

            // Вызываем рестарт через LevelManager (он перезагрузит сцену)
            if (LevelManager.Instance != null)
                LevelManager.Instance.RestartLevel();
        }

    }
}