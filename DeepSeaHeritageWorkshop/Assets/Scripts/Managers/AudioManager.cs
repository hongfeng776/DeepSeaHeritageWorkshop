using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoSingleton<AudioManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Settings")]
    [Range(0f, 1f)] private float musicVolume = 1f;
    [Range(0f, 1f)] private float sfxVolume = 1f;

    private Dictionary<string, AudioClip> audioClipCache = new Dictionary<string, AudioClip>();

    public float MusicVolume
    {
        get => musicVolume;
        set
        {
            musicVolume = Mathf.Clamp01(value);
            if (musicSource != null)
                musicSource.volume = musicVolume;
        }
    }

    public float SfxVolume
    {
        get => sfxVolume;
        set
        {
            sfxVolume = Mathf.Clamp01(value);
            if (sfxSource != null)
                sfxSource.volume = sfxVolume;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        if (Instance == this)
        {
            InitializeAudioSources();
        }
    }

    private void InitializeAudioSources()
    {
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SfxSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
    }

    public void PlayMusic(string clipPath, bool loop = true)
    {
        AudioClip clip = GetAudioClip(clipPath);
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning($"Audio clip not found at path: {clipPath}");
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    public void PlaySfx(string clipPath)
    {
        AudioClip clip = GetAudioClip(clipPath);
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
        else
        {
            Debug.LogWarning($"Audio clip not found at path: {clipPath}");
        }
    }

    public void PlaySfxAtPoint(string clipPath, Vector3 position)
    {
        AudioClip clip = GetAudioClip(clipPath);
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, position, sfxVolume);
        }
        else
        {
            Debug.LogWarning($"Audio clip not found at path: {clipPath}");
        }
    }

    private AudioClip GetAudioClip(string clipPath)
    {
        if (!audioClipCache.TryGetValue(clipPath, out AudioClip clip))
        {
            clip = Resources.Load<AudioClip>(clipPath);
            if (clip != null)
            {
                audioClipCache.Add(clipPath, clip);
            }
        }
        return clip;
    }

    public void ClearCache()
    {
        audioClipCache.Clear();
    }
}
