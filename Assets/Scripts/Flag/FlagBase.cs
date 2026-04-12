using UnityEngine;

public class FlagBase : MonoBehaviour
{
    public Team team;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Test");
        if (!other.TryGetComponent<PlayerFlag>(out var player)) return;
        
        if (player.team != team) return;

        if (player.carriedFlag != null)
        {
            ModeManager.Instance.Score(player.team);
            player.carriedFlag.ReturnToBase();
            player.carriedFlag = null;
        }
    }
}