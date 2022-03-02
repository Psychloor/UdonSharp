using UdonSharp;
using UnityEngine;

using VRC.SDKBase;
using VRC.Udon;

namespace Psychloor.Udon
{
    public class LookAtPlayer : UdonSharpBehaviour
    {

        public float maxDistance = 5f;

        public float rotationSpeed = 0.02f;

        public float updatesPerSecond = 10f;

        [Tooltip("Max Angle side to side to limit itself to. Not total angle"), Range(0f, 180f)]
        public float maxSideAngle = 90f;

        [Tooltip("Main transform to use as forward vector/direction for the limiter. Will use itself if null and won't really limit")]
        public Transform mainForwardTransform;

        [Tooltip("Calculate idle forward rotation when not tracking. instead of using original before")]
        public bool autoForward = true;

        public bool trackOwner;

        [Tooltip("Track nearest player inside limit instead of only local player")]
        public bool trackNearestPlayer;

        [Tooltip("Max players in world before defaulting to local only to save some performance"), Range(1, 81)]
        public int nearestMaxPlayers = 20;

        private VRCPlayerApi localPlayer;
        private float lastUpdateTime;
        private float updateFrequency;
        private bool isLookingAtTarget;

        private VRCPlayerApi[] players = new VRCPlayerApi[81];

        private Quaternion originalRotation, targetRotation;

        private void Start()
        {
            updateFrequency = 1f / updatesPerSecond;
            originalRotation = transform.rotation;

            if(trackOwner)
            {
                localPlayer = Networking.GetOwner(gameObject);
            }
            else
            {
                localPlayer = Networking.LocalPlayer;
            }

            if (mainForwardTransform == null)
            {
                mainForwardTransform = transform;
            }
        }

        private void OnAnimatorIK(int layerIndex)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed);
        }

        private void LateUpdate()
        {
            if (Time.time - lastUpdateTime >= updateFrequency)
            {
                lastUpdateTime = Time.time;

                Vector3 targetPosition;
                if (!trackOwner && trackNearestPlayer && VRCPlayerApi.GetPlayerCount() <= nearestMaxPlayers)
                {
                    targetPosition = new Vector3(1000f, 1000f, 1000f);
                    for(int i = 0; i < VRCPlayerApi.GetPlayerCount(); i++)
                    {
                        if (!Utilities.IsValid(players[i])) continue;

                        var playerPosition = players[i].GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
                        if(Vector3.Distance(playerPosition, transform.position) < Vector3.Distance(targetPosition, transform.position))
                        {
                            Vector3 relativePosition = playerPosition - transform.position;
                            float dotProduct = Vector3.Dot(relativePosition.normalized, mainForwardTransform.forward);
                            float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

                            if(angle <= maxSideAngle)
                                targetPosition = playerPosition;
                        }
                    }

                    isLookingAtTarget = Vector3.Distance(targetPosition, transform.position) <= maxDistance;
                }
                else
                {
                    targetPosition = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
                    isLookingAtTarget = Vector3.Distance(targetPosition, transform.position) <= maxDistance;
                }

                if (isLookingAtTarget)
                {
                    Vector3 relativePosition = targetPosition - transform.position;
                    float dotProduct = Vector3.Dot(relativePosition.normalized, mainForwardTransform.forward);
                    float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

                    if (angle <= maxSideAngle)
                    {
                        targetRotation = Quaternion.LookRotation(relativePosition, Vector3.up);
                    }
                }
                else
                {
                    targetRotation = autoForward ? Quaternion.LookRotation(mainForwardTransform.forward, Vector3.up) : originalRotation;
                }
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed);
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            if (trackOwner)
                localPlayer = player;
        }

        public override void OnPlayerJoined(VRCPlayerApi dummy)
        {
            VRCPlayerApi.GetPlayers(players);
        }

        public override void OnPlayerLeft(VRCPlayerApi dummy)
        {
            for (int i = 0; i < 81; i++)
            {
                if (dummy == players[i])
                {
                    players[i] = null;
                }
            }
            RemoveDuplicatePlayers();
        }

        void RemoveDuplicatePlayers()
        {
            for (int i = 0; i < 81; i++)
            {
                if (players[i] != null)
                {
                    for (int j = i + 1; j < 81; j++)
                    {
                        if (players[i] == players[j])
                        {
                            players[j] = null;
                        }
                    }
                }
            }
        }

    }
}