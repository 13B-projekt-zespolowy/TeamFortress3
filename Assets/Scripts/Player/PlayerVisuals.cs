using PurrNet;
using System;
using System.Collections.Generic;
using UnityEngine;

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

    public void PlayAttack()
    {
        if (!isOwner) return;

        if(playerAnimator) playerAnimator.SetTrigger("Attack");
        if(viewModelAnimator) viewModelAnimator.SetTrigger("Attack");
    }
    public void PlayReload()
    {
        if (!isOwner) return;

        if(playerAnimator) playerAnimator.SetTrigger("Reload");
        if(viewModelAnimator) viewModelAnimator.SetTrigger("Reload");
    }

    public void PlayJump()
    {
        if (!isOwner || !playerAnimator) return;

        playerAnimator.SetTrigger("Jump");
    }

    public void SetCrouch(bool crouching)
    {
        if (!isOwner || !playerAnimator) return;

        playerAnimator.SetBool("Crouching", crouching);
    }
    public void SetGrounded(bool crouching)
    {
        if (!isOwner || !playerAnimator) return;

        playerAnimator.SetBool("Grounded", crouching);
    }
    public void SetMovement(float forwards, float sideways)
    {
        if (!isOwner || !playerAnimator) return;

        playerAnimator.SetFloat("Forwards", forwards);
        playerAnimator.SetFloat("Sideways", sideways);
    }
}
