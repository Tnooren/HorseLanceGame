using UnityEngine;

public class LanceScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("KnockableEnemy"))
        {
            // Handle collision with enemy
            Debug.Log("Lance hit a knockable enemy!");
        }
    }
}

