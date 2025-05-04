using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelSelectButton : MonoBehaviour {
	[SerializeField] private TextMeshProUGUI levelNameText;
	[SerializeField] private TextMeshProUGUI fruitInfoText;
	[SerializeField] private TextMeshProUGUI timeInfoText;
	[SerializeField] private Button button;
	[SerializeField] private GameObject lockedOverlay;
	[SerializeField] private Image backgroundImage;
	[SerializeField] private Image levelPreviewImage;

	private string sceneName;
	private UI_FadeEffect fadeEffect;
	private bool isUnlocked = true;

	private void Awake() {
		if (button == null)
			button = GetComponent<Button>();

		if (levelNameText == null)
			levelNameText = GetComponentInChildren<TextMeshProUGUI>();

		fadeEffect = Object.FindAnyObjectByType<UI_FadeEffect>();
	}

	private void Start() {
		button.onClick.AddListener(OnButtonClick);
	}

	public void Setup(string levelName, string scene, bool unlocked = true) {
		levelNameText.text = levelName;
		sceneName = scene;
		isUnlocked = unlocked;

		// Enable or disable the button based on whether the level is unlocked
		button.interactable = isUnlocked;

		// If we have a locked overlay, show/hide it
		if (lockedOverlay != null) {
			lockedOverlay.SetActive(!isUnlocked);
		}

		// Update fruit information if we have the fruitInfoText component
		if (fruitInfoText != null) {
			if (isUnlocked) {
				// Check if the level has been played before
				bool levelPlayed = SaveManager.Instance.HasLevelBeenPlayed(scene);

				if (levelPlayed) {
					// Get the collected and total fruit counts
					int collectedFruits = SaveManager.Instance.GetCollectedFruitsCount(scene);
					int totalFruits = SaveManager.Instance.GetTotalFruitsCount(scene);

					// Display the collected/total fruits
					fruitInfoText.text = $"Fruits: {collectedFruits}/{totalFruits}";
				}
				else {
					// Level hasn't been played yet, show unknown values
					fruitInfoText.text = "Fruits: ???/???";
				}

				fruitInfoText.gameObject.SetActive(true);
			}
			else {
				fruitInfoText.gameObject.SetActive(false);
			}
		}

		// Update time information if we have the timeInfoText component
		if (timeInfoText != null) {
			float bestTime = SaveManager.Instance.GetLevelBestTime(scene);

			if (bestTime > 0f && isUnlocked) {
				// Format time using the TimerManager's format method
				string formattedTime = TimerManager.Instance.FormatTime(bestTime);
				timeInfoText.text = $"Best Time: {formattedTime}";
				timeInfoText.gameObject.SetActive(true);
			}
			else {
				timeInfoText.gameObject.SetActive(false);
			}
		}
	}

	public void SetLevelPreviewImage(Sprite previewImage) {
		if (levelPreviewImage != null && previewImage != null) {
			levelPreviewImage.sprite = previewImage;
			levelPreviewImage.gameObject.SetActive(true);
		}
		else if (levelPreviewImage != null) {
			levelPreviewImage.gameObject.SetActive(false);
		}

		// If we have a background image component and no separate preview component,
		// use the background image instead
		if (backgroundImage != null && levelPreviewImage == null && previewImage != null) {
			backgroundImage.sprite = previewImage;
		}
	}

	private void OnButtonClick() {
		// Don't allow clicking if the level is locked
		if (!isUnlocked)
			return;

		// Play UI button click sound
		AudioManager.Instance.PlayRandomSFX("SFX_MenuSelect");

		if (fadeEffect != null) {
			fadeEffect.ScreenFadeEffect(1f, 1f, LoadLevel);
		}
		else {
			LoadLevel();
		}
	}

	private void LoadLevel() {
		// Use LevelManager to load the scene
		LevelManager.Instance.LoadLevel(sceneName);
	}
}
