using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public float height = 20f;

    private Transform player;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void LateUpdate()
    {
        if (player != null)
        {
            transform.position = new Vector3(player.position.x, player.position.y + height, player.position.z);
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }
}