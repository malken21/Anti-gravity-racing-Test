using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class RideHideHitbox : UdonSharpBehaviour
{
    [SerializeField]
    private Collider hitbox;

    public override void OnStationEntered(VRCPlayerApi player)
    {
        if (player.isLocal)
            hitbox.enabled = false;
    }

    public override void OnStationExited(VRCPlayerApi player)
    {
        if (player.isLocal)
            hitbox.enabled = true;
    }
}
