using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RayFire
{
    [Serializable]
    public class RFCustom
    {
        // Fragmentation Type
        public enum RFPointCloudSourceType
        {
            ChildrenTransform = 4,
            TransformArray    = 8,
            Vector3Array      = 12
        }
        
        // Fragmentation Type
        public enum RFPointCloudUseType
        {
            VolumePoints = 4,
            //VolumeAroundPath  = 8,
            PointCloud  = 12
        }
        
        // Fragmentation Type
        public enum RFModifierType
        {
            None       = 0,
            Splinters  = 3,
            Slabs      = 6
        }
        
        //public bool info = false;
        
        [Header("  Point Cloud")]
        [Space(2)]

        public RFPointCloudSourceType source;
        [Space(2)]

        public RFPointCloudUseType useAs;
        [Space(2)]
        
        [Header("  Volume")]
        [Space(2)]
        
        [Range(3, 1000)] public int amount;
        [Range(0.01f, 4f)] public float radius;

        [Header("  Preview")]
        [Space(2)]
        
        public bool enable;
        [Range(0.01f, 0.4f)] public float size;
        
        [Header("  Arrays")]
        [Space(2)]
        
        public Transform[] transforms;
        public Vector3[] vector3;

        [HideInInspector]
        public bool noPoints = false;
        
        // Constructor
        public RFCustom()
        {
            source = RFPointCloudSourceType.ChildrenTransform;
            useAs = RFPointCloudUseType.PointCloud;
            amount = 100;
            radius = 1f;
            enable = true;
            size = 0.05f;
        }
        
        // Constructor
        public RFCustom(RFCustom src)
        {
            source     = src.source;
            useAs      = src.useAs;
            amount     = src.amount;
            radius     = src.radius;
            enable     = false;
            size       = src.size;
            transforms = src.transforms;
            vector3    = src.vector3;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////

        // Get final point cloud for custom fragmentation
        public static List<Vector3> GetCustomPointCLoud (RFCustom custom, Transform tm, int seed, Bounds bound)
        {
            // Get input points
            List<Vector3> inputPoints = GetCustomInputCloud (custom, tm);

            // Get final output point cloud
            List<Vector3> outputPoints = GetCustomOutputCloud (custom, inputPoints, seed, bound);
            
            // Get points in bound
            List<Vector3> boundPoints = GetCustomBoundPoints (outputPoints, bound);
            
            // Stop if no points
            if (boundPoints.Count <= 1)
                custom.noPoints = true;
            
            return boundPoints;
        }
        
        // Get custom input cloud
        static List<Vector3> GetCustomInputCloud(RFCustom custom, Transform tm)
        {
            // Vars
            custom.noPoints = false;
            List<Vector3> inputPoints = new List<Vector3> ();
            
            // Children transform
            if (custom.source == RFCustom.RFPointCloudSourceType.ChildrenTransform)
            {
                if (tm.childCount > 0)
                    for (int i = 0; i < tm.childCount; i++)
                        inputPoints.Add (tm.GetChild (i).position);
            }        
            
            // Transform array
            else if (custom.source == RFCustom.RFPointCloudSourceType.TransformArray)
            {
                if (custom.transforms != null && custom.transforms.Length > 0)
                    for (int i = 0; i < custom.transforms.Length; i++)
                         if (custom.transforms[i] != null)
                             inputPoints.Add (custom.transforms[i].position);
            }
            
            // Vector 3 array
            else if (custom.source == RFCustom.RFPointCloudSourceType.Vector3Array)
            {
                if (custom.vector3 != null && custom.vector3.Length > 0)
                    for (int i = 0; i < custom.vector3.Length; i++)
                        inputPoints.Add (custom.vector3[i]);
            }
            
            return inputPoints;
        }

        // Get final output point cloud
        static List<Vector3> GetCustomOutputCloud(RFCustom custom, List<Vector3> inputPoints, int seed, Bounds bound)
        {
            // Use same input point
            if (custom.useAs == RFCustom.RFPointCloudUseType.PointCloud)
                return inputPoints;
            
            // Volume around point
            if (custom.useAs == RFCustom.RFPointCloudUseType.VolumePoints)
            {
                // Stop if no points
                if (inputPoints.Count == 0)
                    return inputPoints;
                
                // Get amount of points in radius 
                int pointsPerPoint = custom.amount / inputPoints.Count;
                int localSeed = seed;
                
                // Generate new points around point
                List<Vector3> newPoints = new List<Vector3>();
                for (int p = 0; p < inputPoints.Count; p++)
                {
                    localSeed++;
                    Random.InitState (localSeed);
                    for (int i = 0; i < pointsPerPoint; i++)
                    {
                        Vector3 randomPoint = RandomPointInRadius (inputPoints[p], custom.radius);
                        if (bound.Contains (randomPoint) == false)
                        {
                            randomPoint = RandomPointInRadius (inputPoints[p], custom.radius);
                            if (bound.Contains (randomPoint) == false)
                                randomPoint = RandomPointInRadius (inputPoints[p], custom.radius);
                        }
                        newPoints.Add (randomPoint);
                    }
                }
                return newPoints;
            }
            return inputPoints;
        }
        
        // Filter world points by bound intersection
        static List<Vector3> GetCustomBoundPoints(List<Vector3> inputPoints, Bounds bound)
        {
            for (int i = inputPoints.Count - 1; i >= 0; i--)
                if (bound.Contains(inputPoints[i]) == false)
                    inputPoints.RemoveAt (i);
            return inputPoints;
        }
        
        // Random point in radius around input point
        static Vector3 RandomPointInRadius(Vector3 point, float radius)
        {
            return RandomVector() * Random.Range (0f, radius) + point;
        }
        
        // Random vector
        static Vector3 RandomVector()
        {
            return new Vector3(Random.Range (-1f, 1f), Random.Range (-1f, 1f), Random.Range (-1f, 1f));
        }

    }
}

