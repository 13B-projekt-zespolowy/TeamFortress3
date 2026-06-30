using UnityEngine;

/// <summary>
/// Manages audio playback for background music and sound effects in the game.
/// Implements singleton pattern for global access.
/// Automatically plays random BGM tracks and provides SFX playback functionality.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    public AudioClip[] bgmPlaylist;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            sfxSource.ignoreListenerPause = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayRandomBGM();
    }

    private void Update()
    {
        if (!bgmSource.isPlaying && bgmPlaylist.Length > 0)
        {
            PlayRandomBGM();
        }
    }

    /// <summary>
    /// Plays a random background music track from the playlist.
    /// </summary>
    public void PlayRandomBGM()
    {
        if (bgmPlaylist.Length == 0) return;

        int randomIndex = Random.Range(0, bgmPlaylist.Length);

        bgmSource.clip = bgmPlaylist[randomIndex];
        bgmSource.loop = false;
        bgmSource.Play();
    }

    /// <summary>
    /// Plays a sound effect clip through the SFX audio source.
    /// </summary>
    /// <param name="clip">The audio clip to play.</param>
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}
