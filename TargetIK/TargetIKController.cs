namespace Psychloor.Udon
{

    using System;
    using System.Linq;

    using JetBrains.Annotations;

    using UdonSharp;

    using UdonToolkit;

    using UnityEditor;

    using UnityEngine;

    [CustomName("Target IK Controller"), OnAfterEditor("AfterEditor")]
    public class TargetIKController : UdonSharpBehaviour
    {

        [SectionHeader("Target")]
        public Transform targetTransform;
        [HelpBox("Which transform to use for aiming towards target")]
        public Transform aimTransform;

        [SectionHeader("Limits")]
        public int iterations = 1;
        [Range(0f, 1f)]
        public float weight = 1f;
        public float angleLimit = 90f;
        [HelpBox("Minimum distance before targeting")]
        public float distanceLimit = 1.5f;

        [Popup("GetHumanBodyBones"), ListView("Bones")]
        public int[] bone;
        [ListView("Bones")]
        public float[] boneWeight;

        private Animator animator;
        private Transform[] boneTransforms;

        public string[] GetHumanBodyBones()
        {
            return new string[]
                       {
                           "Hips", "LeftUpperLeg", "RightUpperLeg", "LeftLowerLeg", "RightLowerLeg", "LeftFoot",
                           "RightFoot", "Spine", "Chest", "Neck", "Head", "LeftShoulder", "RightShoulder",
                           "LeftUpperArm", "RightUpperArm", "LeftLowerArm", "RightLowerArm", "LeftHand", "RightHand",
                           "LeftToes", "RightToes", "LeftEye", "RightEye", "Jaw", "LeftThumbProximal",
                           "LeftThumbIntermediate", "LeftThumbDistal", "LeftIndexProximal", "LeftIndexIntermediate",
                           "LeftIndexDistal", "LeftMiddleProximal", "LeftMiddleIntermediate", "LeftMiddleDistal",
                           "LeftRingProximal", "LeftRingIntermediate", "LeftRingDistal", "LeftLittleProximal",
                           "LeftLittleIntermediate", "LeftLittleDistal", "RightThumbProximal", "RightThumbIntermediate",
                           "RightThumbDistal", "RightIndexProximal", "RightIndexIntermediate", "RightIndexDistal",
                           "RightMiddleProximal", "RightMiddleIntermediate", "RightMiddleDistal", "RightRingProximal",
                           "RightRingIntermediate", "RightRingDistal", "RightLittleProximal", "RightLittleIntermediate",
                           "RightLittleDistal", "UpperChest"
                       };
        }

        private void OnAnimatorIK(int layerIndex)
        {

        }

        public void LateUpdate()
        {
            if (aimTransform == null
                || targetTransform == null) return;

            Vector3 targetPosition = GetTargetPosition();
            for (int i = 0; i < iterations; i++)
            {
                for (int b = 0; b < boneTransforms.Length; b++)
                {
                    AimAtTarget(boneTransforms[b], targetPosition, boneWeight[b] * weight);
                }
            }
        }

        private Vector3 GetTargetPosition()
        {
            Vector3 targetDirection = targetTransform.position - aimTransform.position;
            Vector3 aimDirection = aimTransform.forward;
            float blendOut = 0f;

            float targetAngle = Vector3.Angle(targetDirection, aimDirection);
            if (targetAngle > angleLimit)
            {
                blendOut += (targetAngle - angleLimit) / 50f;
            }

            float targetDistance = targetDirection.magnitude;
            if (targetDistance < distanceLimit)
            {
                blendOut += distanceLimit - targetDistance;
            }

            Vector3 direction = Vector3.Slerp(targetDirection, aimDirection, blendOut);
            return aimTransform.position + direction;
        }

        private void AimAtTarget(Transform boneTransform, Vector3 targetPosition, float targetWeight)
        {
            Vector3 aimDirection = aimTransform.forward;
            Vector3 targetDirection = targetPosition - aimTransform.position;

            Quaternion aimTowards = Quaternion.FromToRotation(aimDirection, targetDirection);
            Quaternion blendedRotation = Quaternion.Slerp(Quaternion.identity, aimTowards, targetWeight);
            boneTransform.rotation = blendedRotation * boneTransform.rotation;
        }

        private void OnEnable()
        {
            /*animator = GetComponent<Animator>();
            boneTransforms = new Transform[bone.Length];
            foreach (int index in bone)
            {
                animator.GetBoneTransform((HumanBodyBones)index);
            }*/
        }

        private void Start()
        {
            /*animator = GetComponent<Animator>();
            boneTransforms = new Transform[bone.Length];
            foreach (int i in bone)
            {
                animator.GetBoneTransform((HumanBodyBones)i);
            }*/
        }
        
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        // ReSharper disable once UnusedMember.Global
        public void AfterEditor(SerializedObject obj)
        {
            var controller = (TargetIKController)obj.targetObject;
            controller.boneTransforms = new Transform[controller.bone.Length];

            animator = GetComponent<Animator>();
            boneTransforms = new Transform[bone.Length];
            for (int i = 0; i < boneTransforms.Length; i++)
            {
                boneTransforms[i] = animator.GetBoneTransform((HumanBodyBones)bone[i]);
            }
        }
#endif

    }

}