using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireConnectivity))]
    public class RayfireConnectivityEditor : Editor
    {
        RayfireConnectivity conn;
        static Color        wireColor   = new Color (0.58f, 0.77f, 1f);
        static Color        stressColor = Color.green;

        // Draw gizmo
        [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawGizmosSelected (RayfireConnectivity targ, GizmoType gizmoType)
        {
            // Missing shards
            if (RFCluster.IntegrityCheck (targ.cluster) == false)
                Debug.Log ("RayFire Connectivity: " + targ.name + " has missing shards. Reset or Setup cluster.", targ.gameObject);

            ClusterDraw (targ);

            StressDraw (targ);

            GizmoDraw (targ);
        }

        static void GizmoDraw (RayfireConnectivity targ)
        {
            if (targ.showGizmo == true)
            {
                // Gizmo properties
                Gizmos.color = wireColor;
                if (targ.transform.childCount > 0)
                {
                    Bounds bound = RFCluster.GetChildrenBound (targ.transform);
                    Gizmos.DrawWireCube (bound.center, bound.size);
                }
            }
        }

        // Inspector
        public override void OnInspectorGUI()
        {
            // Get target
            conn = target as RayfireConnectivity;
            if (conn == null)
                return;

            GUILayout.Space (8);

            ClusterSetupUI();

            ClusterPreviewUI();

            ClusterCollapseUI();

            GUILayout.Space (3);

            if (conn.cluster.shards.Count > 0)
            {
                GUILayout.Label ("    Cluster Shards: " + conn.cluster.shards.Count + "/" + conn.initShardAmount);
                GUILayout.Label ("    Amount Integrity: " + conn.AmountIntegrity + "%");
            }

            DrawDefaultInspector();
        }

        void ClusterSetupUI()
        {
            GUILayout.Label ("  Cluster", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button ("Setup Cluster", GUILayout.Height (25)))
            {
                if (Application.isPlaying == false)
                    foreach (var targ in targets)
                        if (targ as RayfireConnectivity != null)
                        {
                            ResetCluster (targ as RayfireConnectivity);
                            (targ as RayfireConnectivity).SetConnectivity();
                            SetDirty (targ as RayfireConnectivity);
                        }

                SceneView.RepaintAll();
            }

            if (GUILayout.Button ("Reset Cluster", GUILayout.Height (25)))
            {
                if (Application.isPlaying == false)
                    foreach (var targ in targets)
                        if (targ as RayfireConnectivity != null)
                        {
                            ResetCluster (targ as RayfireConnectivity);
                            SetDirty (targ as RayfireConnectivity);
                        }

                SceneView.RepaintAll();
            }

            if (Application.isPlaying == true)
                if (GUILayout.Button ("Reset Shards", GUILayout.Height (25)))
                {
                    foreach (var targ in targets)
                        if (targ as RayfireConnectivity != null)
                        {
                            (targ as RayfireConnectivity).ResetShards();
                            SetDirty (targ as RayfireConnectivity);
                        }

                    SceneView.RepaintAll();
                }


            EditorGUILayout.EndHorizontal();
        }

        void ResetCluster(RayfireConnectivity scr)
        {
            scr.cluster            = new RFCluster();
            scr.stress.strShards   = new List<RFShard>();
            scr.stress.supShards   = new List<RFShard>();
            scr.stress.initialized = false;
        }

        void ClusterCollapseUI()
        {
            GUILayout.Label ("  Collapse", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();

            GUILayout.Label ("By Area:", GUILayout.Width (55));

            // Start check for slider change
            EditorGUI.BeginChangeCheck();
            conn.cluster.areaCollapse = EditorGUILayout.Slider(conn.cluster.areaCollapse, conn.cluster.minimumArea, conn.cluster.maximumArea);
            if (EditorGUI.EndChangeCheck() == true)
                if (Application.isPlaying)
                    RFCollapse.AreaCollapse (conn, conn.cluster.areaCollapse);;

            EditorGUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Label ("By Size:", GUILayout.Width (55));

            // Start check for slider change
            EditorGUI.BeginChangeCheck();
            conn.cluster.sizeCollapse = EditorGUILayout.Slider(conn.cluster.sizeCollapse, conn.cluster.minimumSize, conn.cluster.maximumSize);
            if (EditorGUI.EndChangeCheck() == true)
                if (Application.isPlaying)
                    RFCollapse.SizeCollapse (conn, conn.cluster.sizeCollapse);;

            EditorGUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Label ("Random:", GUILayout.Width (55));

            // Start check for slider change
            EditorGUI.BeginChangeCheck();
            conn.cluster.randomCollapse = EditorGUILayout.IntSlider(conn.cluster.randomCollapse, 0, 100);
            if (EditorGUI.EndChangeCheck() == true)
                if (Application.isPlaying)
                    RFCollapse.RandomCollapse (conn, conn.cluster.randomCollapse, conn.seed);;
            
            EditorGUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();

            if (Application.isPlaying)
            {
                // Start/Stop collapse
                if (conn.collapse.inProgress == false)
                {
                    if (GUILayout.Button ("Start Collapse", GUILayout.Height (25)))
                        foreach (var targ in targets)
                            if (targ as RayfireConnectivity != null)
                                RFCollapse.StartCollapse (targ as RayfireConnectivity);
                }
                else
                {
                    if (GUILayout.Button ("Stop Collapse", GUILayout.Height (25)))
                        foreach (var targ in targets)
                            if (targ as RayfireConnectivity != null)
                                RFCollapse.StopCollapse (targ as RayfireConnectivity);
                }

                // Start/Stop Stress
                if (conn.stress.inProgress == false)
                {
                    if (GUILayout.Button ("Start Stress ", GUILayout.Height (25)))
                        foreach (var targ in targets)
                            if (targ as RayfireConnectivity != null)
                                RFStress.StartStress (targ as RayfireConnectivity);
                }
                else
                {
                    if (GUILayout.Button ("Stop Stress", GUILayout.Height (25)))
                        foreach (var targ in targets)
                            if (targ as RayfireConnectivity != null)
                                RFStress.StopStress (targ as RayfireConnectivity);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        void ClusterPreviewUI()
        {
            // Show center toggle
            EditorGUI.BeginChangeCheck();
            
            conn.showGizmo = GUILayout.Toggle (conn.showGizmo, " Show Gizmo ", "Button", GUILayout.Height (22));
            
            GUILayout.BeginHorizontal();

            // Show nodes
            conn.showConnections = GUILayout.Toggle (conn.showConnections, "Show Connections",    "Button", GUILayout.Height (22));
            conn.showNodes       = GUILayout.Toggle (conn.showNodes,       "Show Nodes", "Button", GUILayout.Height (22));
            conn.showStress      = GUILayout.Toggle (conn.showStress,      "Show Stress","Button", GUILayout.Height (22));
            
            if (EditorGUI.EndChangeCheck())
            { 
                foreach (var targ in targets)
                    if (targ as RayfireConnectivity != null)
                    {
                        (targ as RayfireConnectivity).showConnections = conn.showConnections;
                        (targ as RayfireConnectivity).showNodes = conn.showNodes;
                        SetDirty (targ as RayfireConnectivity);
                    }
                SceneView.RepaintAll();
            }

            EditorGUILayout.EndHorizontal();
        }

        static void ClusterDraw(RayfireConnectivity targ)
        {
            if (targ.showNodes == true || targ.showConnections == true)
            {
                if (targ.cluster != null && targ.cluster.shards.Count > 0)
                {
                    for (int i = 0; i < targ.cluster.shards.Count; i++)
                    {
                        if (targ.cluster.shards[i].tm != null)
                        {
                            // Color
                            if (targ.cluster.shards[i].rigid == null)
                            {
                                if (targ.cluster.shards[i].uny == false)
                                    Gizmos.color = Color.green;
                                else
                                    Gizmos.color = targ.cluster.shards[i].act == true ? Color.magenta : Color.red;
                            }
                            else
                            {
                                if (targ.cluster.shards[i].rigid.activation.unyielding == false)
                                    Gizmos.color = Color.green;
                                else
                                    Gizmos.color = targ.cluster.shards[i].rigid.activation.activatable == true ? Color.magenta : Color.red;
                            }

                            // Nodes
                            if (targ.showNodes == true)
                                Gizmos.DrawWireSphere (targ.cluster.shards[i].tm.position, targ.cluster.shards[i].sz / 12f);
                            
                            // Connection
                            if (targ.showConnections == true)
                            {
                                // has no neibs
                                if (targ.cluster.shards[i].nIds.Count == 0)
                                    continue;
                                
                                // Shard has neibs but neib shards not initialized by nIds
                                if (targ.cluster.shards[i].neibShards == null)
                                    targ.cluster.shards[i].neibShards = new List<RFShard>();
                                
                                // Reinit
                                if (targ.cluster.shards[i].neibShards.Count == 0)
                                    for (int n = 0; n < targ.cluster.shards[i].nIds.Count; n++)
                                        targ.cluster.shards[i].neibShards.Add (targ.cluster.shards[targ.cluster.shards[i].nIds[n]]);
                                
                                // Preview
                                for (int j = 0; j < targ.cluster.shards[i].neibShards.Count; j++)
                                    if (targ.cluster.shards[i].neibShards[j].tm != null)
                                    {
                                        Gizmos.DrawLine (targ.cluster.shards[i].tm.position, 
                                            (targ.cluster.shards[i].neibShards[j].tm.position - targ.cluster.shards[i].tm.position) / 2f + targ.cluster.shards[i].tm.position);
                                        //Gizmos.DrawLine (targ.cluster.shards[i].tm.position, targ.cluster.shards[i].neibShards[j].tm.position);
                                    }
                            }
                        }
                    }
                }
            }
        }

        static void StressDraw (RayfireConnectivity targ)
        {
            if (targ.showStress == true && targ.stress != null && targ.stress.inProgress == true)
            {
                if (targ.cluster != null && targ.cluster.shards.Count > 0)
                {
                    for (int i = 0; i < targ.cluster.shards.Count; i++)
                    {
                        if (targ.cluster.shards[i].tm != null)
                        {
                            // Show Path stress
                            if (false)
                                if (targ.stress.bySize == true)
                                {
                                    Gizmos.color = ColorByValue (stressColor, targ.cluster.shards[i].sSt, 1f);
                                    Gizmos.DrawWireSphere (targ.cluster.shards[i].tm.position, targ.cluster.shards[i].sz / 12f);
                                }

                            if (targ.cluster.shards[i].StressState == true)
                            {
                                for (int n = 0; n < targ.cluster.shards[i].nSt.Count / 3; n++)
                                {
                                    if (targ.cluster.shards[i].uny == true)
                                    {
                                        Gizmos.color = Color.yellow;
                                    }
                                    else
                                    {
                                        if (targ.cluster.shards[i].sIds.Count > 0)
                                        {
                                            //Gizmos.color = Color.blue;
                                            //if (targ.cluster.shards[i].neibShards[n].sIds.Contains (targ.cluster.shards[i].id) == true || targ.cluster.shards[i].sIds.Contains (targ.cluster.shards[i].neibShards[n].id) == true)
                                                Gizmos.color = Color.yellow;
                                        }
                                        else
                                            Gizmos.color = ColorByValue (stressColor, targ.cluster.shards[i].nSt[n * 3], targ.stress.threshold);
                                    }
                                    
                                    Vector3 pos = (targ.cluster.shards[i].neibShards[n].tm.position - targ.cluster.shards[i].tm.position) / 2.5f + targ.cluster.shards[i].tm.position;
                                    Gizmos.DrawLine (targ.cluster.shards[i].tm.position, pos);
                                }
                            }
                        }
                    }
                }
            }
        }

        static Color ColorByValue(Color color, float val, float threshold)
        {
            val     /= threshold;
            color.g =  1f - val;
            color.r =  val;
            return color;
        }
        
        // Set dirty
        void SetDirty (RayfireConnectivity scr)
        {
            if (Application.isPlaying == false)
            {
                EditorUtility.SetDirty (scr);
                EditorSceneManager.MarkSceneDirty (scr.gameObject.scene);
            }
        }
    }
}


/*
 if (targ.cluster.shards[i].uny == true)
    {
        Gizmos.color = Color.yellow;
    }
    else
    {
        //if (targ.cluster.shards[i].sIds.Count > 0)
        //{
            if (targ.cluster.shards[i].neibShards[n].sIds.Contains (targ.cluster.shards[i].id) == true || targ.cluster.shards[i].sIds.Contains (targ.cluster.shards[i].neibShards[n].id) == true)
            {
                Gizmos.color = Color.yellow;
            }
        //}
            else
                Gizmos.color     = ColorByValue (stressColor, targ.cluster.shards[i].nStr[n * 3], targ.stress.threshold);
    }




                                    if (targ.cluster.shards[i].uny == true || targ.cluster.shards[i].sIds.Count > 0)
                                        Gizmos.color = Color.yellow;
                                    else
                                        Gizmos.color = ColorByValue (stressColor, targ.cluster.shards[i].nStr[n*3], targ.stress.threshold);

*/