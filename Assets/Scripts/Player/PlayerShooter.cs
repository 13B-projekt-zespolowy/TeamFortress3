using PurrNet;
using System;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : NetworkBehaviour
{
    public Transform playerCamera;
    [Header("Weapons")]
    public WeaponInfo[] weaponLoadout;

    [Header("Input References")]
    [SerializeField] private InputActionReference fireAction;
    [SerializeField] private InputActionReference reloadAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference switchWeaponAction;

    [Header("Hitscan Settings")]
    public LayerMask hitMask;

    [Header("Projectile Settings")]
    public Transform firePoint;

    [Header("Weapon Sway")]
    public Transform viewModelParent;
    public float swayAmount = 4f;
    public float swaySmooth = 8f;
    public Vector2 swayMinMax = new Vector2(-6f, 6f);

    private Quaternion _initialRot;
    private GameObject[] _weaponViewmodels;

    // State
    private SyncVar<int> _activeWeaponIndex = new(0);
    private SyncList<int> _mags = new();
    private SyncList<int> _reserves = new();
    private float _nextFireTime;
    private bool _isReloading;

    public WeaponInfo CurrentWeapon => weaponLoadout[_activeWeaponIndex.value];
    public int CurrentMag
    { 
        get => _mags[_activeWeaponIndex.value];
        set => _mags[_activeWeaponIndex.value] = value;
    }
    public int CurrentReserve
    {
        get => _reserves[_activeWeaponIndex.value];
        set => _reserves[_activeWeaponIndex.value] = value;
    }

    protected override void OnSpawned()
    {
        if (isServer)
        {
            for (int i = 0; i < weaponLoadout.Length; i++)
            {
                _mags.Add(weaponLoadout[i].magazineSize);
                _reserves.Add(weaponLoadout[i].initialReserve);
            }
        }

        if (isOwner)
        {
            _weaponViewmodels = new GameObject[weaponLoadout.Length];
            for (int i = 0; i < _weaponViewmodels.Length; i++)
            {
                if (weaponLoadout[i].viewmodel)
                    _weaponViewmodels[i] = Instantiate(weaponLoadout[i].viewmodel, viewModelParent);
            }
            if(WeaponSwitchUI.Instance) WeaponSwitchUI.Instance.Initialize(weaponLoadout);
            UpdateWeaponVisual(_activeWeaponIndex);

            _initialRot = viewModelParent.localRotation;
            switchWeaponAction.action.performed += HandleWeaponSwitch;
            _activeWeaponIndex.onChanged += SwitchWeaponLocal;
        }
    }

    protected override void OnDespawned()
    {
        if (isOwner)
            switchWeaponAction.action.performed -= HandleWeaponSwitch;
    }

    void Update()
    {
        if (!isOwner) return;
        HandleSway();

        if ( (CurrentWeapon.fireType == WeaponInfo.FireType.Auto && fireAction.action.IsPressed()) ||
             (CurrentWeapon.fireType == WeaponInfo.FireType.Semi && fireAction.action.WasPressedThisFrame()) )
            TryShoot();

        if (reloadAction.action.WasPressedThisFrame())
            TryReload();
    }

    public void HandleWeaponSwitch(InputAction.CallbackContext context)
    {
        float switchInput = context.ReadValue<float>();
        if (switchInput != 0)
        {
            int newIndex = _activeWeaponIndex.value;
            if (switchInput < 0) newIndex = (newIndex + 1) % weaponLoadout.Length;
            else if (switchInput > 0) newIndex = (newIndex - 1 + weaponLoadout.Length) % weaponLoadout.Length;

            if (newIndex != _activeWeaponIndex.value)
                SwitchWeaponServerRPC(newIndex);
        }
    }

    private void SwitchWeaponLocal(int newIndex)
    {
        UpdateWeaponVisual(newIndex);
        if (WeaponSwitchUI.Instance) WeaponSwitchUI.Instance.ShowUI(newIndex);
    }

    private void UpdateWeaponVisual(int newIndex)
    {
        for (int i = 0; i < _weaponViewmodels.Length; i++)
        {
            if (_weaponViewmodels[i] != null)
                _weaponViewmodels[i].SetActive(i == newIndex);
        }
    }

    public void TryShoot()
    {
        if (!isOwner || _isReloading || Time.time < _nextFireTime) return;

        if (CurrentWeapon.shootMode != WeaponInfo.ShootMode.Melee && _mags[_activeWeaponIndex.value] <= 0)
        {
            TryReload();
            return;
        }

        _nextFireTime = Time.time + (1f / CurrentWeapon.fireRate);
        Debug.Log($"Magazine: {CurrentMag} | Reserve: {CurrentReserve}");
        ShootServerRPC((CurrentWeapon.shootMode == WeaponInfo.ShootMode.Projectile) ? firePoint.position : playerCamera.position, playerCamera.forward);
    }
    
    public void AddAmmo(int amount)
    {
        AddAmmoServerRPC(amount);
    }

    [ServerRpc]
    private void AddAmmoServerRPC(int amount)
    {
        CurrentReserve += amount;
    }

    [ServerRpc]
    private void SwitchWeaponServerRPC(int newIndex)
    {
        if (newIndex < 0 || newIndex >= weaponLoadout.Length) return;

        _isReloading = false;
        _activeWeaponIndex.value = newIndex;
    }

    private void TryReload()
    {
        if (_isReloading || CurrentMag >= CurrentWeapon.magazineSize || CurrentReserve <= 0) return;

        /*int needed = weapon.magazineSize - _currentMag;
        int transfer = Mathf.Min(needed, _currentReserve);

        _currentMag += transfer;
        _currentReserve -= transfer;*/
        ReloadServerRPC();
    }

    private void ShootHitscan(Vector3 pos, Vector3 forward)
    {
        Vector3 endPoint = pos + (forward * CurrentWeapon.range);
        if (Physics.Raycast(pos, forward, out RaycastHit hit, CurrentWeapon.range, hitMask))
        {
            endPoint = hit.point;
            if (hit.collider.TryGetComponent(out PlayerHealth health))
                health.TakeDamage(CurrentWeapon.damage);
        }

        HitscanDebugObserverRPC(pos, endPoint);
    }

    private void ShootProjectile(Vector3 pos, Vector3 forward)
    {
        GameObject proj = Instantiate(CurrentWeapon.projectilePrefab, pos, Quaternion.LookRotation(forward));

        if (proj.TryGetComponent(out WeaponProjectile projectileScript))
            projectileScript.Initialize(CurrentWeapon.damage, GetComponent<Collider>());
    }

    private void ShootMelee(Vector3 pos, Vector3 forward)
    {
        if (Physics.SphereCast(pos, CurrentWeapon.meleeRadius, forward, out RaycastHit hit, CurrentWeapon.range, hitMask))
        {
            if (hit.collider.TryGetComponent(out PlayerHealth health))
                health.TakeDamage(CurrentWeapon.damage);
        }
    }

    private void HandleSway()
    {
        Vector2 delta = lookAction.action.ReadValue<Vector2>();

        Quaternion targetRot = _initialRot * Quaternion.Euler(
            Mathf.Clamp(delta.y * swayAmount, swayMinMax.x, swayMinMax.y), 
            Mathf.Clamp(-delta.x * swayAmount, swayMinMax.x, swayMinMax.y), 0);

        viewModelParent.localRotation = Quaternion.Slerp(viewModelParent.localRotation, targetRot, Time.deltaTime * swaySmooth);
    }

    [ServerRpc]
    private void ShootServerRPC(Vector3 pos, Vector3 forward)
    {
        //if (_isReloading || Time.time < _nextFireTime) return;
        if(CurrentWeapon.shootMode != WeaponInfo.ShootMode.Melee) 
            CurrentMag--;

        switch (CurrentWeapon.shootMode)
        {
            case WeaponInfo.ShootMode.Hitscan:
                ShootHitscan(pos, forward);
                break;
            case WeaponInfo.ShootMode.Projectile:
                ShootProjectile(pos, forward);
                break;
            case WeaponInfo.ShootMode.Melee:
                ShootMelee(pos, forward);
                break;
        }
    }

    [ServerRpc]
    private void ReloadServerRPC()
    {
        if (_isReloading || CurrentMag >= CurrentWeapon.magazineSize || CurrentReserve <= 0) return;

        int needed = CurrentWeapon.magazineSize - CurrentMag;
        int transfer = Mathf.Min(needed, CurrentReserve);

        CurrentMag += transfer;
        CurrentReserve -= transfer;
    }

    [ObserversRpc]
    private void HitscanDebugObserverRPC(Vector3 start, Vector3 end)
    {
        Debug.DrawLine(start, end, Color.yellow, 0.5f);
    }
}