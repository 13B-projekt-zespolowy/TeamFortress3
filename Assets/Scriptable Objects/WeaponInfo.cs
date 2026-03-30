using UnityEngine;

[CreateAssetMenu(fileName = "WeaponInfo", menuName = "Scriptable Objects/Weapon Info")]
public class WeaponInfo : ScriptableObject
{
    public enum FireType { Semi, Auto }
    public enum ShootMode { Hitscan, Projectile }

    [Header("Types")]
    public FireType fireType;
    public ShootMode shootMode;

    [Header("Ammo")]
    public int magazineSize = 30;
    public int initialReserve = 90;
    //public float reloadTime = 2f; //TO DO

    [Header("Shooting")]
    public float fireRate = 10f;
    public float damage = 20f;
    public float range = 100f;
    //public float spread = 0.01f; //TO DO

    [Header("Projectile")]
    public GameObject projectilePrefab;
}
