using System.Collections;
using UnityEngine;

public class FallenKnight : MonoBehaviour
{
    [SerializeField] private Sprite dirtyKnight;

    [SerializeField] private float airTime = 0.6f;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(LifeTime());
    }

    private IEnumerator LifeTime()
    { 
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        yield return new WaitForSeconds(airTime);

        spriteRenderer.sortingOrder = 100;
        spriteRenderer.sortingLayerName = "AboveGround";

        Particle.SpawnDustParticle(transform.position);

        GetComponent<SpriteRenderer>().sprite = dirtyKnight;

    }
}
