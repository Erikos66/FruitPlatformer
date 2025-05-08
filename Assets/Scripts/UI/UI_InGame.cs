using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class UI_InGame : MonoBehaviour {
	public static UI_InGame Instance { get; private set; }
	public UI_FadeEffect FadeEffect { get; private set; }

	[SerializeField] private TextMeshProUGUI timerText;
	[SerializeField] private TextMeshProUGUI fruitCountText; // Added fruit counter text
	[SerializeField] private GameObject pauseMenu; // Reference to the pause menu UI

	// On-screen controls
	[SerializeField] private GameObject onScreenJoystick; // Reference to the on-screen joystick
	[SerializeField] private GameObject onScreenButtonA; // Reference to on-screen button A
	[SerializeField] private GameObject onScreenButtonB; // Reference to on-screen button B

	private bool showTimer = true;
	private bool showFruitCount = true;
	private Player player; // Reference to the Player component

	// Use DefaultInputActions instead of PlayerInput
	private DefaultInputActions defaultInputActions;
	private PlayerInput playerInput; // Keep original PlayerInput for Pause functionality

	// Store the last selected UI element when using controller/keyboard
	private GameObject lastSelectedPauseMenuItem;
	// Flag to track if we're using mouse or keyboard/gamepad
	private bool usingMouse = false;

	// Flag to track if a gamepad is currently connected
	private bool isGamepadConnected = false;
	// Flag to track if we're on a mobile platform
	private bool isMobilePlatform = false;
	// Flag to prevent recursive SetActive calls
	private bool isUpdatingControls = false;
	// Flag to track if initial setup is complete
	private bool initialSetupComplete = false;

	private void Awake() {
		if (Instance == null) {
			Instance = this;
			FadeEffect = GetComponent<UI_FadeEffect>();
		}
		else {
			Destroy(gameObject);
		}
		FadeEffect = GetComponentInChildren<UI_FadeEffect>();
		if (FadeEffect == null) {
			Debug.LogError("FadeEffect is not assigned in the inspector.");
		}

		if (pauseMenu == null) {
			Debug.LogError("Pause menu is not assigned in the inspector.");
		}

		// Check if we're on a mobile platform
		CheckPlatform();

		// Initialize the PlayerInput for Pause functionality
		playerInput = new PlayerInput();
		playerInput.Enable();
		playerInput.UI.Pause.performed += ctx => PauseToggle();

		// Initialize DefaultInputActions for UI navigation
		defaultInputActions = new DefaultInputActions();
		defaultInputActions.Enable();

		// Only detect mouse movement for mouse usage, not click
		defaultInputActions.UI.Point.performed += _ => OnMouseMove();

		// Detect keyboard/gamepad navigation input
		defaultInputActions.UI.Navigate.performed += _ => OnKeyboardOrGamepadInput();
		defaultInputActions.UI.Submit.performed += _ => OnKeyboardOrGamepadInput();
		defaultInputActions.UI.Cancel.performed += _ => OnKeyboardOrGamepadInput();

		// Subscribe to device change events
		InputSystem.onDeviceChange += OnInputDeviceChange;

		// Delay the initial gamepad check to prevent activation issues during initialization
		StartCoroutine(DelayedInitialSetup());
	}

	void OnEnable() {
		// Enable input actions
		if (playerInput != null) {
			playerInput.Enable();
		}

		if (defaultInputActions == null) {
			defaultInputActions = new DefaultInputActions();
		}
		defaultInputActions.Enable();
	}

	void OnDisable() {
		// Disable input actions
		if (playerInput != null) {
			playerInput.Disable();
		}

		if (defaultInputActions != null) {
			defaultInputActions.Disable();
		}

		// Unsubscribe from device change events
		InputSystem.onDeviceChange -= OnInputDeviceChange;
	}

	// Coroutine to delay initial gamepad check
	private IEnumerator DelayedInitialSetup() {
		// Wait for next frame to ensure all components are initialized
		yield return null;

		// Check if any gamepad is already connected
		CheckForConnectedGamepad();
		initialSetupComplete = true;
	}

	// Detect platform type and set flags
	private void CheckPlatform() {
		// Check if we're on a mobile platform (Android or iOS)
#if UNITY_ANDROID || UNITY_IOS
		isMobilePlatform = true;
#else
		isMobilePlatform = false;
#endif

		// Note: We don't update controls visibility here anymore
		// This will be handled by the DelayedInitialSetup coroutine
	}

	// Check if any gamepad is already connected
	private void CheckForConnectedGamepad() {
		var gamepads = Gamepad.all;
		isGamepadConnected = gamepads.Count > 0;

		// Update on-screen controls visibility
		UpdateOnScreenControlsVisibility();
	}

	// Handle device connection/disconnection events
	private void OnInputDeviceChange(InputDevice device, InputDeviceChange change) {
		// Skip device change events until initial setup is complete
		if (!initialSetupComplete) return;

		if (device is Gamepad) {
			if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected) {
				isGamepadConnected = true;
				Debug.Log("Gamepad connected: " + device.name);
			}
			else if (change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected) {
				// Check if there are still other gamepads connected
				var gamepads = Gamepad.all;
				isGamepadConnected = gamepads.Count > 0;
				Debug.Log("Gamepad disconnected. Remaining gamepads: " + gamepads.Count);
			}

			// Update on-screen controls visibility whenever gamepad connection status changes
			UpdateOnScreenControlsVisibility();
		}
	}

	// Update the visibility of on-screen controls based on platform and connected devices
	private void UpdateOnScreenControlsVisibility() {
		// Prevent recursive SetActive calls
		if (isUpdatingControls) return;

		isUpdatingControls = true;

		try {
			bool shouldShowControls = isMobilePlatform && !isGamepadConnected;

			// Only update if the control objects exist
			if (onScreenJoystick != null && onScreenJoystick.activeSelf != shouldShowControls) {
				onScreenJoystick.SetActive(shouldShowControls);
			}

			if (onScreenButtonA != null && onScreenButtonA.activeSelf != shouldShowControls) {
				onScreenButtonA.SetActive(shouldShowControls);
			}

			if (onScreenButtonB != null && onScreenButtonB.activeSelf != shouldShowControls) {
				onScreenButtonB.SetActive(shouldShowControls);
			}

			if (shouldShowControls) {
				Debug.Log("Showing on-screen controls: Mobile platform detected without gamepad");
			}
			else if (isMobilePlatform) {
				Debug.Log("Hiding on-screen controls: Gamepad connected to mobile device");
			}
		}
		finally {
			isUpdatingControls = false;
		}
	}

	private void PauseToggle() {
		if (pauseMenu != null) {
			if (pauseMenu.activeSelf) {
				ResumeGame(); // Resume the game if pause menu is open
			}
			else {
				PauseGame(); // Pause the game if pause menu is closed
			}
		}
	}

	void Start() {
		// Initialize pause menu state - make sure it's initially hidden
		if (pauseMenu != null) {
			pauseMenu.SetActive(false);
		}

		// Start the fade in effect, with player spawning after fade completes
		FadeEffect.ScreenFadeEffect(0f, 1.5f, OnFadeInComplete);

		// Hide timer initially if there is no text component
		if (timerText == null) {
			showTimer = false;
		}

		// Hide fruit counter initially if there is no text component
		if (fruitCountText == null) {
			showFruitCount = false;
		}

		// Initial update of the fruit count
		UpdateFruitCountText();
	}

	// Called when the initial fade-in effect is complete
	private void OnFadeInComplete() {
		// Now spawn the player after fade completes
		PlayerManager.Instance.RespawnPlayer();
	}

	void Update() {
		// Update timer display if we have the text component and timer should be shown
		if (showTimer && timerText != null) {
			float currentTime = TimerManager.Instance.GetCurrentLevelTime();

			if (currentTime > 0) {
				// Format time as just seconds (rounded to 2 decimal places)
				float seconds = Mathf.Floor(currentTime * 100) / 100;
				timerText.text = string.Format("{0:0}s", seconds);
			}
			else {
				timerText.text = "0s";
			}
		}

		// Handle menu selection state when pause menu is active
		if (pauseMenu != null && pauseMenu.activeSelf) {
			UpdateMenuSelection();
		}

		// Update fruit count display
		UpdateFruitCountText();

	}

	// Update the fruit count text to show current collection progress
	private void UpdateFruitCountText() {
		if (showFruitCount && fruitCountText != null) {
			// Get counts from FruitManager
			int collectedFruits = FruitManager.Instance.GetFruitsCollected();
			int totalFruits = FruitManager.Instance.GetFruitsInLevel();

			// Display as "collected/total"
			fruitCountText.text = $"{collectedFruits}/{totalFruits}";
		}
	}

	// Method to be called by GameManager when a fruit is collected
	public void OnFruitCollected() {
		UpdateFruitCountText();
	}

	// Detect mouse movement and deselect menu items
	private void OnMouseMove() {
		if (!usingMouse && pauseMenu != null && pauseMenu.activeSelf) {
			// Only switch to mouse mode if actually moving the mouse
			usingMouse = true;

			// Store the current selection before deselecting
			if (EventSystem.current.currentSelectedGameObject != null) {
				lastSelectedPauseMenuItem = EventSystem.current.currentSelectedGameObject;
			}

			// Deselect UI elements when mouse moves
			EventSystem.current.SetSelectedGameObject(null);
		}
	}

	// Detect keyboard/gamepad input and restore selection
	private void OnKeyboardOrGamepadInput() {
		if (usingMouse && pauseMenu != null && pauseMenu.activeSelf) {
			// Switch from mouse to keyboard/gamepad
			usingMouse = false;

			// Restore last selected object
			if (lastSelectedPauseMenuItem != null && lastSelectedPauseMenuItem.activeInHierarchy) {
				EventSystem.current.SetSelectedGameObject(lastSelectedPauseMenuItem);
			}
			else {
				// Find a selectable element if the previous one isn't valid
				SelectFirstPauseMenuItem();
			}
		}
	}

	// Handle menu selection state
	private void UpdateMenuSelection() {
		// If using keyboard/gamepad but nothing is selected
		if (!usingMouse && EventSystem.current.currentSelectedGameObject == null) {
			// Restore last selection if it exists and is active
			if (lastSelectedPauseMenuItem != null && lastSelectedPauseMenuItem.activeInHierarchy) {
				EventSystem.current.SetSelectedGameObject(lastSelectedPauseMenuItem);
			}
			else {
				// Find a selectable element if the previous one isn't valid
				SelectFirstPauseMenuItem();
			}
		}

		// When keyboard/gamepad is used, update the last selected object
		if (!usingMouse && EventSystem.current.currentSelectedGameObject != null) {
			lastSelectedPauseMenuItem = EventSystem.current.currentSelectedGameObject;
		}
	}

	// Find and select the first interactive UI element in the pause menu
	private void SelectFirstPauseMenuItem() {
		if (pauseMenu != null) {
			Selectable[] selectables = pauseMenu.GetComponentsInChildren<Selectable>();

			// Find first interactable selectable
			Selectable firstInteractable = null;
			foreach (Selectable selectable in selectables) {
				if (selectable.interactable && selectable.gameObject.activeInHierarchy) {
					firstInteractable = selectable;
					break;
				}
			}

			if (firstInteractable != null) {
				lastSelectedPauseMenuItem = firstInteractable.gameObject;
				EventSystem.current.SetSelectedGameObject(lastSelectedPauseMenuItem);
			}
		}
	}

	// method to pause the game
	public void PauseGame() {
		GameObject playerObject = PlayerManager.Instance.GetCurrentPlayer(); // Get the player GameObject
		player = playerObject.GetComponent<Player>(); // Get the Player component
		player.ActionMapping.Disable(); // Disable player input actions
		Time.timeScale = 0f; // Pause the game
		if (pauseMenu != null) {
			pauseMenu.SetActive(true); // Show pause menu UI

			// Reset mouse usage flag and select the first menu item
			usingMouse = false;
			SelectFirstPauseMenuItem();
		}
	}

	// method to resume the game
	public void ResumeGame() {
		Time.timeScale = 1f; // Resume the game
		GameObject playerObject = PlayerManager.Instance.GetCurrentPlayer(); // Get the player GameObject
		player = playerObject.GetComponent<Player>(); // Get the Player component
		player.ActionMapping.Enable(); // Enable player input actions
		if (pauseMenu != null) {
			pauseMenu.SetActive(false); // Hide pause menu UI
		}
	}

	// method to return to main menu
	public void ReturnToMainMenu() {
		LevelManager.Instance.ReturnToMainMenu();
	}
}
