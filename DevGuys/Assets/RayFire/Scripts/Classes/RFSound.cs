using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace RayFire
{
    [Serializable]
    public class RFSound
    {
        [Tooltip ("Enable sound play for this event.")]
        public bool enable;
        [Space (1)]
        
            
        [Tooltip ("Sound volume multiplier for this event.")]
        [Range(0.01f, 1f)] public float multiplier;
        
        [Header ("  Audio Clips")]
        [Space (3)]
        
        [Tooltip ("Enable sound play for this event.")]
        public AudioClip       clip;
        [Space (1)]
        
        [Tooltip ("List of random Audio Clips to play.")]
        public List<AudioClip> clips;
      
        [Header ("  Audio Mixer")]
        [Space (3)]
                
        [Tooltip ("Audio Mixer Output Group.")]
        public AudioMixerGroup outputGroup;
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
        
        // Constructor
        public RFSound()
        {
            enable     = false;
            multiplier = 1f;
        }
        
        // Copy from
        public RFSound (RFSound source)
        {
            enable = source.enable;
            multiplier = source.multiplier;
            clip = source.clip;
            
            if (source.HasClips == true)
            {
                clips = new List<AudioClip>();
                for (int i = 0; i < source.clips.Count; i++)
                    clips.Add (source.clips[i]);
            }

            outputGroup = source.outputGroup;
        }
        
        // Copy debris and dust
        public static void CopyRootMeshSound (RayfireRigid source, List<RayfireRigid> targets)
        {
            // No sound
            if (source.sound == null)
                return;
            
            // TODO CHECK
            
            // Copy sound
            for (int i = 0; i < targets.Count; i++)
            {
                targets[i].sound = targets[i].gameObject.AddComponent<RayfireSound>();
                targets[i].sound.CopyFrom (source.sound);
                targets[i].sound.rigid = targets[i];
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Play on events
        /// /////////////////////////////////////////////////////////

        // Play
        public static void Play(RayfireSound scr, AudioClip clip, AudioMixerGroup group, float volume)
        {
            // Has output group
            if (group != null)
            {
                // Get audio source
                GameObject  audioObject = new GameObject("RFSoundSource");
                audioObject.transform.parent = RayfireMan.inst.transform;
                Object.Destroy (audioObject, clip.length + 1f);
                audioObject.transform.position = scr.gameObject.transform.position;
                AudioSource audioSource = audioObject.AddComponent<AudioSource>();

                // Setup
                audioSource.clip                  = clip;
                audioSource.playOnAwake           = false;
                audioSource.outputAudioMixerGroup = group;
                audioSource.Play ();
            }
            else
                AudioSource.PlayClipAtPoint (clip, scr.gameObject.transform.position, volume);
        }
        
        // Initialization sound
        public static void InitializationSound (RayfireSound scr, float size)
        {
            // Null
            if (scr == null)
                return;

            // Turned off
            if (scr.initialization.enable == false)
                return;

            // No Rigid
            if (scr.rigid == null)
            {
                Debug.Log ("RayFire Sound: " + scr.name + " Initialization sound warning. Rigid component required", scr.gameObject);
                return;
            }

            // Get size if not defined
            if (size <= 0)
                size = scr.rigid.limitations.bboxSize;
            
            // Filtering
            if (FilterCheck(scr, size) == false)
                return;
            
            // Get play clip
            if (scr.initialization.HasClips == true)
                scr.initialization.clip = scr.initialization.clips[Random.Range (0, scr.activation.clips.Count)];
            
            // Has no clip
            if (scr.initialization.clip == null)
                return;

            // Get volume
            float volume = GeVolume (scr, size) * scr.initialization.multiplier;
            
            // Play
            Play (scr, scr.initialization.clip, scr.initialization.outputGroup, volume);
        }
        
        // Activation sound
        public static void ActivationSound (RayfireSound scr, float size)
        {
            // Null
            if (scr == null)
                return;

            // Turned off
            if (scr.activation.enable == false)
                return;
            
            // No Rigid
            if (scr.rigid == null)
            {
                Debug.Log ("RayFire Sound: " + scr.name + " Activation sound warning. Rigid component required", scr.gameObject);
                return;
            }

            // Get size if not defined
            if (size <= 0)
                size = scr.rigid.limitations.bboxSize;
            
            // Filtering
            if (FilterCheck(scr, size) == false)
                return;
            
            // Get play clip
            if (scr.activation.HasClips == true)
                scr.activation.clip = scr.activation.clips[Random.Range (0, scr.activation.clips.Count)];
            
            // Has no clip
            if (scr.activation.clip == null)
                return;

            // Get volume
            float volume = GeVolume (scr, size) * scr.activation.multiplier;;
            
            // Play
            Play (scr, scr.activation.clip, scr.activation.outputGroup, volume);
        }

        // Demolition sound
        public static void DemolitionSound (RayfireSound scr, float size)
        {
            // Null
            if (scr == null)
                return;
            
            // Turned off
            if (scr.demolition.enable == false)
                return;

            // No Rigid
            if (scr.rigid == null)
            {
                Debug.Log ("RayFire Sound: " + scr.name + " Demolition sound warning. Rigid component required", scr.gameObject);
                return;
            }
            
            // Get size if not defined
            if (size <= 0)
                size = scr.rigid.limitations.bboxSize;

            // Filtering
            if (FilterCheck(scr, size) == false)
                return;
           
            // Get play clip
            if (scr.demolition.HasClips == true)
                scr.demolition.clip = scr.demolition.clips[Random.Range (0, scr.demolition.clips.Count)];

            // Has no clip
            if (scr.demolition.clip == null)
                return;

            // Get volume
            float volume = GeVolume (scr, size) * scr.demolition.multiplier;

            // Play
            Play (scr, scr.demolition.clip, scr.demolition.outputGroup, volume);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        // Get volume
        public static float GeVolume (RayfireSound scr, float size)
        {
            // Get size if not defined
            if (size <= 0)
                if (scr.rigid != null)
                    size = scr.rigid.limitations.bboxSize;
            
            // Get volume
            float volume = scr.baseVolume;
            if (scr.sizeVolume > 0)
                volume += size * scr.sizeVolume;
            
            return volume;
        }
        
        // Filters check
        static bool FilterCheck (RayfireSound scr, float size)
        {
            // Small size
            if (scr.minimumSize > 0)
                if (size < scr.minimumSize)
                    return false;

            // Far from camera
            if (scr.cameraDistance > 0)
                if (Camera.main != null)
                    if (Vector3.Distance (Camera.main.transform.position, scr.transform.position) > scr.cameraDistance)
                        return false;
            return true;
        }
        
        // Has clips
        public bool HasClips { get { return clips != null && clips.Count > 0; } }
    }
}

