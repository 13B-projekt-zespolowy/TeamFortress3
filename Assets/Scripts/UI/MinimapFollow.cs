using UnityEngine;

/// <summary>
/// Makes the minimap camera follow the player from a top-down perspective.
/// Positions the camera above the player at a fixed height and looks straight down.
/// </summary>
public class MinimapFollow : MonoBehaviour
{
    public float height = 20f;
    public Transform target = null;

    private void LateUpdate()
    {
        if (target != null)
        {
            transform.position = new Vector3(target.position.x, target.position.y + height, target.position.z);
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
