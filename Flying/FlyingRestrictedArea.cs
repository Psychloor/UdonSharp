
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Psychloor.Udon
{

    using Psychloor.Udon.Flying;

    public class FlyingRestrictedArea : UdonSharpBehaviour
    {
        public FlyingController flyingController;

        public bool enterAllowedFlying = false, exitAllowedFlying = true;

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            if (flyingController == null) return;
            flyingController.IsFlyingAllowed = enterAllowedFlying;
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            if (flyingController == null) return;
            flyingController.IsFlyingAllowed = exitAllowedFlying;
        }
    }
}