using UnityEngine;
using UnityEngine.UI;

public class ButtonSoundController : MonoBehaviour
{
    [SerializeField] private AudioClip clickSound;
    [SerializeField, Range(0f, 1f)] private float clickVolume = 0.5f;
    private AudioSource audioSource;
    private Button button;

    private void Awake()
    {
        // Get or add Button component
        button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }

        // Try to get existing AudioSource first
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure AudioSource
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
    }

    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(PlayClickSound);
        }
    }

    private void OnDestroy()
    {
        // Clean up event listeners
        if (button != null)
        {
            button.onClick.RemoveListener(PlayClickSound);
        }

        // Clean up audio source
        if (audioSource != null)
        {
            audioSource.Stop();
            Destroy(audioSource);
        }
    }

    private void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound, clickVolume);
        }
    }
} 