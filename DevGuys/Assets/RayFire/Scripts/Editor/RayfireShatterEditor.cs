using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireShatter))]
    public class RayfireShatterEditor : Editor
    {
        RayfireShatter shatter;
        Vector3        centerWorldPos;
        Quaternion     centerWorldQuat;

        // Draw gizmo
        [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawGizmosSelected (RayfireShatter shatter, GizmoType gizmoType)
        {
            // Color preview
            if (shatter.colorPreview == true)
                ColorPreview (shatter);

            // Custom point cloud preview
            if (shatter.type == FragType.Custom)
            {
                if (shatter.custom.enable == true)
                {
                    Gizmos.color = Color.green;

                    // Get bounds for preview
                    Bounds bound = shatter.GetBound();
                    if (bound.size.magnitude > 0)
                    {
                        List<Vector3> pointCloud = RFCustom.GetCustomPointCLoud (shatter.custom, shatter.transform, shatter.advanced.seed, bound);
                        if (pointCloud.Count > 0)
                            for (int i = 0; i < pointCloud.Count; i++)
                                Gizmos.DrawSphere (pointCloud[i], shatter.custom.size);
                    }
                }
            }
        }

        // Show center move handle
        private void OnSceneGUI()
        {
            // Get shatter
            shatter = target as RayfireShatter;
            if (shatter == null)
                return;

            Transform transform = shatter.transform;
            centerWorldPos  = transform.TransformPoint (shatter.centerPosition);
            centerWorldQuat = transform.rotation * shatter.centerDirection;

            // Point3 handle
            if (shatter.showCenter == true)
            {
                EditorGUI.BeginChangeCheck();
                centerWorldPos = Handles.PositionHandle (centerWorldPos, centerWorldQuat.RFNormalize());
                if (EditorGUI.EndChangeCheck() == true)
                    Undo.RecordObject (shatter, "Center Move");

                EditorGUI.BeginChangeCheck();
                centerWorldQuat = Handles.RotationHandle (centerWorldQuat, centerWorldPos);
                if (EditorGUI.EndChangeCheck() == true)
                    Undo.RecordObject (shatter, "Center Rotate");
            }

            shatter.centerDirection = Quaternion.Inverse (transform.rotation) * centerWorldQuat;
            shatter.centerPosition  = transform.InverseTransformPoint (centerWorldPos);
        }

        // Inspector
        public override void OnInspectorGUI()
        {
            // Get shatter
            shatter = target as RayfireShatter;
            if (shatter == null)
                return;

            // Get inspector width
            // float width = EditorGUIUtility.currentViewWidth - 20f;

            // Space
            GUILayout.Space (8);

            FragmentUI();
            
            ColliderUI();

            PreviewUI();

            // Reset scale if fragments were deleted
            shatter.ResetScale (shatter.previewScale);

            GUILayout.Space (5);
            
            DrawDefaultInspector();

            GUILayout.Space (3);
            
            ExportUI ();

            GUILayout.Space (5);

            // Info
            InfoUI();

            // Center
            Center();
        }

        /// /////////////////////////////////////////////////////////
        /// Fragment / delete
        /// /////////////////////////////////////////////////////////

        // Fragment
        void FragmentUI()
        {
             GUILayout.Label ("  Fragment", EditorStyles.boldLabel);
            
            // Fragment 
            if (GUILayout.Button ("Fragment", GUILayout.Height (25)))
            {
                foreach (var targ in targets)
                    if (targ as RayfireShatter != null)
                    {
                        (targ as RayfireShatter).Fragment();

                        // TODO APPLY LOCAL SHATTER PREVIEW PROPS TO ALL SELECTED
                    }

                // Scale preview if preview turn on
                if (shatter.previewScale > 0 && shatter.scalePreview == true)
                    ScalePreview (shatter);
            }

            // Space
            GUILayout.Space (1);

            // Fragmentation section Begin
            GUILayout.BeginHorizontal();

            // Delete last
            if (shatter.fragmentsLast.Count > 0) // TODO SUPPORT MASS CHECK
            {
                if (GUILayout.Button ("Fragment to Last", GUILayout.Height (22)))
                {
                    foreach (var targ in targets)
                        if (targ as RayfireShatter != null)
                        {
                            (targ as RayfireShatter).DeleteFragmentsLast (1);
                            (targ as RayfireShatter).resetState = true;
                            (targ as RayfireShatter).Fragment (1);

                            // Scale preview if preview turn on
                            if ((targ as RayfireShatter).previewScale > 0 && (targ as RayfireShatter).scalePreview == true)
                                ScalePreview (targ as RayfireShatter);
                        }
                }

                if (GUILayout.Button ("    Delete Last    ", GUILayout.Height (22)))
                {
                    foreach (var targ in targets)
                        if (targ as RayfireShatter != null)
                        {
                            (targ as RayfireShatter).DeleteFragmentsLast();
                            (targ as RayfireShatter).resetState = true;
                            (targ as RayfireShatter).ResetScale (0f);
                        }
                }
            }

            // Delete all fragments
            if (shatter.fragmentsAll.Count > 0 && shatter.fragmentsAll.Count > shatter.fragmentsLast.Count)
            {
                if (GUILayout.Button (" Delete All ", GUILayout.Height (22)))
                {
                    foreach (var targ in targets)
                        if (targ as RayfireShatter != null)
                        {
                            (targ as RayfireShatter).DeleteFragmentsAll();
                            (targ as RayfireShatter).resetState = true;
                            (targ as RayfireShatter).ResetScale (0f);
                        }
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Info
        /// /////////////////////////////////////////////////////////

        // Info
        void InfoUI()
        {
            if (shatter.fragmentsLast.Count > 0 || shatter.fragmentsAll.Count > 0)
            {
                GUILayout.Label ("  Info", EditorStyles.boldLabel);

                GUILayout.BeginHorizontal();

                GUILayout.Label ("Roots: " + shatter.rootChildList.Count);
                GUILayout.Label ("Last Fragments: " + shatter.fragmentsLast.Count);
                GUILayout.Label ("Total Fragments: " + shatter.fragmentsAll.Count);

                EditorGUILayout.EndHorizontal();
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Export
        /// /////////////////////////////////////////////////////////

        // Export
        void ExportUI ()
        {
            // Export Last fragments
            if (shatter.export.source == RFMeshExport.MeshExportType.LastFragments && shatter.fragmentsLast.Count > 0)
                if (GUILayout.Button ("Export Last Fragments", GUILayout.Height (25)))
                    RFMeshAsset.SaveFragments (shatter, RFMeshAsset.shatterPath);

            // Export children
            if (shatter.export.source == RFMeshExport.MeshExportType.Children && shatter.transform.childCount > 0)
                if (GUILayout.Button ("Export Children", GUILayout.Height (25)))
                    RFMeshAsset.SaveFragments (shatter, RFMeshAsset.shatterPath);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Center
        /// /////////////////////////////////////////////////////////

        // Center
        void Center()
        {
            if ((int)shatter.type <= 5)
            {
                GUILayout.Label ("  Center", EditorStyles.boldLabel);

                GUILayout.BeginHorizontal();

                // Show center toggle
                shatter.showCenter = GUILayout.Toggle (shatter.showCenter, " Show   ", "Button");

                // Reset center
                if (GUILayout.Button ("Reset "))
                {
                    foreach (var targ in targets)
                        if (targ as RayfireShatter != null)
                            (targ as RayfireShatter).ResetCenter();
                    SceneView.RepaintAll();
                }

                EditorGUILayout.EndHorizontal();
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Preview
        /// /////////////////////////////////////////////////////////

        // Preview UI
        void PreviewUI()
        {
            // Preview
            if (shatter.fragmentsLast.Count == 0)
                return;
            
            GUILayout.Label ("  Preview", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            
            // Start check for scale toggle change
            EditorGUI.BeginChangeCheck();
            shatter.scalePreview = GUILayout.Toggle (shatter.scalePreview, "Scale", "Button");
            if (EditorGUI.EndChangeCheck() == true)
            {
                if (shatter.scalePreview == true)
                    ScalePreview (shatter);
                else
                {
                    shatter.resetState = true;
                    shatter.ResetScale (0f);
                }
            }
            
            // Color preview toggle
            shatter.colorPreview = GUILayout.Toggle (shatter.colorPreview, "Color", "Button");

            EditorGUILayout.EndHorizontal();

            GUILayout.Space (3);

            GUILayout.BeginHorizontal();

            GUILayout.Label ("Scale Preview", GUILayout.Width (90));

            // Start check for slider change
            EditorGUI.BeginChangeCheck();
            shatter.previewScale = GUILayout.HorizontalSlider (shatter.previewScale, 0f, 0.99f);
            if (EditorGUI.EndChangeCheck() == true)
                if (shatter.scalePreview == true)
                    ScalePreview (shatter);

            EditorGUILayout.EndHorizontal();
        }
        
        // Color preview
        static void ColorPreview (RayfireShatter scr)
        {
            if (scr.fragmentsLast.Count > 0)
            {
                Random.InitState (1);
                foreach (Transform root in scr.rootChildList)
                {
                    if (root != null)
                    {
                        MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>();
                        foreach (var mf in meshFilters)
                        {
                            Gizmos.color = new Color (Random.Range (0.2f, 0.8f), Random.Range (0.2f, 0.8f), Random.Range (0.2f, 0.8f));
                            Gizmos.DrawMesh (mf.sharedMesh, mf.transform.position, mf.transform.rotation, mf.transform.lossyScale * 1.01f);
                        }
                    }
                }
            }
        }

        // Scale fragments
        static void ScalePreview (RayfireShatter scr)
        {
            if (scr.fragmentsLast.Count > 0 && scr.previewScale > 0f)
            {
                // Do not scale
                if (scr.skinnedMeshRend != null)
                    scr.skinnedMeshRend.enabled = false;
                if (scr.meshRenderer != null)
                    scr.meshRenderer.enabled = false;

                foreach (GameObject fragment in scr.fragmentsLast)
                    if (fragment != null)
                        fragment.transform.localScale = Vector3.one * Mathf.Lerp (1f, 0.3f, scr.previewScale);
                scr.resetState = true;
            }

            if (scr.previewScale == 0f)
            {
                scr.ResetScale (0f);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Colliders
        /// /////////////////////////////////////////////////////////
        
        // Collider UI
        void ColliderUI()
        {
            if (shatter.fragmentsLast.Count == 0)
                return;
            
            GUILayout.Label ("  Colliders", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();

            // Add collider
            if (GUILayout.Button ("Add Mesh Colliders"))
            {
                foreach (var targ in targets)
                    if (targ as RayfireShatter != null)
                        AddColliders (targ as RayfireShatter);
                SceneView.RepaintAll();
            }
            
            // Remove collider
            if (GUILayout.Button (" Remove Colliders "))
            {
                foreach (var targ in targets)
                    if (targ as RayfireShatter != null)
                        RemoveColliders (targ as RayfireShatter);
                SceneView.RepaintAll();
            }

            EditorGUILayout.EndHorizontal();
        }
        
        // Add collider
        static void AddColliders (RayfireShatter scr)
        {
            if (scr.fragmentsLast.Count > 0)
            {
                foreach (var frag in scr.fragmentsLast)
                {
                    MeshCollider mc = frag.gameObject.GetComponent<MeshCollider>();
                    if (mc == null)
                        mc = frag.gameObject.AddComponent<MeshCollider>();
                    mc.convex = true;
                }
            }
        }
        
        // Remove collider
        static void RemoveColliders (RayfireShatter scr)
        {
            if (scr.fragmentsLast.Count > 0)
            {
                foreach (var frag in scr.fragmentsLast)
                {
                    MeshCollider mc = frag.gameObject.GetComponent<MeshCollider>();
                    if (mc != null)
                        DestroyImmediate (mc);
                }
            }
        }
    }
    
    // Normalize quat in order to support Unity 2018.1
    public static class RFQuaternionExtension
    {
        public static Quaternion RFNormalize (this Quaternion q)
        {
            float f = 1f / Mathf.Sqrt (q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
            return new Quaternion (q.x * f, q.y * f, q.z * f, q.w * f);
        }
    }
}

/*
public class ExampleClass: EditorWindow
{
    GameObject gameObject;
    Editor     gameObjectEditor;

    [MenuItem("Example/GameObject Editor")]
    static void ShowWindow()
    {
        GetWindowWithRect<ExampleClass>(new Rect(0, 0, 256, 256));
    }

    void OnGUI()
    {
        gameObject = (GameObject) EditorGUILayout.ObjectField(gameObject, typeof(GameObject), true);

        GUIStyle bgColor = new GUIStyle();
        bgColor.normal.background = EditorGUIUtility.whiteTexture;

        if (gameObject != null)
        {
            if (gameObjectEditor == null)
                gameObjectEditor = Editor.CreateEditor(gameObject);

            gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(256, 256), bgColor);
        }
    }
}


[CustomPreview(typeof(GameObject))]
public class MyPreview : ObjectPreview
{
    public override bool HasPreviewGUI()
    {
        return true;
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        GUI.Label(r, target.name + " is being previewed");
    }
}
*/