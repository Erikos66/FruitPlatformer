using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour {

	#region Variables

	// Singleton instance
	public static AudioManager Instance { get; private set; }

	[Header("Audio Source Containers")]
	[SerializeField] private Transform _sfxContainer;      // Container for all SFX audio sources
	[SerializeField] private Transform _musicContainer;    // Container for all music audio sources

	[Header("Pitch Settings")]
	[SerializeField] private float _minPitch = 0.9f;       // Minimum random pitch variation
	[SerializeField] private float _maxPitch = 1.1f;       // Maximum random pitch variation

	private float _masterVolume = 1f;                      // Master volume multiplier
	private float _sfxVolume = 1f;                         // SFX volume multiplier
	private float _musicVolume = 1f;                       // Music volume multiplier

	// Dictionaries to store audio sources by name
	private Dictionary<string, AudioSource> _sfxDict = new Dictionary<string, AudioSource>();
	private Dictionary<string, List<AudioSource>> _sfxGroupDict = new Dictionary<string, List<AudioSource>>();
	private Dictionary<string, AudioSource> _musicDict = new Dictionary<string, AudioSource>();

	// Properties for volume access
	public float SfxVolume => _sfxVolume;
	public float MusicVolume => _musicVolume;
	public float MasterVolume => _masterVolume;

	#endregion

	#region Unity Methods

	private void Awake() {
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
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Sets the master volume level and applies it to all audio sources
	/// </summary>
	/// <param name="volume">Volume level between 0 and 1</param>
	public void SetMasterVolume(float volume) {
		_masterVolume = volume;
		ApplyAllVolumes();
		SaveVolumeSettings();
	}

	/// <summary>
	/// Sets the SFX volume level and applies it to all SFX audio sources
	/// </summary>
	/// <param name="volume">Volume level between 0 and 1</param>
	public void SetSfxVolume(float volume) {
		_sfxVolume = volume;
		ApplySfxVolume();
		SaveVolumeSettings();
	}

	/// <summary>
	/// Sets the music volume level and applies it to all music audio sources
	/// </summary>
	/// <param name="volume">Volume level between 0 and 1</param>
	public void SetMusicVolume(float volume) {
		_musicVolume = volume;
		ApplyMusicVolume();
		SaveVolumeSettings();
	}

	/// <summary>
	/// Plays a specific SFX by name with random pitch variation
	/// </summary>
	/// <param name="name">Name of the SFX to play</param>
	public void PlaySFX(string name) {
		if (_sfxDict.TryGetValue(name, out AudioSource source)) {
			source.pitch = Random.Range(_minPitch, _maxPitch);
			source.Play();
		}
		else {
			Debug.LogError($"SFX not found: {name}");
		}
	}

	/// <summary>
	/// Plays a random SFX variation from a named group
	/// </summary>
	/// <param name="baseName">Base name of the SFX group</param>
	public void PlayRandomSFX(string baseName) {
		if (_sfxGroupDict.TryGetValue(baseName, out List<AudioSource> sources) && sources.Count > 0) {
			int randomIndex = Random.Range(0, sources.Count);
			sources[randomIndex].pitch = Random.Range(_minPitch, _maxPitch);
			sources[randomIndex].Play();
		}
		else {
			// Fallback to exact match
			PlaySFX(baseName);
		}
	}

	/// <summary>
	/// Plays a specific SFX once without looping
	/// </summary>
	/// <param name="name">Name of the SFX to play</param>
	public void PlaySFXOnce(string name) {
		if (_sfxDict.TryGetValue(name, out AudioSource source)) {
			source.pitch = Random.Range(_minPitch, _maxPitch);
			source.PlayOneShot(source.clip, _sfxVolume * _masterVolume);
		}
		else {
			Debug.LogError($"SFX not found: {name}");
		}
	}

	/// <summary>
	/// Stops a specific SFX
	/// </summary>
	/// <param name="name">Name of the SFX to stop</param>
	public void StopSFX(string name) {
		if (_sfxDict.TryGetValue(name, out AudioSource source)) {
			source.Stop();
		}
		else {
			Debug.LogError($"SFX not found: {name}");
		}
	}

	/// <summary>
	/// Plays a specific music track
	/// </summary>
	/// <param name="name">Name of the music track to play</param>
	public void PlayMusic(string name) {
		if (_musicDict.TryGetValue(name, out AudioSource source)) {
			source.Play();
		}
		else {
			Debug.LogError($"Music not found: {name}");
		}
	}

	/// <summary>
	/// Stops a specific music track
	/// </summary>
	/// <param name="name">Name of the music track to stop</param>
	public void StopMusic(string name) {
		if (_musicDict.TryGetValue(name, out AudioSource source)) {
			source.Stop();
		}
		else {
			Debug.LogError($"Music not found: {name}");
		}
	}

	/// <summary>
	/// Stops all music tracks currently playing
	/// </summary>
	public void StopAllMusic() {
		foreach (var source in _musicDict.Values) {
			source.Stop();
		}
	}

	/// <summary>
	/// Stops all SFX currently playing
	/// </summary>
	public void StopAllSFX() {
		foreach (var source in _sfxDict.Values) {
			source.Stop();
		}
	}

	/// <summary>
	/// Plays a specific SFX by index (legacy method)
	/// </summary>
	/// <param name="index">Index of the SFX to play</param>
	public void PlaySFX(int index) {
		if (index >= 0 && index < _sfxDict.Count) {
			var sources = new List<AudioSource>(_sfxDict.Values);
			sources[index].pitch = Random.Range(_minPitch, _maxPitch);
			sources[index].Play();
		}
		else {
			Debug.LogError($"SFX index out of range: {index}");
		}
	}

	/// <summary>
	/// Stops a specific SFX by index (legacy method)
	/// </summary>
	/// <param name="index">Index of the SFX to stop</param>
	public void StopSFX(int index) {
		if (index >= 0 && index < _sfxDict.Count) {
			var sources = new List<AudioSource>(_sfxDict.Values);
			sources[index].Stop();
		}
		else {
			Debug.LogError($"SFX index out of range: {index}");
		}
	}

	/// <summary>
	/// Plays a specific music track by index (legacy method)
	/// </summary>
	/// <param name="index">Index of the music track to play</param>
	public void PlayMusic(int index) {
		if (index >= 0 && index < _musicDict.Count) {
			var sources = new List<AudioSource>(_musicDict.Values);
			sources[index].Play();
		}
		else {
			Debug.LogError($"Music index out of range: {index}");
		}
	}

	/// <summary>
	/// Stops a specific music track by index (legacy method)
	/// </summary>
	/// <param name="index">Index of the music track to stop</param>
	public void StopMusic(int index) {
		if (index >= 0 && index < _musicDict.Count) {
			var sources = new List<AudioSource>(_musicDict.Values);
			sources[index].Stop();
		}
		else {
			Debug.LogError($"Music index out of range: {index}");
		}
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Applies volume settings to all audio sources
	/// </summary>
	private void ApplyAllVolumes() {
		ApplySfxVolume();
		ApplyMusicVolume();
	}

	/// <summary>
	/// Applies volume settings to all SFX audio sources
	/// </summary>
	private void ApplySfxVolume() {
		foreach (var source in _sfxDict.Values) {
			source.volume = _sfxVolume * _masterVolume;
		}
	}

	/// <summary>
	/// Applies volume settings to all music audio sources
	/// </summary>
	private void ApplyMusicVolume() {
		foreach (var source in _musicDict.Values) {
			source.volume = _musicVolume * _masterVolume;
		}
	}

	/// <summary>
	/// Saves volume settings to PlayerPrefs
	/// </summary>
	private void SaveVolumeSettings() {
		PlayerPrefs.SetFloat("SfxVolume", _sfxVolume);
		PlayerPrefs.SetFloat("MusicVolume", _musicVolume);
		PlayerPrefs.SetFloat("MasterVolume", _masterVolume);
		PlayerPrefs.Save();
	}

	/// <summary>
	/// Loads volume settings from PlayerPrefs with default fallback values
	/// </summary>
	private void LoadVolumeSettings() {
		_masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
		_sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 0.5f);
		_musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);

		// Apply volumes immediately after loading
		ApplyAllVolumes();
	}

	/// <summary>
	/// Initializes audio sources by collecting them from containers
	/// </summary>
	private void InitializeAudioSources() {
		// Collect all SFX audio sources
		if (_sfxContainer != null) {
			foreach (AudioSource source in _sfxContainer.GetComponentsInChildren<AudioSource>()) {
				string name = source.gameObject.name;

				// Check if this is a variation (name contains _ followed by a number)
				string baseName = GetBaseName(name);

				// Add to regular dictionary
				_sfxDict.Add(name, source);

				// Add to grouped dictionary for variations
				if (!_sfxGroupDict.ContainsKey(baseName)) {
					_sfxGroupDict[baseName] = new List<AudioSource>();
				}
				_sfxGroupDict[baseName].Add(source);

				Debug.Log($"Added SFX: {name} (Base: {baseName})");
			}
		}

		// Collect all music audio sources
		if (_musicContainer != null) {
			foreach (AudioSource source in _musicContainer.GetComponentsInChildren<AudioSource>()) {
				_musicDict.Add(source.gameObject.name, source);
				Debug.Log($"Added Music: {source.gameObject.name}");
			}
		}
	}

	/// <summary>
	/// Extracts the base name from a full audio source name by removing numeric suffix
	/// </summary>
	/// <param name="fullName">Full name of the audio source</param>
	/// <returns>Base name without numeric suffix</returns>
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

	#endregion
}
