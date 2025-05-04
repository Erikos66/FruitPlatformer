using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class UI_LevelSelectManager : MonoBehaviour {
	[SerializeField] private GameObject levelButtonPrefab;
	[SerializeField] private Transform carouselContainer;
	[SerializeField] private string levelNamePrefix = "Level_";
	[SerializeField] private string levelDisplayNameFormat = "Level {0}";
	[SerializeField] private bool useBuiltInScenes = true;
	[SerializeField] private bool showLockedLevels = false;
	[SerializeField] private GameObject firstSelectedButton;

	[Header("Carousel Settings")]
	[SerializeField] private Button leftArrowButton;
	[SerializeField] private Button rightArrowButton;
	[SerializeField] private float transitionSpeed = 5f;
	[SerializeField] private float buttonSpacing = 1200f;  // Keep for fallback
	[SerializeField] private Transform selectedButtonPosition;  // Position for the currently selected button
	[SerializeField] private Transform previousButtonPosition;  // Position for the button to the left
	[SerializeField] private Transform nextButtonPosition;      // Position for the button to the right
	[SerializeField] private bool useCoroutineAnimation = true;
	[SerializeField] private AnimationClip slideLeftAnimation;
	[SerializeField] private AnimationClip slideRightAnimation;

	[Header("Level Preview Settings")]
	[SerializeField] private bool useLevelPreviewImages = true;
	[SerializeField] private Sprite defaultLevelPreview;
	[SerializeField] private List<LevelPreviewData> levelPreviews = new List<LevelPreviewData>();

	// For manual level configuration if not using built-in scenes
	[System.Serializable]
	public class LevelInfo {
		public string displayName;
		public string sceneName;
	}

	[System.Serializable]
	public class LevelPreviewData {
		public string sceneName;
		public Sprite previewImage;
	}

	[SerializeField] private List<LevelInfo> manualLevelList = new List<LevelInfo>();

	private List<GameObject> levelButtons = new List<GameObject>();
	private int currentLevelIndex = 0;
	private Coroutine slideCoroutine;
	private Animator carouselAnimator;
	private bool isAnimating = false;

	private void Awake() {
		if (carouselContainer != null && carouselContainer.TryGetComponent<Animator>(out var animator)) {
			carouselAnimator = animator;
		}
	}

	private void Start() {
		GenerateLevelButtons();
		SetupNavigation();
	}

	void OnEnable() {
		if (firstSelectedButton != null) {
			EventSystem.current.SetSelectedGameObject(firstSelectedButton);
		}
		else if (levelButtons.Count > 0) {
			EventSystem.current.SetSelectedGameObject(levelButtons[currentLevelIndex]);
		}

		UpdateNavigationButtons();
	}

	private void SetupNavigation() {
		if (leftArrowButton != null) {
			leftArrowButton.onClick.AddListener(NavigateToPreviousLevel);
		}

		if (rightArrowButton != null) {
			rightArrowButton.onClick.AddListener(NavigateToNextLevel);
		}

		UpdateNavigationButtons();
	}

	public void NavigateToNextLevel() {
		if (isAnimating || currentLevelIndex >= levelButtons.Count - 1)
			return;

		SlideToLevel(currentLevelIndex + 1);
	}

	public void NavigateToPreviousLevel() {
		if (isAnimating || currentLevelIndex <= 0)
			return;

		SlideToLevel(currentLevelIndex - 1);
	}

	private void SlideToLevel(int newIndex) {
		if (newIndex < 0 || newIndex >= levelButtons.Count || newIndex == currentLevelIndex)
			return;

		isAnimating = true;

		// Set the direction for animation
		bool slideLeft = newIndex > currentLevelIndex;

		if (useCoroutineAnimation) {
			// Cancel any ongoing animation
			if (slideCoroutine != null) {
				StopCoroutine(slideCoroutine);
			}

			slideCoroutine = StartCoroutine(SlideAnimation(newIndex, slideLeft));
		}
		else if (carouselAnimator != null) {
			// Use Unity animation system
			carouselAnimator.SetInteger("TargetIndex", newIndex);
			carouselAnimator.SetBool("SlideLeft", slideLeft);

			if (slideLeft) {
				carouselAnimator.Play(slideLeftAnimation.name);
			}
			else {
				carouselAnimator.Play(slideRightAnimation.name);
			}

			// Set a delay to consider animation finished
			StartCoroutine(WaitForAnimationEnd(0.5f, newIndex));
		}
		else {
			// Update directly with no animation
			currentLevelIndex = newIndex;
			PositionButtonsForCarousel(); // This now handles transform-based positioning
			isAnimating = false;
			UpdateNavigationButtons();
		}

		// Play UI selection sound
		if (AudioManager.Instance != null) {
			AudioManager.Instance.PlayRandomSFX("SFX_MenuSelect");
		}
	}

	private IEnumerator SlideAnimation(int newIndex, bool slideLeft) {
		// Store original values for backup
		float startTime = Time.time;
		int oldIndex = currentLevelIndex;

		// Check if we're using transform positions or the old spacing method
		bool useTransformPositions = (selectedButtonPosition != null && previousButtonPosition != null && nextButtonPosition != null);

		if (useTransformPositions) {
			// Prepare dictionaries to store start and target positions/scales for each button
			Dictionary<GameObject, Vector3> startPositions = new Dictionary<GameObject, Vector3>();
			Dictionary<GameObject, Vector3> targetPositions = new Dictionary<GameObject, Vector3>();
			Dictionary<GameObject, Vector3> startScales = new Dictionary<GameObject, Vector3>();
			Dictionary<GameObject, Vector3> targetScales = new Dictionary<GameObject, Vector3>();

			// Set initial visibility and capture starting positions/scales
			for (int i = 0; i < levelButtons.Count; i++) {
				bool wasVisible = levelButtons[i].activeSelf;
				bool willBeVisible = (i >= newIndex - 1 && i <= newIndex + 1);

				// Store current positions and scales
				if (wasVisible || willBeVisible) {
					startPositions[levelButtons[i]] = levelButtons[i].transform.position;
					startScales[levelButtons[i]] = levelButtons[i].transform.localScale;
				}

				// Make all relevant buttons visible for the animation
				if (willBeVisible) {
					levelButtons[i].SetActive(true);
				}

				// Calculate target position and scale based on relationship to new index
				if (i == newIndex) {
					targetPositions[levelButtons[i]] = selectedButtonPosition.position;
					targetScales[levelButtons[i]] = selectedButtonPosition.localScale;
				}
				else if (i == newIndex - 1) {
					targetPositions[levelButtons[i]] = previousButtonPosition.position;
					targetScales[levelButtons[i]] = previousButtonPosition.localScale;
				}
				else if (i == newIndex + 1) {
					targetPositions[levelButtons[i]] = nextButtonPosition.position;
					targetScales[levelButtons[i]] = nextButtonPosition.localScale;
				}
			}

			// Animation loop
			while (Time.time < startTime + (1f / transitionSpeed)) {
				float t = (Time.time - startTime) * transitionSpeed;

				// Update each visible button's position and scale
				foreach (var button in targetPositions.Keys) {
					if (startPositions.ContainsKey(button)) {
						button.transform.position = Vector3.Lerp(startPositions[button], targetPositions[button], t);
					}

					if (startScales.ContainsKey(button)) {
						button.transform.localScale = Vector3.Lerp(startScales[button], targetScales[button], t);
					}
				}

				yield return null;
			}

			// Finalize positions and visibility
			for (int i = 0; i < levelButtons.Count; i++) {
				bool shouldBeVisible = (i >= newIndex - 1 && i <= newIndex + 1);
				levelButtons[i].SetActive(shouldBeVisible);

				if (shouldBeVisible) {
					// Set final positions and scales exactly
					if (i == newIndex) {
						levelButtons[i].transform.position = selectedButtonPosition.position;
						levelButtons[i].transform.localScale = selectedButtonPosition.localScale;
					}
					else if (i == newIndex - 1) {
						levelButtons[i].transform.position = previousButtonPosition.position;
						levelButtons[i].transform.localScale = previousButtonPosition.localScale;
					}
					else if (i == newIndex + 1) {
						levelButtons[i].transform.position = nextButtonPosition.position;
						levelButtons[i].transform.localScale = nextButtonPosition.localScale;
					}
				}
			}
		}
		else {
			// Fall back to the old spacing-based animation
			Vector3 startPosition = carouselContainer.localPosition;
			Vector3 targetPosition = new Vector3(-newIndex * buttonSpacing, 0, 0);

			while (Time.time < startTime + (1f / transitionSpeed)) {
				float t = (Time.time - startTime) * transitionSpeed;
				carouselContainer.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
				yield return null;
			}

			// Ensure we end at exactly the target position
			carouselContainer.localPosition = targetPosition;
		}

		// Update state
		currentLevelIndex = newIndex;
		isAnimating = false;
		UpdateNavigationButtons();

		// Update selected button
		if (levelButtons.Count > currentLevelIndex) {
			EventSystem.current.SetSelectedGameObject(levelButtons[currentLevelIndex]);
		}
	}

	private IEnumerator WaitForAnimationEnd(float delay, int newIndex) {
		yield return new WaitForSeconds(delay);

		currentLevelIndex = newIndex;
		isAnimating = false;
		UpdateNavigationButtons();

		// Update selected button
		if (levelButtons.Count > currentLevelIndex) {
			EventSystem.current.SetSelectedGameObject(levelButtons[currentLevelIndex]);
		}
	}

	private void UpdateNavigationButtons() {
		if (leftArrowButton != null) {
			leftArrowButton.interactable = (currentLevelIndex > 0);
		}

		if (rightArrowButton != null) {
			rightArrowButton.interactable = (currentLevelIndex < levelButtons.Count - 1);
		}
	}

	public void RefreshLevelButtons() {
		GenerateLevelButtons();
	}

	private void GenerateLevelButtons() {
		if (carouselContainer == null) {
			Debug.LogError("Carousel container is not assigned!");
			return;
		}

		if (levelButtonPrefab == null) {
			Debug.LogError("Level button prefab is not assigned!");
			return;
		}

		// Clear existing buttons
		foreach (Transform child in carouselContainer) {
			Destroy(child.gameObject);
		}
		levelButtons.Clear();

		if (useBuiltInScenes) {
			GenerateButtonsFromScenes();
		}
		else {
			GenerateButtonsFromManualList();
		}

		// Position the carousel to show the first button
		carouselContainer.localPosition = Vector3.zero;
		currentLevelIndex = 0;

		// Update navigation button states
		UpdateNavigationButtons();
	}

	private void GenerateButtonsFromScenes() {
		List<string> levelScenes = new List<string>();

		// Find all scenes that start with the level prefix
		for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
			string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
			string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

			if (sceneName.StartsWith(levelNamePrefix)) {
				levelScenes.Add(sceneName);
			}
		}

		// Sort levels by their number
		levelScenes.Sort((a, b) => {
			string aNum = a.Substring(levelNamePrefix.Length);
			string bNum = b.Substring(levelNamePrefix.Length);

			if (int.TryParse(aNum, out int aVal) && int.TryParse(bNum, out int bVal)) {
				return aVal.CompareTo(bVal);
			}
			return string.Compare(a, b);
		});

		// Create a button for each level
		for (int i = 0; i < levelScenes.Count; i++) {
			string sceneName = levelScenes[i];
			string levelNumber = sceneName.Substring(levelNamePrefix.Length);

			// Only show levels that are unlocked, unless showLockedLevels is true
			if (LevelManager.Instance.IsLevelUnlocked(sceneName) || showLockedLevels) {
				bool isUnlocked = LevelManager.Instance.IsLevelUnlocked(sceneName);

				// Try to parse level number
				if (int.TryParse(levelNumber, out int levelNum)) {
					CreateLevelButton(string.Format(levelDisplayNameFormat, levelNum), sceneName, isUnlocked);
				}
				else {
					CreateLevelButton(sceneName, sceneName, isUnlocked);
				}
			}
		}

		// Set the button positions for proper carousel movement
		PositionButtonsForCarousel();
	}

	private void GenerateButtonsFromManualList() {
		for (int i = 0; i < manualLevelList.Count; i++) {
			string sceneName = manualLevelList[i].sceneName;

			// Only show levels that are unlocked, unless showLockedLevels is true
			if (LevelManager.Instance.IsLevelUnlocked(sceneName) || showLockedLevels) {
				bool isUnlocked = LevelManager.Instance.IsLevelUnlocked(sceneName);
				CreateLevelButton(manualLevelList[i].displayName, sceneName, isUnlocked);
			}
		}

		// Set the button positions for proper carousel movement
		PositionButtonsForCarousel();
	}

	private void CreateLevelButton(string displayName, string sceneName, bool isUnlocked) {
		GameObject buttonObj = Instantiate(levelButtonPrefab, carouselContainer);
		LevelSelectButton levelButton = buttonObj.GetComponent<LevelSelectButton>();

		if (levelButton != null) {
			levelButton.Setup(displayName, sceneName, isUnlocked);

			// Set level preview image if enabled
			if (useLevelPreviewImages) {
				// Find the preview image for this scene
				Sprite previewImage = GetPreviewImageForScene(sceneName);
				levelButton.SetLevelPreviewImage(previewImage);
			}
		}
		else {
			Debug.LogError("LevelSelectButton component not found on button prefab!");
		}

		levelButtons.Add(buttonObj);
	}

	private Sprite GetPreviewImageForScene(string sceneName) {
		// Look for a matching preview image in our list
		foreach (LevelPreviewData previewData in levelPreviews) {
			if (previewData.sceneName == sceneName && previewData.previewImage != null) {
				return previewData.previewImage;
			}
		}

		// Return the default preview if no specific one is found
		return defaultLevelPreview;
	}

	private void PositionButtonsForCarousel() {
		if (selectedButtonPosition == null || previousButtonPosition == null || nextButtonPosition == null) {
			Debug.LogError("Button positions are not assigned! Falling back to spacing-based positioning.");
			// Fallback to the old spacing-based approach
			for (int i = 0; i < levelButtons.Count; i++) {
				levelButtons[i].transform.localPosition = new Vector3(i * buttonSpacing, 0, 0);
			}
			return;
		}

		// Hide buttons that are too far from current index to prevent crowding
		for (int i = 0; i < levelButtons.Count; i++) {
			// Only show the current button and immediate neighbors
			bool isVisible = (i >= currentLevelIndex - 1 && i <= currentLevelIndex + 1);
			levelButtons[i].SetActive(isVisible);

			if (isVisible) {
				// Position each button based on its relation to current index
				if (i == currentLevelIndex) {
					// Selected button
					levelButtons[i].transform.position = selectedButtonPosition.position;
					levelButtons[i].transform.localScale = selectedButtonPosition.localScale;
				}
				else if (i == currentLevelIndex - 1) {
					// Previous button
					levelButtons[i].transform.position = previousButtonPosition.position;
					levelButtons[i].transform.localScale = previousButtonPosition.localScale;
				}
				else if (i == currentLevelIndex + 1) {
					// Next button
					levelButtons[i].transform.position = nextButtonPosition.position;
					levelButtons[i].transform.localScale = nextButtonPosition.localScale;
				}
			}
		}
	}
}
