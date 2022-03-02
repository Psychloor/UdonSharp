namespace Psychloor.Udon.Flying
{

    using UdonSharp;

    using UnityEngine;

    using VRC.SDKBase;
    using VRC.Udon.Common;

    public class FlyingController : UdonSharpBehaviour
    {
        // @formatter:off
        [SerializeField, FieldChangeCallback(nameof(IsFlyingAllowed))]
        private bool isFlyingAllowed;
        public KeyCode desktopUpKey = KeyCode.E, desktopDownKey = KeyCode.Q;
        public float flyingSpeed = 10f;

        [Tooltip("Max time between jumps to count as double")]
        public float jumpTimeThreshold = .5f;

        [Header("Listeners")]
        public UdonSharpBehaviour[] listeners;
        public string onFlyingDisabledName = "OnFlyingDisabled";
        public string onFlyingEnabledName = "OnFlyingEnabled";

        private float currentSpeed;

        private float forwardInput, sideInput, verticalInput;

        private Vector3 forwardVector, sideVector, verticalVector;

        private bool isFlying;

        private bool isInVR;

        private float lastTimeJumped = 200f;

        private VRCPlayerApi localPlayer;

        private float previousGravityStrength;
        // @formatter:on
        public bool IsFlyingAllowed
        {
            get => isFlyingAllowed;
            set
            {
                if (value && isFlying) ToggleFlying();
                isFlyingAllowed = value;
            }
        }

        public override void InputJump(bool value, UdonInputEventArgs _)
        {
            if (value)
            {
                // Double jump to toggle flying
                if (Time.time - lastTimeJumped <= jumpTimeThreshold)
                {
                    ToggleFlying();
                }
                else
                {
                    lastTimeJumped = Time.time;
                }
            }
        }

        public override void InputLookVertical(float value, UdonInputEventArgs _)
        {
            if (isInVR) verticalInput = value;
        }

        public override void InputMoveHorizontal(float value, UdonInputEventArgs _)
        {
            sideInput = value;
        }

        public override void InputMoveVertical(float value, UdonInputEventArgs _)
        {
            forwardInput = value;
        }

        private void OnDisable()
        {
            if (isFlying) ToggleFlying();
        }

        private void Start()
        {
            if (Networking.LocalPlayer != null)
                isInVR = Networking.LocalPlayer.IsUserInVR();
        }

        private void ToggleFlying()
        {
            localPlayer = Networking.LocalPlayer;
            if (!isFlying) previousGravityStrength = localPlayer.GetGravityStrength();

            isFlying = !isFlying && IsFlyingAllowed;

            localPlayer.SetGravityStrength(isFlying ? 0f : previousGravityStrength);
            for (var i = 0; i < listeners.Length; i++)
            {
                if (listeners[i])
                    listeners[i].SendCustomEvent(isFlying ? onFlyingEnabledName : onFlyingDisabledName);
            }
        }

        private void Update()
        {
            if (!isFlying) return;
            currentSpeed = flyingSpeed;

            if (!isInVR)
            {
                verticalInput = 0;
                if (Input.GetKey(desktopUpKey)) ++verticalInput;
                if (Input.GetKey(desktopDownKey)) --verticalInput;
                if (Input.GetKey(KeyCode.LeftShift)) currentSpeed *= 2f;
            }

            VRCPlayerApi.TrackingData viewPointData = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            forwardVector = viewPointData.rotation * Vector3.forward * (forwardInput * currentSpeed);
            sideVector = viewPointData.rotation * Vector3.right * (sideInput * currentSpeed);
            verticalVector = Vector3.up * (verticalInput * currentSpeed);

            localPlayer.SetVelocity(forwardVector + sideVector + verticalVector);
        }

    }

}