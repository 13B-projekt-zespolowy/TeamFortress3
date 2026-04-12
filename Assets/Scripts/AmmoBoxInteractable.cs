using UnityEngine;

class AmmoBoxInteractable : ContactInteractable
{
    [SerializeField] private int ammoBonus = 30;
    public override InteractionResultAction OnInteractedWith(GameObject sender)
    {
        if (!sender.TryGetComponent(out PlayerShooter shooter)) return InteractionResultAction.None;
        shooter.AddAmmo(ammoBonus);
        return InteractionResultAction.Destroy;
    }
}