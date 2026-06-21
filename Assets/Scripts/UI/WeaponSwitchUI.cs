using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

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

    public void EnableGameplayHUD()
    {
        if (statsPanel) statsPanel.SetActive(true);
        if (ammoPanel) ammoPanel.SetActive(true);
    }

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

    private void SetActiveSlot(int index)
    {
        for (int i = 0; i < _cards.Count; i++)
        {
            if (_cards[i] != null)
                _cards[i].SetActive(i == index);
        }
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