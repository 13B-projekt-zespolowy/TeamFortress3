using UnityEngine;

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

    public void PlayRandomBGM()
    {
        if (bgmPlaylist.Length == 0) return;

        int randomIndex = Random.Range(0, bgmPlaylist.Length);

        bgmSource.clip = bgmPlaylist[randomIndex];
        bgmSource.loop = false;
        bgmSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}