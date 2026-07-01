using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages the weapon switch UI including ammo display, weapon cards, and HUD elements.
/// Handles fade animations for weapon selection cards and updates ammo/crosshair state.
/// </summary>
public class WeaponSwitchUI : MonoBehaviour
{
    public static WeaponSwitchUI Instance;

    [Header("Main Panels (Always Visible)")]
    public GameObject statsPanel;
    public GameObject ammoPanel;

    [Header("HUD Elements (Bottom Left)")]
    public GameObject crosshairObject;
    public Image ammoIconImage;
    public TextMeshProUGUI ammoText;
    public Sprite rangedSprite;
    public Sprite meleeSprite;

    [Header("Setup (Center Cards - Optional)")]
    public GameObject cardPrefab;
    public CanvasGroup canvasGroup;

    [Header("Settings (Center Cards)")]
    public float displayDuration = 2f;
    public float fadeSpeed = 5f;

    private List<WeaponSwitchCardUI> _cards = new();
    private Coroutine _fadeRoutine;

    void Awake()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0;

        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    /// <summary>
    /// Enables the gameplay HUD panels (stats and ammo).
    /// </summary>
    public void EnableGameplayHUD()
    {
        if (statsPanel) statsPanel.SetActive(true);
        if (ammoPanel) ammoPanel.SetActive(true);
    }

    /// <summary>
    /// Initializes weapon cards from the weapon loadout.
    /// </summary>
    /// <param name="loadout">Array of WeaponInfo for available weapons.</param>
    public void Initialize(WeaponInfo[] loadout)
    {
        if (cardPrefab != null)
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);
            _cards.Clear();

            for (int i = 0; i < loadout.Length; i++)
            {
                GameObject go = Instantiate(cardPrefab, transform);
                WeaponSwitchCardUI card = go.GetComponent<WeaponSwitchCardUI>();

                if (card != null)
                {
                    card.Initialize(i, loadout[i]);
                    _cards.Add(card);
                }
            }
        }
    }

    /// <summary>
    /// Shows the weapon switch UI and updates HUD for the active weapon.
    /// </summary>
    /// <param name="activeWeaponIndex">The index of the currently active weapon.</param>
    public void ShowUI(int activeWeaponIndex)
    {
        UpdateHUD(activeWeaponIndex);

        if (cardPrefab != null && canvasGroup != null && _cards.Count > 0)
        {
            SetActiveSlot(activeWeaponIndex);

            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeSequence());
        }
    }

    /// <summary>
    /// Sets the active weapon card by expanding its width.
    /// </summary>
    /// <param name="index">The index of the active weapon.</param>
    private void SetActiveSlot(int index)
    {
        for (int i = 0; i < _cards.Count; i++)
        {
            if (_cards[i] != null)
                _cards[i].SetActive(i == index);
        }
    }

    /// <summary>
    /// Updates HUD elements based on the active weapon type.
    /// </summary>
    /// <param name="index">The active weapon index.</param>
    private void UpdateHUD(int index)
    {
        if (crosshairObject) crosshairObject.SetActive(index == 0);

        WeaponSwitchCardUI card = _cards.ElementAtOrDefault(index);
        if (ammoIconImage && card) ammoIconImage.sprite = card.weaponImage.sprite;
    }

    /// <summary>
    /// Updates the ammo display text.
    /// </summary>
    /// <param name="currentMag">Current magazine ammo count.</param>
    /// <param name="currentReserve">Current reserve ammo count.</param>
    /// <param name="isMelee">Whether the active weapon is melee (hides ammo text).</param>
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

    /// <summary>
    /// Coroutine that handles the fade-in, display, and fade-out sequence for weapon cards.
    /// </summary>
    private IEnumerator FadeSequence()
    {
        if (canvasGroup == null) yield break;

        canvasGroup.alpha = 1;

        yield return new WaitForSeconds(displayDuration);

        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
    }
}
