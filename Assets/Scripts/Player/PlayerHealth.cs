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

        PlayerInfoUI.Instance.healthBar.maxValue = maxHealth;
        PlayerInfoUI.Instance.UpdateHealthBar(maxHealth);
    }

    private void OnEnable()
    {
        if (isOwner)
        {
            PlayerInfoUI.Instance.SetActive(true);

            GameObject sceneCamera = GameManager.Instance.GetSceneCamera();
            if (sceneCamera) sceneCamera.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (isOwner)
        {
            PlayerInfoUI.Instance.SetActive(false);

            GameObject sceneCamera = GameManager.Instance.GetSceneCamera();
            if(sceneCamera) sceneCamera.SetActive(true);
        }
    }

    protected override void OnSpawned()
    {
        if (isOwner)
        {
            currentHealth.onChanged += PlayerInfoUI.Instance.UpdateHealthBar;
            OnEnable();
        }
    }

    protected override void OnDespawned()
    {
        if (isOwner)
            currentHealth.onChanged -= PlayerInfoUI.Instance.UpdateHealthBar;
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