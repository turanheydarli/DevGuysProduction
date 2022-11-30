using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;
using Random = UnityEngine.Random;

namespace RayFire
{
    [Serializable]
    public class RFFade
    {
        // Fade life Type
        public enum RFFadeLifeType
        {
            ByLifeTime = 4,
            BySimulationAndLifeTime = 8
        }

        [Header ("  Initiate")]
        [Space (2)]
        
        public bool onDemolition;
        public bool onActivation;

        [Header ("  Life")]
        [Space (2)]

        public RFFadeLifeType lifeType;
        [Range (0f, 90f)] public float lifeTime;
        [Range (0f, 20f)] public float lifeVariation;
        
        [Header("  Fade")]
        [Space(2)]
        
        public FadeType fadeType;
        [Range (1f, 20f)] public float fadeTime;
        [Range (0f, 20f)] public float sizeFilter;
        
        [NonSerialized] public int state;
        [NonSerialized] public bool stop;
        [NonSerialized] public Vector3 position;
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RFFade()
        {
            onDemolition = true;
            onActivation = false;
            
            lifeType      = RFFadeLifeType.ByLifeTime;
            lifeTime      = 7f;
            lifeVariation = 3f;
                        
            fadeType      = FadeType.None;
            fadeTime      = 5f;
            sizeFilter    = 0f;
            
            Reset();
        }

        // Copy from
        public void CopyFrom (RFFade fade)
        {
            onDemolition  = fade.onDemolition;
            onActivation  = fade.onActivation;
            
            lifeType      = fade.lifeType;
            lifeTime      = fade.lifeTime;
            lifeVariation = fade.lifeVariation;
            
            fadeType      = fade.fadeType;
            fadeTime      = fade.fadeTime;
            sizeFilter    = fade.sizeFilter;
            
            Reset();
        }
        
        // Reset
        public void Reset()
        {
            state = 0;
            stop  = false;
        }

        /// /////////////////////////////////////////////////////////
        /// Fade for demolished fragments
        /// /////////////////////////////////////////////////////////

        // Fading init from parent node
        public void DemolitionFade (List<RayfireRigid> fadeObjects)
        {
            // No fading
            if (fadeType == FadeType.None)
                return;

            // No objects
            if (fadeObjects.Count == 0)
                return;

            // Life time fix
            if (lifeTime < 1f)
                lifeTime = 1f;

            // Add Fade script and init fading
            for (int i = 0; i < fadeObjects.Count; i++)
            {
                // Check for null
                if (fadeObjects[i] == null)
                    continue;
                
                // Size check
                if (sizeFilter > 0 && fadeObjects[i].limitations.bboxSize > sizeFilter)
                    continue;
                
                // Init fading
                Fade (fadeObjects[i]);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Object Fade init
        /// /////////////////////////////////////////////////////////
        
        // Fading init for Rigid object
        public static void Fade (RayfireRigid scr)
        {
            // Initialize if not
            if (scr.initialized == false)
                scr.Initialize();
            
            // Check if fading allowed
            if (FadeCheck (scr.gameObject, scr.fading) == false)
                return;
            
            // Start life coroutine
            scr.StartCoroutine (scr.fading.LivingCor (scr));
        }
        
        // Fading init for Shard object
        public static void Fade (RayfireRigidRoot scr, RFShard shard)
        {
            // No fading
            if (scr.fading.fadeType == FadeType.None)
                return;

            
            // Start life coroutine
            scr.StartCoroutine (scr.fading.LivingCor (scr, shard));
        }

        // Check if fading allowed
        static bool FadeCheck(GameObject go, RFFade fading)
        {
            // No fading
            if (fading.fadeType == FadeType.None)
                return false;
            
            // Object inactive, Skip
            if (go.activeSelf == false)
                return false;
                       
            // Object living, fading or faded
            if (fading.state > 0)
                return false;
            
            return true;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Living
        /// /////////////////////////////////////////////////////////

        // Start life coroutine
        IEnumerator LivingCor (RayfireRigid scr)
        {
            // Wait for simulation get rest
            if (scr.fading.lifeType == RFFadeLifeType.BySimulationAndLifeTime)
                yield return scr.StartCoroutine(SimulationCor (scr.transForm));
            
            // Set living
            scr.fading.state = 1;
            
            // Get final life duration
            float lifeDuration = scr.fading.lifeTime;
            if (scr.fading.lifeVariation > 0)
                lifeDuration += Random.Range (0f, scr.fading.lifeVariation);
            
            // Wait life time
            if (lifeDuration > 0)
                yield return new WaitForSeconds (lifeDuration);

            // Stop fading
            if (stop == true)
            {
                scr.fading.Reset();
                yield break;
            }
            
            // Set fading
            scr.fading.state = 2;

            // TODO MAKE RESETABLE
            // scr.reset.action = RFReset.PostDemolitionType.DestroyWithDelay;
            
            // Exclude from simulation and keep object in scene
            if (scr.fading.fadeType == FadeType.SimExclude)
                FadeExclude (scr);

            // Exclude from simulation, move under ground, destroy
            else if (scr.fading.fadeType == FadeType.MoveDown)
                scr.StartCoroutine (FadeMoveDown (scr));

            // Start scale down and destroy
            else if (scr.fading.fadeType == FadeType.ScaleDown)
                scr.StartCoroutine (FadeScaleDownCor (scr));

            // Destroy object
            else if (scr.fading.fadeType == FadeType.Destroy)
                RayfireMan.DestroyFragment (scr, scr.rootParent);
        }
        
        // Start life coroutine
        IEnumerator LivingCor (RayfireRigidRoot root, RFShard shard)
        {
            // Wait for simulation get rest
            if (root.fading.lifeType == RFFadeLifeType.BySimulationAndLifeTime)
                yield return root.StartCoroutine(SimulationCor (shard.tm));

            // Get final life duration
            float lifeDuration = root.fading.lifeTime;
            if (root.fading.lifeVariation > 0)
                lifeDuration += Random.Range (0f, root.fading.lifeVariation);
            
            // Wait life time
            if (lifeDuration > 0)
                yield return new WaitForSeconds (lifeDuration);

            // Exclude from simulation and keep object in scene
            if (root.fading.fadeType == FadeType.SimExclude)
                FadeExclude (root, shard);
            
            // Exclude from simulation, move under ground, destroy
            else if (root.fading.fadeType == FadeType.MoveDown)
                root.StartCoroutine (FadeMoveDown (root, shard));
            
            // Start scale down and destroy
            else if (root.fading.fadeType == FadeType.ScaleDown)
                root.StartCoroutine (FadeScaleDownCor (root, shard));

            // Destroy object // TODO destroy via DestroyShard ()
            else if (root.fading.fadeType == FadeType.Destroy)
                RayfireMan.DestroyGo (shard.tm.gameObject);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Exclude Fade
        /// /////////////////////////////////////////////////////////
        
        // Exclude from simulation and keep object in scene
        static void FadeExclude (RayfireRigid rigid)
        {
            // Set faded
            rigid.fading.state = 2;

            // Not going to be reused
            if (rigid.reset.action == RFReset.PostDemolitionType.DestroyWithDelay)
            {
                UnityEngine.Object.Destroy (rigid.physics.rigidBody);
                UnityEngine.Object.Destroy (rigid.physics.meshCollider);
                UnityEngine.Object.Destroy (rigid);
            }

            // Going to be reused 
            else if (rigid.reset.action == RFReset.PostDemolitionType.DeactivateToReset)
            {
                rigid.physics.rigidBody.isKinematic = true;
                rigid.physics.meshCollider.enabled = false; // TODO CHECK CLUSTER COLLIDERS
                rigid.StopAllCoroutines();
            }
        }
        
        // Exclude from simulation and keep object in scene
        static void FadeExclude (RayfireRigidRoot root, RFShard shard)
        {
            // Not going to be reused
            if (root.reset.action == RFReset.PostDemolitionType.DestroyWithDelay)
            {
                UnityEngine.Object.Destroy (shard.rb);
                UnityEngine.Object.Destroy (shard.col);
            }

            // Going to be reused 
            else if (root.reset.action == RFReset.PostDemolitionType.DeactivateToReset)
            {
                shard.rb.isKinematic = true;
                shard.col.enabled    = false;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Move Down fade
        /// /////////////////////////////////////////////////////////
        
        // Exclude from simulation, move under ground, destroy
        static IEnumerator FadeMoveDown (RayfireRigid rigid)
        {
            // Activate inactive
            if (rigid.simulationType == SimType.Inactive)
                rigid.Activate();

            // Wake up if sleeping
            rigid.physics.rigidBody.WakeUp();
            
            // Turn off collider
            if (rigid.objectType == ObjectType.Mesh)
            {
                if (rigid.physics.meshCollider != null)
                    rigid.physics.meshCollider.enabled = false;
            }
            else if (rigid.objectType == ObjectType.ConnectedCluster || rigid.objectType == ObjectType.NestedCluster)
            {
                if (rigid.physics.clusterColliders != null)
                    for (int i = 0; i < rigid.physics.clusterColliders.Count; i++)
                        rigid.physics.clusterColliders[i].enabled = false;
            }
            
            // Wait to fall down
            yield return new WaitForSeconds (rigid.fading.fadeTime);
            
            // Check if fragment is the last child in root and delete root as well
            RayfireMan.DestroyFragment (rigid, rigid.rootParent);
        }
        
        // Exclude from simulation, move under ground, destroy
        static IEnumerator FadeMoveDown (RayfireRigidRoot root, RFShard shard)
        {
            // Activate inactive
            if (shard.sm == SimType.Inactive)
                RFActivation.ActivateShard (shard, root);

            // Wake up if sleeping
            shard.rb.WakeUp();
            
            // Turn off collider
            if (shard.col != null)
                shard.col.enabled = false;

            // Wait to fall down
            yield return new WaitForSeconds (root.fading.fadeTime);
            
            // TODO destroy via DestroyShard ()
            RayfireMan.DestroyGo (shard.tm.gameObject);
        }
                
        /// /////////////////////////////////////////////////////////
        /// Scale Down
        /// /////////////////////////////////////////////////////////
        
        // Exclude from simulation, move under ground, destroy
        static IEnumerator FadeScaleDownCor (RayfireRigid scr)
        {
            // Scale object down during fade time
            float   waitStep   = 0.04f;
            int     steps      = (int)(scr.fading.fadeTime / waitStep);
            Vector3 vectorStep = scr.transForm.localScale / steps;
            
            // Repeat
            while (steps > 0)
            {
                steps--;
                
                // Scale down
                scr.transForm.localScale -= vectorStep;
                
                // Wait
                yield return new WaitForSeconds (waitStep);

                // Destroy when too small
                if (steps < 4)
                {
                    RayfireMan.DestroyFragment (scr, scr.rootParent);
                }
            }
        }
        
        // Exclude from simulation, move under ground, destroy
        static IEnumerator FadeScaleDownCor (RayfireRigidRoot root, RFShard shard)
        {
            // Scale object down during fade time
            float   waitStep   = 0.04f;
            int     steps      = (int)(root.fading.fadeTime / waitStep);
            Vector3 vectorStep = shard.tm.localScale / steps;
            
            // Repeat
            while (steps > 0)
            {
                if (shard.tm == null)
                    break;

                steps--;
                
                // Scale down
                shard.tm.localScale -= vectorStep;
                
                // Wait
                yield return new WaitForSeconds (waitStep);

                // Destroy when too small // TODO destroy via DestroyShard ()
                if (steps < 4)
                {
                    RayfireMan.DestroyGo (shard.tm.gameObject);
                    yield break;
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Simulation
        /// /////////////////////////////////////////////////////////

        // Check for simulation state
        static IEnumerator SimulationCor (Transform tm)
        {
            float   timeStep          = Random.Range (2.5f, 3.5f);
            Vector3 oldPos            = tm.position;
            float   distanceThreshold = 0.15f;
            while (true)
            {
                // Save position
                oldPos = tm.position;;
                
                // Wait step time
                yield return new WaitForSeconds (timeStep);
                
                float dist = Vector3.Distance (tm.position, oldPos);           
                if (dist < distanceThreshold)
                    break;
            }
        }
    }
}