using PurrNet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages player visual representation including character models, weapon visuals, and animations.
/// Handles differentiation between local player (viewmodel) and remote players (world model).
/// </summary>
public class PlayerVisuals : NetworkBehaviour
{
    [Serializable]
    class WeaponVisuals
    {
        public Material redTeamMaterial;
        public Material blueTeamMaterial;

        public GameObject worldModel;
        public GameObject viewModel;

        public List<Renderer> weaponRenderers;

        [Header("Effects")]
        public ParticleSystem worldMuzzleFlash;
        public ParticleSystem viewModelMuzzleFlash;
    }

    [Header("Team Colors")]
    [SerializeField] private Material redTeamMaterial;
    [SerializeField] private Material blueTeamMaterial;
    [SerializeField] private GameObject[] materialIgnoredObjects;

    [Header("Transforms")]
    [SerializeField] private Transform cameraMimicTransform;
    [SerializeField] private Transform spineTransform;

    [Header("Visuals")]
    [SerializeField] private GameObject playerVisuals;
    [SerializeField] private GameObject viewmodelParent;
    [SerializeField] private List<WeaponVisuals> weaponVisuals;

    [Header("Animators")]
    [SerializeField] private NetworkAnimator playerAnimator;
    [SerializeField] private Animator viewModelAnimator;

    /// <summary>
    /// Initializes the player visuals by setting up appropriate visibility for owner vs remote players.
    /// </summary>
    public void Init()
    {
        if (playerVisuals)
        {
            foreach (Transform part in playerVisuals.transform)
                part.gameObject.SetActive(!isOwner);
        }

        for (int i = 0; i < weaponVisuals.Count; i++)
        {
            WeaponVisuals visual = weaponVisuals[i];
            if ((visual.weaponRenderers != null && visual.weaponRenderers.Count > 0)) continue;
            List<Renderer> buffer = new List<Renderer>();
            visual.weaponRenderers = new List<Renderer>();

            visual.worldModel.GetComponentsInChildren<Renderer>(true, buffer);
            visual.weaponRenderers.AddRange(buffer);

            visual.viewModel.GetComponentsInChildren<Renderer>(true, buffer);
            visual.weaponRenderers.AddRange(buffer);
        }

        if (viewmodelParent) viewmodelParent.SetActive(isOwner);
        SwitchWeapon(GetComponent<PlayerShooter>().CurrentWeaponIndex);
    }

    private void LateUpdate()
    {
        if(spineTransform)
            spineTransform.localRotation = Quaternion.Euler(cameraMimicTransform.eulerAngles.x, 0f, 0f);
    }

    /// <summary>
    /// Switches the active weapon visual and triggers corresponding animations.
    /// </summary>
    /// <param name="weaponIndex">The index of the weapon to switch to.</param>
    public void SwitchWeapon(int weaponIndex)
    {
        if (weaponIndex >= weaponVisuals.Count) return;

        if (isOwner)
        {
            if (playerAnimator)
            {
                playerAnimator.SetInteger("Weapon", weaponIndex);
                playerAnimator.SetTrigger("SwitchWeapon");
            }

            if (viewModelAnimator)
            {
                viewModelAnimator.SetInteger("Weapon", weaponIndex);
                viewModelAnimator.SetTrigger("SwitchWeapon");
            }

            if (WeaponSwitchUI.Instance) WeaponSwitchUI.Instance.ShowUI(weaponIndex);
        }

        for (int i = 0; i < weaponVisuals.Count; i++)
        {
            WeaponVisuals weapon = weaponVisuals[i];

            weapon.worldModel.SetActive(i == weaponIndex);
            if(weapon.viewModel) weapon.viewModel.SetActive(i == weaponIndex);
        }
    }

    /// <summary>
    /// Triggers the attack animation for both player and viewmodel.
    /// Only executes for the owning client.
    /// </summary>
    public void PlayAttack()
    {
        if (!isOwner) return;

        if(playerAnimator) playerAnimator.SetTrigger("Attack");
        if(viewModelAnimator) viewModelAnimator.SetTrigger("Attack");
    }

    /// <summary>
    /// Triggers the reload animation for both player and viewmodel.
    /// Only executes for the owning client.
    /// </summary>
    public void PlayReload()
    {
        if (!isOwner) return;

        if(playerAnimator) playerAnimator.SetTrigger("Reload");
        if(viewModelAnimator) viewModelAnimator.SetTrigger("Reload");
    }

    /// <summary>
    /// Triggers the jump animation for the player.
    /// Only executes for the owning client.
    /// </summary>
    public void PlayJump()
    {
        if (!isOwner || !playerAnimator) return;

        playerAnimator.SetTrigger("Jump");
    }

    public void PlayMuzzleFlash(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= weaponVisuals.Count) return;
        WeaponVisuals visual = weaponVisuals[weaponIndex];

        if (isOwner && visual.viewModelMuzzleFlash != null)
            visual.viewModelMuzzleFlash.Play();
        else if (!isOwner && visual.worldMuzzleFlash != null)
            visual.worldMuzzleFlash.Play();
    }

    public void SetAutoAttacking(bool autoAttacking)
    {
        if (!isOwner) return;

        if (playerAnimator) playerAnimator.SetBool("AutoAttacking", autoAttacking);
        if (viewModelAnimator) viewModelAnimator.SetBool("AutoAttacking", autoAttacking);
    }
    public void SetRevving(bool revving)
    {
        if (!isOwner) return;

        if (playerAnimator) playerAnimator.SetBool("Revving", revving);
        if (viewModelAnimator) viewModelAnimator.SetBool("Revving", revving);
    }

    /// <summary>
    /// Sets the crouch state on the player animator.
    /// Only executes for the owning client.
    /// </summary>
    /// <param name="crouching">Whether the player is crouching.</param>
    public void SetCrouch(bool crouching)
    {
        if (!isOwner || !playerAnimator) return;

        playerAnimator.SetBool("Crouching", crouching);
    }

    /// <summary>
    /// Sets the grounded state on the player animator.
    /// Only executes for the owning client.
    /// </summary>
    /// <param name="grounded">Whether the player is grounded.</param>
    public void SetGrounded(bool grounded)
    {
        if (!isOwner || !playerAnimator) return;

        playerAnimator.SetBool("Grounded", grounded);
    }

    /// <summary>
    /// Updates the movement blend parameters on the player animator.
    /// Only executes for the owning client.
    /// </summary>
    /// <param name="forwards">The forward movement value (-1 to 1).</param>
    /// <param name="sideways">The sideways movement value (-1 to 1).</param>
    public void SetMovement(float forwards, float sideways)
    {
        if (!isOwner || !playerAnimator) return;

        playerAnimator.SetFloat("Forwards", forwards);
        playerAnimator.SetFloat("Sideways", sideways);
    }

    public void SetTeam(Team team)
    {
        Material bodyMaterial = (team == Team.Blue) ? blueTeamMaterial : redTeamMaterial;

        if (bodyMaterial != null)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            ApplyMaterial(renderers, bodyMaterial);
        }

        for (int i = 0; i < weaponVisuals.Count; i++)
        {
            WeaponVisuals visual = weaponVisuals[i];
            Material weaponMaterial = (team == Team.Blue) ? visual.blueTeamMaterial : visual.redTeamMaterial;
            if (weaponMaterial == null) continue;

            ApplyMaterial(visual.weaponRenderers.ToArray(), weaponMaterial);
        }
    }

    private void ApplyMaterial(Renderer[] renderers, Material mat)
    {
        foreach (Renderer rend in renderers)
        {
            bool isChild = materialIgnoredObjects.Any((o) => rend.transform.IsChildOf(o.transform));
            if (!isChild)
                rend.sharedMaterial = mat;
        }
    }
}
