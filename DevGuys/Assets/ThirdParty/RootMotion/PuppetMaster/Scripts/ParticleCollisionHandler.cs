using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RootMotion.Dynamics
{

    /// <summary>
    /// Registers particle collisions and sends them over to PuppetMaster's MuscleCollisionBroadcaster as raycast hits.
    /// </summary>
    public class ParticleCollisionHandler : MonoBehaviour
    {
        /// <summary>
        /// PuppetMaster ragdoll layers to hit.
        /// </summary>
        [Tooltip("PuppetMaster ragdoll layers to hit.")]
        public LayerMask ragdollLayers;

        /// <summary>
        /// Multiplier for unpinning the puppet on particle hit (velocity.magnitude * colliderForce * unpin).
        /// </summary>
        [Tooltip("Multiplier for unpinning the puppet on particle hit (velocity.magnitude * colliderForce * unpin).")]
        public float unpin = 0.02f;
        
        private ParticleSystem p;
        private List<ParticleCollisionEvent> particleCollisionEvents = new List<ParticleCollisionEvent>();

        private void Start()
        {
            p = GetComponent<ParticleSystem>();

            if (!p.collision.sendCollisionMessages) Debug.LogError("ParticleSystems with ParticleCollisionHandler need to have 'Send Collision Messages' enabled in the Collision module.");
            if (p.collision.colliderForce <= 0f) Debug.LogError("ParticleSystems with ParticleCollisionHandler need to have 'Collider Force' > 0f in the Collision module.");
            if (p.collision.collidesWith == 0) Debug.LogError("ParticleSystems with ParticleCollisionHandler need to have 'Collides With' LayerMask set in the Collision module.");
        }

        private void OnParticleCollision(GameObject other)
        {
            if (!enabled) return;
            if (!RootMotion.LayerMaskExtensions.Contains(ragdollLayers, other.layer)) return;

            // Find the collider so we could find its attachedRigidbody and the MuscleCollisionBroadcaster on that.
            var collider = other.GetComponent<Collider>();
            if (collider.attachedRigidbody == null) return;

            var broadcaster = collider.attachedRigidbody.GetComponent<MuscleCollisionBroadcaster>();
            if (broadcaster == null) return;

            int num = p.GetCollisionEvents(other, particleCollisionEvents);

            int i = 0;
            while (i < num)
            {
                Vector3 pos = particleCollisionEvents[i].intersection;
                float u = particleCollisionEvents[i].velocity.magnitude * p.collision.colliderForce * unpin;
                broadcaster.Hit(u, Vector3.zero, pos);
                i++;
            }
        }
    }
}
