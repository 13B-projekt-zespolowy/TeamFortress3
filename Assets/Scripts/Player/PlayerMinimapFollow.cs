using PurrNet;

public class PlayerMinimapFollow : NetworkBehaviour
{
    private void Start()
    {
        if (isOwner)
        {
            var minimapFollow = FindAnyObjectByType<MinimapFollow>();
            minimapFollow.target = transform;
        }
    }
}
