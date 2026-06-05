using UnityEngine;

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