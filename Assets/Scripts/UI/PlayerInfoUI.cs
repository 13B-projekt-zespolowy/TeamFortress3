using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public void SetActive(bool active)
    {
        if(playerInfoUIParent) playerInfoUIParent.SetActive(active);
    }

    public void UpdateHealthBar(int value)
    {
        healthBar.value = value;
        healthBarText.text = value.ToString();
    }
}
