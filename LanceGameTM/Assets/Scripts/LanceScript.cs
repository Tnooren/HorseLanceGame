using UnityEngine;

public class LanceScript : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;

    [SerializeField] private float lanceStrength = 10f;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("KnockableEnemy"))
        {
            Vector2 relativeVelocity = rb.linearVelocity - collision.attachedRigidbody.linearVelocity;
            float deltaSpeed = relativeVelocity.magnitude;

            HitManager hitManager = collision.transform.root.GetComponent<HitManager>();
            if (hitManager != null)
                hitManager.OnHit(deltaSpeed, lanceStrength, rb.linearVelocity.normalized);
        }
    }
}

