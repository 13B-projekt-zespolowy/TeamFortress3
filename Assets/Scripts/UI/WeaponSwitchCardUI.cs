using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public void Initialize(int index, WeaponInfo weapon)
    {
        if (numberText) numberText.text = (index + 1).ToString();
        if (nameText) nameText.text = weapon.weaponName;
        if (weaponImage) weaponImage.sprite = weapon.weaponIcon;
    }

    public void SetActive(bool isActive)
    {
        if (isActive)
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _defaultWidth + 25f);
        else
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _defaultWidth);
    }
}
