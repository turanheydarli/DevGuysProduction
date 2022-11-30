using UnityEngine;
using UnityEditor;
using System.Collections;
using RootMotion.Dynamics;
using UnityEditor.SceneManagement;

namespace RootMotion.Demos {
	
	[CustomEditor(typeof(CharacterMeleeDemo))]
	public class CharacterMeleeDemoInspector : Editor {
		
		private CharacterMeleeDemo script { get { return target as CharacterMeleeDemo; }}
		
		private GameObject replace;

		private static Color pro = new Color(0.7f, 0.9f, 0.5f, 1f);
		private static Color free = new Color(0.4f, 0.5f, 0.3f, 1f);
		
		public override void OnInspectorGUI() {
			GUI.changed = false;

			if (!Application.isPlaying) {
				GUI.color = EditorGUIUtility.isProSkin? pro: free;
				EditorGUILayout.BeginHorizontal();
				
				replace = (GameObject)EditorGUILayout.ObjectField("Replace Character Model", replace, typeof(GameObject), true);
				
				if (replace != null) {
					if (GUILayout.Button("Replace")) {
                        // Find Prop Muscle
                        bool hasPropMuscle = script.propMuscle != null;
                        PropMuscle propMuscle = script.propMuscle;
                        Vector3 propMusclePosition = Vector3.zero;
                        Quaternion propMuscleRotation = Quaternion.identity;
                        Vector3 additionalPinOffset = Vector3.zero;

                        if (hasPropMuscle)
                        {
                            var cJ = propMuscle.GetComponent<ConfigurableJoint>().connectedBody.transform;
                            propMusclePosition = cJ.InverseTransformPoint(propMuscle.transform.position);
                            propMuscleRotation = Quaternion.Inverse(cJ.rotation) * propMuscle.transform.rotation;
                            additionalPinOffset = propMuscle.additionalPinOffset;
                        }

                        // Run the rest of the puppet replacement code
						CharacterPuppetInspector.ReplacePuppetModel(script as CharacterThirdPerson, replace);

                        // Prop Muscle again
                        if (hasPropMuscle)
                        {
                            Animator animator = script.characterAnimation.GetComponent<Animator>();
                            PuppetMaster puppetMaster = script.transform.parent.GetComponentInChildren<PuppetMaster>();
                            var rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                            var connectToJoint = GetJoint(puppetMaster, animator, HumanBodyBones.RightLowerArm);

                            PuppetMasterInspector.AddPropMuscle(puppetMaster, connectToJoint, connectToJoint.transform.TransformPoint(propMusclePosition), connectToJoint.transform.rotation * propMuscleRotation, additionalPinOffset, rightHand);
                            script.propMuscle = puppetMaster.muscles[puppetMaster.muscles.Length - 1].joint.GetComponent<PropMuscle>();

                            Debug.LogWarning("If bone orientations of the new and old models mismatch, PropMuscle position and rotation needs to be adjusted manually. This can be done by selecting the PropMuscle GameObject and moving/rotating it.");
                            Selection.activeGameObject = script.propMuscle.gameObject;
                        }

                        UserControlAI[] userControls = (UserControlAI[])GameObject.FindObjectsOfType<UserControlAI>();
						foreach (UserControlAI ai in userControls) {
							if (ai.moveTarget == null) {
								ai.moveTarget = script.transform.parent.GetComponentInChildren<PuppetMaster>().muscles[0].joint.transform;
                                EditorUtility.SetDirty(ai);
							}
						}

                        // Mark dirty (so changes could be saved)
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        EditorUtility.SetDirty(script);
                    }
				}
				
				EditorGUILayout.EndHorizontal();
				GUI.color = Color.white;
			}
			
			DrawDefaultInspector();

			if (GUI.changed) EditorUtility.SetDirty(script);
		}

        private static ConfigurableJoint GetJoint(PuppetMaster puppetMaster, Animator animator, HumanBodyBones bone)
        {
            var boneTransform = animator.GetBoneTransform(bone);
            foreach (Muscle m in puppetMaster.muscles)
            {
                if (m.target == boneTransform) return m.joint;
            }
            return null;
        }

		private Rigidbody GetRigidbody(PuppetMaster puppetMaster, Transform target) {
			foreach (Muscle m in puppetMaster.muscles) {
				if (m.target == target) return m.joint.GetComponent<Rigidbody>();
			}
			return null;
		}
	}
}
