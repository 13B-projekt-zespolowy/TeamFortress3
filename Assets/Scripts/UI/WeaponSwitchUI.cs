using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponSwitchUI : MonoBehaviour
{
    public static WeaponSwitchUI Instance;

    [Header("Main Panels")]
    public GameObject statsPanel;
    public GameObject ammoPanel;

    [Header("HUD Elements")]
    public GameObject crosshairObject;
    public Image ammoIconImage;
    public TextMeshProUGUI ammoText;
    public Sprite rangedSprite;
    public Sprite meleeSprite;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void EnableGameplayHUD()
    {
        if (statsPanel) statsPanel.SetActive(true);
        if (ammoPanel) ammoPanel.SetActive(true);
    }

    public void Initialize(WeaponInfo[] loadout)
    {
    }

    public void ShowUI(int activeWeaponIndex)
    {
        UpdateHUD(activeWeaponIndex);
    }

    private void UpdateHUD(int index)
    {
        if (index == 0)
        {
            if (crosshairObject) crosshairObject.SetActive(true);
            if (ammoIconImage && rangedSprite) ammoIconImage.sprite = rangedSprite;
        }
        else if (index == 1)
        {
            if (crosshairObject) crosshairObject.SetActive(false);
            if (ammoIconImage && meleeSprite) ammoIconImage.sprite = meleeSprite;
        }
    }

    public void UpdateAmmo(int currentMag, int currentReserve, bool isMelee)
    {
        if (ammoText == null) return;

        if (isMelee)
        {
            ammoText.gameObject.SetActive(false);
        }
        else
        {
            ammoText.gameObject.SetActive(true);
            ammoText.text = $"{currentMag}/{currentReserve}";
        }
    }
}