using PurrNet;
using UnityEngine;

/// <summary>
/// Manages player health, damage, death, and UI updates.
/// Handles health synchronization across the network and respawn functionality.
/// </summary>
public class PlayerHealth : NetworkBehaviour
{
    /// <summary>
    /// The current health value synchronized across the network.
    /// </summary>
    public SyncVar<int> currentHealth = new();
    private SyncVar<int> maxHealth = new();

    private PlayerFlagCarry _flagCarry;

    /// <summary>
    /// Initializes the player's health to the maximum value.
    /// Server-only operation.
    /// </summary>
    /// <param name="maxHP">The maximum health value.</param>
    public void Initialize(int maxHP)
    {
        if (!isServer) return;
        maxHealth.value = maxHP;
        currentHealth.value = maxHealth.value;
        _flagCarry = GetComponent<PlayerFlagCarry>();
    }

    private void OnEnable()
    {
        if (isOwner)
        {
            if (PlayerInfoUI.Instance != null)
                PlayerInfoUI.Instance.SetActive(true);

            if (GameManager.Instance != null)
            {
                GameObject sceneCamera = GameManager.Instance.GetSceneCamera();
                if (sceneCamera) sceneCamera.SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        if (isOwner)
        {
            if (PlayerInfoUI.Instance != null)
                PlayerInfoUI.Instance.SetActive(false);

            if (GameManager.Instance != null)
            {
                GameObject sceneCamera = GameManager.Instance.GetSceneCamera();
                if (sceneCamera) sceneCamera.SetActive(true);
            }
        }
    }

    protected override void OnSpawned()
    {
        if (isOwner)
        {
            currentHealth.onChanged += OnHealthChanged;
            OnEnable();
            OnHealthChanged(currentHealth.value);
        }
    }

    protected override void OnDespawned()
    {
        if (isOwner)
            currentHealth.onChanged -= OnHealthChanged;
    }

    /// <summary>
    /// Updates the health UI when the health value changes.
    /// </summary>
    /// <param name="newHealth">The new health value.</param>
    private void OnHealthChanged(int newHealth)
    {
        if (PlayerInfoUI.Instance != null)
        {
            /*if (PlayerInfoUI.Instance.healthBar != null && maxHealth > 0)
                PlayerInfoUI.Instance.healthBar.maxValue = maxHealth;*/

            PlayerInfoUI.Instance.UpdateHealthBar(newHealth, maxHealth.value);
        }
    }

    /// <summary>
    /// Applies damage to the player. Triggers death if health reaches zero.
    /// Server-only operation.
    /// </summary>
    /// <param name="amount">The amount of damage to apply.</param>
    public void TakeDamage(int amount)
    {
        if (!isServer || currentHealth.value <= 0) return;

        currentHealth.value -= amount;
        if (currentHealth.value <= 0)
            Die();
    }

    /// <summary>
    /// Restores the player's health to maximum.
    /// Server-only operation.
    /// </summary>
    public void RefillHealth()
    {
        if (!isServer) return;
        currentHealth.value = maxHealth.value;
    }

    /// <summary>
    /// Handles the player's death by starting the respawn countdown and deactivating the player object.
    /// </summary>
    private void Die()
    {
        GameManager.Instance.StartRespawnCountdown((PlayerID)owner);

        Flag flag = _flagCarry.carriedFlag;
        if (flag != null)
            flag.ReturnToBase();

        SetActiveObserverRPC(false);
    }

    /// <summary>
    /// RPC to set the player object's active state for all clients.
    /// </summary>
    /// <param name="active">Whether the object should be active.</param>
    [ObserversRpc]
    private void SetActiveObserverRPC(bool active)
    {
        gameObject.SetActive(active);
    }
}
