using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour {

    // Singleton instance
    public static AudioManager Instance { get; private set; }

    [Header("Audio Source Containers")]
    [SerializeField] private Transform sfxContainer;
    [SerializeField] private Transform musicContainer;

    [Header("Volume Controls")]
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;

    [Header("Pitch Settings")]
    [SerializeField] private float minPitch = 0.9f;
    [SerializeField] private float maxPitch = 1.1f;

    private float masterVolume = 1f;
    private float sfxVolume = 1f;
    private float musicVolume = 1f;

    // Dictionaries to store audio sources by name
    private Dictionary<string, AudioSource> sfxDict = new Dictionary<string, AudioSource>();
    private Dictionary<string, List<AudioSource>> sfxGroupDict = new Dictionary<string, List<AudioSource>>();
    private Dictionary<string, AudioSource> musicDict = new Dictionary<string, AudioSource>();

    // Properties for volume access
    public float SfxVolume => sfxVolume;
    public float MusicVolume => musicVolume;
    public float MasterVolume => masterVolume;

    public void Awake() {
        // Singleton setup
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this) {
            Destroy(gameObject);
        }
        InitializeAudioSources();
    }

    private void Start() {
        LoadVolumeSettings();

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = sfxVolume;

        if (musicVolumeSlider != null)
            musicVolumeSlider.value = musicVolume;
    }

    private void Update() {

        if (sfxVolumeSlider != null && sfxVolume != sfxVolumeSlider.value) {
            sfxVolume = sfxVolumeSlider.value;
            ApplySfxVolume();
            SaveVolumeSettings();
        }

        if (musicVolumeSlider != null && musicVolume != musicVolumeSlider.value) {
            musicVolume = musicVolumeSlider.value;
            ApplyMusicVolume();
            SaveVolumeSettings();
        }
    }

    // Apply volume to all audio sources
    private void ApplyAllVolumes() {
        ApplySfxVolume();
        ApplyMusicVolume();
    }

    // Apply volume to all SFX audio sources
    private void ApplySfxVolume() {
        foreach (var source in sfxDict.Values) {
            source.volume = sfxVolume * masterVolume;
        }
    }

    // Apply volume to all music audio sources
    private void ApplyMusicVolume() {
        foreach (var source in musicDict.Values) {
            source.volume = musicVolume * masterVolume;
        }
    }

    // Save volume settings to PlayerPrefs
    private void SaveVolumeSettings() {
        PlayerPrefs.SetFloat("SfxVolume", sfxVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.Save();
    }

    // Load volume settings from PlayerPrefs
    private void LoadVolumeSettings() {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 0.5f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);

        // Apply volumes immediately after loading
        ApplyAllVolumes();
    }

    private void InitializeAudioSources() {
        // Collect all SFX audio sources
        if (sfxContainer != null) {
            foreach (AudioSource source in sfxContainer.GetComponentsInChildren<AudioSource>()) {
                string name = source.gameObject.name;

                // Check if this is a variation (name contains _ followed by a number)
                string baseName = GetBaseName(name);

                // Add to regular dictionary
                sfxDict.Add(name, source);

                // Add to grouped dictionary for variations
                if (!sfxGroupDict.ContainsKey(baseName)) {
                    sfxGroupDict[baseName] = new List<AudioSource>();
                }
                sfxGroupDict[baseName].Add(source);

                Debug.Log($"Added SFX: {name} (Base: {baseName})");
            }
        }

        // Collect all music audio sources
        if (musicContainer != null) {
            foreach (AudioSource source in musicContainer.GetComponentsInChildren<AudioSource>()) {
                musicDict.Add(source.gameObject.name, source);
                Debug.Log($"Added Music: {source.gameObject.name}");
            }
        }
    }

    // Extract base name (e.g. "SFX_PickUp" from "SFX_PickUp_1")
    private string GetBaseName(string fullName) {
        // Check if the name has a numeric suffix pattern like "_1"
        int lastUnderscoreIndex = fullName.LastIndexOf('_');
        if (lastUnderscoreIndex > 0 && lastUnderscoreIndex < fullName.Length - 1) {
            string potentialNumber = fullName.Substring(lastUnderscoreIndex + 1);
            if (int.TryParse(potentialNumber, out _)) {
                return fullName.Substring(0, lastUnderscoreIndex);
            }
        }
        return fullName;
    }

    // Public methods to set sliders programmatically if needed
    public void SetSliderReferences(Slider master, Slider sfx, Slider music) {
        sfxVolumeSlider = sfx;
        musicVolumeSlider = music;

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = sfxVolume;

        if (musicVolumeSlider != null)
            musicVolumeSlider.value = musicVolume;
    }

    // Audio playback methods
    public void PlaySFX(string name) {
        if (sfxDict.TryGetValue(name, out AudioSource source)) {
            source.pitch = Random.Range(minPitch, maxPitch);
            source.Play();
        }
        else {
            Debug.LogError($"SFX not found: {name}");
        }
    }

    public void PlayRandomSFX(string baseName) {
        if (sfxGroupDict.TryGetValue(baseName, out List<AudioSource> sources) && sources.Count > 0) {
            int randomIndex = Random.Range(0, sources.Count);
            sources[randomIndex].pitch = Random.Range(minPitch, maxPitch);
            sources[randomIndex].Play();
        }
        else {
            // Fallback to exact match
            PlaySFX(baseName);
        }
    }

    public void StopSFX(string name) {
        if (sfxDict.TryGetValue(name, out AudioSource source)) {
            source.Stop();
        }
        else {
            Debug.LogError($"SFX not found: {name}");
        }
    }

    public void PlayMusic(string name) {
        if (musicDict.TryGetValue(name, out AudioSource source)) {
            source.Play();
        }
        else {
            Debug.LogError($"Music not found: {name}");
        }
    }

    public void StopMusic(string name) {
        if (musicDict.TryGetValue(name, out AudioSource source)) {
            source.Stop();
        }
        else {
            Debug.LogError($"Music not found: {name}");
        }
    }

    public void StopAllMusic() {
        foreach (var source in musicDict.Values) {
            source.Stop();
        }
    }

    public void StopAllSFX() {
        foreach (var source in sfxDict.Values) {
            source.Stop();
        }
    }

    // Backwards compatibility methods if you still need to use indices in some places
    public void PlaySFX(int index) {
        if (index >= 0 && index < sfxDict.Count) {
            var sources = new List<AudioSource>(sfxDict.Values);
            sources[index].pitch = Random.Range(minPitch, maxPitch);
            sources[index].Play();
        }
        else {
            Debug.LogError($"SFX index out of range: {index}");
        }
    }

    public void StopSFX(int index) {
        if (index >= 0 && index < sfxDict.Count) {
            var sources = new List<AudioSource>(sfxDict.Values);
            sources[index].Stop();
        }
        else {
            Debug.LogError($"SFX index out of range: {index}");
        }
    }

    public void PlayMusic(int index) {
        if (index >= 0 && index < musicDict.Count) {
            var sources = new List<AudioSource>(musicDict.Values);
            sources[index].Play();
        }
        else {
            Debug.LogError($"Music index out of range: {index}");
        }
    }

    public void StopMusic(int index) {
        if (index >= 0 && index < musicDict.Count) {
            var sources = new List<AudioSource>(musicDict.Values);
            sources[index].Stop();
        }
        else {
            Debug.LogError($"Music index out of range: {index}");
        }
    }
}