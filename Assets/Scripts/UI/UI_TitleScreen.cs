using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_TitleScreen : MonoBehaviour {
	public string sceneName;
	private UI_FadeEffect fadeEffect;
	private DefaultInputActions defaultInputActions;

	// Store the last selected GameObject when using controller/keyboard
	private GameObject lastSelectedObject;
	// Flag to track if we're using mouse or keyboard/gamepad
	private bool usingMouse = true;

	[SerializeField] private GameObject[] UIElements;
	[SerializeField] private GameObject levelSelectUI; // Reference to the level select UI panel

	private void Awake() {
		fadeEffect = GetComponentInChildren<UI_FadeEffect>();

		// Initialize the input actions
		defaultInputActions = new DefaultInputActions();
		defaultInputActions.Enable();

		// Detect mouse movement for mouse usage
		defaultInputActions.UI.Point.started += _ => OnMouseMove();
		defaultInputActions.UI.Point.performed += _ => OnMouseMove();
		defaultInputActions.UI.Click.started += _ => OnMouseMove(); // Also detect clicks to switch to mouse mode

		// Detect ANY keyboard/gamepad navigation input
		defaultInputActions.UI.Navigate.performed += _ => OnKeyboardOrGamepadInput();

		// Additional keyboard input detection for WASD, arrow keys and Tab key
		defaultInputActions.UI.Submit.performed += _ => OnKeyboardOrGamepadInput();
		defaultInputActions.UI.Cancel.performed += _ => OnKeyboardOrGamepadInput();
		defaultInputActions.UI.TrackedDevicePosition.performed += _ => OnKeyboardOrGamepadInput();
	}

	private void OnEnable() {
		if (defaultInputActions == null) {
			defaultInputActions = new DefaultInputActions();
		}
		defaultInputActions.Enable();
	}

	private void OnDisable() {
		defaultInputActions.Disable();
	}

	private void Start() {
		fadeEffect.ScreenFadeEffect(0f, 1.5f);

		// No longer automatically select anything on start
		// Clear any selection that might be present
		EventSystem.current.SetSelectedGameObject(null);

		// Check if we should show the level select UI
		if (LevelManager.Instance != null && LevelManager.Instance.showLevelSelectOnMainMenu) {
			// Reset the flag
			LevelManager.Instance.showLevelSelectOnMainMenu = false;

			// Wait a brief moment to ensure UI is fully loaded, then show level select
			Invoke(nameof(ShowLevelSelect), 0.1f);
		}
	}

	private void Update() {
		UpdateSelectedMenuItem();
	}

	private void ShowLevelSelect() {
		// Show the level select UI
		if (levelSelectUI != null) {
			SwitchUI(levelSelectUI);

			// Find the first selectable element but don't select it yet
			// It will only be selected when keyboard/gamepad input is detected
			Selectable firstSelectable = levelSelectUI.GetComponentInChildren<Selectable>();
			if (firstSelectable != null) {
				lastSelectedObject = firstSelectable.gameObject;
				// Don't auto-select: EventSystem.current.SetSelectedGameObject(lastSelectedObject);
			}
		}
	}

	private void OnMouseMove() {
		// When mouse moves, always set to mouse input mode and unselect elements
		usingMouse = true;

		// Store the current selection before deselecting
		if (EventSystem.current.currentSelectedGameObject != null) {
			lastSelectedObject = EventSystem.current.currentSelectedGameObject;
			// Deselect UI elements when mouse moves
			EventSystem.current.SetSelectedGameObject(null);
		}
	}
	private void OnKeyboardOrGamepadInput() {
		// Always mark that we're using keyboard/gamepad and show selection
		usingMouse = false;

		// Restore last selected object or find a new one
		if (lastSelectedObject != null && lastSelectedObject.activeInHierarchy) {
			EventSystem.current.SetSelectedGameObject(lastSelectedObject);
		}
		else {
			// Find a selectable element if the previous one isn't valid
			SelectFirstAvailableButton();
		}
	}
	private void UpdateSelectedMenuItem() {
		// If using keyboard/gamepad but nothing is selected
		if (!usingMouse && EventSystem.current.currentSelectedGameObject == null) {
			// Restore last selection if it exists and is active
			if (lastSelectedObject != null && lastSelectedObject.activeInHierarchy) {
				EventSystem.current.SetSelectedGameObject(lastSelectedObject);
			}
			else {
				// Find a selectable element if the previous one isn't valid
				SelectFirstAvailableButton();
			}
		}

		// When using mouse, ensure nothing is selected
		if (usingMouse && EventSystem.current.currentSelectedGameObject != null) {
			EventSystem.current.SetSelectedGameObject(null);
		}

		// When keyboard/gamepad is used, update the last selected object
		if (!usingMouse && EventSystem.current.currentSelectedGameObject != null) {
			lastSelectedObject = EventSystem.current.currentSelectedGameObject;
		}
	}

	// Helper method to find and select the first available selectable element
	private void SelectFirstAvailableButton() {
		// Find the currently active UI container
		GameObject activeContainer = null;
		foreach (GameObject uiElement in UIElements) {
			if (uiElement.activeInHierarchy) {
				activeContainer = uiElement;
				break;
			}
		}

		if (activeContainer != null) {
			// Try to find selectable elements in the active container
			Selectable[] selectables = activeContainer.GetComponentsInChildren<Selectable>();

			// Find first interactable selectable
			Selectable firstInteractable = null;
			foreach (Selectable selectable in selectables) {
				if (selectable.interactable && selectable.gameObject.activeInHierarchy) {
					firstInteractable = selectable;
					break;
				}
			}

			if (firstInteractable != null) {
				lastSelectedObject = firstInteractable.gameObject;
				EventSystem.current.SetSelectedGameObject(lastSelectedObject);
			}
		}
	}

	public void NewGame() {
		// Play UI button click sound
		AudioManager.Instance.PlayRandomSFX("SFX_MenuSelect");
		fadeEffect.ScreenFadeEffect(1f, 1.5f, LoadLevelScene);
	}
	private void LoadLevelScene() {
		SceneManager.LoadScene(sceneName);
	}

	public void QuitGame() {
		// Play UI button click sound
		AudioManager.Instance.PlayRandomSFX("SFX_MenuSelect");
		Application.Quit();
		Debug.Log("Quit Game");
	}

	public void SwitchUI(GameObject uiToEnable) {
		// Play UI button click sound
		AudioManager.Instance.PlayRandomSFX("SFX_MenuSelect");

		foreach (GameObject uiElement in UIElements) {
			uiElement.SetActive(false);
		}
		uiToEnable.SetActive(true);

		// After switching UI, find the first selectable element but don't select it
		// Store it for when keyboard/gamepad is used
		Invoke(nameof(FindFirstSelectableWithoutSelecting), 0.05f);
	}

	// New method to find the first selectable but not select it
	private void FindFirstSelectableWithoutSelecting() {
		// Find the currently active UI container
		GameObject activeContainer = null;
		foreach (GameObject uiElement in UIElements) {
			if (uiElement.activeInHierarchy) {
				activeContainer = uiElement;
				break;
			}
		}

		if (activeContainer != null) {
			// Try to find selectable elements in the active container
			Selectable[] selectables = activeContainer.GetComponentsInChildren<Selectable>();

			// Find first interactable selectable
			Selectable firstInteractable = null;
			foreach (Selectable selectable in selectables) {
				if (selectable.interactable && selectable.gameObject.activeInHierarchy) {
					firstInteractable = selectable;
					break;
				}
			}

			if (firstInteractable != null) {
				lastSelectedObject = firstInteractable.gameObject;
				// Don't select it, just remember it for later when keyboard/gamepad is used
			}
		}
	}

	/// <summary>
	/// Unlocks all levels in the game and refreshes the level select UI.
	/// Assign this to a button in the Unity Editor.
	/// </summary>
	public void UnlockAllLevels() {
		// Play UI button click sound
		AudioManager.Instance.PlayRandomSFX("SFX_MenuSelect");
		LevelManager.Instance.UnlockAllLevels();
		Debug.Log("All levels have been unlocked!");
	}
}
