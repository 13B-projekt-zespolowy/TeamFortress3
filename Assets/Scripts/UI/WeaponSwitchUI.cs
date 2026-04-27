using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitchUI : MonoBehaviour
{
    public static WeaponSwitchUI Instance;
    [Header("Setup")]
    public GameObject cardPrefab;
    public CanvasGroup canvasGroup;

    [Header("Settings")]
    public float displayDuration = 2f;
    public float fadeSpeed = 5f;

    private List<WeaponSwitchCardUI> _cards = new();
    private Coroutine _fadeRoutine;

    void Awake()
    {
        canvasGroup.alpha = 0;

        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void Initialize(WeaponInfo[] loadout)
    {
        foreach (Transform child in transform) 
            Destroy(child.gameObject);
        _cards.Clear();

        for (int i = 0; i < loadout.Length; i++)
        {
            GameObject go = Instantiate(cardPrefab, transform);

            WeaponSwitchCardUI card = go.GetComponent<WeaponSwitchCardUI>();
            card.Initialize(i, loadout[i]);
            _cards.Add(card);
        }
    }

    public void ShowUI(int activeWeaponIndex)
    {
        SetActiveSlot(activeWeaponIndex);

        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeSequence());
    }

    void SetActiveSlot(int index)
    {
        for (int i = 0; i < _cards.Count; i++)
            _cards[i].SetActive(i == index);
    }

    private IEnumerator FadeSequence()
    {
        canvasGroup.alpha = 1;

        yield return new WaitForSeconds(displayDuration);

        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
    }
}