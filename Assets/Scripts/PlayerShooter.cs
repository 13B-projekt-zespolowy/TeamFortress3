using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : NetworkBehaviour
{
    public Transform playerCamera;
    [Header("Weapon")]
    public WeaponInfo weapon;

    [Header("Input References")]
    [SerializeField] private InputActionReference fireAction;
    [SerializeField] private InputActionReference reloadAction;
    [SerializeField] private InputActionReference lookAction;

    [Header("Hitscan Settings")]
    public LayerMask hitMask;

    [Header("Projectile Settings")]
    public Transform firePoint;

    [Header("Weapon Sway")]
    public Transform viewModel;
    public float swayAmount = 4f;
    public float swaySmooth = 8f;
    public Vector2 swayMinMax = new Vector2(-6f, 6f);

    private Quaternion _initialRot;
    // State
    private SyncVar<int> _currentMag = new();
    private SyncVar<int> _currentReserve = new();
    private float _nextFireTime;
    private bool _isReloading;

    protected override void OnSpawned()
    {
        if (isServer)
        {
            _currentMag.value = weapon.magazineSize;
            _currentReserve.value = weapon.initialReserve;
        }

        if (isOwner)
        {
            _initialRot = viewModel.localRotation;
            fireAction.action.Enable();
            reloadAction.action.Enable();
            lookAction.action.Enable();
        }
    }

    void Update()
    {
        if (!isOwner) return;
        HandleSway();

        if ( (weapon.fireType == WeaponInfo.FireType.Auto && fireAction.action.IsPressed()) ||
             (weapon.fireType == WeaponInfo.FireType.Semi && fireAction.action.WasPressedThisFrame()) )
            TryShoot();

        if (reloadAction.action.WasPressedThisFrame())
            TryReload();

    }

    public void TryShoot()
    {
        if (!isOwner || _isReloading || Time.time < _nextFireTime) return;

        if (_currentMag.value <= 0)
        {
            TryReload();
            return;
        }

        _nextFireTime = Time.time + (1f / weapon.fireRate);
        Debug.Log($"Magazine: {_currentMag.value} | Reserve: {_currentReserve.value}");
        ShootServerRPC((weapon.shootMode == WeaponInfo.ShootMode.Hitscan) ? playerCamera.position : firePoint.position, playerCamera.forward);
    }
    
    public void AddAmmo(int amount)
    {
        AddAmmoRPC(amount);
    }

    [ServerRpc]
    private void AddAmmoRPC(int amount)
    {
        _currentReserve.value += amount;
    }

    private void TryReload()
    {
        if (_isReloading || _currentMag >= weapon.magazineSize || _currentReserve <= 0) return;

        /*int needed = weapon.magazineSize - _currentMag;
        int transfer = Mathf.Min(needed, _currentReserve);

        _currentMag += transfer;
        _currentReserve -= transfer;*/
        ReloadServerRPC();
    }

    private void ShootHitscan(Vector3 pos, Vector3 forward)
    {
        Vector3 endPoint = pos + (forward * weapon.range);
        if (Physics.Raycast(pos, forward, out RaycastHit hit, weapon.range, hitMask))
        {
            endPoint = hit.point;
            if (hit.collider.TryGetComponent(out PlayerHealth health))
                health.TakeDamage(weapon.damage);
        }

        HitscanDebugObserverRPC(pos, endPoint);
    }

    private void ShootProjectile(Vector3 pos, Vector3 forward)
    {
        GameObject proj = Instantiate(weapon.projectilePrefab, pos, Quaternion.LookRotation(forward));

        if (proj.TryGetComponent(out WeaponProjectile projectileScript))
            projectileScript.Initialize(weapon.damage, GetComponent<Collider>());
    }

    private void HandleSway()
    {
        Vector2 delta = lookAction.action.ReadValue<Vector2>();

        Quaternion targetRot = _initialRot * Quaternion.Euler(
            Mathf.Clamp(delta.y * swayAmount, swayMinMax.x, swayMinMax.y), 
            Mathf.Clamp(-delta.x * swayAmount, swayMinMax.x, swayMinMax.y), 0);

        viewModel.localRotation = Quaternion.Slerp(viewModel.localRotation, targetRot, Time.deltaTime * swaySmooth);
    }

    [ServerRpc]
    private void ShootServerRPC(Vector3 pos, Vector3 forward)
    {
        //if (_isReloading || Time.time < _nextFireTime) return;
        _currentMag.value--;
        if (weapon.shootMode == WeaponInfo.ShootMode.Hitscan)
            ShootHitscan(pos, forward);
        else
            ShootProjectile(pos, forward);
    }

    [ServerRpc]
    private void ReloadServerRPC()
    {
        if (_isReloading || _currentMag.value >= weapon.magazineSize || _currentReserve.value <= 0) return;

        int needed = weapon.magazineSize - _currentMag;
        int transfer = Mathf.Min(needed, _currentReserve.value);

        _currentMag.value += transfer;
        _currentReserve.value -= transfer;
    }

    [ObserversRpc]
    private void HitscanDebugObserverRPC(Vector3 start, Vector3 end)
    {
        Debug.DrawLine(start, end, Color.yellow, 0.5f);
    }
}