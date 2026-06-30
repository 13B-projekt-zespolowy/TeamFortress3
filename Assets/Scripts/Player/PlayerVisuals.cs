using PurrNet;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages player visual representation including character models, weapon visuals, and animations.
/// Handles differentiation between local player (viewmodel) and remote players (world model).
/// </summary>
public class PlayerVisuals : NetworkBehaviour
{
    [Serializable]
    struct WeaponVisuals
    {
        public GameObject worldModel;
        public GameObject viewModel;
    }

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

        if(viewmodelParent) viewmodelParent.SetActive(isOwner);
        SwitchWeapon(0);
    }

    private void Update()
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
}
