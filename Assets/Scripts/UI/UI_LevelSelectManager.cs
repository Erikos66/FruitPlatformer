using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour {
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private string levelNamePrefix = "Level_";
    [SerializeField] private string levelDisplayNameFormat = "Level {0}";
    [SerializeField] private Vector2 buttonSpacing = new Vector2(0, -80);
    [SerializeField] private bool useBuiltInScenes = true;
    [SerializeField] private bool showLockedLevels = false; // Option to show locked levels (e.g. as disabled buttons)

    // For manual level configuration if not using built-in scenes
    [System.Serializable]
    public class LevelInfo {
        public string displayName;
        public string sceneName;
    }

    [SerializeField] private List<LevelInfo> manualLevelList = new List<LevelInfo>();

    private void Start() {
        GenerateLevelButtons();
    }

    public void RefreshLevelButtons() {
        GenerateLevelButtons();
    }

    private void GenerateLevelButtons() {
        if (buttonContainer == null) {
            Debug.LogError("Button container is not assigned!");
            return;
        }

        if (levelButtonPrefab == null) {
            Debug.LogError("Level button prefab is not assigned!");
            return;
        }

        // Clear existing buttons
        foreach (Transform child in buttonContainer) {
            Destroy(child.gameObject);
        }

        if (useBuiltInScenes) {
            GenerateButtonsFromScenes();
        }
        else {
            GenerateButtonsFromManualList();
        }

        // Adjust content size if container is a RectTransform
        if (buttonContainer.TryGetComponent<RectTransform>(out var rectTransform)) {
            // Give extra padding at the bottom
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
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
            if (GameManager.instance.IsLevelUnlocked(sceneName) || showLockedLevels) {
                bool isUnlocked = GameManager.instance.IsLevelUnlocked(sceneName);

                // Try to parse level number
                if (int.TryParse(levelNumber, out int levelNum)) {
                    CreateLevelButton(string.Format(levelDisplayNameFormat, levelNum), sceneName, isUnlocked);
                }
                else {
                    CreateLevelButton(sceneName, sceneName, isUnlocked);
                }
            }
        }
    }

    private void GenerateButtonsFromManualList() {
        for (int i = 0; i < manualLevelList.Count; i++) {
            string sceneName = manualLevelList[i].sceneName;

            // Only show levels that are unlocked, unless showLockedLevels is true
            if (GameManager.instance.IsLevelUnlocked(sceneName) || showLockedLevels) {
                bool isUnlocked = GameManager.instance.IsLevelUnlocked(sceneName);
                CreateLevelButton(manualLevelList[i].displayName, sceneName, isUnlocked);
            }
        }
    }

    private void CreateLevelButton(string displayName, string sceneName, bool isUnlocked) {
        GameObject buttonObj = Instantiate(levelButtonPrefab, buttonContainer);
        LevelSelectButton levelButton = buttonObj.GetComponent<LevelSelectButton>();

        if (levelButton != null) {
            levelButton.Setup(displayName, sceneName, isUnlocked);
        }
        else {
            Debug.LogError("LevelSelectButton component not found on button prefab!");
        }
    }
}