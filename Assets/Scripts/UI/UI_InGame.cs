using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UI_InGame : MonoBehaviour {
    public static UI_InGame Instance { get; private set; }
    public UI_FadeEffect FadeEffect { get; private set; }

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI fruitCountText; // Added fruit counter text
    [SerializeField] private GameObject pauseMenu; // Reference to the pause menu UI
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
                timerText.text = "00s";
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
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}