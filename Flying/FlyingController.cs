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
        private bool isFlyingAllowed = true;
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
        private Vector3 inputVector = Vector3.zero;
        private Vector3 movementVector = Vector3.zero, verticalVector = Vector3.zero, previousPlayerPosition = Vector3.zero;

        private bool isFlying;

        private bool isInVR;

        private float lastTimeJumped = -5000f;
        private bool previousJumpInput;

        private VRCPlayerApi localPlayer;
        // @formatter:on
        public bool IsFlyingAllowed
        {
            get => isFlyingAllowed;
            set
            {
                isFlyingAllowed = value;
                if (value && isFlying) ToggleFlying();
            }
        }

        public override void InputJump(bool value, UdonInputEventArgs _)
        {
            if (value && !previousJumpInput)
            {
                // Double jump to toggle flying
                if (Time.time - lastTimeJumped <= jumpTimeThreshold)
                {
                    lastTimeJumped = -5000f;
                    ToggleFlying();
                }
                else
                {
                    lastTimeJumped = Time.time;
                }
            }

            previousJumpInput = value;
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

        private void OnEnable()
        {
            if (Networking.LocalPlayer == null) return;
            isInVR = Networking.LocalPlayer.IsUserInVR();
            if (isFlying) ToggleFlying();
            lastTimeJumped = -5000f;
        }

        private void OnDisable()
        {
            if (isFlying)
            {
                isFlying = false;
                if (listeners != null
                    && listeners.Length > 0)
                    for (var i = 0; i < listeners.Length; i++)
                    {
                        if (listeners[i])
                            listeners[i].SendCustomEvent(isFlying ? onFlyingEnabledName : onFlyingDisabledName);
                    }
            }

            lastTimeJumped = -5000f;
        }

        private void Start()
        {
            if (Networking.LocalPlayer == null) return;
            isInVR = Networking.LocalPlayer.IsUserInVR();
        }

        private void ToggleFlying()
        {
            localPlayer = Networking.LocalPlayer;

            isFlying = !isFlying && IsFlyingAllowed;

            if (listeners != null
                && listeners.Length > 0)
                for (var i = 0; i < listeners.Length; i++)
                {
                    if (listeners[i])
                        listeners[i].SendCustomEvent(isFlying ? onFlyingEnabledName : onFlyingDisabledName);
                }

            previousPlayerPosition = localPlayer.GetPosition();
        }

        private void FixedUpdate()
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

            // Set bugs out so manual it is?
            inputVector.x = sideInput;
            inputVector.z = forwardInput;

            movementVector = viewPointData.rotation * inputVector.normalized
                                                    * (Mathf.InverseLerp(.05f, 1f, inputVector.magnitude)
                                                       * currentSpeed);

            verticalVector = Vector3.up * (verticalInput * currentSpeed);
            localPlayer.SetVelocity(
                movementVector + verticalVector
                               + (-Physics.gravity * (localPlayer.GetGravityStrength() * Time.fixedDeltaTime)));

            previousPlayerPosition = localPlayer.GetPosition();
        }

    }

}