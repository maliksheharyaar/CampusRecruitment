using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSoundController : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private float clickVolume = 0.7f;
    
    private AudioSource audioSource;
    private Button button;

    private void Start()
    {
        SetupAudio();
        SetupButton();
    }

    private void SetupAudio()
    {
        // Try to get existing AudioSource first
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    private void SetupButton()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    private void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound, clickVolume);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(PlayClickSound);
        }
    }
} 