using UnityEngine;
using System.Collections;
using System;

namespace RootMotion.Dynamics
{
    public partial class PuppetMaster : MonoBehaviour
    {
#if UNITY_EDITOR

        [ContextMenu("Save Settings As Humanoid Config")]
        private void SaveToHumanoidConfig()
        {
            var path = "Assets/Saved Humanoid Config.asset";

            if (targetRoot == null)
            {
                Debug.LogWarning("Please assign 'Target Root' for PuppetMaster using a Humanoid Config.", transform);
                return;
            }

            if (targetAnimator == null)
            {
                Debug.LogError("PuppetMaster 'Target Root' does not have an Animator component. Can not use Humanoid Config.", transform);
                return;
            }

            if (!targetAnimator.isHuman)
            {
                Debug.LogError("PuppetMaster target is not a Humanoid. Can not use Humanoid Config.", transform);
                return;
            }

            var p = ScriptableObject.CreateInstance<PuppetMasterHumanoidConfig>();

            p.state = state;
            p.stateSettings = stateSettings;
            p.mode = mode;
            p.blendTime = blendTime;
            p.fixTargetTransforms = fixTargetTransforms;
            p.solverIterationCount = solverIterationCount;
            p.visualizeTargetPose = visualizeTargetPose;
            p.mappingWeight = mappingWeight;
            p.pinWeight = pinWeight;
            p.muscleWeight = muscleWeight;
            p.muscleSpring = muscleSpring;
            p.muscleDamper = muscleDamper;
            p.pinPow = pinPow;
            p.pinDistanceFalloff = pinDistanceFalloff;
            p.angularPinning = angularPinning;
            p.updateJointAnchors = updateJointAnchors;
            p.supportTranslationAnimation = supportTranslationAnimation;
            p.angularLimits = angularLimits;
            p.internalCollisions = internalCollisions;

            p.muscles = new PuppetMasterHumanoidConfig.HumanoidMuscle[muscles.Length];
            var allHumanBodyBones = (HumanBodyBones[])System.Enum.GetValues(typeof(HumanBodyBones));

            for (int i = 0; i < muscles.Length; i++)
            {
                var m = muscles[i];
                var h = new PuppetMasterHumanoidConfig.HumanoidMuscle();
                h.props = new Muscle.Props();
                
                h.bone = GetHumanBodyBone(m.target, allHumanBodyBones);
                h.props.group = m.props.group;
                h.props.mappingWeight = m.props.mappingWeight;
                h.props.muscleDamper = m.props.muscleDamper;
                h.props.muscleWeight = m.props.muscleWeight;
                h.props.pinWeight = m.props.pinWeight;

                p.muscles[i] = h;
            }

            UnityEditor.AssetDatabase.CreateAsset(p, path);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            var saved = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(PuppetMasterHumanoidConfig)) as PuppetMasterHumanoidConfig;
            if (saved != null)
            {
                Debug.Log("PuppetMasterHumanoidConfig successfully created at " + path);
                UnityEditor.Selection.activeObject = saved;
            }
        }

        private HumanBodyBones GetHumanBodyBone(Transform t, HumanBodyBones[] allBones)
        {
            for (int i = 0; i < allBones.Length - 1; i++)
            {
                var bone = allBones[i];
                if (targetAnimator.GetBoneTransform(bone) == t) return bone;
            }
            //Debug.LogError("Unable to find HumanBodyBone of Transform " + t.name, transform);
            return HumanBodyBones.LastBone;
        }
#endif
    }

    [CreateAssetMenu(fileName = "PuppetMaster Humanoid Config", menuName = "PuppetMaster/Humanoid Config", order = 1)]
    public class PuppetMasterHumanoidConfig : ScriptableObject
    {

        [System.Serializable]
        public class HumanoidMuscle
        {
            [HideInInspector] public string name;
            public HumanBodyBones bone;
            public Muscle.Props props;
        }

        [LargeHeader("Simulation")]

        public PuppetMaster.State state;
        public PuppetMaster.StateSettings stateSettings = PuppetMaster.StateSettings.Default;
        public PuppetMaster.Mode mode;
        public float blendTime = 0.1f;
        public bool fixTargetTransforms = true;
        public int solverIterationCount = 6;
        public bool visualizeTargetPose = true;

        [LargeHeader("Master Weights")]

        [Range(0f, 1f)] public float mappingWeight = 1f;
        [Range(0f, 1f)] public float pinWeight = 1f;
        [Range(0f, 1f)] public float muscleWeight = 1f;

        [LargeHeader("Joint and Muscle Settings")]

        public float muscleSpring = 100f;
        public float muscleDamper = 0f;
        [Range(1f, 8f)] public float pinPow = 4f;
        [Range(0f, 100f)] public float pinDistanceFalloff = 5;
        public bool angularPinning;
        public bool updateJointAnchors = true;
        public bool supportTranslationAnimation;
        public bool angularLimits;
        public bool internalCollisions;

        [LargeHeader("Individual Muscle Settings")]

        public HumanoidMuscle[] muscles = new HumanoidMuscle[0];

        /// <summary>
        /// Applies this config to the specified PuppetMaster.
        /// </summary>
        /// <param name="p">P.</param>
        public void ApplyTo(PuppetMaster p)
        {
            if (p.targetRoot == null)
            {
                Debug.LogWarning("Please assign 'Target Root' for PuppetMaster using a Humanoid Config.", p.transform);
                return;
            }

            if (p.targetAnimator == null)
            {
                Debug.LogError("PuppetMaster 'Target Root' does not have an Animator component. Can not use Humanoid Config.", p.transform);
                return;
            }

            if (!p.targetAnimator.isHuman)
            {
                Debug.LogError("PuppetMaster target is not a Humanoid. Can not use Humanoid Config.", p.transform);
                return;
            }

            p.state = state;
            p.stateSettings = stateSettings;
            p.mode = mode;
            p.blendTime = blendTime;
            p.fixTargetTransforms = fixTargetTransforms;
            p.solverIterationCount = solverIterationCount;
            p.visualizeTargetPose = visualizeTargetPose;
            p.mappingWeight = mappingWeight;
            p.pinWeight = pinWeight;
            p.muscleWeight = muscleWeight;
            p.muscleSpring = muscleSpring;
            p.muscleDamper = muscleDamper;
            p.pinPow = pinPow;
            p.pinDistanceFalloff = pinDistanceFalloff;
            p.angularPinning = angularPinning;
            p.updateJointAnchors = updateJointAnchors;
            p.supportTranslationAnimation = supportTranslationAnimation;
            p.angularLimits = angularLimits;
            p.internalCollisions = internalCollisions;

            for (int i = 0; i < muscles.Length; i++)
            {
                var m = GetMuscle(muscles[i].bone, p.targetAnimator, p);
                if (m == null && i < p.muscles.Length)
                {
                    m = p.muscles[i];
                }

                if (m != null)
                {
                    var h = muscles[i];
                    m.props.group = h.props.group;
                    m.props.mappingWeight = h.props.mappingWeight;
                    //m.props.mapPosition = h.props.mapPosition;
                    m.props.muscleDamper = h.props.muscleDamper;
                    m.props.muscleWeight = h.props.muscleWeight;
                    m.props.pinWeight = h.props.pinWeight;
                }
            }
        }

        private Muscle GetMuscle(HumanBodyBones boneId, Animator animator, PuppetMaster puppetMaster)
        {
            if (boneId == HumanBodyBones.LastBone) return null;

            Transform bone = animator.GetBoneTransform(boneId);
            if (bone == null) return null;

            foreach (Muscle m in puppetMaster.muscles)
            {
                if (m.target == bone) return m;
            }

            return null;
        }
    }
}