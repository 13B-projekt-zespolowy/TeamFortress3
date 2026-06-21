using PurrNet;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    public SyncVar<int> currentHealth = new();
    private int maxHealth;

    public void Initialize(int maxHP)
    {
        if (!isServer) return;
        maxHealth = maxHP;
        currentHealth.value = maxHealth;
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

    private void OnHealthChanged(int newHealth)
    {
        if (PlayerInfoUI.Instance != null)
        {
            if (PlayerInfoUI.Instance.healthBar != null && maxHealth > 0)
                PlayerInfoUI.Instance.healthBar.maxValue = maxHealth;

            PlayerInfoUI.Instance.UpdateHealthBar(newHealth);
        }
    }

    public void TakeDamage(int amount)
    {
        if (!isServer || currentHealth.value <= 0) return;

        currentHealth.value -= amount;
        if (currentHealth.value <= 0)
            Die();
    }

    public void RefillHealth()
    {
        if (!isServer) return;
        currentHealth.value = maxHealth;
    }

    private void Die()
    {
        GameManager.Instance.StartRespawnCountdown((PlayerID)owner);
        SetActiveRpc(false);
    }

    [ObserversRpc]
    private void SetActiveRpc(bool active)
    {
        gameObject.SetActive(active);
    }
}