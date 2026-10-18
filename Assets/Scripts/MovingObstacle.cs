using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    [Header("Точки патрулирования")]
    public Vector3 pointA;
    public Vector3 pointB;

    [Header("Настройки")]
    public float speed = 3f;
    public bool startAtPointB = false;

    private Vector3 currentTarget;

    /// <summary>
    /// Устанавливает начальную позицию объекта и первую цель патрулирования
    /// </summary>
    void Start()
    {
        transform.localPosition = startAtPointB ? pointB : pointA;
        currentTarget = startAtPointB ? pointA : pointB;
    }

    /// <summary>
    /// Двигает объект к текущей цели и переключает цель по достижении точки
    /// </summary>
    void Update()
    {
        // Плавно двигаемся к цели
        transform.localPosition = Vector3.MoveTowards(
            transform.localPosition,
            currentTarget,
            speed * Time.deltaTime
        );

        // Если дошли до точки — меняем цель
        if (Vector3.Distance(transform.localPosition, currentTarget) < 0.05f)
        {
            currentTarget = (currentTarget == pointA) ? pointB : pointA;
        }
    }

    /// <summary>
    /// Рисует в редакторе линию между точками патрулирования и сферы в них
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(pointA, pointB);
        Gizmos.DrawWireSphere(pointA, 0.5f);
        Gizmos.DrawWireSphere(pointB, 0.5f);
    }
}