using UnityEngine;

/// <summary>
/// Provides ammo to players when interacted with.
/// The ammo is added to the player's reserve and the object dissolves/disables upon interaction.
/// </summary>
public class AmmoBoxInteractable : ContactInteractable
{
    [SerializeField] private int ammoBonus = 30;

    /// <summary>
    /// Handles interaction with the ammo box.
    /// Adds ammo to the player's reserve, triggers dissolve effect, and disables the collider.
    /// </summary>
    /// <param name="sender">The GameObject that interacted with this object.</param>
    /// <returns>InteractionResultAction.None as this interaction doesn't require further action.</returns>
    public override InteractionResultAction OnInteractedWith(GameObject sender)
    {
        if (!sender.TryGetComponent(out PlayerShooter shooter)) return InteractionResultAction.None;

        shooter.AddAmmo(ammoBonus);
        /*
        if (TryGetComponent(out DissolveController dissolveController))
        {
            dissolveController.StartDissolve();
        }
        */
        /*
        if (TryGetComponent(out Collider col))
        {
            col.enabled = false;
        }
        */

        return InteractionResultAction.Destroy;
    }
}
