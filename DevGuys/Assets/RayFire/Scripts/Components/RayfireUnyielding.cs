using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RayFire
{
    [AddComponentMenu ("RayFire/Rayfire Unyielding")]
    [HelpURL ("http://rayfirestudios.com/unity-online-help/unity-unyielding-component/")]
    public class RayfireUnyielding : MonoBehaviour
    {
        [Header ("  Properties")]
        [Space (3)]
        
        [Tooltip ("Set Unyielding property for children Rigids.")]
        public bool unyielding = true;
        [Space (2)]
        
        [Tooltip ("Set Activatable property for children Rigids.")]
        public bool activatable = false;
        
        [Header ("  Gizmo")]
        [Space (3)]
        
        [Tooltip ("Unyielding gizmo center.")]
        public Vector3 centerPosition;
        [Space (2)]
        
        [Tooltip ("Unyielding gizmo size.")]
        public Vector3 size = new Vector3(1f,1f,1f);
        
        // Hidden
        [HideInInspector] public RayfireConnectivity connHost;
        [HideInInspector] public RayfireRigid        rigidHost;
        [HideInInspector] public List<RayfireRigid>  rigidList;
        [HideInInspector] public List<RFShard>       shardList;
        [HideInInspector] public bool                showGizmo = true;
        [HideInInspector] public bool                showCenter;
        [HideInInspector] public int                 id;
        
        /// /////////////////////////////////////////////////////////
        /// Cluster setup
        /// /////////////////////////////////////////////////////////
        
        // Set clusterized rigids uny state and mesh root rigids
        public static void ClusterSetup (RayfireRigid rigid)
        {
            if (rigid.simulationType == SimType.Inactive || rigid.simulationType == SimType.Kinematic)
            {
                RayfireUnyielding[] unyArray =  rigid.GetComponents<RayfireUnyielding>();
                for (int i = 0; i < unyArray.Length; i++)
                    if (unyArray[i].enabled == true)
                    {
                        unyArray[i].rigidHost = rigid;
                        ClusterOverlap (unyArray[i]);
                    }
            }
        }
        
        // Set uny state for mesh root rigids. Used by Mesh Root. Can be used for cluster shards
        static void ClusterOverlap (RayfireUnyielding uny)
        {
            // Get target mask and overlap colliders
            int        finalMask = ClusterLayerMask(uny.rigidHost);
            Collider[] colliders = Physics.OverlapBox (uny.transform.TransformPoint (uny.centerPosition), uny.Extents, uny.transform.rotation, finalMask);
            
            // Check with connected cluster
            uny.shardList = new List<RFShard>();
            if (uny.rigidHost.objectType == ObjectType.ConnectedCluster)
                for (int i = 0; i < uny.rigidHost.physics.clusterColliders.Count; i++)
                    if (uny.rigidHost.physics.clusterColliders[i] != null)
                        if (colliders.Contains (uny.rigidHost.physics.clusterColliders[i]) == true)
                        {
                            SetShardUnyState (uny.rigidHost.clusterDemolition.cluster.shards[i], uny.unyielding, uny.activatable);
                            uny.shardList.Add (uny.rigidHost.clusterDemolition.cluster.shards[i]);
                        }
        }
        
        // Get combined layer mask
        public static int ClusterLayerMask(RayfireRigid rigid)
        {
            int mask = 0;
            if (rigid.objectType == ObjectType.ConnectedCluster)
                for (int i = 0; i < rigid.physics.clusterColliders.Count; i++)
                    if (rigid.physics.clusterColliders[i] != null)
                        mask = mask | 1 << rigid.clusterDemolition.cluster.shards[i].tm.gameObject.layer;
            return mask;
        }
        
        // Set unyielding state
        static void SetShardUnyState (RFShard shard, bool unyielding, bool activatable)
        {
            shard.uny = unyielding;
            shard.act = activatable;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Connectivity setup
        /// /////////////////////////////////////////////////////////
        
        // Set clusterized rigids uny state and mesh root rigids
        public static void ConnectivitySetup (RayfireConnectivity connectivity)
        {
            RayfireUnyielding[] unyArray =  connectivity.GetComponents<RayfireUnyielding>();
            for (int i = 0; i < unyArray.Length; i++)
                if (unyArray[i].enabled == true)
                {
                    unyArray[i].connHost = connectivity;
                    ConnectivityOverlap (unyArray[i]);
                }
        }
        
        // Set uny state for mesh root rigids. Used by Mesh Root. Can be used for cluster shards
        static void ConnectivityOverlap(RayfireUnyielding uny)
        {
            // Get target mask
            int        finalMask = ConnectivityLayerMask(uny.connHost);
            Collider[] colliders = Physics.OverlapBox (uny.transform.TransformPoint (uny.centerPosition), uny.Extents, uny.transform.rotation, finalMask);

            // Check with connectivity rigids
            uny.rigidList = new List<RayfireRigid>();
            for (int i = 0; i < uny.connHost.rigidList.Count; i++)
                if (uny.connHost.rigidList[i].physics.meshCollider != null)
                    if (colliders.Contains (uny.connHost.rigidList[i].physics.meshCollider) == true)
                    {
                        SetRigidUnyState (uny.connHost.rigidList[i], uny.id, uny.unyielding, uny.activatable);
                        uny.rigidList.Add (uny.connHost.rigidList[i]);
                    }
        }
        
        // Get combined layer mask
        static int ConnectivityLayerMask(RayfireConnectivity connectivity)
        {
            int mask = 0;
            for (int i = 0; i < connectivity.rigidList.Count; i++)
                if (connectivity.rigidList[i].physics.meshCollider != null)
                    mask = mask | 1 << connectivity.rigidList[i].gameObject.layer;
            return mask;
        }

        // Set unyielding state
        public static void SetRigidUnyState (RayfireRigid rigid, int unyId, bool unyielding, bool activatable)
        {
            rigid.activation.unyielding  = unyielding;
            rigid.activation.activatable = activatable;
            
            // Set uny id
            if (rigid.activation.unyList == null)
                rigid.activation.unyList = new List<int>();

            rigid.activation.unyList.Add (unyId);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Rigid Root Setup
        /// /////////////////////////////////////////////////////////
        
        // Set uny state for mesh root rigids. Used by Mesh Root. Can be used for cluster shards
        public void SetRigidRootUnyByOverlap(RayfireRigidRoot rigidRoot)
        {
            if (enabled == false)
                return;

            // Get target mask TODO check fragments layer
            int mask = 0;
            
            // Check with rigid root shards colliders
            for (int i = 0; i < rigidRoot.cluster.shards.Count; i++)
                if (rigidRoot.cluster.shards[i].col != null)
                    mask = mask | 1 << rigidRoot.cluster.shards[i].tm.gameObject.layer;
                            
            // Get box overlap colliders
            Collider[] colliders = Physics.OverlapBox (transform.TransformPoint (centerPosition), Extents, transform.rotation, mask);
            
            // Check with rigid root shards colliders
            for (int i = 0; i < rigidRoot.cluster.shards.Count; i++)
                if (rigidRoot.cluster.shards[i].col != null)
                    if (colliders.Contains (rigidRoot.cluster.shards[i].col) == true)
                    {
                        rigidRoot.cluster.shards[i].uny = true;
                    }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Activate
        /// /////////////////////////////////////////////////////////
        
        // Activate inactive\kinematic shards/fragments
        public void Activate()
        {
            // Activate all rigids, init connectivity check after last activation, nullify connectivity for every
            if (HasRigids == true)
            {
                for (int i = 0; i < rigidList.Count; i++)
                {
                    // Activate if activatable
                    if (rigidList[i].activation.activatable == true)
                    {
                        rigidList[i].Activate (i == rigidList.Count - 1);
                        rigidList[i].activation.connect = null;
                    }
                }
            }

            // Activate connected clusters shards
            if (HasShards == true)
            {
                // Collect shards colliders
                List<Collider> colliders = new List<Collider>();
                for (int i = 0; i < shardList.Count; i++)
                    if (shardList[i].col != null)
                        colliders.Add (shardList[i].col);

                // No colliders
                if (colliders.Count == 0)
                    return;
                
                // Get Unyielding shards
                List<RFShard> shards = RFDemolitionCluster.DemolishConnectedCluster (rigidHost, colliders.ToArray());

                // Activate
                if (shards != null && shards.Count > 0)
                    for (int i = 0; i < shards.Count; i++)
                        RFActivation.ActivateShard (shards[i], null);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Manager register
        /// /////////////////////////////////////////////////////////
        
        // Register in manager
        void Register()
        {
            // TODO prevent double registering
            
            RFUny uny = new RFUny();
            uny.id       = GetUnyId();
            uny.scr      = this;
            uny.size     = Extents;

            uny.center   = transform.TransformPoint (centerPosition);
            uny.rotation = transform.rotation;

            // Add in all uny list
            RayfireMan.inst.unyList.Add (uny);

            // Save uny id to this id
            id = uny.id;
        }
        
        // Get uniq id
        static int GetUnyId()
        {
            return RayfireMan.inst.unyList.Count + 1;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Getters
        /// /////////////////////////////////////////////////////////
        
        // Had child cluster
        bool HasRigids { get { return rigidList != null && rigidList.Count > 0; } }
        bool HasShards { get { return shardList != null && shardList.Count > 0; } }
        
        // Get final extents
        Vector3 Extents
        {
            get
            {
                Vector3 ext = size / 2f;
                ext.x *= transform.localScale.x;
                ext.y *= transform.localScale.y;
                ext.z *= transform.localScale.z;
                return ext;
            }
        }
    }

    [Serializable]
    public class RFUny
    {
        public int               id;
        public RayfireUnyielding scr;
        
        public Vector3    size;
        public Vector3    center;
        public Quaternion rotation;
    }
}