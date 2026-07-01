using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the player information UI including health bar display.
/// Singleton pattern for global access to UI updates.
/// </summary>
public class PlayerInfoUI : MonoBehaviour
{
    public static PlayerInfoUI Instance;

    public GameObject playerInfoUIParent;

    public Slider healthBar;
    public TextMeshProUGUI healthBarText;

    void Awake()
    {
        SetActive(false);

        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    /// <summary>
    /// Sets the visibility of the player info UI.
    /// </summary>
    /// <param name="active">Whether the UI should be active.</param>
    public void SetActive(bool active)
    {
        if(playerInfoUIParent) playerInfoUIParent.SetActive(active);
    }

    /// <summary>
    /// Updates the health bar slider and text with the current health value.
    /// </summary>
    /// <param name="health">The current health value.</param>
    /// <param name="maxHealth">The max health value.</param>
    public void UpdateHealthBar(int health, int maxHealth)
    {
        healthBar.value = health / (float)maxHealth;
        healthBarText.text = health.ToString();
    }
}
