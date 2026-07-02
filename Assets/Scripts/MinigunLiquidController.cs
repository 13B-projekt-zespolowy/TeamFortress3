using UnityEngine;

public class MinigunLiquidController : MonoBehaviour
{
    [SerializeField] private LiquidWobble worldLiquids;
    [SerializeField] private LiquidWobble viewmodelLiquids;

    public void OnAmmoChange(PlayerShooter shooter)
    {
        SetFillAmount(shooter.CurrentMag, shooter.CurrentWeapon.magazineSize);
    }

    private void SetFillAmount(int currentAmmo, int maxAmmo)
    {
        float amount = currentAmmo / (float)maxAmmo;

        if (worldLiquids)
            worldLiquids.SetFillAmount(amount);
        if (viewmodelLiquids)
            viewmodelLiquids.SetFillAmount(amount);
    }
}
