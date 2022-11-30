using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace RayFire
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [AddComponentMenu ("RayFire/Rayfire Rigid Root")]
    [HelpURL ("http://rayfirestudios.com/unity-online-help/unity-rigid-root-component/")]
    public class RayfireRigidRoot : MonoBehaviour
    {
        public enum InitType
        {
            ByMethod = 0,
            AtStart  = 1
        }
        
        [Space (2)]
        public InitType initialization = InitType.AtStart;
        
        [Header ("  Simulation")]
        [Space (3)]
        
        public SimType simulationType = SimType.Dynamic;
        [Space (2)]
        public RFPhysic     physics    = new RFPhysic();
        [Space (2)]
        public RFActivation activation = new RFActivation();
        [Space (2)]
        public RFFade       fading     = new RFFade();
        [Space (2)]
        public RFReset      reset      = new RFReset();
        
        
        [HideInInspector] public List<RFShard> inactiveShards;
        [HideInInspector] public RFCluster     cluster;
        
        // Hidden
        [HideInInspector] public Transform               tm;
        [HideInInspector] public RayfireConnectivity     connect;
        [HideInInspector] public List<RayfireDebris>     debrisList;
        [HideInInspector] public List<RayfireDust>       dustList;
        [HideInInspector] public List<RayfireUnyielding> unyList;
        
        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////
        
        void OnTransformChildrenChanged()
        {
            //childrenChanged = true; TODO copy from connectivity
        }
        
        // Awake
        void Awake()
        {
            if (initialization == InitType.AtStart)
            {
                AwakeMethods();
            }
            
            // TODO ACTIVATOR ActivationCheck fix for both types!!!!!!!!!!
            
            // TODO set sim state for fragments (field not serialized)
            
            // TODO init shards initPos at init even if setup 
            
            // TODO do not check for rigid UNy state, check for shard uny instead
        }
        
   
        /// /////////////////////////////////////////////////////////
        /// Setup
        /// /////////////////////////////////////////////////////////
        
        // Awake ops
        void AwakeMethods()
        {
            // Create RayFire manager if not created
            RayfireMan.RayFireManInit();
            
            // Set components
            SetComponents();
            
            // Set shards components
            SetShards();
            
            // Set components for mesh / skinned mesh / clusters
            SetPhysics();
            
            // Setup list for activation
            SetInactive ();
            
            // Set Particle Components: Initialize, collect
            SetParticles();

            // Set unyielding shards
            SetUnyielding();
            
            // Start all necessary coroutines
            StartAllCoroutines();
        }

        // Define basic components
        void SetComponents()
        {
            tm      = GetComponent<Transform>();
            connect = GetComponent<RayfireConnectivity>();
            unyList = GetComponents<RayfireUnyielding>().ToList();
        }
        
        // Set shards components
        void SetShards()
        { 
            // Get children
            List<Transform> children = new List<Transform>();
            for (int i = 0; i < tm.childCount; i++)
                children.Add (tm.GetChild (i));

            // Get rigid root sim state
            SimType simState = simulationType;
            
            // Set new cluster
            cluster = new RFCluster();
            for (int i = 0; i < children.Count; i++)
            {
                // Skip inactive children
                if (children[i].gameObject.activeSelf == false)
                    continue;
                
                // Check if already has rigid
                RayfireRigid rigid = children[i].gameObject.GetComponent<RayfireRigid>();
                
                // has own rigid
                if (rigid != null)
                {
                    // Init
                    rigid.Initialize();
                    
                    // Mesh
                    if (rigid.objectType == ObjectType.Mesh)
                    {
                        RFShard shard = new RFShard (children[i].transform, i);
                        shard.rigid = rigid;
                        shard.mf    = shard.rigid.meshFilter;
                        cluster.shards.Add (shard);
                    }
                    
                    // Mesh Root
                    if (rigid.objectType == ObjectType.MeshRoot)
                    {
                        if (rigid.fragments.Count > 0)
                        {
                            for (int j = 0; j < rigid.fragments.Count; j++)
                            {
                                RFShard shard = new RFShard (rigid.fragments[j].transform, i); // TODO Set if considering all shard ids
                                shard.rigid = rigid.fragments[j];
                                cluster.shards.Add (shard);
                            }
                        }
                    }
                    
                    // Connected Cluster TODO 
                    if (rigid.objectType == ObjectType.ConnectedCluster || rigid.objectType == ObjectType.NestedCluster)
                    {
                        RFShard shard = new RFShard (children[i].transform, i);
                        shard.rigid = rigid;
                        cluster.shards.Add (shard);
                    }
                }

                // Has no own rigid
                if (rigid == null)
                {
                    // Mesh
                    if (children[i].childCount == 0)
                    {
                        RFShard shard = new RFShard (children[i].transform, i);
                        shard.mf = children[i].transform.GetComponent<MeshFilter>();
                        shard.sm = simState;
                        
                        // Has mesh
                        if (shard.mf != null && shard.mf.sharedMesh != null)
                        {
                            shard.rb  = children[i].transform.GetComponent<Rigidbody>();
                            shard.col = children[i].transform.GetComponent<Collider>();
                            cluster.shards.Add (shard);
                        }
                    }

                    // Mesh Root TODO
                    else if (children[i].childCount > 0)
                    {
                        if (IsNestedCluster (children[i]) == true)
                        {
                             // Nested
                        }
                        else
                        {
                            // Connected
                        }
                    }
                }
            }
            
            
            
            // Set shards id TODO exclude all shards without meshfilter
            for (int j = 0; j < cluster.shards.Count; j++)
            {
                cluster.shards[j].id = j;
            }
        }

        // Define components
        void SetPhysics()
        {
            // Set density.
            float density     = RayfireMan.inst.materialPresets.Density (physics.materialType);
            float drag        = RayfireMan.inst.materialPresets.Drag (physics.materialType);
            float dragAngular = RayfireMan.inst.materialPresets.AngularDrag (physics.materialType);
            
            // Add Collider and Rigid body if has no Rigid component
            for (int i = 0; i < cluster.shards.Count; i++)
            {
                // Has no own rigid component: add collider and rigidbody
                if (cluster.shards[i].rigid == null)
                {
                    // Set mesh collier
                    if (cluster.shards[i].col == null && cluster.shards[i].mf != null)
                    {
                        MeshCollider col = cluster.shards[i].tm.gameObject.AddComponent<MeshCollider>();
                        col.sharedMesh        = cluster.shards[i].mf.sharedMesh;
                        col.convex            = true;
                        cluster.shards[i].col = col;
                    }

                    // Set Rigid body
                    if (cluster.shards[i].rb == null)
                        cluster.shards[i].rb = cluster.shards[i].tm.gameObject.AddComponent<Rigidbody>();
                    
                    // MeshCollider physic material preset. Set new or take from parent 
                    RFPhysic.SetColliderMaterial (this, cluster.shards[i]);
                    
                    // Set simulation
                    RFPhysic.SetSimulationType (cluster.shards[i].rb, simulationType, ObjectType.Mesh, physics.useGravity);
                    
                    // Set density. After collider defined
                    RFPhysic.SetDensity (this, cluster.shards[i], density);

                    // Set drag properties
                    RFPhysic.SetDrag (cluster.shards[i], drag, dragAngular);
                }
            }
            
            // Set debris collider material
            RFPhysic.SetParticleColliderMaterial (debrisList);
            
            // Set material solidity and destructible
            physics.solidity     = physics.Solidity;
            physics.destructible = physics.Destructible;
        }
        
        // Setup inactive shards
        void SetInactive()
        {
            if (simulationType == SimType.Inactive || simulationType == SimType.Kinematic)
            {
                inactiveShards = new List<RFShard>();
                for (int i = 0; i < cluster.shards.Count; i++)
                    inactiveShards.Add (cluster.shards[i]);
            }
        }
        
        // Set Particle Components: Initialize, collect
        void SetParticles()
        {
            // Get all Debris and initialize
            if (HasDebris == false)
            {
                RayfireDebris[] debrisArray = GetComponents<RayfireDebris>();
                if (debrisArray.Length > 0)
                {
                    for (int i = 0; i < debrisArray.Length; i++)
                        debrisArray[i].Initialize();

                    debrisList = new List<RayfireDebris>();
                    for (int i = 0; i < debrisArray.Length; i++)
                        if (debrisArray[i].initialized == true)
                        {
                            debrisList.Add (debrisArray[i]);
                        }
                }
            }

            // Get all Dust and initialize
            if (HasDust == false)
            {
                RayfireDust[] dustArray = GetComponents<RayfireDust>();
                if (dustArray.Length > 0)
                {
                    for (int i = 0; i < dustArray.Length; i++)
                        dustArray[i].Initialize();

                    dustList = new List<RayfireDust>();
                    for (int i = 0; i < dustArray.Length; i++)
                        if (dustArray[i].initialized == true)
                        {
                            dustList.Add (dustArray[i]);
                        }
                }
            }
        }

        // Set unyielding shards
        void SetUnyielding()
        {
            for (int i = 0; i < unyList.Count; i++)
                unyList[i].SetRigidRootUnyByOverlap (this);
        }
        
        // Start all coroutines
        void StartAllCoroutines()
        {
            // Stop if static
            if (simulationType == SimType.Static)
                return;
            
            // Inactive
            if (gameObject.activeSelf == false)
                return;
            
            // Prevent physics cors
            if (physics.exclude == true)
                return;
            
            // Init inactive every frame update coroutine
            if (simulationType == SimType.Inactive || simulationType == SimType.Kinematic)
                StartCoroutine (InactiveCor());
        }
        
        // Prepare shards. Set bounds, set neibs
        static void SetShardsByRigids(RFCluster cluster, List<RayfireRigid> rigidList, ConnectivityType connectivity)
        {
            for (int i = 0; i < rigidList.Count; i++)
            {
                // Get mesh filter
                MeshFilter mf = rigidList[i].GetComponent<MeshFilter>();

                // Child has no mesh
                if (mf == null)
                    continue;

                // Create new shard
                RFShard shard = new RFShard(rigidList[i].transform, i);
                shard.cluster = cluster;
                shard.rigid   = rigidList[i];
                shard.uny     = rigidList[i].activation.unyielding;
                shard.act     = rigidList[i].activation.activatable;
                shard.col     = rigidList[i].physics.meshCollider;

                // Set faces data for connectivity
                if (connectivity == ConnectivityType.ByMesh)
                    RFTriangle.SetTriangles(shard, mf);

                // Collect shard
                cluster.shards.Add(shard);
            }
        }

        // Get simulation state
        int SimState()
        {
            if (simulationType == SimType.Dynamic)
                return 1;
            if (simulationType == SimType.Sleeping)
                return 2;
            if (simulationType == SimType.Inactive)
                return 3;
            if (simulationType == SimType.Kinematic)
                return 4;
            if (simulationType == SimType.Static)
                return 5;
            return 1;
        }

        /// /////////////////////////////////////////////////////////
        /// Inactive
        /// /////////////////////////////////////////////////////////
        
        // Activation by velocity and offset
        IEnumerator InactiveCor ()
        {
            while (inactiveShards.Count > 0)
            {
                // Remove activated shards
                for (int i = inactiveShards.Count - 1; i >= 0; i--)
                    if (inactiveShards[i].sm == SimType.Dynamic)
                        inactiveShards.RemoveAt (i);

                // Velocity activation
                if (activation.byVelocity > 0)
                {
                    for (int i = inactiveShards.Count - 1; i >= 0; i--)
                    {
                        if (inactiveShards[i].tm.hasChanged == true)
                            if (inactiveShards[i].rb.velocity.magnitude > activation.byVelocity)
                            {
                                RFActivation.ActivateShard (inactiveShards[i], this);
                                inactiveShards.RemoveAt (i);
                            }
                    }

                    // Stop 
                    if (inactiveShards.Count == 0)
                        yield break;
                }

                // Offset activation
                if (activation.byOffset > 0)
                {
                    for (int i = inactiveShards.Count - 1; i >= 0; i--)
                    {
                        if (inactiveShards[i].tm.hasChanged == true)
                            if (Vector3.Distance (inactiveShards[i].tm.position, inactiveShards[i].pos) > activation.byOffset)
                            {
                                RFActivation.ActivateShard (inactiveShards[i], this);
                                inactiveShards.RemoveAt (i);
                            }
                    }

                    // Stop 
                    if (inactiveShards.Count == 0)
                        yield break;
                }
                
                // Stop velocity
                for (int i = inactiveShards.Count - 1; i >= 0; i--)
                {
                    if (inactiveShards[i].tm.hasChanged == true)
                    {
                        inactiveShards[i].rb.velocity        = Vector3.zero;
                        inactiveShards[i].rb.angularVelocity = Vector3.zero;
                        inactiveShards[i].tm.hasChanged      = false;
                    }
                }
                
                // TODO repeat 30 times per second, not every frame
                yield return null;
            }
        }

        // Activate shard by collider
        public void ActivateCollider (Collider coll)
        {
            for (int i = inactiveShards.Count - 1; i >= 0; i--)
            {
                if (inactiveShards[i].col == coll)
                {
                    RFActivation.ActivateShard (inactiveShards[i], this);
                    inactiveShards.RemoveAt (i);
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        // Copy rigid root properties to rigid
        public void CopyPropertiesTo (RayfireRigid toScr)
        {
            // Object type
            toScr.demolitionType = DemolitionType.None;
            toScr.objectType     = ObjectType.Mesh;
            toScr.simulationType = simulationType;
            toScr.simulationType = SimType.Dynamic;
            
            // Copy physics
            toScr.physics.CopyFrom (physics);
            toScr.activation.CopyFrom (activation);
            //toScr.limitations.CopyFrom (limitations);
            //toScr.meshDemolition.CopyFrom (meshDemolition);
            //toScr.clusterDemolition.CopyFrom (clusterDemolition);
            //toScr.materials.CopyFrom (materials);
            //toScr.damage.CopyFrom (damage);
            toScr.fading.CopyFrom (fading);
            //toScr.reset.CopyFrom (this);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        // Check if root is nested cluster
        static bool IsNestedCluster (Transform trans)
        {
            for (int c = 0; c < trans.childCount; c++)
                if (trans.GetChild (c).childCount > 0)
                    return true;
            return false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Getters
        /// /////////////////////////////////////////////////////////
        
        bool HasDebris { get { return debrisList != null && debrisList.Count > 0; } }
        bool HasDust { get { return dustList != null && dustList.Count > 0; } }
    }
}
