# Music and Audio Setup Guide for Endless Runner

This guide will help you implement a professional audio system for your endless runner game, enhancing the player experience through sound effects and music.

## Audio Manager Setup

1. Create an Audio Manager GameObject:
   - Create an empty GameObject in your scene named "AudioManager"
   - Add AudioSource components for:
     - Background Music (set Loop = true)
     - Sound Effects (set Loop = false)
     - UI Sounds (set Loop = false)
   - Position the AudioManager at (0, 0, 0)
   - Set it to DontDestroyOnLoad if your game has multiple scenes

2. Create an AudioManager script and attach it to the AudioManager GameObject:
   ```csharp
   using UnityEngine;
   using System.Collections.Generic;
   
   public class AudioManager : MonoBehaviour
   {
       [Header("Audio Sources")]
       [SerializeField] private AudioSource musicSource;
       [SerializeField] private AudioSource sfxSource;
       [SerializeField] private AudioSource uiSource;
       
       [Header("Music Clips")]
       [SerializeField] private AudioClip mainMenuMusic;
       [SerializeField] private AudioClip gameplayMusic;
       [SerializeField] private AudioClip victoryMusic;
       [SerializeField] private AudioClip gameOverMusic;
       
       [Header("Player SFX")]
       [SerializeField] private AudioClip jumpSound;
       [SerializeField] private AudioClip slideSound;
       [SerializeField] private AudioClip hitSound;
       [SerializeField] private AudioClip deathSound;
       
       [Header("Gameplay SFX")]
       [SerializeField] private AudioClip coinCollectSound;
       [SerializeField] private AudioClip powerUpSound;
       [SerializeField] private AudioClip finishLineSound;
       
       [Header("UI SFX")]
       [SerializeField] private AudioClip buttonClickSound;
       [SerializeField] private AudioClip pauseSound;
       [SerializeField] private AudioClip unpauseSound;
       
       [Header("Audio Settings")]
       [Range(0f, 1f)]
       [SerializeField] private float masterVolume = 1f;
       [Range(0f, 1f)]
       [SerializeField] private float musicVolume = 0.6f;
       [Range(0f, 1f)]
       [SerializeField] private float sfxVolume = 0.8f;
       [Range(0f, 1f)]
       [SerializeField] private float uiVolume = 0.7f;
       
       // Dictionary to store all sound effects for easy access
       private Dictionary<string, AudioClip> soundDictionary;
       
       private void Awake()
       {
           InitializeSoundDictionary();
           ApplyVolumeSettings();
       }
       
       private void InitializeSoundDictionary()
       {
           soundDictionary = new Dictionary<string, AudioClip>
           {
               // Player sounds
               { "Jump", jumpSound },
               { "Slide", slideSound },
               { "Hit", hitSound },
               { "Death", deathSound },
               
               // Gameplay sounds
               { "Coin", coinCollectSound },
               { "PowerUp", powerUpSound },
               { "Finish", finishLineSound },
               
               // UI sounds
               { "ButtonClick", buttonClickSound },
               { "Pause", pauseSound },
               { "Unpause", unpauseSound }
           };
       }
       
       private void ApplyVolumeSettings()
       {
           musicSource.volume = masterVolume * musicVolume;
           sfxSource.volume = masterVolume * sfxVolume;
           uiSource.volume = masterVolume * uiVolume;
       }
       
       // Play background music
       public void PlayMusic(string musicType)
       {
           AudioClip clipToPlay = null;
           
           switch (musicType)
           {
               case "MainMenu":
                   clipToPlay = mainMenuMusic;
                   break;
               case "Gameplay":
                   clipToPlay = gameplayMusic;
                   break;
               case "Victory":
                   clipToPlay = victoryMusic;
                   break;
               case "GameOver":
                   clipToPlay = gameOverMusic;
                   break;
           }
           
           if (clipToPlay != null)
           {
               musicSource.clip = clipToPlay;
               musicSource.Play();
           }
       }
       
       // Play sound effect
       public void PlaySFX(string sfxName)
       {
           if (soundDictionary.TryGetValue(sfxName, out AudioClip clip))
           {
               sfxSource.PlayOneShot(clip);
           }
       }
       
       // Play UI sound
       public void PlayUISound(string soundName)
       {
           if (soundDictionary.TryGetValue(soundName, out AudioClip clip))
           {
               uiSource.PlayOneShot(clip);
           }
       }
       
       // Volume control methods
       public void SetMasterVolume(float volume)
       {
           masterVolume = Mathf.Clamp01(volume);
           ApplyVolumeSettings();
       }
       
       public void SetMusicVolume(float volume)
       {
           musicVolume = Mathf.Clamp01(volume);
           musicSource.volume = masterVolume * musicVolume;
       }
       
       public void SetSFXVolume(float volume)
       {
           sfxVolume = Mathf.Clamp01(volume);
           sfxSource.volume = masterVolume * sfxVolume;
       }
       
       public void SetUIVolume(float volume)
       {
           uiVolume = Mathf.Clamp01(volume);
           uiSource.volume = masterVolume * uiVolume;
       }
       
       // Toggle mute functions
       public void ToggleMusicMute()
       {
           musicSource.mute = !musicSource.mute;
       }
       
       public void ToggleSFXMute()
       {
           sfxSource.mute = !sfxSource.mute;
           uiSource.mute = !uiSource.mute;
       }
   }
   ```

3. Configure the AudioManager in the Inspector:
   - Assign the three AudioSource components to their respective fields
   - Add your audio clips to the appropriate fields
   - Adjust default volume levels as needed

## Audio Assets Organization

1. Create a dedicated folder structure for audio assets:
   ```
   Assets/
     StudentRecruitment/
       EndlessRunner/
         Audio/
           Music/           (for background music tracks)
           SFX/             (for sound effects)
             Player/        (player-specific sounds)
             Environment/   (environment sounds)
             UI/            (UI interaction sounds)
           Mixers/          (for audio mixers)
   ```

2. Import your audio files into the appropriate folders:
   - For background music:
     - Main Menu Theme (looping, calm but energetic)
     - Gameplay Theme (looping, fast-paced, energetic)
     - Victory Jingle (short, celebratory)
     - Game Over Theme (short, somber)
   
   - For player sound effects:
     - Jump Sound (short, upward swoosh)
     - Slide Sound (quick sliding effect)
     - Hit Sound (impact sound)
     - Death Sound (dramatic fall or explosion)
   
   - For gameplay sound effects:
     - Coin Collection Sound (short jingle)
     - Power-Up Collection Sound (magical effect)
     - Finish Line Sound (victory fanfare)
   
   - For UI sounds:
     - Button Click (short click)
     - Pause Sound (downward tone)
     - Unpause Sound (upward tone)

## Audio Format Optimization

1. Configure import settings for background music:
   - Select music files and in the Inspector:
     - Set Compression Format to "Vorbis"
     - Quality: 70-80% (balance between quality and size)
     - Load Type: "Streaming" (reduces memory usage)
     - Ensure "Loop" is checked for continuous tracks

2. Configure import settings for sound effects:
   - Select SFX files and in the Inspector:
     - Set Compression Format to "PCM" for short effects, "ADPCM" for longer ones
     - Load Type: "Decompress On Load" for quick playback
     - Ensure mono for positional sounds, stereo for UI/ambient

## Audio Mixer Setup (Advanced)

1. Create an Audio Mixer:
   - In Project window: Create > Audio > Audio Mixer
   - Name it "GameAudioMixer"

2. Set up mixer groups:
   - Create three groups: "Music", "SFX", and "UI"
   - Set them as children of the Master group

3. Configure the mixer:
   - Add volume control to each group
   - Optionally add effects like reverb or EQ to specific groups
   - Create exposed parameters for volume controls

4. Assign the mixer to AudioSources:
   - Select each AudioSource component
   - Set Output to the appropriate mixer group

## Implementing Audio in Game Elements

1. Player Audio Implementation:
   - In your RunnerController script, add calls to AudioManager:
   ```csharp
   private AudioManager audioManager;
   
   private void Start()
   {
       audioManager = FindObjectOfType<AudioManager>();
   }
   
   // Call in your jump method
   private void Jump()
   {
       // Jump logic
       audioManager.PlaySFX("Jump");
   }
   
   // Call in slide method
   private void Slide()
   {
       // Slide logic
       audioManager.PlaySFX("Slide");
   }
   
   // Call when player gets hit
   public void TakeDamage()
   {
       // Damage logic
       audioManager.PlaySFX("Hit");
       
       if (lives <= 0)
       {
           audioManager.PlaySFX("Death");
       }
   }
   ```

2. Collectible Audio Implementation:
   - In your Coin.cs script:
   ```csharp
   private void OnTriggerEnter(Collider other)
   {
       if (other.CompareTag("Player"))
       {
           FindObjectOfType<AudioManager>().PlaySFX("Coin");
           // Coin collection logic
       }
   }
   ```

   - In your PowerUp.cs script:
   ```csharp
   private void OnTriggerEnter(Collider other)
   {
       if (other.CompareTag("Player"))
       {
           FindObjectOfType<AudioManager>().PlaySFX("PowerUp");
           // Power-up logic
       }
   }
   ```

3. UI Audio Implementation:
   - Add to UIManager.cs:
   ```csharp
   private AudioManager audioManager;
   
   private void Start()
   {
       audioManager = FindObjectOfType<AudioManager>();
   }
   
   public void OnButtonClick()
   {
       audioManager.PlayUISound("ButtonClick");
   }
   ```

4. Game State Audio:
   - In EndlessRunnerManager or GameManager:
   ```csharp
   private AudioManager audioManager;
   
   private void Start()
   {
       audioManager = FindObjectOfType<AudioManager>();
       audioManager.PlayMusic("Gameplay");
   }
   
   public void OnGameOver()
   {
       audioManager.PlayMusic("GameOver");
   }
   
   public void OnVictory()
   {
       audioManager.PlayMusic("Victory");
   }
   ```

## Audio Settings UI Integration

1. Create an Audio Settings UI:
   - Add sliders for Master, Music, SFX, and UI volume
   - Add toggle buttons for Music Mute and SFX Mute

2. Connect UI elements to the AudioManager:
   ```csharp
   public class AudioSettingsUI : MonoBehaviour
   {
       [SerializeField] private Slider masterVolumeSlider;
       [SerializeField] private Slider musicVolumeSlider;
       [SerializeField] private Slider sfxVolumeSlider;
       [SerializeField] private Slider uiVolumeSlider;
       
       [SerializeField] private Toggle musicMuteToggle;
       [SerializeField] private Toggle sfxMuteToggle;
       
       private AudioManager audioManager;
       
       private void Start()
       {
           audioManager = FindObjectOfType<AudioManager>();
           
           // Set initial values
           // (You'd ideally get these from saved settings)
           
           // Setup listeners
           masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
           musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
           sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
           uiVolumeSlider.onValueChanged.AddListener(SetUIVolume);
           
           musicMuteToggle.onValueChanged.AddListener(ToggleMusicMute);
           sfxMuteToggle.onValueChanged.AddListener(ToggleSFXMute);
       }
       
       public void SetMasterVolume(float volume)
       {
           audioManager.SetMasterVolume(volume);
       }
       
       public void SetMusicVolume(float volume)
       {
           audioManager.SetMusicVolume(volume);
       }
       
       public void SetSFXVolume(float volume)
       {
           audioManager.SetSFXVolume(volume);
       }
       
       public void SetUIVolume(float volume)
       {
           audioManager.SetUIVolume(volume);
       }
       
       public void ToggleMusicMute(bool isMuted)
       {
           audioManager.ToggleMusicMute();
       }
       
       public void ToggleSFXMute(bool isMuted)
       {
           audioManager.ToggleSFXMute();
       }
   }
   ```

## Testing and Optimization

1. Test audio in different game scenarios:
   - Player actions (jumping, sliding)
   - Collectible pickups
   - Power-up activations
   - Game state changes
   - UI interactions

2. Adjust and balance volumes:
   - Ensure no sounds are too loud or too quiet
   - Make sure important gameplay sounds are clearly audible
   - Verify background music doesn't overpower game sounds

3. Performance considerations:
   - Use Object Pooling for frequently played sounds
   - Limit simultaneous sounds (prioritize important ones)
   - Use distance-based volume attenuation for 3D sounds
   - Consider using sound variations to avoid repetition

## Adding Polish and Finishing Touches

1. Add audio transitions:
   - Cross-fade between different music tracks
   - Add slight pitch variations to repeated sounds

2. Implement 3D spatial audio for better immersion:
   - Set SFX AudioSource's Spatial Blend to 1 (fully 3D)
   - Configure 3D Sound Settings for distance attenuation

3. Add environmental effects:
   - Apply reverb zones to create different acoustic environments
   - Use low-pass filters when the player is underwater or slowed

4. Test on different devices and systems:
   - Check audio quality on various speakers/headphones
   - Verify volume levels are appropriate across devices 