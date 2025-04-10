using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Collections.Generic;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;
    [Range(0f, 1f)]
    public float spatialBlend = 0f;
    
    public bool loop = false;
    public bool playOnAwake = false;
    
    [HideInInspector]
    public AudioSource source;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Settings")]
    [SerializeField] private AudioMixerGroup mainMixerGroup;
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    
    [Header("Sound Collections")]
    [SerializeField] private Sound[] musicTracks;
    [SerializeField] private Sound[] sfxSounds;
    
    [Header("Emotion-Based Ambient Sounds")]
    [SerializeField] private Sound[] emotionAmbience;

    private Dictionary<EmotionType, Sound> emotionSoundMap;
    private Sound currentAmbience;
    private Sound currentMusic;
    
    private const float CROSSFADE_DURATION = 2f;
    private const float MIN_PITCH = 0.9f;
    private const float MAX_PITCH = 1.1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudio()
    {
        try
        {
            // Initialize sound sources
            InitializeSoundSources(musicTracks, musicMixerGroup);
            InitializeSoundSources(sfxSounds, sfxMixerGroup);
            InitializeSoundSources(emotionAmbience, mainMixerGroup);

            // Map emotion sounds
            MapEmotionSounds();

            // Subscribe to events
            RoomManager.OnRoomEmotionChanged += HandleEmotionChanged;
            GameManager.OnGameStateChanged += HandleGameStateChanged;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize AudioManager: {e.Message}");
        }
    }

    private void InitializeSoundSources(Sound[] sounds, AudioMixerGroup mixerGroup)
    {
        foreach (Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
            sound.source.playOnAwake = sound.playOnAwake;
            sound.source.spatialBlend = sound.spatialBlend;
            
            if (mixerGroup != null)
            {
                sound.source.outputAudioMixerGroup = mixerGroup;
            }
        }
    }

    private void MapEmotionSounds()
    {
        emotionSoundMap = new Dictionary<EmotionType, Sound>();
        
        foreach (Sound sound in emotionAmbience)
        {
            if (Enum.TryParse(sound.name, out EmotionType emotion))
            {
                emotionSoundMap[emotion] = sound;
            }
        }
    }

    public void PlayMusic(string name, bool fadeIn = true)
    {
        try
        {
            Sound music = Array.Find(musicTracks, sound => sound.name == name);
            
            if (music == null)
            {
                Debug.LogWarning($"Music track '{name}' not found!");
                return;
            }

            if (currentMusic != null && currentMusic.source.isPlaying)
            {
                StartCoroutine(CrossfadeMusic(currentMusic, music));
            }
            else
            {
                if (fadeIn)
                {
                    StartCoroutine(FadeIn(music.source, CROSSFADE_DURATION));
                }
                else
                {
                    music.source.Play();
                }
            }

            currentMusic = music;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to play music '{name}': {e.Message}");
        }
    }

    public void PlaySFX(string name, Vector3? position = null)
    {
        try
        {
            Sound sfx = Array.Find(sfxSounds, sound => sound.name == name);
            
            if (sfx == null)
            {
                Debug.LogWarning($"SFX '{name}' not found!");
                return;
            }

            // Randomize pitch slightly for variety
            sfx.source.pitch = UnityEngine.Random.Range(MIN_PITCH, MAX_PITCH);

            if (position.HasValue)
            {
                AudioSource.PlayClipAtPoint(sfx.clip, position.Value, sfx.volume);
            }
            else
            {
                sfx.source.Play();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to play SFX '{name}': {e.Message}");
        }
    }

    private void HandleEmotionChanged(EmotionType newEmotion)
    {
        try
        {
            if (emotionSoundMap.TryGetValue(newEmotion, out Sound newAmbience))
            {
                if (currentAmbience != null && currentAmbience != newAmbience)
                {
                    StartCoroutine(CrossfadeAmbience(currentAmbience, newAmbience));
                }
                else if (currentAmbience == null)
                {
                    newAmbience.source.Play();
                    StartCoroutine(FadeIn(newAmbience.source, CROSSFADE_DURATION));
                }

                currentAmbience = newAmbience;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to handle emotion change: {e.Message}");
        }
    }

    private void HandleGameStateChanged(string state)
    {
        switch (state)
        {
            case "GameStarted":
                PlayMusic("GameplayMusic");
                break;
            case "GamePaused":
                PauseAllAudio();
                break;
            case "GameResumed":
                UnpauseAllAudio();
                break;
            case "LevelCompleted":
                PlaySFX("LevelComplete");
                break;
        }
    }

    private System.Collections.IEnumerator CrossfadeMusic(Sound oldMusic, Sound newMusic)
    {
        float timeElapsed = 0;
        newMusic.source.volume = 0;
        newMusic.source.Play();

        while (timeElapsed < CROSSFADE_DURATION)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / CROSSFADE_DURATION;

            oldMusic.source.volume = Mathf.Lerp(oldMusic.volume, 0, t);
            newMusic.source.volume = Mathf.Lerp(0, newMusic.volume, t);

            yield return null;
        }

        oldMusic.source.Stop();
    }

    private System.Collections.IEnumerator CrossfadeAmbience(Sound oldAmbience, Sound newAmbience)
    {
        float timeElapsed = 0;
        newAmbience.source.volume = 0;
        newAmbience.source.Play();

        while (timeElapsed < CROSSFADE_DURATION)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / CROSSFADE_DURATION;

            oldAmbience.source.volume = Mathf.Lerp(oldAmbience.volume, 0, t);
            newAmbience.source.volume = Mathf.Lerp(0, newAmbience.volume, t);

            yield return null;
        }

        oldAmbience.source.Stop();
    }

    private System.Collections.IEnumerator FadeIn(AudioSource audioSource, float duration)
    {
        float startVolume = 0;
        float targetVolume = audioSource.volume;
        float timeElapsed = 0;

        audioSource.volume = startVolume;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, timeElapsed / duration);
            yield return null;
        }
    }

    public void PauseAllAudio()
    {
        foreach (Sound sound in musicTracks) sound.source.Pause();
        foreach (Sound sound in sfxSounds) sound.source.Pause();
        foreach (Sound sound in emotionAmbience) sound.source.Pause();
    }

    public void UnpauseAllAudio()
    {
        foreach (Sound sound in musicTracks) sound.source.UnPause();
        foreach (Sound sound in sfxSounds) sound.source.UnPause();
        foreach (Sound sound in emotionAmbience) sound.source.UnPause();
    }

    public void StopAllAudio()
    {
        foreach (Sound sound in musicTracks) sound.source.Stop();
        foreach (Sound sound in sfxSounds) sound.source.Stop();
        foreach (Sound sound in emotionAmbience) sound.source.Stop();
    }

    public void SetMasterVolume(float volume)
    {
        if (mainMixerGroup != null)
        {
            mainMixerGroup.audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (musicMixerGroup != null)
        {
            musicMixerGroup.audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxMixerGroup != null)
        {
            sfxMixerGroup.audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        }
    }

    private void OnDestroy()
    {
        if (RoomManager.OnRoomEmotionChanged != null)
            RoomManager.OnRoomEmotionChanged -= HandleEmotionChanged;
        if (GameManager.OnGameStateChanged != null)
            GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }
}
