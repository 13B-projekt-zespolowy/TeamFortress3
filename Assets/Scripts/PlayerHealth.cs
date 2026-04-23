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

    public void TakeDamage(int amount)
    {
        if (!isServer || currentHealth.value <= 0) return;

        currentHealth.value -= amount;
        if (currentHealth.value <= 0)
            Die();
    }

    private void Die()
    {
        currentHealth.value = maxHealth;
        GameManager.Instance.RespawnPlayer((PlayerID)owner, gameObject);
    }

    // TO REMOVE LATER
    [ObserversRpc]
    public void RespawnSnapRpc(Vector3 position, Quaternion rotation)
    {
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        transform.position = position;
        transform.rotation = rotation;

        if (controller != null) controller.enabled = true;
    }
}