using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace RayFire
{
    [System.Serializable]
    public class RFReferenceDemolition
    {
        // Mass Type
        public enum ActionType
        {
            Instantiate    = 0,
            SetActive       = 1
        }
        
        [Header ("  Source")]
        [Space (3)]
        
        public GameObject reference;
        [Space (2)]
        public List<GameObject> randomList;
        
        [Header ("  Properties")]
        [Space (3)]
        
        
        public ActionType action;
        [Space (2)]
        
        [Tooltip ("Add RayFire Rigid component to reference with mesh")]
        public bool addRigid;
        [Space (2)]
        public bool inheritScale;
        [Space (2)]
        public bool inheritMaterials;
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
        
        // Constructor
        public RFReferenceDemolition()
        {
            reference        = null;
            addRigid         = true;
            inheritScale     = true;
            inheritMaterials = false;
        }

        // Copy from
        public void CopyFrom (RFReferenceDemolition referenceDemolitionDml)
        {
            reference    = referenceDemolitionDml.reference;
            if (referenceDemolitionDml.randomList != null && referenceDemolitionDml.randomList.Count > 0)
            {
                if (randomList == null)
                    randomList = new List<GameObject>();
                randomList = referenceDemolitionDml.randomList;
            }
            addRigid         = referenceDemolitionDml.addRigid;
            inheritScale     = referenceDemolitionDml.inheritScale;
            inheritMaterials = referenceDemolitionDml.inheritMaterials;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////   
        
        // Get reference
        public GameObject GetReference()
        {
            // Return reference if action type is SetActive
            if (action == ActionType.SetActive)
            {
                // Reference not defined or destroyed
                if (reference == null)
                    return null;
                
                // Reference is prefab asset
                if (reference.scene.rootCount == 0)
                    return null;
                
                return reference;
            }

            // Return single ref
            if (reference != null && randomList.Count == 0)
                return reference;
            
            // Get random ref
            List<GameObject> refs = new List<GameObject>();
            if (randomList.Count > 0)
            {
                for (int i = 0; i < randomList.Count; i++)
                    if (randomList[i] != null)
                        refs.Add (randomList[i]);
                if (refs.Count > 0)
                    return refs[Random.Range (0, refs.Count)];
            }

            return null;
        }
        
        // Demolish object to reference
        public static bool DemolishReference (RayfireRigid scr)
        {
            if (scr.demolitionType == DemolitionType.ReferenceDemolition)
            {
                // Demolished
                scr.limitations.demolished = true;
                
                // Turn off original
                scr.gameObject.SetActive (false);
                
                // Get reference
                GameObject refGo = scr.referenceDemolition.GetReference();
                
                // Has no reference
                if (refGo == null)
                    return true;
                
                // Set object to swap
                GameObject instGo = GetInstance (scr, refGo);

                // Set root to manager or to the same parent
                RayfireMan.SetParent (instGo.transform, scr.transForm);
                
                // Set tm
                scr.rootChild = instGo.transform;
                
                // Copy scale
                if (scr.referenceDemolition.inheritScale == true)
                    scr.rootChild.localScale = scr.transForm.localScale;

                // Inherit materials
                InheritMaterials (scr, instGo);

                // Clear list for fragments
                scr.fragments = new List<RayfireRigid>();
                
                // Check root for rigid props
                RayfireRigid refScr = instGo.gameObject.GetComponent<RayfireRigid>();

                // Reference Root has not rigid. Add to
                if (refScr == null && scr.referenceDemolition.addRigid == true)
                {
                    // Add rigid and copy
                    refScr = instGo.gameObject.AddComponent<RayfireRigid>();

                    // Copy rigid
                    scr.CopyPropertiesTo (refScr);

                    // Copy particles
                    RFParticles.CopyParticles (scr, refScr);   
                    
                    // Single mesh TODO improve
                    if (instGo.transform.childCount == 0)
                    {
                        refScr.objectType = ObjectType.Mesh;
                    }

                    // Multiple meshes
                    if (instGo.transform.childCount > 0)
                    {
                        refScr.objectType = ObjectType.MeshRoot;
                    }
                }

                // Activate and init rigid
                instGo.transform.gameObject.SetActive (true);

                // Reference has rigid
                if (refScr != null)
                {
                    // Init if not initialized yet
                    refScr.Initialize();
                    
                    // Create rigid for root children
                    if (refScr.objectType == ObjectType.MeshRoot)
                    {
                        for (int i = 0; i < refScr.fragments.Count; i++)
                            refScr.fragments[i].limitations.currentDepth++;
                        scr.fragments.AddRange (refScr.fragments);
                        scr.DestroyRigid (refScr);
                    }

                    // Get ref rigid
                    else if (refScr.objectType == ObjectType.Mesh ||
                             refScr.objectType == ObjectType.SkinnedMesh)
                    {
                        refScr.meshDemolition.runtimeCaching.type = CachingType.Disable;
                        RFDemolitionMesh.DemolishMesh(refScr);
                        
                        // TODO COPY MESH DATA FROM ROOTSCR TO THIS TO REUSE
                        
                        scr.fragments.AddRange (refScr.fragments);
                        
                        
                        RayfireMan.DestroyFragment (refScr, refScr.rootParent, 1f);
                    }

                    // Get ref rigid
                    else if (refScr.objectType == ObjectType.NestedCluster ||
                             refScr.objectType == ObjectType.ConnectedCluster)
                    {
                        refScr.Default();
                        
                        // Copy contact data
                        refScr.limitations.contactPoint   = scr.limitations.contactPoint;
                        refScr.limitations.contactVector3 = scr.limitations.contactVector3;
                        refScr.limitations.contactNormal  = scr.limitations.contactNormal;
                        
                        // Demolish
                        RFDemolitionCluster.DemolishCluster (refScr);
                        
                        // Collect new fragments
                        scr.fragments.AddRange (refScr.fragments);
                        
                        
                        //refScr.physics.exclude = true;
                        //RayfireMan.DestroyFragment (refScr, refScr.rootParent, 1f);
                    }
                }
            }

            return true;
        }

        // Get final instance accordingly to action type
        static GameObject GetInstance (RayfireRigid scr, GameObject refGo)
        {
            GameObject instGo;
            if (scr.referenceDemolition.action == ActionType.Instantiate)
            {
                // Instantiate turned off reference with null parent
                instGo = Object.Instantiate (refGo, scr.transForm.position, scr.transForm.rotation);
                instGo.name = refGo.name;
            }
                
            // Set active
            else
            {
                instGo = refGo;
                instGo.transform.position = scr.transform.position;
                instGo.transform.rotation = scr.transform.rotation;
            }
            return instGo;
        } 
    
    
        // Inherit materials from original object to referenced fragments
        static void InheritMaterials (RayfireRigid scr, GameObject instGo)
        {
            if (scr.referenceDemolition.inheritMaterials == true)
            {
                Renderer[] renderers = instGo.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                    for (int r = 0; r < renderers.Length; r++)
                    {
                        int min = Math.Min (scr.meshRenderer.materials.Length, renderers[r].materials.Length);
                        for (int m = 0; m < min; m++)
                            renderers[r].materials[m] = scr.meshRenderer.materials[m];
                    }
            }
        }
    }
}