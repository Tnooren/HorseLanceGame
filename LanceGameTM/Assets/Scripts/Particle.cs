using UnityEngine;

public class Particle : MonoBehaviour
{
    public GameObject HitParticle;
    public GameObject Dustparticle;

    public static Particle Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public static void SpawnHitParticle(Vector3 position, Quaternion rotation, float speed)
    {
        if (Instance == null)
            return;

        GameObject particle = Instantiate(Instance.HitParticle, position, rotation);
        ParticleSystem particleSystem = particle.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            var main = particleSystem.main;
            main.startSpeed = speed;
        }
    }

    public static void SpawnDustParticle(Vector3 position)
    {
        if (Instance == null)
            return;
        GameObject particle = Instantiate(Instance.Dustparticle, position, Quaternion.identity);

    }
}
