using System;
using UnityEngine;

// Namespace
namespace RayFire
{
    [Serializable]
    public class RFManDemolition
    {
        // Post dml fragments 
        public enum FragmentParentType
        {
            Manager = 0,
            Parent  = 1
        }

        [Header ("  Fragments")]
        [Space(2)]
        
        [Tooltip("Defines parent for all new fragments.")]
        public FragmentParentType parent;
        [Space (2)]
        
        [Tooltip("Maximum amount of allowed fragments. Object won't be demolished if existing amount of fragments "+
                 "in scene higher that this value. Fading allows to decrease amount of fragments in scene.")]
        public int maximumAmount = 1000;
        [Space (2)]
        
        [Tooltip("Amount of attempts to fragment mesh with topology issues. After object will fail to be fragments "+
                 "defined amount of times it will be marked as Bad Mesh and it won't be possible to fragment it again.")]
        [Range (1, 10)]   public int badMeshTry    = 3;

        [Header ("  Shadow Casting")]
        [Space(2)]
        
        [Tooltip("Disable Shadow Casting for all objects with size less than this value.")]
        [Range (0, 1f)] public float sizeThreshold = 0.05f;

        
        [HideInInspector] public int currentAmount = 0;
        
        // TODO Inherit velocity by impact normal
    }
}