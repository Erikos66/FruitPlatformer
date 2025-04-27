using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour {
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioMixer audioMixer;

    [System.Serializable]
    public class SoundEffect {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0.5f, 1.5f)]
        public float pitch = 1f;
    }

    [SerializeField] private List<SoundEffect> soundEffects = new();
    private Dictionary<string, SoundEffect> soundEffectsDict = new();

    private void Awake() {
        // Singleton pattern
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize the dictionary
            foreach (SoundEffect sound in soundEffects) {
                if (!string.IsNullOrEmpty(sound.name) && sound.clip != null) {
                    soundEffectsDict[sound.name] = sound;
                }
            }
        }
        else {
            Destroy(gameObject);
        }
    }

    public void PlayMusic(AudioClip musicClip, bool loop = true) {
        if (musicSource == null || musicClip == null)
            return;

        musicSource.clip = musicClip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic() {
        if (musicSource != null)
            musicSource.Stop();
    }

    public void PlaySFX(string sfxName) {
        if (sfxSource == null || !soundEffectsDict.ContainsKey(sfxName))
            return;

        SoundEffect sound = soundEffectsDict[sfxName];
        sfxSource.pitch = sound.pitch;
        sfxSource.PlayOneShot(sound.clip, sound.volume);
    }

    public void PlaySFX(AudioClip clip, float volume = 1f) {
        if (sfxSource == null || clip == null)
            return;

        sfxSource.PlayOneShot(clip, volume);
    }

    // Set volume via code (used by UI_Settings)
    public void SetMusicVolume(float volume) {
        if (audioMixer != null) {
            // Convert to decibels
            float dbValue = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20;
            audioMixer.SetFloat("MusicVolume", dbValue);
        }
    }

    public void SetSFXVolume(float volume) {
        if (audioMixer != null) {
            // Convert to decibels
            float dbValue = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20;
            audioMixer.SetFloat("SFXVolume", dbValue);
        }
    }
}