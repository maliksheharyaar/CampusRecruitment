using System.Collections.Generic;
using UnityEngine;

namespace StudentRecruitment.EndlessRunner
{
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0.1f, 3f)]
        public float pitch = 1f;
        public bool loop = false;
        
        [HideInInspector]
        public AudioSource source;
    }
    
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        
        [SerializeField] private Sound[] sounds;
        
        private Dictionary<string, Sound> soundDictionary = new Dictionary<string, Sound>();
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            // Initialize sounds
            foreach (Sound sound in sounds)
            {
                // Create audio source for each sound
                sound.source = gameObject.AddComponent<AudioSource>();
                sound.source.clip = sound.clip;
                sound.source.volume = sound.volume;
                sound.source.pitch = sound.pitch;
                sound.source.loop = sound.loop;
                
                // Add to dictionary for quick lookup
                soundDictionary[sound.name] = sound;
            }
        }
        
        public void PlaySound(string name)
        {
            if (soundDictionary.TryGetValue(name, out Sound sound))
            {
                sound.source.Play();
            }
            else
            {
                Debug.LogWarning($"Sound {name} not found!");
            }
        }
        
        public void StopSound(string name)
        {
            if (soundDictionary.TryGetValue(name, out Sound sound))
            {
                sound.source.Stop();
            }
            else
            {
                Debug.LogWarning($"Sound {name} not found!");
            }
        }
        
        public void PlayOneShot(string name)
        {
            if (soundDictionary.TryGetValue(name, out Sound sound))
            {
                sound.source.PlayOneShot(sound.clip);
            }
            else
            {
                Debug.LogWarning($"Sound {name} not found!");
            }
        }
        
        public void SetVolume(string name, float volume)
        {
            if (soundDictionary.TryGetValue(name, out Sound sound))
            {
                sound.source.volume = Mathf.Clamp01(volume);
            }
            else
            {
                Debug.LogWarning($"Sound {name} not found!");
            }
        }
        
        public void SetPitch(string name, float pitch)
        {
            if (soundDictionary.TryGetValue(name, out Sound sound))
            {
                sound.source.pitch = Mathf.Clamp(pitch, 0.1f, 3f);
            }
            else
            {
                Debug.LogWarning($"Sound {name} not found!");
            }
        }
    }
} 