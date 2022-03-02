
using UdonSharp;

using UnityEditor;

using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace Psychloor.Udon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Any)]
    public class Jumppad : UdonSharpBehaviour
    {
        public float velocity = 20f;
        [Tooltip("If it should ignore the players current velocity or not")]
        public bool ignoreCurrentVelocity = false;

        [Header("Listeners - Local Event")]
        public UdonSharpBehaviour[] listeners;
        public string eventName = "OnLaunchpadTriggered";

        [Header("Animator Trigger")]
        public Animator animator;
        public string animatorTriggerName = "OnLaunchpadTriggered";
        [Tooltip("Make sure sync mode isn't set to none if synching. can use manual")]
        public bool synchronize = true;
        private int animatorTriggerHash;

        private void Start()
        {
            animatorTriggerHash = Animator.StringToHash(animatorTriggerName);
        }

        // If it's a collider
        public override void OnPlayerCollisionEnter(VRCPlayerApi player)
        {
            if (!player.isLocal) return;

            if (ignoreCurrentVelocity)
                player.SetVelocity(transform.up * velocity);
            else
                player.SetVelocity(transform.up * velocity + player.GetVelocity());

            foreach (var listener in listeners)
            {
                if (listener)
                    listener.SendCustomEvent(eventName);
            }

            if (animator)
            {
                if (synchronize) SendCustomNetworkEvent(NetworkEventTarget.All, nameof(TriggerAnimator));
                else animator.SetTrigger(animatorTriggerHash);
            }
        }

        // if it's a trigger
        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!player.isLocal) return;

            if (ignoreCurrentVelocity)
                player.SetVelocity(transform.up * velocity);
            else
                player.SetVelocity(transform.up * velocity + player.GetVelocity());

            foreach (var listener in listeners)
            {
                if (listener)
                    listener.SendCustomEvent(eventName);
            }

            if (animator)
            {
                if (synchronize) SendCustomNetworkEvent(NetworkEventTarget.All, nameof(TriggerAnimator));
                else animator.SetTrigger(animatorTriggerHash);
            }
        }

        public void TriggerAnimator()
        {
            animator.SetTrigger(animatorTriggerHash);
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Handles.color = Color.yellow;
            Handles.ArrowHandleCap(0, transform.position, transform.rotation * Quaternion.LookRotation(Vector3.up), 1f, EventType.Repaint);
        }
#endif
    }
}