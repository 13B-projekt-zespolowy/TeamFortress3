using PurrNet;
using UnityEngine;

public class WeaponProjectile : NetworkBehaviour
{
    [Header("Stats")]
    public float speed = 50f;
    public float lifetime = 5f;

    private int _damage;

    public void Initialize(int damage, Collider shooterCollider)
    {
        _damage = damage;
        if (TryGetComponent<Collider>(out var collider) && shooterCollider != null)
            Physics.IgnoreCollision(collider, shooterCollider);
    }

    protected override void OnSpawned()
    {
        if (isServer)
            Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += transform.forward * (speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        if (other.TryGetComponent(out PlayerHealth health))
            health.TakeDamage(_damage);

        Destroy(gameObject);
    }
}