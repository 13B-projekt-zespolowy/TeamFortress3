using UnityEngine;

/// <summary>
/// Makes the minimap camera follow the player from a top-down perspective.
/// Positions the camera above the player at a fixed height and looks straight down.
/// </summary>
public class MinimapFollow : MonoBehaviour
{
    public float height = 20f;
    private Transform player;

    private void LateUpdate()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                return;
            }
        }

        transform.position = new Vector3(player.position.x, player.position.y + height, player.position.z);
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
