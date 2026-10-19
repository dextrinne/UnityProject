using UnityEngine;

public class SlowZone : MonoBehaviour
{
    /// <summary>
    /// При входе игрока в зону увеличивает линейное сопротивление Rigidbody, замедляя шар
    /// </summary>
    /// <param name="other">Коллайдер объекта, вошедшего в триггер</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null) rb.linearDamping = 5f; // сильное трение
        }
    }

    /// <summary>
    /// При выходе игрока из зоны возвращает линейное сопротивление Rigidbody к норме
    /// </summary>
    /// <param name="other">Коллайдер объекта, покинувшего триггер</param>
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null) rb.linearDamping = 0.4f; // возвращаем норму
        }
    }
}