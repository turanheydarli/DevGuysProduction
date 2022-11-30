using System;
using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
	[Serializable]
    public class RFMeshExport
    {
        // Export type
        public enum MeshExportType
        {
            LastFragments         = 0,
            Children              = 3
        }

        // Mesh source
        public MeshExportType source;
        
    	// by object, by suffix
    	public string suffix = "_frags";
    	
    	// by path, by window
    	// public string path = "RayFireFragments";
    	
    	// all, last
        // generate colliders
    }
    
	[Serializable]
	public class RFShatterAdvanced
	{
		[Header ("  Common")]
		[Space (2)]
		
		[Tooltip ("Seed for point cloud generator. Set to 0 to get random point cloud every time.")]
		[Range (0, 100)] public int seed; 
		
		[Space(1)]
		public bool decompose;
		
		[Space(1)]
        public bool removeCollinear;
        
        [Space(1)]
        public bool copyComponents;
        
        [Header ("  Editor Mode")]
        [Space (2)]
        
        [Tooltip ("Create extra triangles to connect open edges and close mesh volume.")]
        public bool inputPrecap;
        
        [Space(1)]
        [Tooltip ("Keep or Delete fragment's faces created by Input Precap.")]
        public bool outputPrecap;
        
        [Space(1)]
        [Tooltip ("Delete faces which overlap with each other.")]
        public bool removeDoubleFaces;
        
        [Space(1)]
        [Tooltip ("Measures in percents relative to original object size. Do not fragment elements with size less than this value.")]
        [Range (1, 100)] public int elementSizeThreshold;
        
        
        [Header ("  Filters")]
        [Space (2)]
        
        [Space(1)]
        [Tooltip ("Do not output inner fragments which has no outer surface.")]
        public bool inner;
        
        [Space(1)]
        [Tooltip ("Do not output planar fragments which mesh vertices lie in the same plane.")]
        public bool planar;
        
        [Space(1)]
        [Tooltip ("Do not output small fragments. Measures is percentage relative to original object size.")]
        [Range (0, 10)] public int relativeSize; 
        
        [Space(1)]
        [Tooltip ("Do not output small fragments which size in world units is less than this value.")]
        [Range (0, 1)] public float absoluteSize;

        [Header ("  Limitations")]
        [Space (2)]
        
        public bool vertexLimitation;
        
        [Space(1)]
        [Range(100, 1900)] public int vertexAmount;
        
        // Planar mesh vert offset threshold
        public static float planarThreshold = 0.01f;
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
		
		// Constructor
		public RFShatterAdvanced()
		{
			seed                  = 0;
			decompose             = true;
			removeCollinear       = false;
			copyComponents        = false;

			inputPrecap           = true;
			outputPrecap          = false;
			
			removeDoubleFaces     = true;
			
			elementSizeThreshold  = 5;

			inner        = false;
			planar       = false;
			absoluteSize = 0.1f;
			relativeSize = 4;
			
			vertexLimitation = false;
			vertexAmount     = 300;
		}
        
        // Constructor
        public RFShatterAdvanced (RFShatterAdvanced src)
        {
	        seed            = src.seed;
	        decompose       = src.decompose;
	        removeCollinear = src.removeCollinear;
	        copyComponents  = src.copyComponents;

	        inputPrecap  = src.inputPrecap;
	        outputPrecap = src.outputPrecap;
			
	        removeDoubleFaces     = src.removeDoubleFaces;
	        inner = src.inner;
	        elementSizeThreshold  = src.elementSizeThreshold;
	        
	        vertexLimitation = src.vertexLimitation;
	        vertexAmount     = src.vertexAmount;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        // Check if mesh is coplanar. All verts on a plane
        public static bool IsCoplanar(Mesh mesh, float threshold)
        {
            // Coplanar 3 verts
            if (mesh.vertices.Length <= 3)
                return true;
            
            // Get second vert for plane
            int ind = 1;
            List<int> ids = new List<int>() {0};
            for (int i = ind; i < mesh.vertices.Length; i++)
                if (Vector3.Distance (mesh.vertices[0], mesh.vertices[i]) > threshold)
                {
                    ids.Add (i);
                    ind = i;
                    break;
                }

            // No second vert
            if (ids.Count == 1)
                return true;

            // Second vert is the last ver
            if (ind == mesh.vertices.Length - 1)
                return true;
            
            // Get third vert
            ind++;
            Vector3 vector1 = (mesh.vertices[ids[1]] - mesh.vertices[ids[0]]).normalized;
            for (int i = ind; i < mesh.vertices.Length; i++)
            {
                if (Vector3.Distance (mesh.vertices[1], mesh.vertices[i]) > threshold)
                {
                    Vector3 vector2  = (mesh.vertices[i] - mesh.vertices[ids[0]]).normalized;
                    float   distance = Vector3.Cross (vector1, vector2).magnitude;
                    if (distance > threshold)
                    {
                        ids.Add (i);
                        break;
                    }
                }
            }
            
            // No third vert
            if (ids.Count == 2)
                return true;

            // Create plane and check other verts for coplanar
            Plane plane = new Plane(mesh.vertices[ids[0]], mesh.vertices[ids[1]], mesh.vertices[ids[2]]);
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                if (i != ids[0] && i != ids[1] && i != ids[2])
                {
                    float dist = plane.GetDistanceToPoint (mesh.vertices[i]);
                    if (Math.Abs (dist) > threshold)
                        return false;
                }
            }
            
            return true;
        }
	}
}