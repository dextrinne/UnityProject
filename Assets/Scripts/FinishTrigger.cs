using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    public GameObject winText;
    public GameObject ball;
    private bool isFinished = false;

    void OnTriggerEnter(Collider other)
    {
        if (isFinished) return;
        if (!other.CompareTag("Player")) return;

        isFinished = true;
        Debug.Log("Победа!");

        if (winText != null) winText.SetActive(true);

        // Останавливаем шар
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = Vector3.zero;
    }
}