using PurrNet;
using TMPro;
using UnityEngine;

/// <summary>
/// Displays the respawn timer UI for the local player.
/// Shows remaining time until respawn and automatically hides when not needed.
/// </summary>
public class RespawnTimerUI : MonoBehaviour
{
    public GameObject container;
    public TextMeshProUGUI timerText;

    void Awake()
    {
        container.SetActive(false);
    }

    void Update()
    {
        if (PlayerConnection.Local == null)
        {
            container.SetActive(false);
            return;
        }
        SyncTimer respawnTimer = PlayerConnection.Local.respawnTimer;
        float remaining = respawnTimer.remaining;

        if (remaining > 0)
        {
            container.SetActive(true);
            timerText.text = $"{respawnTimer.remainingInt} s";
        }
        else
        {
            container.SetActive(false);
        }
    }
}
