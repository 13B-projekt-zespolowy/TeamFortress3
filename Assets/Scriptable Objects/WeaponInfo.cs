using UnityEngine;

[CreateAssetMenu(fileName = "WeaponInfo", menuName = "Scriptable Objects/Weapon Info")]
public class WeaponInfo : ScriptableObject
{
    public enum FireType { Semi, Auto }
    public enum ShootMode { Hitscan, Projectile, Melee }

    public string weaponName;
    public Sprite weaponIcon;
    public GameObject viewmodel;
    [Header("Types")]
    public FireType fireType;
    public ShootMode shootMode;

    [Header("Ammo")]
    public int magazineSize = 30;
    public int initialReserve = 90;
    public float reloadTime = 2f;

    [Header("Shooting")]
    public float fireRate = 10f;
    public int damage = 20;
    public float range = 100f;
    public float spread = 0.01f;
    public AudioClip shootSound;

    [Header("Revving")]
    public bool requiresRevving = false;
    public float revUpTime = 1.5f;

    [Header("Melee")]
    public float meleeRadius = 0.5f;
    [Header("Projectile")]
    public GameObject projectilePrefab;
}
