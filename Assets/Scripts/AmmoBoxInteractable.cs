using UnityEngine;

public class AmmoBoxInteractable : ContactInteractable
{
    [SerializeField] private int ammoBonus = 30;

    public override InteractionResultAction OnInteractedWith(GameObject sender)
    {
        Debug.Log("CONTACT!");
        if (!sender.TryGetComponent(out PlayerShooter shooter)) return InteractionResultAction.None;
        shooter.AddAmmo(ammoBonus);
        return InteractionResultAction.Destroy;
    }
}