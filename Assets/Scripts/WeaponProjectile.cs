using PurrNet;
using UnityEngine;

/// <summary>
/// Represents a projectile fired from a weapon in a networked multiplayer environment.
/// Handles movement, collision detection, damage application, and lifetime management.
/// </summary>
public class WeaponProjectile : NetworkBehaviour
{
    [Header("Stats")]
    public float speed = 50f;
    public float lifetime = 5f;

    [Header("Gravity")]
    public bool useGravity = false;
    public float gravityForce = 1f;

    [Header("Explosive")]
    public bool isExplosive = false;
    public float explosionRadius = 4f;
    public float explosionForce = 15f;
    public float upwardForceModifier = 0.5f;
    public GameObject explosionEffect;
    public float explosionEffectLifetime = 0.1f;

    private int _damage;
    private Team _shooterTeam;
    private Vector3 _currentVelocity;

    /// <summary>
    /// Initializes the projectile with damage, shooter collision ignore, and team information.
    /// </summary>
    /// <param name="damage">The damage amount to apply on hit.</param>
    /// <param name="shooterCollider">The collider of the shooter to ignore collisions with.</param>
    /// <param name="shooterTeam">The team of the shooter for team-based damage validation.</param>
    public void Initialize(int damage, Collider shooterCollider, Team shooterTeam)
    {
        _damage = damage;
        _shooterTeam = shooterTeam;
        _currentVelocity = transform.forward * speed;

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
        if (useGravity)
        {
            _currentVelocity += -Vector3.up * gravityForce * Time.deltaTime;

            if (_currentVelocity.sqrMagnitude > 0.1f)
                transform.rotation = Quaternion.LookRotation(_currentVelocity);
        }
        transform.position += _currentVelocity * Time.deltaTime;
    }

    /// <summary>
    /// Handles trigger collisions with other objects.
    /// Applies damage to players on opposing teams and destroys the projectile.
    /// Server-authoritative operation.
    /// </summary>
    /// <param name="other">The collider that triggered the collision.</param>
    void OnTriggerEnter(Collider other)
    {
        if (!isServer || other.isTrigger) return;

        if (isExplosive)
        {
            Explode();
        }
        else
        {
            if (other.TryGetComponent(out PlayerHealth health) && other.TryGetComponent(out PlayerTeam targetTeam))
                if (_shooterTeam != targetTeam.Team)
                    health.TakeDamage(_damage);
        }

        Destroy(gameObject);
    }

    private void Explode()
    {
        if (explosionEffect)
        {
            GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            explosion.transform.localScale = Vector3.one * explosionRadius;
            Destroy(explosion, explosionEffectLifetime);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent(out PlayerTeam targetTeam) && _shooterTeam != targetTeam.Team)
            {
                if (hit.TryGetComponent(out PlayerHealth health))
                    health.TakeDamage(_damage);

                if (hit.TryGetComponent(out PlayerController controller))
                {
                    Vector3 pushDirection = (hit.transform.position - transform.position).normalized;
                    pushDirection.y += upwardForceModifier;
                    controller.ApplyKnockbackObserverRPC(pushDirection.normalized * explosionForce);
                }
            }
        }
    }
}
