using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a weapon selection card in the weapon switch UI.
/// Displays weapon number, name, and icon with visual highlighting for the active selection.
/// </summary>
public class WeaponSwitchCardUI : MonoBehaviour
{
    public TextMeshProUGUI numberText;
    public TextMeshProUGUI nameText;
    public Image weaponImage;

    private RectTransform _rectTransform;
    private float _defaultWidth;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _defaultWidth = _rectTransform.rect.width;
    }

    /// <summary>
    /// Initializes the weapon card with the provided weapon data.
    /// </summary>
    /// <param name="index">The weapon index (0-based).</param>
    /// <param name="weapon">The WeaponInfo containing display data.</param>
    public void Initialize(int index, WeaponInfo weapon)
    {
        if (numberText) numberText.text = (index + 1).ToString();
        if (nameText) nameText.text = weapon.weaponName;
        if (weaponImage) weaponImage.sprite = weapon.weaponIcon;
    }

    /// <summary>
    /// Sets the active state of the card, expanding its width when active.
    /// </summary>
    /// <param name="isActive">Whether this card is the currently selected weapon.</param>
    public void SetActive(bool isActive)
    {
        if (isActive)
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _defaultWidth + 25f);
        else
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _defaultWidth);
    }
}
