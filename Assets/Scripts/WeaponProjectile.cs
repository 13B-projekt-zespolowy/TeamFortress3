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

    private int _damage;
    private Team _shooterTeam;

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

    /// <summary>
    /// Handles trigger collisions with other objects.
    /// Applies damage to players on opposing teams and destroys the projectile.
    /// Server-authoritative operation.
    /// </summary>
    /// <param name="other">The collider that triggered the collision.</param>
    void OnTriggerEnter(Collider other)
    {
        if (!isServer || other.isTrigger) return;

        if (other.TryGetComponent(out PlayerHealth health) && other.TryGetComponent(out PlayerTeam targetTeam))
            if (_shooterTeam != targetTeam.Team)
                health.TakeDamage(_damage);

        /*if (other.TryGetComponent(out PlayerHealth health))
            health.TakeDamage(_damage);*/

        Destroy(gameObject);
    }
}
