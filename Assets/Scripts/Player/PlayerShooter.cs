using PurrNet;
using System.Net;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// Manages player shooting mechanics including weapon switching, reloading, ammo management,
/// and different weapon types (hitscan, projectile, melee). Handles both server and client-side logic.
/// </summary>
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

    [Header("Events")]
    public UnityEvent<PlayerShooter> onAmmoChanged;

    private Quaternion _initialRot;
    private GameObject[] _weaponViewmodels;
    private PlayerVisuals _playerVisuals;
    private PlayerTeam _playerTeam;

    private SyncVar<int> _activeWeaponIndex = new(0);
    private SyncList<int> _mags = new();
    private SyncList<int> _reserves = new();
    private float _nextFireTime = 0f;
    private float _reloadEndTime = 0f;
    private float _currentRevTime = 0f;

    private bool _isReloading = false;
    private bool _isRevving = false;

    private int _lastMag = -1;
    private int _lastReserve = -1;

    /// <summary>
    /// Gets the currently equipped weapon information.
    /// </summary>
    public WeaponInfo CurrentWeapon => weaponLoadout.Length > _activeWeaponIndex.value ? weaponLoadout[_activeWeaponIndex.value] : null;
    public int CurrentWeaponIndex => _activeWeaponIndex.value;

    /// <summary>
    /// Gets or sets the current magazine ammo count for the active weapon.
    /// </summary>
    public int CurrentMag
    {
        get => _mags.Count > _activeWeaponIndex.value ? _mags[_activeWeaponIndex.value] : 0;
        set { if (_mags.Count > _activeWeaponIndex.value) _mags[_activeWeaponIndex.value] = value; }
    }

    /// <summary>
    /// Gets or sets the current reserve ammo count for the active weapon.
    /// </summary>
    public int CurrentReserve
    {
        get => _reserves.Count > _activeWeaponIndex.value ? _reserves[_activeWeaponIndex.value] : 0;
        set { if (_reserves.Count > _activeWeaponIndex.value) _reserves[_activeWeaponIndex.value] = value; }
    }

    private void Awake()
    {
        _playerVisuals = GetComponent<PlayerVisuals>();
        _playerTeam = GetComponent<PlayerTeam>();
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

        _mags.onChanged += OnMagsChanged;

        if (isOwner)
        {
            _weaponViewmodels = new GameObject[weaponLoadout.Length];
            for (int i = 0; i < _weaponViewmodels.Length; i++)
            {
                if (weaponLoadout[i].viewmodel)
                    _weaponViewmodels[i] = Instantiate(weaponLoadout[i].viewmodel, viewModelParent);
            }

            if (WeaponSwitchUI.Instance)
            {
                WeaponSwitchUI.Instance.Initialize(weaponLoadout);
                WeaponSwitchUI.Instance.EnableGameplayHUD();
                WeaponSwitchUI.Instance.ShowUI(_activeWeaponIndex.value);
            }

            _initialRot = viewModelParent.localRotation;
            switchWeaponAction.action.performed += HandleWeaponSwitch;

            if (!_playerVisuals)
            {
                UpdateWeaponVisual(_activeWeaponIndex.value);
                _activeWeaponIndex.onChanged += SwitchWeaponLocal;
            }
        }

        if (_playerVisuals) _activeWeaponIndex.onChanged += _playerVisuals.SwitchWeapon;
    }

    protected override void OnDespawned()
    {
        if (isOwner)
        {
            switchWeaponAction.action.performed -= HandleWeaponSwitch;
            if (!_playerVisuals) _activeWeaponIndex.onChanged -= SwitchWeaponLocal;
        }

        if (_playerVisuals) _activeWeaponIndex.onChanged -= _playerVisuals.SwitchWeapon;
    }

    void Update()
    {
        if (CurrentWeapon == null || _mags.Count == 0 || _reserves.Count == 0) return;

        if (isServer && _isReloading && Time.time >= _reloadEndTime)
        {
            _isReloading = false;

            int needed = CurrentWeapon.magazineSize - CurrentMag;
            int transfer = Mathf.Min(needed, CurrentReserve);

            CurrentMag += transfer;
            CurrentReserve -= transfer;
        }

        if (!isOwner) return;

        HandleSway();

        if (_lastMag != CurrentMag || _lastReserve != CurrentReserve)
        {
            _lastMag = CurrentMag;
            _lastReserve = CurrentReserve;
            RefreshAmmoUI();
        }

        if (!isServer && _isReloading && Time.time >= _reloadEndTime)
            _isReloading = false;

        if (!_isReloading && CurrentWeapon != null)
        {
            bool isFirePressed = fireAction.action.IsPressed();
            bool wasFirePressed = fireAction.action.WasPressedThisFrame();
            bool canAutoShoot = isFirePressed && CurrentMag > 0;

            // REVVING
            if (CurrentWeapon.requiresRevving)
            {
                SetRevving(isFirePressed);
                canAutoShoot &= (_currentRevTime >= CurrentWeapon.revUpTime);
            }

            // SHOOTING
            if ((CurrentWeapon.fireType == WeaponInfo.FireType.Auto && canAutoShoot) ||
                (CurrentWeapon.fireType == WeaponInfo.FireType.Semi && wasFirePressed))
                TryShoot();
        }
        else if (CurrentWeapon != null && CurrentWeapon.fireType == WeaponInfo.FireType.Auto)
        {
            SetRevving(false);
        }

        if (reloadAction.action.WasPressedThisFrame())
            TryReload();
    }

    /// <summary>
    /// Handles weapon switching input and initiates the switch.
    /// </summary>
    /// <param name="context">The input action context.</param>
    public void HandleWeaponSwitch(InputAction.CallbackContext context)
    {
        float switchInput = context.ReadValue<float>();
        if (switchInput != 0)
        {
            int newIndex = _activeWeaponIndex.value;
            if (switchInput < 0) newIndex = (newIndex + 1) % weaponLoadout.Length;
            else if (switchInput > 0) newIndex = (newIndex - 1 + weaponLoadout.Length) % weaponLoadout.Length;

            if (newIndex != _activeWeaponIndex.value)
            {
                _isReloading = false;
                SwitchWeaponServerRPC(newIndex);
            }
        }
    }

    /// <summary>
    /// Updates weapon visuals locally when the weapon index changes.
    /// </summary>
    /// <param name="newIndex">The new weapon index.</param>
    private void SwitchWeaponLocal(int newIndex)
    {
        UpdateWeaponVisual(newIndex);

        if (WeaponSwitchUI.Instance)
            WeaponSwitchUI.Instance.ShowUI(newIndex);

        _lastMag = -1;
    }

    /// <summary>
    /// Updates the visibility of weapon viewmodels.
    /// </summary>
    /// <param name="newIndex">The active weapon index.</param>
    private void UpdateWeaponVisual(int newIndex)
    {
        for (int i = 0; i < _weaponViewmodels.Length; i++)
        {
            if (_weaponViewmodels[i] != null)
                _weaponViewmodels[i].SetActive(i == newIndex);
        }
    }

    /// <summary>
    /// Updates the ammo UI display.
    /// </summary>
    private void RefreshAmmoUI()
    {
        if (WeaponSwitchUI.Instance)
        {
            bool isMelee = CurrentWeapon.shootMode == WeaponInfo.ShootMode.Melee;
            WeaponSwitchUI.Instance.UpdateAmmo(CurrentMag, CurrentReserve, isMelee);
        }
    }

    /// <summary>
    /// Attempts to shoot the current weapon.
    /// </summary>
    public void TryShoot()
    {
        if (!isOwner || _isReloading || Time.time < _nextFireTime || CurrentWeapon == null) return;

        if (CurrentWeapon.shootMode != WeaponInfo.ShootMode.Melee && CurrentMag <= 0)
        {
            TryReload();
            return;
        }

        _nextFireTime = Time.time + (1f / CurrentWeapon.fireRate);

        if (_playerVisuals && !CurrentWeapon.requiresRevving) 
            _playerVisuals.PlayAttack();

        // STARTING POSITION & DIRECTION
        Vector3 startPos;
        Vector3 shootDirection;
        if (CurrentWeapon.shootMode == WeaponInfo.ShootMode.Projectile)
        {
            startPos = firePoint.position;

            Vector3 targetPoint;
            if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, CurrentWeapon.range, hitMask, QueryTriggerInteraction.Ignore))
                targetPoint = hit.point;
            else
                targetPoint = playerCamera.position + (playerCamera.forward * CurrentWeapon.range);

            shootDirection = (targetPoint - startPos).normalized;
        }
        else
        {
            startPos = playerCamera.position;
            shootDirection = playerCamera.forward;
        }

        // BULLET SPREAD
        if (CurrentWeapon.spread > 0f && CurrentWeapon.shootMode != WeaponInfo.ShootMode.Melee)
        {
            shootDirection += Random.insideUnitSphere * CurrentWeapon.spread;
            shootDirection.Normalize();
        }
        ShootServerRPC(startPos, shootDirection);
    }

    /// <summary>
    /// Adds ammo to the current reserve.
    /// </summary>
    /// <param name="amount">The amount of ammo to add.</param>
    public void AddAmmo(int amount)
    {
        AddAmmoServerRPC(amount);
    }

    /// <summary>
    /// Refills all ammo and resets to the first weapon. Server-only operation.
    /// </summary>
    public void RefillAmmo()
    {
        if (!isServer) return;

        for (int i = 0; i < weaponLoadout.Length; i++)
        {
            _mags[i] = weaponLoadout[i].magazineSize;
            _reserves[i] = weaponLoadout[i].initialReserve;
        }
        _activeWeaponIndex.value = 0;
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

    /// <summary>
    /// Attempts to reload the current weapon.
    /// </summary>
    private void TryReload()
    {
        if (_isReloading || CurrentWeapon == null || CurrentMag >= CurrentWeapon.magazineSize || CurrentReserve <= 0) return;

        _isReloading = true;
        _reloadEndTime = Time.time + CurrentWeapon.reloadTime;

        if (_playerVisuals) _playerVisuals.PlayReload();

        StartReloadServerRPC();
    }

    /// <summary>
    /// Performs hitscan shooting logic.
    /// </summary>
    /// <param name="pos">The origin position of the shot.</param>
    /// <param name="forward">The direction of the shot.</param>
    private void ShootHitscan(Vector3 pos, Vector3 forward)
    {
        if (CurrentWeapon == null) return;
        Vector3 endPoint = pos + (forward * CurrentWeapon.range);

        if (Physics.Raycast(pos, forward, out RaycastHit hit, CurrentWeapon.range, hitMask, QueryTriggerInteraction.Ignore))
        {
            endPoint = hit.point;

            if (hit.collider.TryGetComponent(out PlayerHealth health) && hit.collider.TryGetComponent(out PlayerTeam targetTeam))
                if (_playerTeam != null && _playerTeam.Team != targetTeam.Team)
                    health.TakeDamage(CurrentWeapon.damage);
        }

        HitscanDebugObserverRPC(pos, endPoint);
    }

    /// <summary>
    /// Performs projectile shooting logic.
    /// </summary>
    /// <param name="pos">The spawn position of the projectile.</param>
    /// <param name="forward">The direction of the projectile.</param>
    private void ShootProjectile(Vector3 pos, Vector3 forward)
    {
        if (CurrentWeapon == null) return;
        GameObject proj = Instantiate(CurrentWeapon.projectilePrefab, pos, Quaternion.LookRotation(forward));

        if (proj.TryGetComponent(out WeaponProjectile projectileScript))
            projectileScript.Initialize(CurrentWeapon.damage, GetComponent<Collider>(), _playerTeam.Team);
    }

    /// <summary>
    /// Performs melee attack logic using a sphere cast.
    /// </summary>
    /// <param name="pos">The origin position of the attack.</param>
    /// <param name="forward">The direction of the attack.</param>
    private void ShootMelee(Vector3 pos, Vector3 forward)
    {
        if (CurrentWeapon == null) return;

        if (Physics.SphereCast(pos, CurrentWeapon.meleeRadius, forward, out RaycastHit hit, CurrentWeapon.range, hitMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.TryGetComponent(out PlayerHealth health) && hit.collider.TryGetComponent(out PlayerTeam targetTeam))
                if (_playerTeam != null && _playerTeam.Team != targetTeam.Team)
                    health.TakeDamage(CurrentWeapon.damage);
        }

        HitscanDebugObserverRPC(pos, pos+forward*CurrentWeapon.range);
    }

    /// <summary>
    /// Handles weapon sway based on mouse input.
    /// </summary>
    private void HandleSway()
    {
        Vector2 delta = lookAction.action.ReadValue<Vector2>();

        Quaternion targetRot = _initialRot * Quaternion.Euler(
            Mathf.Clamp(delta.y * swayAmount, swayMinMax.x, swayMinMax.y),
            Mathf.Clamp(-delta.x * swayAmount, swayMinMax.x, swayMinMax.y), 0);

        viewModelParent.localRotation = Quaternion.Slerp(viewModelParent.localRotation, targetRot, Time.deltaTime * swaySmooth);
    }

    private void SetRevving(bool revving)
    {
        if (_isRevving != revving)
        {
            _isRevving = revving;
            if (_playerVisuals) _playerVisuals.SetRevving(revving);
        }
        _currentRevTime = (revving) ? _currentRevTime + Time.deltaTime : 0f;
    }

    private void OnMagsChanged(SyncListChange<int> change)
    {
        if(change.operation == SyncListOperation.Set)
            onAmmoChanged?.Invoke(this);
    }

    [ServerRpc]
    private void ShootServerRPC(Vector3 pos, Vector3 forward)
    {
        if (CurrentWeapon == null) return;

        if (CurrentWeapon.shootMode != WeaponInfo.ShootMode.Melee)
            CurrentMag--;

        PlayShootEffectsObserverRPC();

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
    private void StartReloadServerRPC()
    {
        if (_isReloading || CurrentWeapon == null || CurrentMag >= CurrentWeapon.magazineSize || CurrentReserve <= 0) return;

        _isReloading = true;
        _reloadEndTime = Time.time + CurrentWeapon.reloadTime;
    }

    [ObserversRpc]
    private void HitscanDebugObserverRPC(Vector3 start, Vector3 end)
    {
        Debug.DrawLine(start, end, Color.yellow, 0.5f);
    }

    [ObserversRpc]
    private void PlayShootEffectsObserverRPC()
    {
        if (CurrentWeapon != null && CurrentWeapon.shootSound != null)
        {
            if (TryGetComponent(out AudioSource playerAudio))
                playerAudio.PlayOneShot(CurrentWeapon.shootSound);
        }

        if (_playerVisuals) _playerVisuals.PlayMuzzleFlash(_activeWeaponIndex.value);
    }
}
