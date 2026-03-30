using PurrNet;
using UnityEngine;

public class WeaponProjectile : NetworkBehaviour
{
    [Header("Stats")]
    public float speed = 50f;
    public float lifetime = 5f;
    private float damage = 1f;

    protected override void OnSpawned()
    {
        if (isServer)
            Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        Destroy(gameObject);
    }
}