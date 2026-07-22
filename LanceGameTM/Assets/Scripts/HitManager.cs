using System.Collections;
using UnityEngine;

public class HitManager : MonoBehaviour
{
    [SerializeField] private GameObject knight;
    [SerializeField] private GameObject fallenKnightPrefab;

    [SerializeField] private float knockBackForce = 10f;

    [SerializeField] private float resistance = 10f;
    [SerializeField] private float stamina = 10f;

    private bool mounted = true;

    public void OnHit(float deltaSpeed, float lanceStrength, Vector2 knockBackDirection)
    { 
        float finalForce = deltaSpeed * lanceStrength;
        float finalSteadiness = resistance * stamina;

        if (finalForce > finalSteadiness && mounted)
        {
            KnockOff(knockBackDirection);
            mounted = false;
        }
        else
        {
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, knockBackDirection);

            Particle.SpawnHitParticle(transform.position, rotation, 1f);
        }
    }

    private void KnockOff(Vector2 direction)
    {
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);

        Particle.SpawnHitParticle(transform.position, rotation, 10f);

        GameObject fallenKnight = Instantiate(fallenKnightPrefab, knight.transform.position, rotation);
        knight.SetActive(false);

        Rigidbody2D rb = fallenKnight.GetComponent<Rigidbody2D>();

        rb.AddForce(direction * knockBackForce, ForceMode2D.Impulse);

    }
}
