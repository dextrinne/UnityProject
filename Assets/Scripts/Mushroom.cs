using UnityEngine;
using DG.Tweening;

public class Mushroom : MonoBehaviour
{
    [Header("Настройки")]
    public float rotationSpeed = 90f;
    public float floatAmplitude = 0.2f; // Плавное покачивание вверх-вниз
    public float floatFrequency = 2f;

    private Vector3 startPos;

    /// <summary>
    /// Запоминает начальную позицию и запускает бесконечное вращение через DOTween
    /// </summary>
    void Start()
    {
        startPos = transform.localPosition;

        // Красивая анимация вращения через DOTween
        transform.DORotate(new Vector3(0, 360, 0), 3f, RotateMode.FastBeyond360)
                 .SetLoops(-1, LoopType.Restart)
                 .SetEase(Ease.Linear);
    }

    /// <summary>
    /// Покачивает объект вверх-вниз по синусоиде вокруг начальной позиции
    /// </summary>
    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
    }

    /// <summary>
    /// При касании игроком сообщает LevelManager о сборе гриба и уничтожает объект
    /// </summary>
    /// <param name="other">Коллайдер объекта, вошедшего в триггер</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Сообщаем менеджеру уровня, что гриб собран
            if (LevelGoal.Instance != null)
                LevelGoal.Instance.CollectMushroom();

            // Уничтожаем гриб (в будущем можно добавить эффект частиц)
            Destroy(gameObject);
        }
    }
}